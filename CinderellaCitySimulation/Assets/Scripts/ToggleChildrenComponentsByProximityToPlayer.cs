using UnityEngine;

using System.Collections.Generic;

// this script should be attached to a parent GameObject containing
// children with components that need to be disabled when too far from the player

public class ToggleChildrenComponentsByProximityToPlayer : MonoBehaviour {

    // the component types to toggle are passed in from AssetImportPipeline
    public string[] toggleComponentTypes;

    // get the player, its position, and its camera
    GameObject player;
    Vector3 playerPosition;
    Camera playerCamera;

    // instantiate lists for children
    List<Transform> childrenTransformsList = new List<Transform>();
    List<GameObject> childrenObjectsList = new List<GameObject>();
    List<Vector3> childrenPositionsList = new List<Vector3>();
    List<GameObject> activeChildrenList = new List<GameObject>();
    List<int> childrenNotInFrameCountsList = new List<int>();

    //instantiate arrays for children
    GameObject[] childrenObjects;
    Vector3[] childrenPositions;
    int[] childrenNotInFrameCounts;

    // the screen-space position of an object in world space
    Vector3 screenSpacePoint;

    // these "extend" the camera extents (usualy between 0 and 1), to more quickly enable components
    // when the camera is sweeping around -so the player won't notice the components being enabled
    public float minScreenSpacePoint = -2;
    public float maxScreenSpacePoint = 3;

    // the current distance between this object and the player
    public float currentDistance = 0;

    // the maximum distance this object can be from the player before the specified components are disabled
    // this may be overridden by AssetImportPipeline
    public float maxDistance = 20f;

    // keep track of the number of frames between updates
    public int frameCount = 0;

    // the number of frames between proximity checks
    // not doing this calculation every frame helps with performance
    public int maxFramesBetweenCheck = 5;

    // the max number of times we check and if the object is out of view, disable its components
    // this prevents moving objects from stopping right in front of the camera when components are disabled
    public int maxNotInFrameCount = 250;

    private void Awake()
    {
        // get the player
        player = GameObject.FindWithTag("Player");

        // get the player's camera
        playerCamera = player.transform.GetChild(0).GetComponent<Camera>();

        // get the initial player position
        playerPosition = player.transform.position;

        // get all the children of this object - starting with their transforms
        childrenTransformsList = Utils.GeometryUtils.GetAllChildrenTransforms(this.transform);

        // get the corresponding gameobjects
        foreach (Transform childrenTransform in childrenTransformsList)
        {
            if (childrenTransform.GetComponent(toggleComponentTypes[0]) as Behaviour)
            {
                childrenObjectsList.Add(childrenTransform.gameObject);
            }

            // disable the children components on this tranform's gamobject initially
            DisableComponents(childrenTransform.gameObject);
        }

        // fill out lists about the children
        foreach (GameObject childObject in childrenObjectsList)
        {
            childrenPositionsList.Add(childObject.transform.position);
            childrenNotInFrameCountsList.Add(0);
        }

        // convert the lists to arrays
        childrenObjects = childrenObjectsList.ToArray();
        childrenPositions = childrenPositionsList.ToArray();
        childrenNotInFrameCounts = childrenNotInFrameCountsList.ToArray();
    }

    private void OnEnable()
    {
        // on enable, set the current count to the max so we immediately update
        frameCount = maxFramesBetweenCheck;
    }

    // Update is called once per frame
    void Update()
    {
        // increment the framecount
        frameCount++;

        // do this expensive check only when we've reached the max amount of frames between checks
        if (frameCount >= maxFramesBetweenCheck)
        {
            // reset the frame count
            frameCount = 0;

            // get the latest player position
            playerPosition = player.transform.position;

            // update every child's position, and check if they are within range
            for (var i = 0; i < childrenPositions.Length; i++)
            {
                // update this child's position
                childrenPositions[i] = new Vector3(childrenObjects[i].transform.position.x, childrenObjects[i].transform.position.y + 1, childrenObjects[i].transform.position.z);

                // get the screen space position of this object
                screenSpacePoint = playerCamera.WorldToViewportPoint(childrenPositions[i]);

                // determine if the child's position is visible to screen space
                bool isOnScreen = false;

                if (screenSpacePoint.z > -1 && screenSpacePoint.x > -1 && screenSpacePoint.x < 2 && screenSpacePoint.y > -1 && screenSpacePoint.y < 2)
                {
                    isOnScreen = true;
                }

                // if we're within range, and the object is visible, enable the components
                if (Utils.GeometryUtils.GetFastDistance(playerPosition, childrenPositions[i]) < maxDistance && isOnScreen)
                {
                    // first, ensure we're not tracking this object already
                    if (!activeChildrenList.Contains(childrenObjects[i]))
                    {
                        EnableComponents(childrenObjects[i]);
                        activeChildrenList.Add(childrenObjects[i]);
                        childrenNotInFrameCounts[i] = 0;
                    }
                }
                // otherwise, disable the components
                else
                {
                    childrenNotInFrameCounts[i]++;

                    // check if this object is being tracked as active, and if it's been out of frame for enough time
                    if (activeChildrenList.Contains(childrenObjects[i]) && childrenNotInFrameCounts[i] >= maxNotInFrameCount)
                    {
                        DisableComponents(childrenObjects[i]);
                        activeChildrenList.Remove(childrenObjects[i]);
                        childrenNotInFrameCounts[i] = 0;
                    }
                }
            }
        }
    }

    public void EnableComponents(GameObject hostObject)
    {
        foreach (string componentType in toggleComponentTypes)
        {
            if (hostObject.GetComponent(componentType) as Behaviour)
            {
                if (!(hostObject.GetComponent(componentType) as Behaviour).enabled)
                {
                    //Utils.DebugUtils.DebugLog("Enabled " + componentType + " on " + this.name);
                    (hostObject.GetComponent(componentType) as Behaviour).enabled = true;
                }
            }

        }
    }

    public void DisableComponents(GameObject hostObject)
    {
        foreach (string componentType in toggleComponentTypes)
        {
            if (hostObject.GetComponent(componentType) as Behaviour)
            {
                if ((hostObject.GetComponent(componentType) as Behaviour).enabled)
                {
                    //Utils.DebugUtils.DebugLog("Enabled " + componentType + " on " + this.name);
                    (hostObject.GetComponent(componentType) as Behaviour).enabled = false;
                }
            }
        }
    }
}

