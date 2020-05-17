using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControllerGlobals
{
    // the folder path inside Resources to find the NPC Controllers
    public static string animatorControllerFolderPath = "Animator Controllers/";

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
        if (nameWithGender.Contains("female"))
        {
            return NPCControllerGlobals.animatorControllerFilePathFemaleIdle;
        }
        else
        {
            return NPCControllerGlobals.animatorControllerFilePathMaleIdle;
        }
    }
}

