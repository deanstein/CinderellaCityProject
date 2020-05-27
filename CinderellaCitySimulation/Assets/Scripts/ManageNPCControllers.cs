using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.AI;

public class NPCControllerGlobals
{
    // the folder path inside Resources to find the NPC Controllers
    public static string animatorControllerFolderPath = "Animator Controllers/";

    // max distance the NPCController can be from the player to disable itself, and pathfinding logic
    public static float maxDistanceBeforeSuspend = 10f;

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
    // define the default animator controller based on this asset's name
    public static string GetDefaultAnimatorControllerFilePathByName(string objectName)
    {
        switch (objectName)
        {
            // talking - androgynous
            case string partialName when partialName.Contains("talking"):
                return  NPCControllerGlobals.animatorControllerFilePathTalking1;

            // walking - male or female
            case string partialName when partialName.Contains("male") && partialName.Contains("walking"):
                return NPCControllerGlobals.animatorControllerFilePathMaleWalking;
            case string partialName when partialName.Contains("female") && partialName.Contains("walking"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleWalking;

            // sitting - male or female
            case string partialName when partialName.Contains("male") && partialName.Contains("sitting"):
                return NPCControllerGlobals.animatorControllerFilePathMaleSitting;
            case string partialName when partialName.Contains("female") && partialName.Contains("sitting"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleSitting;

            // listening - male or female
            case string partialName when partialName.Contains("male") && partialName.Contains("listening"):
                return NPCControllerGlobals.animatorControllerFilePathMaleListening;
            case string partialName when partialName.Contains("female") && partialName.Contains("listening"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleListening;

            // idle - male or female
            case string partialName when partialName.Contains("male") && partialName.Contains("idle"):
                return NPCControllerGlobals.animatorControllerFilePathMaleIdle;
            case string partialName when partialName.Contains("female") && partialName.Contains("idle"):
                return NPCControllerGlobals.animatorControllerFilePathFemaleIdle;

            // if no posture specified, go idle
            default:
                if (objectName.Contains("female"))
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
        if (nameWithGender.Contains("female") || nameWithGender.Contains("_f_") || nameWithGender.Contains("Girl"))
        {
            return NPCControllerGlobals.animatorControllerFilePathFemaleIdle;
        }
        else
        {
            return NPCControllerGlobals.animatorControllerFilePathMaleIdle;
        }
    }


    // get the correct gender controller for idling
    public static string GetWalkingAnimatorControllerByGender(string nameWithGender)
    {
        if (nameWithGender.Contains("female") || nameWithGender.Contains("_f_") || nameWithGender.Contains("Girl"))
        {
            return NPCControllerGlobals.animatorControllerFilePathFemaleWalking;
        }
        else
        {
            return NPCControllerGlobals.animatorControllerFilePathMaleWalking;
        }
    }

    public static void ConfigureNPCForPathfinding(GameObject NPCObject)
    {
        // NPCs need only animate and follow paths when within a certain radius of the player
        ToggleComponentByProximityToPlayer toggleByProximityScript = NPCObject.AddComponent<ToggleComponentByProximityToPlayer>();
        toggleByProximityScript.maxDistance = NPCControllerGlobals.maxDistanceBeforeSuspend;
        toggleByProximityScript.toggleComponentTypes = new string[] { "NavMeshAgent", "FollowPathOnNavMesh" };

        // everyone walks by default
        // add a navigation mesh agent to this person so it can find its way on the navmesh
        NavMeshAgent thisAgent = NPCObject.AddComponent<NavMeshAgent>();
        thisAgent.speed = 1.0f;
        thisAgent.angularSpeed = 60f;
        thisAgent.radius = 0.25f;
        thisAgent.autoTraverseOffMeshLink = false;

        // set the quality of the non-static obstacle avoidance
        thisAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        // add the script to follow a path
        FollowPathOnNavMesh followPathByNameScript = NPCObject.AddComponent<FollowPathOnNavMesh>();

        // add the script to update the animation based on the speed
        UpdateAnimatorBySpeed updateAnimatorScript = NPCObject.AddComponent<UpdateAnimatorBySpeed>();
    }

    // TODO: get this to work
    public static void setAnimationFrame(Animator animatorToSet, string animationName)
    {
        animatorToSet.Play(animationName);
        animatorToSet.Update(Time.deltaTime);
    }

    // add the typical controller, nav mesh agent, and associated scripts to a gameObject
    public static void ConfigureNPCForAnimationAndPathfinding(GameObject proxyObject, GameObject NPCObject)
    {
        // if there's a proxy object, check the proxy name for a specific animation to use
        if (proxyObject)
        {
            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(proxyObject.name));

            // TODO: get the initial animation to appear in the editor
            /*
            string animationName = thisAnimator.runtimeAnimatorController.animationClips[0].name;

            setAnimationFrame(thisAnimator, animationName);
            */

            // anyone not the following gestures will get configured to walk
            if (!proxyObject.name.Contains("talking") && !proxyObject.name.Contains("listening") && !proxyObject.name.Contains("idle") && !proxyObject.name.Contains("sitting"))
            {
                ConfigureNPCForPathfinding(NPCObject);
            }
        }

        // otherwise, this is a random filler person and can be configured to walk
        else
        {
            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetWalkingAnimatorControllerByGender(NPCObject.name));

            // TODO: get the initial animation to appear in the editor

            // configure the random filler person for pathfinding
            ConfigureNPCForPathfinding(NPCObject);
        }
    }
}

