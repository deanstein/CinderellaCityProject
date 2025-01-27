using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// "Hoists" scene containers and other objects up or down, 
/// so they aren't occupying the same space which could cause Occlusion Culling to fail
/// </summary>

public static class HoistSceneGlobals
{
    // the space interval used to hoist each scene vertically to avoid levels sharing the same space
    public static float hoistInterval = 90f;
}

public class HoistSceneObjects : MonoBehaviour
{
    public static Transform AdjustTransformForHoistBetweenScenes(Transform transformToModify, string referringSceneName, string upcomingSceneName)
    {
        Vector3 newPosition = AdjustPositionForHoistBetweenScenes(transformToModify.position, referringSceneName, upcomingSceneName);

        Transform adjustedTransform = transformToModify;
        adjustedTransform.position = newPosition;

        return adjustedTransform;
    }

    public static Vector3 AdjustPositionForHoistBetweenScenes(Vector3 positionToModify, string referringSceneName, string upcomingSceneName)
    {
        float hoistDelta = GetHoistDeltaBetweenScenes(referringSceneName, upcomingSceneName);

        //DebugUtils.DebugLog("Scene hoist delta: " + hoistDelta + " between " + SceneGlobals.referringSceneName + " and " + SceneGlobals.upcomingSceneName);

        Vector3 newPosition = new Vector3(positionToModify.x, positionToModify.y + hoistDelta, positionToModify.z);

        //Debug.Log("New position: " + newPosition);
        return newPosition;
    }

    // returns a height by which to host an entire scene, or the player in a scene
    public static float GetHoistHeightBySceneName(string partialName)
    {
        switch (partialName)
        {
            case string name when partialName.Contains("60s70s"):
                return 0;
            case string name when partialName.Contains("80s90s"):
                return HoistSceneGlobals.hoistInterval * 1;
            case string name when partialName.Contains("AltFuture"):
                return HoistSceneGlobals.hoistInterval * 2;
            default:
                return -1;
        }
    }

    // of the time period scenes, get only the ones that actually require hoisting
    // used before scenes are open in the editor, to avoid opening unnecessary scenes for hoisting only
    public static List<string> GetScenesRequiringHoisting()
    {
        List<string> timePeriodSceneNames = SceneGlobals.availableTimePeriodSceneNamesList;
        List<string> timePeriodSceneNamesForHoist = new List<string>();

        // check each available time period for a hoist height
        foreach (string timePeriodSceneName in timePeriodSceneNames)
        {
            if (HoistSceneObjects.GetHoistHeightBySceneName(timePeriodSceneName) != 0)
            {
                timePeriodSceneNamesForHoist.Add(timePeriodSceneName);
            }
        }

        return timePeriodSceneNamesForHoist;
    }

    // determines the distance to move the player
    // depending on which scene is outgoing and incoming
    public static float GetHoistDeltaBetweenScenes(string oldSceneName, string newSceneName)
    {
        float hoistDelta = 0;

        float oldSceneHoistHeight = GetHoistHeightBySceneName(oldSceneName);
        float newSceneHoistHeight = GetHoistHeightBySceneName(newSceneName);

        // if either of the scene hoist heights are -1,
        // that scene is a menu and shouldn't affect hoisting
        if (oldSceneHoistHeight == -1 || newSceneHoistHeight == -1)
        {
            hoistDelta = 0;
        }
        // otherwise, calculate the hoist height between time period scenes
        else
        {
            hoistDelta = newSceneHoistHeight - oldSceneHoistHeight;
        }

        return hoistDelta;
    }

    // hoists the current scene container up
    public static Vector3 HoistCurrentSceneContainer(GameObject sceneObject)
    {
        var newPosition = Vector3.zero;

        GameObject currentSceneContainer = ManageSceneObjects.GetSceneContainerObject(sceneObject.scene);

        float hoistHeight = GetHoistHeightBySceneName(currentSceneContainer.name);
        //DebugUtils.DebugLog("Current scene container (" + currentSceneContainer.name + ") hoist height: " + hoistHeight);

        if (hoistHeight != 0)
        {
            newPosition = new Vector3(currentSceneContainer.transform.position.x, currentSceneContainer.transform.position.y + hoistHeight, currentSceneContainer.transform.position.z);

            if (newPosition != currentSceneContainer.transform.position)
            {
                currentSceneContainer.transform.position = newPosition;
            }

            return newPosition;
        }

        return newPosition;
    }

    // hoists an object when time traveling
    public static Vector3 HoistObjectOnSceneChange(GameObject objectToHoist)
    {
        Vector3 newPosition = Vector3.zero;

        if (objectToHoist != null)
        {
            float hoistDelta = GetHoistDeltaBetweenScenes(SceneGlobals.lastKnownTimePeriodSceneName, SceneGlobals.upcomingSceneName);

            //DebugUtils.DebugLog("Scene hoist delta: " + hoistDelta + " between " + SceneGlobals.referringScene + " and " + SceneGlobals.upcomingScene);

            newPosition = new Vector3(objectToHoist.transform.position.x, objectToHoist.transform.position.y + hoistDelta, objectToHoist.transform.position.z);

            if (hoistDelta != 0)
            {
                objectToHoist.transform.position = newPosition;
            }

            //Debug.Log("New position: " + newPosition);
            return newPosition;
        }

        // return the origin if the object didn't exist
        return newPosition;
    }

}