using UnityEngine;

using System.Collections.Generic;

// this script should be attached to a parent GameObject containing
// other children with components that need to be disabled when too far from the player

public class ToggleChildrenComponentsByProximityToPlayer : MonoBehaviour {

    // get the player and its position
    GameObject player;
    Vector3 playerPosition;

    // instantiate lists for children
    List<Transform> childrenTransformsList = new List<Transform>();
    List<GameObject> childrenObjectsList = new List<GameObject>();
    List<Vector3> childrenPositionsList = new List<Vector3>();
    List<GameObject> activeChildrenList = new List<GameObject>();

    //instantiate arrays for children
    GameObject[] childrenObjects;
    Vector3[] childrenPositions;

    // the component types to toggle are passed in from AssetImportPipeline
    public string[] toggleComponentTypes;

    // the current distance between this object and the player
    public float currentDistance = 0;

    // the maximum distance this object can be from the FPSController before the specified component is disabled
    // this may be overridden by AssetImportPipeline
    public float maxDistance = 20f;

    public float frames = 0;

    private void Awake()
    {
        // get the player
        player = GameObject.FindWithTag("Player");

        // get the initial position of the player
        playerPosition = player.transform.position;

        // get all the NPCs - starting with their transforms
        childrenTransformsList = Utils.GeometryUtils.GetAllChildrenTransforms(this.transform);

        // record every transform as a GameObject in a new list
        foreach (Transform NPCTransform in childrenTransformsList)
        {
            if (NPCTransform.GetComponent(toggleComponentTypes[0]) as Behaviour)
            {
                childrenObjectsList.Add(NPCTransform.gameObject);
            }

            // disable the children components on this tranform's gamobject initially
            DisableComponents(NPCTransform.gameObject);
        }

        // convert the list to an array
        childrenObjects = childrenObjectsList.ToArray();

        foreach (GameObject NPCObject in childrenObjectsList)
        {
            childrenPositionsList.Add(NPCObject.transform.position);
        }

        // convert the list to an array
        childrenPositions = childrenPositionsList.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        frames++;

        if (frames == 5)
        {
            frames = 0;

            // get the latest player position
            playerPosition = player.transform.position;

            // update every child's position, and check if they are within range
            for (var i = 0; i < childrenPositions.Length; i++)
            {
                // update this child's position
                childrenPositions[i] = childrenObjects[i].transform.position;

                // if we're within range, enable the components
                if (GetFastDistance(playerPosition, childrenPositions[i]) < maxDistance)
                {
                    // first, ensure we're not tracking this object already
                    if (!activeChildrenList.Contains(childrenObjects[i]))
                    {
                        EnableComponents(childrenObjects[i]);
                        activeChildrenList.Add(childrenObjects[i]);
                    }
                }
                // otherwise, disable the components
                else
                {
                    // first, check if this object is being tracked as active
                    if (activeChildrenList.Contains(childrenObjects[i]))
                    {
                        DisableComponents(childrenObjects[i]);
                        activeChildrenList.Remove(childrenObjects[i]);
                    }
                }
            }
        }
    }

    float GetFastDistance(Vector3 v1, Vector3 v2)
    {
        float f;
        float f2;
        f = v1.x - v2.x;
        if (f < 0) { f = f * -1; }
        f2 = v1.z - v2.z;
        if (f2 < 0) { f2 = f2 * -1; }

        if (f > f2) { f2 = f; }
        // simulates a box-shaped distance calculation

        return f2;
    }

    public void EnableComponents(GameObject hostObject)
    {
        foreach (string componentType in toggleComponentTypes)
        {
            if (hostObject.GetComponent(componentType) as Behaviour)
            {
                if (!(hostObject.GetComponent(componentType) as Behaviour).enabled)
                {
                    //Debug.Log("Enabled " + componentType + " on " + this.name);
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
                    //Debug.Log("Enabled " + componentType + " on " + this.name);
                    (hostObject.GetComponent(componentType) as Behaviour).enabled = false;
                }
            }
        }
    }
}

