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
        // load all scenes additively
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);

        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetAllTimePeriodSceneContainers();

        // record the original positions of the time period scene containers
        // so we can move the containers back to their original positions when done
        List<Vector3> originalTimePeriodSceneContainerPositions = new List<Vector3>();

        // for each of the time period scene containers, move them up at intervals
        // so that when we bake OC data, the different scenes are not on top of each other
        for (var i = 0; i < timePeriodSceneContainers.Count; i++)
        {
            // the new height will be a multiple of the global height interval and i
            float addNewHeight = i * EditorSceneGlobals.moveSceneContainerIntervalForOC;

            Vector3 originalPosition = timePeriodSceneContainers[i].transform.position;
            originalTimePeriodSceneContainerPositions.Add(originalPosition);

            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y + addNewHeight, originalPosition.z);

            // only bother moving the scene container if the new height is not 0
            if (addNewHeight != 0)
            {
                timePeriodSceneContainers[i].transform.position = newPosition;
            }
        }

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();

        // return the scene containers to their original positions
        for (var i = 0; i < timePeriodSceneContainers.Count; i++)
        {
            // only bother moving the scene container if its position doesn't match the original position
            if (timePeriodSceneContainers[i].transform.position != originalTimePeriodSceneContainerPositions[0])
            {
                timePeriodSceneContainers[i].transform.position = originalTimePeriodSceneContainerPositions[0];
            }
        }
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
        GameObject sceneContainer = ManageEditorScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

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
        GameObject sceneContainer = ManageEditorScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

        // get all the scene objects
        GameObject[] sceneObjects = AssetImportUpdate.GetAllTopLevelChildrenInObject(sceneContainer);

        // set the static flags for each scene object
        foreach (GameObject sceneObject in sceneObjects)
        {
            AssetImportUpdate.SetCustomLightmapSettingsByName(sceneObject);
        }
    }
}