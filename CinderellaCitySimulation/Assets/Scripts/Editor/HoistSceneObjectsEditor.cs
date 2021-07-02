using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
///  "Hoists" scene containers objects up or down, so they aren't occupying the same space which could cause Occlusion Culling to fail
/// </summary>

public class HoistSceneObjectsEditor : MonoBehaviour
{
    // opens scenes as required to hoist their containers up 
    // used for operations like playing in editor and building the game
    public static void HoistAllRequiredSceneContainersUp()
    {
        // first, get any scenes that require hoisting
        List<string> timePeriodHoistSceneNames = HoistSceneObjects.GetScenesRequiringHoisting();

        // for each time period requiring hoisting, open the scene,
        // and update its position in EditorPrefs if necessary
        foreach (string timePeriodHoistSceneName in timePeriodHoistSceneNames)
        {
            // open the scene if it's not already open
            if (SceneManager.GetActiveScene().name != timePeriodHoistSceneName)
            {
                EditorSceneManager.OpenScene(SceneGlobals.GetScenePathByName(timePeriodHoistSceneName));
            }

            // get the current scene container
            GameObject currentSceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());
            // need to have this in a list format for another function
            List<GameObject> currentSceneContainerList = new List<GameObject>();
            currentSceneContainerList.Add(currentSceneContainer);

            // hoist the current scene container
            bool sceneModified = HoistSceneContainersUp(currentSceneContainerList);

            // if changes were made, mark the scene dirty and save it
            if (sceneModified)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
        }
    }

    // opens scenes as required to hoist their containers down 
    // used for cleaning up operations like playing in editor and building the game
    public static void HoistAllRequiredSceneContainersDown()
    {
        // first, get any scenes that require hoisting
        List<string> timePeriodHoistSceneNames = HoistSceneObjects.GetScenesRequiringHoisting();

        // for each time period requiring hoisting, open the scene,
        // and update its position in EditorPrefs if necessary
        foreach (string timePeriodHoistSceneName in timePeriodHoistSceneNames)
        {
            // open the scene if it's not already open
            if (SceneManager.GetActiveScene().name != timePeriodHoistSceneName)
            {
                EditorSceneManager.OpenScene(SceneGlobals.GetScenePathByName(timePeriodHoistSceneName));
            }

            // get the current scene container
            GameObject currentSceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());
            // need to have this in a list format for another function
            List<GameObject> currentSceneContainerList = new List<GameObject>();
            currentSceneContainerList.Add(currentSceneContainer);

            // move all the scene containers down
            bool sceneModified = HoistSceneContainersDown(currentSceneContainerList);

            // if changes were made, mark the scene dirty and save it
            if (sceneModified)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
        }
    }

    // hoists the given scene containers up for operations like building nav meshes
    // ***assumes the given scene containers belong to scenes that are open in the editor***
    // ***assumes the given scene containers have already been filtered and require hoisting***
    public static bool HoistSceneContainersUp(List<GameObject> timePeriodSceneContainers)
    {
        // record whether the scene was hoisted
        bool sceneModified = false;

        // for each of the given time period scene containers, move them up at intervals
        // so that when we bake OC data, the different scenes are not on top of each other
        foreach (GameObject sceneContainer in timePeriodSceneContainers)
        {
            Vector3 currentPosition = sceneContainer.transform.position;

            // get the Z-pos EditorPrefs key by name
            string originalSceneContainerYPosKey = ManageEditorPrefs.GetEditorPrefKeyBySceneName(sceneContainer.scene.name, ManageEditorPrefs.EditorPrefsGlobals.originalContainerYPosKeyID);

            // get the new height to add based on this scene container
            float addNewHeight = HoistSceneObjects.GetHoistHeightBySceneName(sceneContainer.name);
            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + addNewHeight, currentPosition.z);

            // if a position is already recorded, and it seems like this scene container has already been hoisted,
            // return without modifying the scene
            if (EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) != -1 && Mathf.Approximately(currentPosition.y, EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) + HoistSceneObjects.GetHoistHeightBySceneName(sceneContainer.scene.name)))
            {

                Utils.DebugUtils.DebugLog("This scene container appears to already have been hoisted, and won't be modified: " + sceneContainer.name);
                sceneModified = false;
            }
            else
            {
                // record the current scene container position to EditorPrefs for reference later
                ManageEditorPrefs.SetCurrentSceneContainerYPosEditorPref();

                sceneContainer.transform.position = newPosition;

                Utils.DebugUtils.DebugLog("This scene container was hoisted up: " + sceneContainer.name);
                sceneModified = true;
            }
        }
        return sceneModified;
    }

    // hoists the given scene containers up for operations like building nav meshes
    // ***assumes the given scene containers belong to scenes that are open in the editor***
    // ***assumes the given scene containers have already been filtered and require hoisting***
    public static bool HoistSceneContainersDown(List<GameObject> timePeriodSceneContainers)
    {
        // record whether the scene was hoisted
        bool sceneModified = false;

        // return the scene containers to their original positions
        foreach (GameObject timePeriodSceneContainer in timePeriodSceneContainers)
        {
            float currentSceneContainerYPos = timePeriodSceneContainer.transform.position.y;

            // get the Z-pos EditorPrefs key by name
            string originalSceneContainerYPosKey = ManageEditorPrefs.GetEditorPrefKeyBySceneName(timePeriodSceneContainer.scene.name, ManageEditorPrefs.EditorPrefsGlobals.originalContainerYPosKeyID);

            // if the original height is not set in preferences, don't do anything
            // and display a debug log message
            if (EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) == -1)
            {
                Utils.DebugUtils.DebugLog("ERROR: Cannot hoist scene container down, because there is no EditorPrefs record of an original YPosition for this scene container: " + timePeriodSceneContainer.name);

                sceneModified = false;
            }
            // otherwise, there's a value stored, so check if the value
            // indicates the current scene has been hoisted up and needs to be hoisted back down
            else if (Mathf.Approximately(currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1), HoistSceneObjects.GetHoistHeightBySceneName(timePeriodSceneContainer.scene.name)))
            {
                // get the new height to remove based on this scene container
                float removeHeight = HoistSceneObjects.GetHoistHeightBySceneName(timePeriodSceneContainer.name);
                Vector3 newPosition = new Vector3(timePeriodSceneContainer.transform.position.x, timePeriodSceneContainer.transform.position.y - removeHeight, timePeriodSceneContainer.transform.position.z);

                timePeriodSceneContainer.transform.position = newPosition;

                Utils.DebugUtils.DebugLog("This scene container was moved back down to align with its recorded original YPos: " + timePeriodSceneContainer.name);

                sceneModified = true;
            }
            else if (Mathf.Approximately(currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1), 0))
            {
                Utils.DebugUtils.DebugLog("Made no changes to this scene container because it is at its original YPos already: " + timePeriodSceneContainer.name + " " + EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1));
                sceneModified = false;
            }
            else
            {
                Utils.DebugUtils.DebugLog("Made no changes to this scene container because its original YPos and current YPos do not have a delta of the global hoist interval. Did something go wrong? " + timePeriodSceneContainer.name + " Delta: " + (currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey)));
                sceneModified = false;
            }
        }
        return sceneModified;
    }

    // hoist an individual object, not a scene container
    public static void HoistObjectUp(GameObject objectToHoist)
    {
        Vector3 currentPosition = objectToHoist.transform.position;

        // get the new height to add based on this scene container
        float addNewHeight = HoistSceneObjects.GetHoistHeightBySceneName(objectToHoist.scene.name);
        Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + addNewHeight, currentPosition.z);

        objectToHoist.transform.position = newPosition;
    }
}