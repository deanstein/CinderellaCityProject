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

    [MenuItem("Cinderella City Project/Restart Unity")]
    public static void ReopenProject()
    {
        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
    }

    /* ---------- Mark Scene Dirty ---------- */

    [MenuItem("Cinderella City Project/Mark Current Scene Dirty")]
    public static void MarkCurrentSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
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
        string mainMenuScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.mainMenuSceneName);
        EditorSceneManager.OpenScene(mainMenuScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Visibility Menu")]
    public static void OpenVisibilityMenuScene()
    {
        string visibilityMenuScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.visibilityMenuSceneName);
        EditorSceneManager.OpenScene(visibilityMenuScenePath);
    }


    [MenuItem("Cinderella City Project/Open Scene/How to Play")]
    public static void OpenHowToPlayScene()
    {
        string howToPlayScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.howToPlaySceneName);
        EditorSceneManager.OpenScene(howToPlayScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Credits")]
    public static void OpenCreditsScene()
    {
        string creditsScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.creditsSceneName);
        EditorSceneManager.OpenScene(creditsScenePath);
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

    /* ---------- Object Visibility Menu ---------- */

    [MenuItem("Cinderella City Project/Object State/Toggle People Replacements ON")]
    public static void TogglePeopleReplacementsOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.peopleObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle People Replacements OFF")]
    public static void TogglePeopleReplacementsOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.peopleObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Proxy Blocker NPC Meshes ON")]
    public static void ToggleProxyBlockerNPCMeshesOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.blockerObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Proxy Blocker NPC Meshes OFF")]
    public static void ToggleProxyBlockerNPCMeshesOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.blockerObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Vegetation Replacements ON")]
    public static void ToggleVegetationReplacementsOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.vegetationObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Vegetation Replacements OFF")]
    public static void ToggleVegetationReplacementsOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.vegetationObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Thumbnail Camera Meshes ON")]
    public static void ToggleThumbnailCameraMeshesOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.thumbnailCameraObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Thumbnail Camera Meshes OFF")]
    public static void ToggleThumbnailCameraMeshesOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.thumbnailCameraObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Meshes ON")]
    public static void ToggleHistoricPhotoMeshesOn()
    {
        GameObject historicPhotoParentObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicPhotoParentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Meshes OFF")]
    public static void ToggleHistoricPhotoMeshesOff()
    {
        GameObject historicPhotoParentObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicPhotoParentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Cameras ON")]
    public static void ToggleHistoricPhotoCamerasOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.historicPhotographObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Cameras OFF")]
    public static void ToggleHistoricPhotoCamerasOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.historicPhotographObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, false);
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
            GameObject[] sceneContainerObjects = ManageSceneObjects.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene());
            /// ... and as a list
            List<GameObject> sceneObjectsList = new List<GameObject>(sceneContainerObjects);

            // if update all is checked, return all objects from the scene like normal
            if (isUpdateModeAll)
            {
                DebugUtils.DebugLog("All objects were selected for update.");

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

                    DebugUtils.DebugLog("Found " + sceneObjectsList.Count + " scene objects to update.");
                    return sceneObjectsList;
                }
                else
                {
                    DebugUtils.DebugLog("No models selected for post-update processing.");
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

    /* ---------- Guided Tour ---------- */

    // prints the names and indices of all historic cameras in the scene
    [MenuItem("Cinderella City Project/Guided Tour/Log Guided Tour Camera Data")]
    public static void PrintHistoricPhotoData()
    {
        GameObject[] historicPhotoObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(SceneManager.GetActiveScene().name);

        // log the original indices - this is helpful for next runs
        for (int i = 0; i < historicPhotoObjects.Length; i++)
        {
            Debug.Log(historicPhotoObjects[i].name + " is at original index " + i);
        }
    }

    // attempts to find all indices of the "curated" guided tour
    // check the console for any issues
    [MenuItem("Cinderella City Project/Guided Tour/Find All Curated Tour Objects")]
    public static void FindCuratedTourIndices()
    {
        GameObject[] historicPhotoObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(SceneManager.GetActiveScene().name);

        ManageSceneObjects.ProxyObjects.FindAllCuratedGuidedTourObjects(historicPhotoObjects);
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Show Guided Tour Camera Position Debug Lines")]
    public static void ShowGuidedTourCameraPosDebugLines()
    {
        // move back from camera this distance 
        float distanceFromCamera = 1.15f;

        // get all guided tour camera objects
        GameObject[] guidedTourCameraObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(SceneManager.GetActiveScene().name);

        foreach (GameObject guidedTourCameraObject in guidedTourCameraObjects)
        {
            Camera objectCamera = guidedTourCameraObject.GetComponent<Camera>();
            // record the positions of the cameras
            Vector3 objectCameraPos = objectCamera.transform.position;
            // also record the adjusted positions
            Vector3 objectCameraPosAdjusted = Utils.GeometryUtils.AdjustPositionAwayFromCamera(objectCamera.transform.position, objectCamera, distanceFromCamera);
            // project the adjusted positions down onto the NavMesh
            Vector3 objectCameraPosAdjustedOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(objectCameraPosAdjusted, 2);

            // draw debug lines
            Debug.DrawLine(objectCameraPos, objectCameraPosAdjusted, Color.red, 10);
            Debug.DrawLine(objectCameraPosAdjusted, objectCameraPosAdjustedOnNavMesh, Color.blue, 10);
        }
    }

    /* ---------- Update Data ---------- */

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
                AssetImportUpdate.SetAllDependentMaterialsSpecularByName(sceneObject);
            }
        }
    }

    [MenuItem("Cinderella City Project/Material Adjustments/Smoothness and Metallic/Update for Current Scene")]
    public static void SetAllMaterialSmoothnessMetallicInCurrentScene()
    {
        // get all the scene objects
        GameObject[] sceneObjects = UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode().ToArray();

        if (sceneObjects.Length > 0)
        {
            // set the static flags for each scene object
            foreach (GameObject sceneObject in sceneObjects)
            {
                AssetImportUpdate.SetAllDependentMaterialsSmoothnessMetallicByName(sceneObject);
            }
        }
    }


    [MenuItem("Cinderella City Project/Material Adjustments/Emission/Update for Current Scene")]
    public static void SetAllMaterialEmissionInCurrentScene()
    {
        // get all the scene objects
        GameObject[] sceneObjects = UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode().ToArray();

        if (sceneObjects.Length > 0)
        {
            // set the static flags for each scene object
            foreach (GameObject sceneObject in sceneObjects)
            {
                AssetImportUpdate.SetAllDependentMaterialsEmissionByName(sceneObject);
            }
        }
    }

    /* ---------- Nav Meshes ---------- */

    // add all non-walkable mesh renderers to the selection
    // editor operator must manually set these to non-walkable in the Navigation tab
    [MenuItem("Cinderella City Project/Nav Meshes/Select All Non-Walkable Meshes")]
    public static void SelectAllNonWalkableMeshRenderers()
    {
        // make sure proxy blockers are visible
        ToggleProxyBlockerNPCMeshesOn();

        // get all top-level non-walkable objects
        GameObject[] topLevelNonWalkableObjects = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.floorNonWalkableKeywords);

        // prepare a list for all the found objects
        List<GameObject> meshRendererGameObjectList = new List<GameObject>();
        
        // for each top-level object, get all mesh renderers
        foreach (GameObject nonWalkableGameObject in topLevelNonWalkableObjects)
        {
            MeshRenderer[] meshRenderers = nonWalkableGameObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRendererGameObjectList.Add(meshRenderer.gameObject);
            }
        }

        // convert the list to an array
        GameObject[] finalSelection = meshRendererGameObjectList.ToArray();
        // set the selection to the array
        Selection.objects = finalSelection;
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Select Meshes in Selection")]
    public static void SelectMeshesInSelection()
    {
        // prepare a list for all the found objects
        List<GameObject> meshRendererGameObjectList = new List<GameObject>();

        // for each selected object, get the mesh renderers
        foreach (GameObject selectedGameObjects in Selection.objects)
        {
            MeshRenderer[] meshRenderers = selectedGameObjects.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRendererGameObjectList.Add(meshRenderer.gameObject);
            }
        }

        // convert the list to an array
        GameObject[] finalSelection = meshRendererGameObjectList.ToArray();
        // set the selection to the array
        Selection.objects = finalSelection;
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Update for Current Scene")]
    public static void RebuildCurrentSceneNavMesh()
    {
        // get this scene's container
        GameObject sceneContainer = ManageSceneObjects.GetSceneContainerObject(EditorSceneManager.GetActiveScene());
        List<GameObject> sceneContainers = new List<GameObject>();
        sceneContainers.Add(sceneContainer);

        // get the blocker object proxy host
        GameObject proxyHost = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.blockerObjectKeywords[0], true)[0];

        // first, move this scene container as appropriate
        HoistSceneObjectsEditor.HoistSceneContainersUp(sceneContainers);

        // ensure that "blocker" objects are enabled before building the nav mesh
        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(proxyHost, true, false);

        // build the nav mesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        DebugUtils.DebugLog("Updated the nav mesh in scene: " + EditorSceneManager.GetActiveScene().name);

        // re-hide the proxy blocker
        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(proxyHost, false, false);

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

    [MenuItem("Cinderella City Project/Occlusion Culling/Update for All Scenes")]
    public static void UpdateOcclusionCulling()
    {
        // load all scenes additively
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);

        HoistCurrentEditorSceneContainersUp();

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();
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
        AssetImportUpdate.DeleteAllLegacyTextureFolders();

        // save the scene
        // required to see some of the post-processing changes take effect in the editor
        EditorSceneManager.SaveOpenScenes();

        // occlusion culling requires opening all scenes additively, so it's saved for last
        UpdateOcclusionCulling();
    }

    // deletes folders that may linger from older versions
    // for example, the /Textures folder which used to replace the asset-name.fbm folder
    [MenuItem("Cinderella City Project/Batch Operations/Delete Legacy Folders")]
    public static void DeleteLegacyFolders()
    {
        AssetImportUpdate.DeleteAllLegacyTextureFolders();
    }

    /* --------- Editor Debug ---------- */
#if false

    [MenuItem("Cinderella City Project/CCP Debug/Get Bounding Box for Selected")]
    public static void TestMeshBounds()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        DebugUtils.DebugLog("Max bounding box dimension " + Utils.GeometryUtils.GetMaxGOBoundingBoxDimension(selectedObjects[0]) + " for " + selectedObjects[0]);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Hoist Object Up")]
    public static void HoistObjectUp()
    {
        HoistSceneObjectsEditor.HoistObjectUp(GameObject.Find("proxy-people"));
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Current Scene Name")]
    public static void LogCurrentScene()
    {
        DebugUtils.DebugLog("Current Editor scene: " + SceneManager.GetActiveScene().name);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Current Scene Object Count")]
    public static void LogCurrentSceneObjectCount()
    {
        int sceneObjectCount = ManageSceneObjects.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene()).Length;

        Debug.Log("Scene Objects: " + sceneObjectCount);
    }

    [MenuItem("Cinderella City Project/CCP Debug/Log Scene Object Count by Update Mode")]
    public static void GetAllSceneObjectsByModeTest()
    {
        UpdateModeSelectorMenu.GetSceneObjectsByUpdateMode();
    }

    [MenuItem("Cinderella City Project/CCP Debug/Get All Thumbnails to Delete")]
    public static void GetAllThumbnailsToDeleteTest()
    {
        string[] extensionsToDelete = { "png", "meta" };
        string[] filesToDelete = FileDirUtils.GetAllFilesInDirOfTypes(UIGlobals.projectUIPath + UIGlobals.mainMenuThumbnailsSubdir, extensionsToDelete);
        foreach (string fileToDelete in filesToDelete)
        {
            DebugUtils.DebugLog(fileToDelete);
        }
    }
#endif
}
