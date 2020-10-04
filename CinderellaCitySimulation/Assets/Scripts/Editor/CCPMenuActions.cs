using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CCPMenuActions : MonoBehaviour
{
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

    [MenuItem("Cinderella City Project/Play Full Simulation in Editor")]
    public static void PlayFullSimulationInEditor()
    {
        OpenLoadingScene();
        EditorApplication.EnterPlaymode();
    }

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

    [MenuItem("Cinderella City Project/Occlusion Culling/Move Containers Up")]
    public static void MoveContainersUp()
    {
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetAllTimePeriodSceneContainers();

        HoistSceneObjects.HoistAllSceneContainersUp(timePeriodSceneContainers);
    }

    [MenuItem("Cinderella City Project/Occlusion Culling/Move Containers Down")]
    public static void MoveContainersDown()
    {
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetAllTimePeriodSceneContainers();

        HoistSceneObjects.HoistAllSceneContainersDown(timePeriodSceneContainers);
    }

    [MenuItem("Cinderella City Project/Occlusion Culling/Update for Current Scene")]
    public static void BakeOCData()
    {
        StaticOcclusionCulling.Compute();
    }

    [MenuItem("Cinderella City Project/Occlusion Culling/Update for All Scenes")]
    public static void UpdateOcclusionCulling()
    {
        // load all scenes additively
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);

        MoveContainersUp();

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();

        MoveContainersDown();
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

            // get this scene's container
            GameObject sceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());
            List<GameObject> sceneContainers = new List<GameObject>();
            sceneContainers.Add(sceneContainer);

            // first, move this scene container as appropriate
            HoistSceneObjects.HoistAllSceneContainersUp(sceneContainers);
            Debug.Log("Hoisting nav mesh for scene: " + sceneContainer.name);

            // build the nav mesh
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

            Utils.DebugUtils.DebugLog("Updated the nav mesh in scene: " + sceneName);

            // now move the scene container back down
            HoistSceneObjects.HoistAllSceneContainersDown(sceneContainers);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        // TODO: return to the loading screen once all nav meshes are updated
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
}