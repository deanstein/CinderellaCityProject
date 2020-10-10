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
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetAllTimePeriodSceneContainers();

        HoistSceneObjects.HoistAllSceneContainersUp(timePeriodSceneContainers);
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist Open Scene Containers Down")]
    public static void HoistCurrentEditorSceneContainersDown()
    {
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetAllTimePeriodSceneContainers();

        HoistSceneObjects.HoistAllSceneContainersDown(timePeriodSceneContainers);
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

        HoistCurrentEditorSceneContainersUp();

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();

        HoistCurrentEditorSceneContainersDown();
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

    /* ---------- Build Prep ---------- */

    [MenuItem("Cinderella City Project/Build Prep/Run Pre-Build Steps")]
    public static void PreBuild()
    {
        // before building, need to iterate through scenes 
        // to find any requiring hoisting, then hoist and record original height to EditorPrefs
        // note that this operation will change the scene and save it

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

            float currentSceneContainerYPos = currentSceneContainer.transform.position.z;

            // get the Z-pos EditorPrefs key by name
            string editorPrefsYPosKey = ManageEditorPrefs.GetEditorPrefKeyBySceneName(SceneManager.GetActiveScene().name, ManageEditorPrefs.EditorPrefsGlobals.originalContainerYPosKeyID);

            // record the current scene container position to EditorPrefs for reference later
            ManageEditorPrefs.SetCurrentSceneContainerYPosEditorPref();

            // hoist the current scene container
            HoistSceneObjects.HoistAllSceneContainersUp(currentSceneContainerList);
        }
    }

    [MenuItem("Cinderella City Project/Build Prep/Run Post-Build Steps")]
    public static void PostBuild()
    {
        // after building, need to hoist scenes back down to their original height
        // using EditorPrefs to find and compare the original height to current height

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

            float currentSceneContainerYPos = currentSceneContainer.transform.position.y;

            // get the Z-pos EditorPrefs key by name
            string originalSceneContainerYPosKey = ManageEditorPrefs.GetEditorPrefKeyBySceneName(SceneManager.GetActiveScene().name, ManageEditorPrefs.EditorPrefsGlobals.originalContainerYPosKeyID);

            // if the original height is not set in preferences, don't do anything
            // and display a debug log message
            if (EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) == -1)
            {
                Utils.DebugUtils.DebugLog("Error: Cannot hoist scene container down, because there is no EditorPrefs record of an original YPosition for this scene container: " + currentSceneContainer.name);

                return;
            }
            // otherwise, there's a value stored, so check if the value
            // indicates the current scene has been hoisted up and needs to be hoisted down
            else if (currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) == HoistSceneGlobals.hoistInterval)
            {
                // hoist the current scene container
                HoistSceneObjects.HoistAllSceneContainersDown(currentSceneContainerList);

                Utils.DebugUtils.DebugLog("This scene container was moved back down to align with its recorded original YPosition: " + currentSceneContainer.name);
            }
            else if (currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1) == 0)
            {
                Utils.DebugUtils.DebugLog("Made no changes to this scene container because it is at its original height already: " + currentSceneContainer.name + " " + EditorPrefs.GetFloat(originalSceneContainerYPosKey, -1));
            }
            else
            {
                Utils.DebugUtils.DebugLog("Made no changes to this scene container because its original height and current height do not have a delta of the global hoist interval. Did something go wrong? " + currentSceneContainer.name + " Delta: " + (currentSceneContainerYPos - EditorPrefs.GetFloat(originalSceneContainerYPosKey)));
            }
        }
    }
}