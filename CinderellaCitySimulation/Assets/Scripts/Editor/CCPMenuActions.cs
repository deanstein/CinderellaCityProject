using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
[InitializeOnLoad]

/// <summary>
/// Creates a Cinderella City Project menu in the Unity menu bar
/// Offers a series of one-click actions that automate several tedious workflows, like updating Nav Meshes and Occlusion Culling for all Scenes.
/// </summary>

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
        string mainMenuScenePath = SceneGlobals.GetScenePathByName("MainMenu");
        EditorSceneManager.OpenScene(mainMenuScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Visibility Menu")]
    public static void OpenVisibilityMenuScene()
    {
        string visibilityMenuScenePath = SceneGlobals.GetScenePathByName("VisibilityMenu");
        EditorSceneManager.OpenScene(visibilityMenuScenePath);
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

    /* ---------- Update Mode Menu ---------- */

    public static class UpdateModeSelectorMenu
    {
        static List<string> GetUpdateModeSearchKeysByUIState()
        {
            List<string> updateModeSearchKeys = new List<string>();

            if (isUpdateModeAnchors)
            {
                string keyWord = "anchor-";
                updateModeSearchKeys.Add(keyWord);
            }

            if (isUpdateModeMall)
            {
                string keyWord1 = "mall-";
                updateModeSearchKeys.Add(keyWord1);

                string keyWord2 = "store-";
                updateModeSearchKeys.Add(keyWord2);

                string keyWord3 = "blocker-";
                updateModeSearchKeys.Add(keyWord3);

                string keyWord4 = "proxy-";
                updateModeSearchKeys.Add(keyWord4);
            }

            if (isUpdateModeSite)
            {
                string keyWord = "site-";
                updateModeSearchKeys.Add(keyWord);
            }

            return updateModeSearchKeys;
        }

        // get all the scene objects requiring update, based on the CCP menu's update mode choices
        public static List<GameObject> GetSceneObjectsByUpdateMode()
        {
            // get all the scene objects
            GameObject[] sceneContainerObjects = ManageScenes.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene());
            /// ... and as a list
            List<GameObject> sceneObjectsList = new List<GameObject>(sceneContainerObjects);

            // if update all is checked, return all objects from the scene like normal
            if (isUpdateModeAll)
            {
                Utils.DebugUtils.DebugLog("All objects were selected for update.");

                return sceneObjectsList;
            }

            // otherwise, accumulate the selected objects
            else
            {
                // get the string search keys for the selected update modes
                List<string> searchKeys = GetUpdateModeSearchKeysByUIState();

                // reset to an empty list to store each pass of searches
                sceneObjectsList = new List<GameObject>();

                if (searchKeys.Count > 0)
                {
                    foreach (string searchKey in searchKeys)
                    {
                        foreach (GameObject sceneContainerObject in sceneContainerObjects)
                        {
                            if (sceneContainerObject.name.Contains(searchKey))
                            {
                                sceneObjectsList.Add(sceneContainerObject);
                            }
                        }
                    }

                    Utils.DebugUtils.DebugLog("Found " + sceneObjectsList.Count + " scene objects to update.");
                    return sceneObjectsList;
                }
                else
                {
                    Utils.DebugUtils.DebugLog("No models selected for post-update processing.");
                    return null;
                }
            }
        }

        // initialize the update mode flags from editor prefs
        static UpdateModeSelectorMenu()
        {
            isUpdateModeAll = EditorPrefs.GetBool(isUpdateModeAllKey, true);
            isUpdateModeAnchors = EditorPrefs.GetBool(isUpdateModeAnchorsKey, true);
            isUpdateModeMall = EditorPrefs.GetBool(isUpdateModeMallKey, true);
            isUpdateModeSite = EditorPrefs.GetBool(isUpdateModeSiteKey, true);
        }

        /// create a checkbox menu item for each update mode

        private const string isUpdateModeAllMenuItem = "Cinderella City Project/Set Update Modes/All Models";
        private const string isUpdateModeAllKey = "CCP.IsUpdateModeAll";
        public static bool isUpdateModeAll;

        [MenuItem(isUpdateModeAllMenuItem)]
        private static void SetIsUpdateModeAll()
        {            isUpdateModeMall = EditorPrefs.GetBool(isUpdateModeMallKey, true);
            isUpdateModeAll = !isUpdateModeAll;
            EditorPrefs.SetBool(isUpdateModeAllKey, isUpdateModeAll);
        }
        [MenuItem(isUpdateModeAllMenuItem, true)]
        private static bool SetIsUpdateModeAllValidate()
        {
            Menu.SetChecked(isUpdateModeAllMenuItem, isUpdateModeAll);
            return true;
        }


        private const string isUpdateModeAnchorsMenuItem = "Cinderella City Project/Set Update Modes/Anchors";
        private const string isUpdateModeAnchorsKey = "CCP.IsUpdateModeAnchor";
        public static bool isUpdateModeAnchors;

        [MenuItem(isUpdateModeAnchorsMenuItem)]
        private static void SetIsUpdateModeAnchor()
        {
            isUpdateModeAnchors = !isUpdateModeAnchors;
            EditorPrefs.SetBool(isUpdateModeAnchorsKey, isUpdateModeAnchors);
        }
        [MenuItem(isUpdateModeAnchorsMenuItem, true)]
        private static bool SetIsUpdateModeAnchorsValidate()
        {
            Menu.SetChecked(isUpdateModeAnchorsMenuItem, isUpdateModeAnchors);
            return true;
        }


        private const string isUpdateModeMallMenuItem = "Cinderella City Project/Set Update Modes/Mall + Stores";
        private const string isUpdateModeMallKey = "CCP.IsUpdateModeMall";
        public static bool isUpdateModeMall;

        [MenuItem(isUpdateModeMallMenuItem)]
        private static void SetIsUpdateModeMall()
        {
            isUpdateModeMall = !isUpdateModeMall;
            EditorPrefs.SetBool(isUpdateModeMallKey, isUpdateModeMall);
        }
        [MenuItem(isUpdateModeMallMenuItem, true)]
        private static bool SetIsUpdateModeMallValidate()
        {
            Menu.SetChecked(isUpdateModeMallMenuItem, isUpdateModeMall);
            return true;
        }


        private const string isUpdateModeSiteMenuItem = "Cinderella City Project/Set Update Modes/Site";
        private const string isUpdateModeSiteKey = "CCP.IsUpdateModeSite";
        public static bool isUpdateModeSite;

        [MenuItem(isUpdateModeSiteMenuItem)]
        private static void SetIsUpdateModeSite()
        {
            isUpdateModeSite = !isUpdateModeSite;
            EditorPrefs.SetBool(isUpdateModeSiteKey, isUpdateModeSite);
        }
        [MenuItem(isUpdateModeSiteMenuItem, true)]
        private static bool SetIsUpdateModeSiteValidate()
        {
            Menu.SetChecked(isUpdateModeSiteMenuItem, isUpdateModeSite);
            return true;
        }
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

        // ensure that "blocker" objects are enabled before building the nav mesh
        AssetImportUpdate.UnhideProxyObjects("proxy-blocker-npc");

        // build the nav mesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        Utils.DebugUtils.DebugLog("Updated the nav mesh in scene: " + EditorSceneManager.GetActiveScene().name);

        // re-hide the proxy blocker
        AssetImportUpdate.HideProxyObjects("proxy-blocker-npc");

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
        // get all the scene objects
        GameObject[] sceneObjects = UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode().ToArray();

        if (sceneObjects.Length > 0)
        {
            // set the static flags for each scene object
            foreach (GameObject sceneObject in sceneObjects)
            {
                AssetImportUpdate.SetStaticFlagsByName(sceneObject);
            }
        }
    }

    [MenuItem("Cinderella City Project/Lightmap Resolutions/Update for Current Scene")]
    public static void SetAllLightmapResolutionsInCurrentScene()
    {
        // get all the scene objects
        GameObject[] sceneObjects = UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode().ToArray();

        if (sceneObjects.Length > 0)
        {
            // set the static flags for each scene object
            foreach (GameObject sceneObject in sceneObjects)
            {
                AssetImportUpdate.SetCustomLightmapSettingsByName(sceneObject);
            }
        }
    }

    [MenuItem("Cinderella City Project/Material Adjustments/Specular/Update for Current Scene")]
    public static void SetAllMaterialSpecularInCurrentScene()
    {
        // get all the scene objects
        GameObject[] sceneObjects = UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode().ToArray();

        if (sceneObjects.Length > 0)
        {
            // set the static flags for each scene object
            foreach (GameObject sceneObject in sceneObjects)
            {
                AssetImportUpdate.SetMaterialSpecularByName(sceneObject);
            }
        }
    }

    /* ---------- Batch Operations ---------- */

    [MenuItem("Cinderella City Project/Batch Operations/Delete All FBM Folders")]
    public static void DeleteAllFBMFoldersTest()
    {
        AssetImportUpdate.DeleteAllFBMFolders();
    }

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
#if false

    [MenuItem("Cinderella City Project/CCP Debug/Get Bounding Box for Selected")]
    public static void TestMeshBounds()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        Utils.DebugUtils.DebugLog("Max bounding box dimension " + Utils.GeometryUtils.GetMaxGOBoundingBoxDimension(selectedObjects[0]) + " for " + selectedObjects[0]);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Hoist Object Up")]
    public static void HoistObjectUp()
    {
        HoistSceneObjectsEditor.HoistObjectUp(GameObject.Find("proxy-people"));
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Current Scene Name")]
    public static void LogCurrentScene()
    {
        Utils.DebugUtils.DebugLog("Current Editor scene: " + SceneManager.GetActiveScene().name);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Current Scene Object Count")]
    public static void LogCurrentSceneObjectCount()
    {
        int sceneObjectCount = ManageScenes.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene()).Length;

        Debug.Log("Scene Objects: " + sceneObjectCount);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Scene Object Count by Update Mode")]
    public static void GetAllSceneObjectsByModeTest()
    {
        UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode();
    }
#endif
}
