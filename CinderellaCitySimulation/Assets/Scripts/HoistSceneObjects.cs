using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

// this script is used to "hoist" an object or an entire scene
// to solve the issue of Occlusion Culling not working because scens are occupying the same space

public static class HoistSceneGlobals
{
    // the space interval used to hoist each scene vertically to avoid levels sharing the same space
    public static float hoistInterval = 20f;

    // editor only: record the original positions of the time period scene containers
    // so we can move the containers back to their original positions when done
    public static List<Vector3> originalTimePeriodSceneContainerPositions = new List<Vector3>();
}

public class HoistSceneObjects : MonoBehaviour
{
    // when attached to a scene container object, the object moves at the start of the game
    private void Awake()
    {
        HoistCurrentSceneContainer(this.gameObject);
    }

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

        Utils.DebugUtils.DebugLog("Scene hoist delta: " + hoistDelta + " between " + SceneGlobals.referringSceneName + " and " + SceneGlobals.upcomingSceneName);

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

    // determines the distance to move the player
    // depending on which scene is outgoing and incoming
    public static float GetHoistDeltaBetweenScenes(string oldSceneName, string newSceneName)
    {
        float hoistDelta = 0;

        float oldSceneHoistHeight = GetHoistHeightBySceneName(oldSceneName);
        float newSceneHoistHeight = GetHoistHeightBySceneName(newSceneName);

        Debug.Log("Old scene (" + oldSceneName + ") hoist height: " + oldSceneHoistHeight + " and new scene (" + newSceneName + ") hoist height: " + newSceneHoistHeight);

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

        GameObject currentSceneContainer = ManageScenes.GetSceneContainerObject(sceneObject.scene);

        float hoistHeight = GetHoistHeightBySceneName(currentSceneContainer.name);
        //Utils.DebugUtils.DebugLog("Current scene container (" + currentSceneContainer.name + ") hoist height: " + hoistHeight);

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

            //Utils.DebugUtils.DebugLog("Scene hoist delta: " + hoistDelta + " between " + SceneGlobals.referringScene + " and " + SceneGlobals.upcomingScene);

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

    // used only the in editor to hoist all scene containers up
    // for follow-on operations like updating nav meshes or occlusion culling
    public static void HoistAllSceneContainersUp(List<GameObject> timePeriodSceneContainers)
    {
#if UNITY_EDITOR
        // for each of the time period scene containers, move them up at intervals
        // so that when we bake OC data, the different scenes are not on top of each other
        foreach (GameObject sceneContainer in timePeriodSceneContainers)
        {
            // the new height will be a multiple of the global height interval and i
            float addNewHeight = HoistSceneObjects.GetHoistHeightBySceneName(sceneContainer.name);

            Vector3 originalPosition = sceneContainer.transform.position;
            HoistSceneGlobals.originalTimePeriodSceneContainerPositions.Add(originalPosition);

            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y + addNewHeight, originalPosition.z);

            sceneContainer.transform.position = newPosition;
        }
#endif
    }

    // used only the in editor to move all scene containers back down
    // after operations like updating nav meshes or occlusion culling
    public static void HoistAllSceneContainersDown(List<GameObject> timePeriodSceneContainers)
    {
#if UNITY_EDITOR
        // return the scene containers to their original positions
        for (var i = 0; i < timePeriodSceneContainers.Count; i++)
        {
            // only bother moving the scene container if its position doesn't match the original position
            if (timePeriodSceneContainers[i].transform.position != HoistSceneGlobals.originalTimePeriodSceneContainerPositions[i])
            {
                timePeriodSceneContainers[i].transform.position = HoistSceneGlobals.originalTimePeriodSceneContainerPositions[i];
            }
        }
#endif
    }

}