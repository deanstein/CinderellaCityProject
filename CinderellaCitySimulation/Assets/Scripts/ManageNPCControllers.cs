using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Maps 3D mall patrons (non-player characters) to animation controllers, depending on name or gender
/// </summary>

public class NPCControllerGlobals
{
    // keep track of how many NPCControllers are active and pathfinding
    public static int activeNPCControllersCount = 0;
    public static List<GameObject> activeNPCControllersList = new List<GameObject>();
    public static GameObject[] activeNPCControllersArray;

    // keep track of the initial NPC positions, for use in randomly picking from later
    public static List<Vector3> initialNPCPositionsList = new List<Vector3>();
    public static Vector3[] initialNPCPositionsArray;

    // the agent radius, and also the stopping distance to avoid congestion near destination points
    public static float defaultNPCRadius = 0.4f;
    public static float defaultNPCStoppingDistance = defaultNPCRadius * 8;

    // the maximum step height that NPCs can navigate over
    public static float maxStepHeight = 0.18f;

    // the minimum agent speed allowed before trying to re-path
    public static float minimumSpeedBeforeRepath = 0.3f;

    // define the walking speed range
    public static float minWalkingSpeed = 0.9f;
    public static float maxWalkingSpeed = 1.0f;

    // the max distance for new NPC destinations
    public static float maxDestinationDistance = 25f;

    // when NPCs get into a traffic jam, they get set to a "discard" destination, far away
    // this helps self-correct when areas get too congested
    public static float minDiscardDistance = 10f;
    public static float maxDiscardDistance = 40f;

    // on import, max disance an NPC can be from a valid navmesh point before it's relocated randomly
    public static float maxDistanceForClosestPointAdjustment = 5f;

    // max distance the NPCController can be from the player to disable itself, and its pathfinding logic
    public static float maxDistanceBeforeSuspend = 60f;

    // the folder path inside Resources to find the NPC Controllers
    public static string animatorControllerFolderPath = "Animator Controllers/";

    // different prefabs from different sources refer to 
    // male and female in a variety of ways, so capture them here
    public static List<string> maleDescriptorsList = new List<string> { "male", "_m_", "Man" };
    public static List<string> femaleDescriptorsList = new List<string> { "female", "_f_", "Girl", "granny" };

    // define the available animator controller file paths
    public static string animatorControllerFilePathTalking1 = animatorControllerFolderPath + "talking 1";

    public static string animatorControllerFilePathMaleWalking = animatorControllerFolderPath + "male walking";
    public static string animatorControllerFilePathFemaleWalking = animatorControllerFolderPath + "female walking";

    public static string animatorControllerFilePathMaleSitting = animatorControllerFolderPath + "male sitting";
    public static string animatorControllerFilePathFemaleSitting = animatorControllerFolderPath + "female sitting";

    public static string animatorControllerFilePathMaleListening = animatorControllerFolderPath + "male listening";
    public static string animatorControllerFilePathFemaleListening = animatorControllerFolderPath + "female listening";

    public static string animatorControllerFilePathMaleIdle = animatorControllerFolderPath + "male idle 1";
    public static string animatorControllerFilePathFemaleIdle = animatorControllerFolderPath + "female idle 1";
}

public class ManageNPCControllers
{
    // default configurations for NPCs (and also FPC)
    public static void ConfigureAgentWIthDefaultNPCSettings(NavMeshAgent agentToConfigure)
    {
        agentToConfigure.speed = 1.0f;
        agentToConfigure.angularSpeed = 200f;
        agentToConfigure.radius = NPCControllerGlobals.defaultNPCRadius;
        agentToConfigure.autoTraverseOffMeshLink = false;
        agentToConfigure.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agentToConfigure.enabled = false;
    }

    // define the default animator controller based on this asset's name
    public static string GetDefaultAnimatorControllerFilePathByName(string objectName)
    {
        switch (objectName)
        {
            // talking - androgynous
            case string partialName when partialName.Contains("talking"):
                return  NPCControllerGlobals.animatorControllerFilePathTalking1;

            // walking - male or female
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.maleDescriptorsList, partialName) && partialName.Contains("walking"):
                return NPCControllerGlobals.animatorControllerFilePathMaleWalking;
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, partialName) && partialName.Contains("walking"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleWalking;

            // sitting - male or female
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.maleDescriptorsList, partialName) && partialName.Contains("sitting"):
                return NPCControllerGlobals.animatorControllerFilePathMaleSitting;
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, partialName) && partialName.Contains("sitting"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleSitting;

            // listening - male or female
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.maleDescriptorsList, partialName) && partialName.Contains("listening"):
                return NPCControllerGlobals.animatorControllerFilePathMaleListening;
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, partialName) && partialName.Contains("listening"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleListening;

            // idle - male or female
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.maleDescriptorsList, partialName) && partialName.Contains("idle"):
                return NPCControllerGlobals.animatorControllerFilePathMaleIdle;
            case string partialName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, partialName) && partialName.Contains("idle"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleIdle;

            // if no posture specified, default to walking
            default:
                if (Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, objectName))
                {
                    return NPCControllerGlobals.animatorControllerFilePathFemaleWalking;
                }
                else
                {
                    return NPCControllerGlobals.animatorControllerFilePathMaleWalking;
                }
        }
    }

    // get the correct gender controller for idling
    public static string GetIdleAnimatorControllerByGender(string nameWithGender)
    {
        if (Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, nameWithGender))
        {
            return NPCControllerGlobals.animatorControllerFilePathFemaleIdle;
        }
        else
        {
            return NPCControllerGlobals.animatorControllerFilePathMaleIdle;
        }
    }

    // get the correct gender controller for walking
    public static string GetWalkingAnimatorControllerByGender(string nameWithGender)
    {
        if (Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, nameWithGender))
        {
            return NPCControllerGlobals.animatorControllerFilePathFemaleWalking;
        }
        else
        {
            return NPCControllerGlobals.animatorControllerFilePathMaleWalking;
        }
    }

    // the people prefabs that come from some vendors may over-scale when matched to their proxy's height
    // mitigate this, while also making the female characters slightly smaller
    public static float GetRandomNPCScaleDownFactor(GameObject gameObjectToScale)
    {
        switch (gameObjectToScale.name)
        {
            // if the provided gameobject is named to indicate it's a male
            case string objectName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.maleDescriptorsList, gameObjectToScale.name):
                return Random.Range(0.89f, 0.92f);

            // if the provided gameobject is named to indicate it's a female
            case string objectName when Utils.StringUtils.TestIfAnyListItemContainedInString(NPCControllerGlobals.femaleDescriptorsList, gameObjectToScale.name):
                return Random.Range(0.86f, 0.89f);

            // otherwise, don't recommend a scale change
            default:
                return 1;
        }
    }

    // reset all NPCs to their original location
    public static void ResetAllNPCsToOriginalLocation()
    {
        for (var i = 0; i < NPCControllerGlobals.activeNPCControllersArray.Length; i++)
        {
            NPCControllerGlobals.activeNPCControllersArray[i].transform.position = NPCControllerGlobals.initialNPCPositionsArray[i];
        }
    }
}

