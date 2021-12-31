using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enables or disables children GameObject Components (NavMeshAgents, scripts...) 
/// based on their proximity to the player
/// Used for dynamic objects like NPCs
/// </summary>

// this script should be attached to a parent GameObject containing
// children with components that need to be disabled when too far from the player

public class ToggleChildrenComponentsByProximityToPlayer : MonoBehaviour {

    // the component types to toggle are passed in from AssetImportPipeline
    public string[] toggleComponentTypes;

    // distribute the large arrays into smaller ones to be evaluated over many frames
    int numberOfDistributedArrays = 8; // also the number of frames before the entire array is updated
    int distributedChildrenListLength = 0;

    // instantiate lists for children
    List<Transform> childrenTransformsList = new List<Transform>();
    List<GameObject> childrenObjectsList = new List<GameObject>();
    List<Vector3> childrenPositionsList = new List<Vector3>();
    List<GameObject> activeChildrenList = new List<GameObject>();
    List<int> childrenNotInFrameCountsList = new List<int>();

    // instantiate arrays for children
    Transform[] childrenTransforms;
    GameObject[] childrenObjects;
    Vector3[] childrenPositions;
    int[] childrenNotInFrameCounts;

    // instantiate distributed arrays - these are small subsets of the large arrays
    // to enhance performance by breaking up the potentially huge list of objects over many frames
    GameObject[] distributedChildrenObjects;
    Vector3[] distributedChildrenPositions;
    int[] distributedChildrenNotInFrameCounts;

    // some use cases, like NPCs, need to also check if they are in frame of the player's camera
    // others, like speakers, don't care about this
    // this is set by AssetImportPipeline
    public bool checkIfInFrame;

    // the screen-space position of an object in world space
    Vector3 screenSpacePoint;

    // these "extend" the camera extents (usualy between 0 and 1), to more quickly enable components
    // when the camera is sweeping around, so the player won't notice the components being enabled
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
    public int maxNotInFrameCount = 150;

    private void Awake()
    {
        // get all the children of this object - including empty transforms (instances or groups)
        childrenTransforms = this.GetComponentsInChildren<Transform>();

        // loop through the gameobjects and get transforms containing one of the desired behaviors
        foreach (Transform childrenTransform in childrenTransforms)
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
            distributedChildrenObjects = ArrayUtils.RangeSubset(childrenObjects, distributedListIndexStart, distributedChildrenListLength);
            distributedChildrenPositions = ArrayUtils.RangeSubset(childrenPositions, distributedListIndexStart, distributedChildrenListLength);
            distributedChildrenNotInFrameCounts = ArrayUtils.RangeSubset(childrenNotInFrameCounts, distributedListIndexStart, distributedChildrenListLength);
            frameCount++;
        }
        // check if we're at the end of the array, and adjust the last index to stay in bounds
        // and reset the frame count
        else
        {
            distributedChildrenObjects = ArrayUtils.RangeSubset(childrenObjects, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenObjects.Length));
            distributedChildrenPositions = ArrayUtils.RangeSubset(childrenPositions, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenPositions.Length));
            distributedChildrenNotInFrameCounts = ArrayUtils.RangeSubset(childrenNotInFrameCounts, distributedListIndexStart, distributedChildrenListLength - (distributedListIndexEnd - childrenNotInFrameCounts.Length));
            frameCount = 0;
        }

        // update every child's position, and check if they are within range
        for (int i = 0; i < distributedChildrenObjects.Length; i++)
        {
            // use this to reference and update the values in the original array
            int originalIndex = distributedListIndexStart + i;

            // update this child's position
            distributedChildrenPositions[i] = distributedChildrenObjects[i].transform.position;
            childrenPositions[originalIndex] = distributedChildrenPositions[i]; // update the original array

            // if this is a speaker object, its max distance may be different
            maxDistance = distributedChildrenObjects[i].name.Contains("speaker-") ? PlayAudioSequencesByName.AssociateSpeakerParamsByName(distributedChildrenObjects[i].name).maxDistance : maxDistance;

            // skip if there's no player camera available
            if (!ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera)
            {
                return;
            }

            // determine if the child is within the specified max distance of the player
            bool isWithinRange = Vector3.Distance(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position, distributedChildrenPositions[i]) < maxDistance;

            // update the isOnScreen bool only if the checkIfInFrame flag is set
            // otherwise, isOnScreen will be set to match isWithinrange
            bool isInFrame;
            if (checkIfInFrame)
            {
                // get the screen space position of this object
                screenSpacePoint = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.WorldToViewportPoint(distributedChildrenPositions[i]);

                isInFrame = screenSpacePoint.z > -1 && screenSpacePoint.x > -1 && screenSpacePoint.x < 2 && screenSpacePoint.y > -1 && screenSpacePoint.y < 2;
            }
            else
            {
                isInFrame = isWithinRange;
            }

            // if we're within range, and the object is visible, enable the components
            if (isWithinRange && isInFrame)
            {
                // first, ensure we're not tracking this object already
                if (!activeChildrenList.Contains(distributedChildrenObjects[i]))
                {
                    EnableComponents(distributedChildrenObjects[i]);
                    activeChildrenList.Add(distributedChildrenObjects[i]);
                    childrenNotInFrameCounts[originalIndex] = 0; // update the original array
                }
            }
            // otherwise, disable the components
            else
            {
                distributedChildrenNotInFrameCounts[i]++;
                childrenNotInFrameCounts[originalIndex] = distributedChildrenNotInFrameCounts[i]; // update the original array

                bool isOutOfFrame;
                if (checkIfInFrame)
                {
                    isOutOfFrame = distributedChildrenNotInFrameCounts[i] >= maxNotInFrameCount;
                }
                else
                {
                    isOutOfFrame = true;
                }

                // check if this object is being tracked as active, and if it's been out of frame for enough time
                if (activeChildrenList.Contains(distributedChildrenObjects[i]) && isOutOfFrame)
                {
                    // check for the CanDisableComponents script, if attached
                    CanDisableComponents thisDisableComponentsScript = distributedChildrenObjects[i].GetComponent<CanDisableComponents>();
                    if (thisDisableComponentsScript)
                    {
                        // only disable the object if the canDisableComponents script allows it
                        if (thisDisableComponentsScript.canDisableComponents == true)
                        {
                            DisableComponents(distributedChildrenObjects[i]);
                            activeChildrenList.Remove(distributedChildrenObjects[i]);
                            childrenNotInFrameCounts[originalIndex] = 0; // update the original array
                        }
                    }
                    else
                    {
                        DisableComponents(distributedChildrenObjects[i]);
                        activeChildrenList.Remove(distributedChildrenObjects[i]);
                        childrenNotInFrameCounts[originalIndex] = 0; // update the original array
                    }
                }
            }
        }
    }

    public void EnableComponents(GameObject hostObject)
    {
        foreach (string componentType in toggleComponentTypes)
        {
            Behaviour behaviorComponent = hostObject.GetComponent(componentType) as Behaviour;

            if (behaviorComponent)
            {
                if (!behaviorComponent.enabled)
                {
                    behaviorComponent.enabled = true;
                }
            }

        }
    }

    public void DisableComponents(GameObject hostObject)
    {
        foreach (string componentType in toggleComponentTypes)
        {
            Behaviour behaviorComponent = hostObject.GetComponent(componentType) as Behaviour;

            if (behaviorComponent)
            {
                if (behaviorComponent.enabled)
                {
                    behaviorComponent.enabled = false;
                }
                else
                {
                    behaviorComponent.enabled = false;
                }
            }
        }
    }
}

