using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using System.IO;

[ExecuteInEditMode]
public class CCPMenuActions : MonoBehaviour
{
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

    [MenuItem("Cinderella City Project/Update This Scene Thumbnail Screenshots")]
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

    [MenuItem("Cinderella City Project/Update All Scenes Occlusion Culling")]
    public static void UpdateOcclusionCulling()
    {
        PlayFullSimulationInEditor();
        StaticOcclusionCulling.Compute();
        // TODO: return to the loading screen once occlusion culling is computed
    }

    [MenuItem("Cinderella City Project/Update All Scenes Nav Meshes")]
    public static void RebuildAllNavMeshes()
    {
        foreach (string sceneName in SceneGlobals.availableTimePeriodSceneNames)
        {
            // open the scene if it's not open already
            if (sceneName != EditorSceneManager.GetActiveScene().name)
            {
                EditorSceneManager.OpenScene(SceneGlobals.GetScenePathByName(sceneName));

                // build the nav mesh and save the scene
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

                Utils.DebugUtils.DebugLog("Updated the nav mesh in scene: " + sceneName);
            }
            // otherwise, we're already in the request scene, so build the nav mesh
            else
            {
                // build the nav mesh and save the scene
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }
        // TODO: return to the loading screen once all nav meshes are updated
    }

    [MenuItem("Cinderella City Project/Update This Scene Static Flags")]
    public static void SetAllStaticFlagsInCurrentScene()
    {
        // get the current scene's container
        GameObject sceneContainer = AssetImportUpdate.GetCurrentSceneContainer();

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        // set the static flags for each scene object
        foreach (GameObject sceneObject in sceneObjects)
        {
            AssetImportUpdate.SetStaticFlagsByName(sceneObject);
        }
    }

    [MenuItem("Cinderella City Project/Update This Scene Lightmap Resolutions")]
    public static void SetAllLightmapResolutionsInCurrentScene()
    {
        // get the current scene's container
        GameObject sceneContainer = AssetImportUpdate.GetCurrentSceneContainer();

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        // set the static flags for each scene object
        foreach (GameObject sceneObject in sceneObjects)
        {
            AssetImportUpdate.SetCustomLightmapSettingsByName(sceneObject);
        }
    }
}