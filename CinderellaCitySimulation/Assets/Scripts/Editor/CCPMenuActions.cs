using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CCPMenuActions : MonoBehaviour
{
    /* ---------- Play ---------- */

    [MenuItem("Cinderella City Project/Play Full Simulation in Editor", false, 0)]
    public static void PlayFullSimulationInEditor()
    {
        // first, hoist scenes up
        HoistSceneObjectsEditor.HoistAllRequiredSceneContainersUp();

        OpenLoadingScene();
        EditorApplication.EnterPlaymode();
    }

    /* ---------- Open Scene ---------- */ 

    [MenuItem("Cinderella City Project/Open Scene/Open All Scenes Additively")]
    public static void OpenAllScenesAdditively()
    {
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);
    }

    [MenuItem("Cinderella City Project/Open Scene/Loading")]
    public static void OpenLoadingScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.loadingSceneName);
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Main Menu")]
    public static void OpenMainMenuScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("MainMenu");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/60s70s")]
    public static void Open60s70sScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("60s70s");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/80s90s")]
    public static void Open90s90sScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("80s90s");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Experimental")]
    public static void OpenExperimentalScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("Experimental");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    /* ---------- Scene Hoisting ---------- */

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist Open Scene Containers Up")]
    public static void HoistCurrentEditorSceneContainersUp()
    {
        // of the open scene containers, get only the ones requiring hoisting
        List<GameObject> openScenContainersRequiringHoist = ManageEditorScenes.GetOpenTimePeriodSceneContainersRequiringHoist();

        HoistSceneObjectsEditor.HoistSceneContainersUp(openScenContainersRequiringHoist);
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist Open Scene Containers Down")]
    public static void HoistCurrentEditorSceneContainersDown()
    {
        // of the open scene containers, get only the ones requiring hoisting
        List<GameObject> openScenContainersRequiringHoist = ManageEditorScenes.GetOpenTimePeriodSceneContainersRequiringHoist();

        HoistSceneObjectsEditor.HoistSceneContainersDown(openScenContainersRequiringHoist);
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist All Required Scene Containers Up")]
    public static void HoistAllRequiredSceneContainersUp()
    {
        HoistSceneObjectsEditor.HoistAllRequiredSceneContainersUp();
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist All Required Scene Containers Down")]
    public static void HoistAllRequiredSceneContainersDown()
    {
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetOpenTimePeriodSceneContainers();

        HoistSceneObjectsEditor.HoistSceneContainersDown(timePeriodSceneContainers);
    }


    /* ---------- Update Data ---------- */

    [MenuItem("Cinderella City Project/Thumbnail Screenshots/Update for Current Scene")]
    public static void CaptureThisSceneThumbnailScreenshots()
    {
        // ensure the required directory is available
        if (!Directory.Exists(ManageCameraActions.CameraActionGlobals.inGameScreenshotsPath))
        {
            Directory.CreateDirectory(ManageCameraActions.CameraActionGlobals.inGameScreenshotsPath);
        }

        // set the camera actions script to know that we're in screenshot mode
        // this enables the script to start the process OnEnable()
        EditorPrefs.SetBool(ManageCameraActions.CameraActionGlobals.screenshotModeFlag, true);

        // in play mode, all screenshots are captured
        // and when the application stops, the screenshots are copied to the correct directory
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("Cinderella City Project/Occlusion Culling/Update for All Scenes")]
    public static void UpdateOcclusionCulling()
    {
        // load all scenes additively
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);

        HoistCurrentEditorSceneContainersUp();

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Update for Current Scene")]
    public static void RebuildCurrentSceneNavMesh()
    {
            // get this scene's container
            GameObject sceneContainer = ManageScenes.GetSceneContainerObject(EditorSceneManager.GetActiveScene());
            List<GameObject> sceneContainers = new List<GameObject>();
            sceneContainers.Add(sceneContainer);

            // first, move this scene container as appropriate
            HoistSceneObjectsEditor.HoistSceneContainersUp(sceneContainers);

            // build the nav mesh
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

            Utils.DebugUtils.DebugLog("Updated the nav mesh in scene: " + EditorSceneManager.GetActiveScene().name);

            // now move the scene container back down
            HoistSceneObjectsEditor.HoistSceneContainersDown(sceneContainers);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Update for All Scenes")]
    public static void RebuildAllNavMeshes()
    {
        foreach (string sceneName in SceneGlobals.availableTimePeriodSceneNames)
        {
            // open the scene if it's not open already
            if (sceneName != EditorSceneManager.GetActiveScene().name)
            {
                EditorSceneManager.OpenScene(SceneGlobals.GetScenePathByName(sceneName));

            }

            // otherwise, we're already in the requested scene, so build the nav mesh

            RebuildCurrentSceneNavMesh();
        }
    }

    [MenuItem("Cinderella City Project/Static Flags/Update for Current Scene")]
    public static void SetAllStaticFlagsInCurrentScene()
    {
        // get the current scene's container
        GameObject sceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        // set the static flags for each scene object
        foreach (GameObject sceneObject in sceneObjects)
        {
            AssetImportUpdate.SetStaticFlagsByName(sceneObject);
        }
    }

    [MenuItem("Cinderella City Project/Lightmap Resolutions/Update for Current Scene")]
    public static void SetAllLightmapResolutionsInCurrentScene()
    {
        // get the current scene's container
        GameObject sceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        // set the static flags for each scene object
        foreach (GameObject sceneObject in sceneObjects)
        {
            AssetImportUpdate.SetCustomLightmapSettingsByName(sceneObject);
        }
    }

    /* ---------- Batch Operations ---------- */

    [MenuItem("Cinderella City Project/Batch Operations/Post Process Scene Update")]
    public static void PostProcessSceneUpdate()
    {
        // first, ensure the current scene is hoisted as required so downstream operations work correctly
        HoistCurrentEditorSceneContainersUp();

        // update the static flags - this is required for the following operations
        SetAllStaticFlagsInCurrentScene();

        // update the nav meshes
        RebuildCurrentSceneNavMesh();

        // update the lightmap resolutions
        SetAllLightmapResolutionsInCurrentScene();

        // remove aany FBM folders that might be present
        AssetImportUpdate.DeleteAllFBMFolders();

        // save the scene
        // required to see some of the post-processing changes take effect in the editor
        EditorSceneManager.SaveOpenScenes();

        // occlusion culling requires opening all scenes additively, so it's saved for last
        UpdateOcclusionCulling();
    }

    /* --------- Editor Debug ---------- */


    [MenuItem("Cinderella City Project/Editor Debug/Log Current Scene Name")]
    public static void LogCurrentScene()
    {
        Utils.DebugUtils.DebugLog("Current Editor scene: " + SceneManager.GetActiveScene().name);
    }

    [MenuItem("Cinderella City Project/Editor Debug/Log Current Scene Object Count")]
    public static void LogCurrentSceneObjectCount()
    {
        // get the current scene's container
        GameObject sceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        Debug.Log("Scene Objects: " + sceneObjects.Length);
    }

    [MenuItem("Cinderella City Project/Editor Debug/Delete All FBM Folders")]
    public static void DeleteAllFBMFolders()
    {
        AssetImportUpdate.DeleteAllFBMFolders();
    }

}
