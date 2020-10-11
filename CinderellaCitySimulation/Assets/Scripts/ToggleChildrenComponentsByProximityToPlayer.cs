using UnityEngine;

using System.Collections.Generic;

// this script should be attached to a parent GameObject containing
// children with components that need to be disabled when too far from the player

public class ToggleChildrenComponentsByProximityToPlayer : MonoBehaviour {

    // the component types to toggle are passed in from AssetImportPipeline
    public string[] toggleComponentTypes;

    // distribute the large arrays into smaller ones to be evaluated over many frames
    int numberOfDistributedArrays = 8; // also the number of frames before the entire array is updated
    int distributedChildrenListLength = 0;

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

    // instantiate arrays for children
    GameObject[] childrenObjects;
    Vector3[] childrenPositions;
    int[] childrenNotInFrameCounts;

    // instantiate distributed arrays - these are small subsets of the large arrays
    // to enhance performance by breaking up the potentially huge list of objects over many frames
    GameObject[] distributedChildrenObjects;
    Vector3[] distributedChildrenPositions;
    int[] distributedChildrenNotInFrameCounts;

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

        distributedChildrenListLength = Mathf.RoundToInt(childrenObjects.Length / numberOfDistributedArrays);
    }

    private void OnEnable()
    {
        // on enable, set the current count to the max so we immediately update
        frameCount = maxFramesBetweenCheck;

        // get the player, its camera, and its position
        player = ManageFPSControllers.FPSControllerGlobals.activeFPSController;
        if (player)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
            playerPosition = player.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int distributedListIndexStart = frameCount * distributedChildrenListLength;
        int distributedListIndexEnd = distributedListIndexStart + distributedChildrenListLength;

        // update the distributed array subsets based on the current frame
        // and increment the frame count
        if (distributedListIndexEnd < childrenObjects.Length - 1)
        {
            distributedChildrenObjects = ArrayUtilities.RangeSubset(childrenObjects, distributedListIndexStart, distributedChildrenListLength);
            distributedChildrenPositions = ArrayUtilities.RangeSubset(childrenPositions, distributedListIndexStart, distributedChildrenListLength);
            distributedChildrenNotInFrameCounts = ArrayUtilities.RangeSubset(childrenNotInFrameCounts, distributedListIndexStart, distributedChildrenListLength);
            frameCount++;
        }
        // check if we're at the end of the array, and adjust the last index to stay in bounds
        // and reset the frame count
        else
        {
            distributedChildrenObjects = ArrayUtilities.RangeSubset(childrenObjects, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenObjects.Length));
            distributedChildrenPositions = ArrayUtilities.RangeSubset(childrenPositions, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenPositions.Length));
            distributedChildrenNotInFrameCounts = ArrayUtilities.RangeSubset(childrenNotInFrameCounts, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenNotInFrameCounts.Length));
            frameCount = 0;
        }

        // get the latest player position
        playerPosition = player.transform.position;

        // update every child's position, and check if they are within range
        for (var i = 0; i < distributedChildrenPositions.Length; i++)
        {
            // update this child's position
            distributedChildrenPositions[i] = new Vector3(distributedChildrenObjects[i].transform.position.x, distributedChildrenObjects[i].transform.position.y + 1, distributedChildrenObjects[i].transform.position.z);

            // get the screen space position of this object
            screenSpacePoint = playerCamera.WorldToViewportPoint(distributedChildrenPositions[i]);

            // determine if the child's position is visible to screen space
            bool isOnScreen = false;

            if (screenSpacePoint.z > -1 && screenSpacePoint.x > -1 && screenSpacePoint.x < 2 && screenSpacePoint.y > -1 && screenSpacePoint.y < 2)
            {
                isOnScreen = true;
            }

            // if we're within range, and the object is visible, enable the components
            if (Utils.GeometryUtils.GetFastDistance(playerPosition, distributedChildrenPositions[i]) < maxDistance && isOnScreen)
            {
                // first, ensure we're not tracking this object already
                if (!activeChildrenList.Contains(distributedChildrenObjects[i]))
                {
                    EnableComponents(distributedChildrenObjects[i]);
                    activeChildrenList.Add(distributedChildrenObjects[i]);
                    distributedChildrenNotInFrameCounts[i] = 0;
                }
            }
            // otherwise, disable the components
            else
            {
                distributedChildrenNotInFrameCounts[i]++;

                // check if this object is being tracked as active, and if it's been out of frame for enough time
                if (activeChildrenList.Contains(distributedChildrenObjects[i]) && distributedChildrenNotInFrameCounts[i] >= maxNotInFrameCount)
                {
                    DisableComponents(distributedChildrenObjects[i]);
                    activeChildrenList.Remove(distributedChildrenObjects[i]);
                    distributedChildrenNotInFrameCounts[i] = 0;
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

