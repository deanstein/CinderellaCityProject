using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
[InitializeOnLoad]

/// <summary>
/// Creates a Cinderella City Project menu in the Unity menu bar
/// Offers a series of one-click actions that automate several tedious workflows, like updating Nav Meshes and Occlusion Culling for all Scenes.
/// </summary>

public class CCPMenuActions : MonoBehaviour
{
    /* ---------- PLAY SECTION ---------- */

    ///// PLAY SIM /////
    [MenuItem("Cinderella City Project/Play Full Simulation in Editor", false, 0)]
    public static void PlayFullSimulationInEditor()
    {
        // first, hoist scenes up
        HoistSceneObjectsEditor.HoistAllRequiredSceneContainersUp();

        OpenLoadingScene();
        EditorApplication.EnterPlaymode();
    }

    /* ---------- SCENE SECTION ---------- */

    ///// OPEN ALL SCENES /////
    [MenuItem("Cinderella City Project/Open Scene/Open All Scenes Additively", false, 100)]
    public static void OpenAllScenesAdditively()
    {
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);
    }

    ///// OPEN SCENE /////
    [MenuItem("Cinderella City Project/Open Scene/Loading", false, 101)]
    public static void OpenLoadingScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.loadingSceneName);
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Main Menu", false, 102)]
    public static void OpenMainMenuScene()
    {
        string mainMenuScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.mainMenuSceneName);
        EditorSceneManager.OpenScene(mainMenuScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Visibility Menu", false, 103)]
    public static void OpenVisibilityMenuScene()
    {
        string visibilityMenuScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.visibilityMenuSceneName);
        EditorSceneManager.OpenScene(visibilityMenuScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/How to Play", false, 104)]
    public static void OpenHowToPlayScene()
    {
        string howToPlayScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.howToPlaySceneName);
        EditorSceneManager.OpenScene(howToPlayScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Credits", false, 105)]
    public static void OpenCreditsScene()
    {
        string creditsScenePath = SceneGlobals.GetScenePathByName(SceneGlobals.creditsSceneName);
        EditorSceneManager.OpenScene(creditsScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/60s70s", false, 106)]
    public static void Open60s70sScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("60s70s");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/80s90s", false, 107)]
    public static void Open90s90sScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("80s90s");
        EditorSceneManager.OpenScene(loadingScenePath);
    }

    [MenuItem("Cinderella City Project/Open Scene/Experimental", false, 108)]
    public static void OpenExperimentalScene()
    {
        string loadingScenePath = SceneGlobals.GetScenePathByName("Experimental");
        EditorSceneManager.OpenScene(loadingScenePath);
    }


    ///// SCENE HOISTING /////
    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist Open Scene Containers Up", false, 110)]
    public static void HoistCurrentEditorSceneContainersUp()
    {
        // of the open scene containers, get only the ones requiring hoisting
        List<GameObject> openScenContainersRequiringHoist = ManageEditorScenes.GetOpenTimePeriodSceneContainersRequiringHoist();

        HoistSceneObjectsEditor.HoistSceneContainersUp(openScenContainersRequiringHoist);
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist Open Scene Containers Down", false, 111)]
    public static void HoistCurrentEditorSceneContainersDown()
    {
        // of the open scene containers, get only the ones requiring hoisting
        List<GameObject> openScenContainersRequiringHoist = ManageEditorScenes.GetOpenTimePeriodSceneContainersRequiringHoist();

        HoistSceneObjectsEditor.HoistSceneContainersDown(openScenContainersRequiringHoist);
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist All Required Scene Containers Up", false, 112)]
    public static void HoistAllRequiredSceneContainersUp()
    {
        HoistSceneObjectsEditor.HoistAllRequiredSceneContainersUp();
    }

    [MenuItem("Cinderella City Project/Hoist Scenes/Hoist All Required Scene Containers Down", false, 113)]
    public static void HoistAllRequiredSceneContainersDown()
    {
        // of the open scenes, get the time period scene containers
        List<GameObject> timePeriodSceneContainers = ManageEditorScenes.GetOpenTimePeriodSceneContainers();

        HoistSceneObjectsEditor.HoistSceneContainersDown(timePeriodSceneContainers);
    }

    ///// MARK SCENE DIRTY /////
    [MenuItem("Cinderella City Project/Mark Current Scene Dirty", false, 120)]
    public static void MarkCurrentSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    /* ---------- OBJECT SECTION ---------- */

    ///// OBJECT VISIBILITY /////
    [MenuItem("Cinderella City Project/Object State/Toggle People Replacements ON", false, 200)]
    public static void TogglePeopleReplacementsOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.peopleObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle People Replacements OFF", false, 201)]
    public static void TogglePeopleReplacementsOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.peopleObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Proxy Blocker NPC Meshes ON", false, 202)]
    public static void ToggleProxyBlockerNPCMeshesOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.blockerObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Proxy Blocker NPC Meshes OFF", false, 203)]
    public static void ToggleProxyBlockerNPCMeshesOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.blockerObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Vegetation Replacements ON", false, 204)]
    public static void ToggleVegetationReplacementsOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.vegetationObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Vegetation Replacements OFF", false, 205)]
    public static void ToggleVegetationReplacementsOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.vegetationObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, true);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Thumbnail Camera Meshes ON", false, 206)]
    public static void ToggleThumbnailCameraMeshesOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.thumbnailCameraObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Thumbnail Camera Meshes OFF", false, 207)]
    public static void ToggleThumbnailCameraMeshesOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.thumbnailCameraObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(parentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Meshes ON", false, 208)]
    public static void ToggleHistoricPhotoMeshesOn()
    {
        GameObject historicPhotoParentObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicPhotoParentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Meshes OFF", false, 209)]
    public static void ToggleHistoricPhotoMeshesOff()
    {
        GameObject historicPhotoParentObject = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicPhotoParentObject, false, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Cameras ON", false, 210)]
    public static void ToggleHistoricPhotoCamerasOn()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.historicPhotographObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, true, false);
    }

    [MenuItem("Cinderella City Project/Object State/Toggle Historic Photo Cameras OFF", false, 211)]
    public static void ToggleHistoricPhotoCamerasOff()
    {
        GameObject parentObject = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.historicPhotographObjectKeywords[0], true)[0];

        ManageSceneObjects.ProxyObjects.ToggleProxyHostReplacementsToState(parentObject, false, false);
    }

    ///// NPCs /////
    // logs the number of NPCs in the scene
    [MenuItem("Cinderella City Project/NPCs/Log Number of NPCs", false, 202)]
    public static void LogNumNPCs()
    {
        // get all NPC objects
        GameObject[] NPCObjects = ManageSceneObjects.GetAllTopLevelChildrenInObject(ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName("proxy-people")[0]);
        Debug.Log("Number of NPCs: " + NPCObjects.Length);
    }

    // sets the Traverse Off Mesh Link option to TRUE
    [MenuItem("Cinderella City Project/NPCs/Set NPCs Traverse Off Mesh Link TRUE", false, 203)]
    public static void SetNPCsToTraverseOffMeshLink()
    {
        // note if we made any changes
        bool isSceneModified = false;
        // get all NPC objects
        GameObject[] NPCObjects = ManageSceneObjects.GetAllTopLevelChildrenInObject(ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName("proxy-people")[0]);

        // for each, enable traversing off mesh links
        // but only if there's an agent (skip the ones standing)
        foreach (GameObject NPCObject in NPCObjects)
        {
            NavMeshAgent agent = NPCObject.GetComponentInChildren<NavMeshAgent>();
            if (agent)
            {
                agent.autoTraverseOffMeshLink = true;
                isSceneModified = true;
            }
        }

        // if scene is modified, mark it as dirty
        if (isSceneModified)
        {
            MarkCurrentSceneDirty();
        }
    }

    // sets the Traverse Off Mesh Link option to FALSE
    [MenuItem("Cinderella City Project/NPCs/Set NPCs Traverse Off Mesh Link FALSE", false, 203)]
    public static void SetNPCsNoTraverseOffMeshLink()
    {
        // note if we made any changes
        bool isSceneModified = false;

        // get all NPC objects
        GameObject[] NPCObjects = ManageSceneObjects.GetAllTopLevelChildrenInObject(ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName("proxy-people")[0]);

        // for each, disable traversing off mesh links
        // but only if there's an agent (skip the ones standing)
        foreach (GameObject NPCObject in NPCObjects)
        {
            NavMeshAgent agent = NPCObject.GetComponentInChildren<NavMeshAgent>();
            if (agent)
            {
                agent.autoTraverseOffMeshLink = false;
            }
        }

        // if scene is modified, mark it as dirty
        if (isSceneModified)
        {
            MarkCurrentSceneDirty();
        }
    }

    ///// STATIC FLAGS /////
    [MenuItem("Cinderella City Project/Static Flags/Update for Current Scene", false, 202)]
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

    ///// LIGHTMAP RESOLUTIONS /////
    [MenuItem("Cinderella City Project/Lightmap Resolutions/Update for Current Scene", false, 203)]
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

    ///// MATERIAL ADJUSTMENTS /////
    [MenuItem("Cinderella City Project/Material Adjustments/Specular/Update for Current Scene", false, 204)]
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

    [MenuItem("Cinderella City Project/Material Adjustments/Smoothness and Metallic/Update for Current Scene", false, 205)]
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


    [MenuItem("Cinderella City Project/Material Adjustments/Emission/Update for Current Scene", false, 206)]
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

    // set all guided tour photos to opaque
    [MenuItem("Cinderella City Project/Material Adjustments/Set Historic Photos Opaque", false, 208)]
    public static void SetAllPhotosOpaque()
    {
        ObjectVisibility.SetHistoricPhotosOpaque(true);
    }

    // set all guided tour photos to translucent
    [MenuItem("Cinderella City Project/Material Adjustments/Set Historic Photos Transparent", false, 209)]
    public static void SetAllPhotosTranslucent()
    {
        ObjectVisibility.SetHistoricPhotosOpaque(false);
    }

    ///// UPDATE MODES /////
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

        [MenuItem(isUpdateModeAllMenuItem, false, 207)]
        private static void SetIsUpdateModeAll()
        {
            isUpdateModeMall = EditorPrefs.GetBool(isUpdateModeMallKey, true);
            isUpdateModeAll = !isUpdateModeAll;
            EditorPrefs.SetBool(isUpdateModeAllKey, isUpdateModeAll);
        }
        [MenuItem(isUpdateModeAllMenuItem, true, 208)]
        private static bool SetIsUpdateModeAllValidate()
        {
            Menu.SetChecked(isUpdateModeAllMenuItem, isUpdateModeAll);
            return true;
        }


        private const string isUpdateModeAnchorsMenuItem = "Cinderella City Project/Set Update Modes/Anchors";
        private const string isUpdateModeAnchorsKey = "CCP.IsUpdateModeAnchor";
        public static bool isUpdateModeAnchors;

        [MenuItem(isUpdateModeAnchorsMenuItem, false, 209)]
        private static void SetIsUpdateModeAnchor()
        {
            isUpdateModeAnchors = !isUpdateModeAnchors;
            EditorPrefs.SetBool(isUpdateModeAnchorsKey, isUpdateModeAnchors);
        }
        [MenuItem(isUpdateModeAnchorsMenuItem, true, 210)]
        private static bool SetIsUpdateModeAnchorsValidate()
        {
            Menu.SetChecked(isUpdateModeAnchorsMenuItem, isUpdateModeAnchors);
            return true;
        }


        private const string isUpdateModeMallMenuItem = "Cinderella City Project/Set Update Modes/Mall + Stores";
        private const string isUpdateModeMallKey = "CCP.IsUpdateModeMall";
        public static bool isUpdateModeMall;

        [MenuItem(isUpdateModeMallMenuItem, false, 210)]
        private static void SetIsUpdateModeMall()
        {
            isUpdateModeMall = !isUpdateModeMall;
            EditorPrefs.SetBool(isUpdateModeMallKey, isUpdateModeMall);
        }
        [MenuItem(isUpdateModeMallMenuItem, true, 211)]
        private static bool SetIsUpdateModeMallValidate()
        {
            Menu.SetChecked(isUpdateModeMallMenuItem, isUpdateModeMall);
            return true;
        }

        private const string isUpdateModeSiteMenuItem = "Cinderella City Project/Set Update Modes/Site";
        private const string isUpdateModeSiteKey = "CCP.IsUpdateModeSite";
        public static bool isUpdateModeSite;

        [MenuItem(isUpdateModeSiteMenuItem, false, 212)]
        private static void SetIsUpdateModeSite()
        {
            isUpdateModeSite = !isUpdateModeSite;
            EditorPrefs.SetBool(isUpdateModeSiteKey, isUpdateModeSite);
        }
        [MenuItem(isUpdateModeSiteMenuItem, true, 213)]
        private static bool SetIsUpdateModeSiteValidate()
        {
            Menu.SetChecked(isUpdateModeSiteMenuItem, isUpdateModeSite);
            return true;
        }
    }

    /* ---------- NAVIGATION SECTION ---------- */

    // add all non-walkable mesh renderers to the selection
    // editor operator must manually set these to non-walkable in the Navigation tab
    [MenuItem("Cinderella City Project/Nav Meshes/Select All Non-Walkable Meshes", false, 300)]
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

    [MenuItem("Cinderella City Project/Nav Meshes/Select Non-Walkable Meshes in Selection", false, 301)]
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

    [MenuItem("Cinderella City Project/Nav Meshes/Update for Current Scene", false, 302)]
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

    [MenuItem("Cinderella City Project/Nav Meshes/Update for All Scenes", false, 303)]
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

    ///// GUIDED TOUR /////
    // prints the names and indices of all historic cameras in the scene
    [MenuItem("Cinderella City Project/Guided Tour/Log Guided Tour Camera Data", false, 304)]
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
    [MenuItem("Cinderella City Project/Guided Tour/Find All Curated Tour Objects", false, 305)]
    public static void FindCuratedTourIndices()
    {
        DebugUtils.DebugLog("Finding curated Guided Tour objects in current scene...");

        GameObject[] allHistoricPhotoObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(SceneManager.GetActiveScene().name);

        GameObject[] curatedHistoricPhotoObjects = ManageSceneObjects.ProxyObjects.GetCuratedGuidedTourCameras(allHistoricPhotoObjects);

        if (curatedHistoricPhotoObjects.Length > 0)
        {

            foreach (GameObject gameObject in curatedHistoricPhotoObjects)
            {
                DebugUtils.DebugLog("Found curated Guided Tour object: " + gameObject.name);
            }
        }
        else
        {
            DebugUtils.DebugLog("No curated Guided Tour objects found!");
        }
    }

    // ensures agent and FPSController are aligned
    // to avoid unexpected position updates during GuidedTour
    [MenuItem("Cinderella City Project/Guided Tour/Align Agent and FPSController", false, 307)]
    public static void AlignAgentAndController()
    {
        GameObject firstPersonControllerParent = ManageFPSControllers.GetFirstPersonControllerInScene().gameObject;
        if (firstPersonControllerParent != null)
        {
            NavMeshAgent agent = firstPersonControllerParent.GetComponentInChildren<NavMeshAgent>();
            if (agent != null)
            {
                agent.transform.position = firstPersonControllerParent.transform.position;
            }
            else
            {
                Debug.LogWarning("NavMeshAgent component not found on the first-person controller.");
            }
        }
        else
        {
            Debug.LogWarning("First-person controller not found in the scene.");
        }
    }

    // gets all guided tour objects
    [MenuItem("Cinderella City Project/Guided Tour/Draw Shuffled Destinations and Paths", false, 308)]
    public static void DrawAllGuidedTourPaths()
    {
        int shuffleSeed = Random.Range(0, 10000);
        //int shuffleSeed = DebugGlobals.guidedTourShuffleDebugSeed1;

        // clear any debug gizmos if there are any
        DebugUtils.ClearLines();
        DebugUtils.ClearConsole();

        // get the FPSController in this scene
        GameObject FPSControllerObject = ManageFPSControllers.GetFirstPersonControllerInScene().gameObject;
        if (FPSControllerObject != null)
        {
            // get the FollowGuidedTour script instance on the FPSController
            FollowGuidedTour followGuidedTourScriptInstance = FPSControllerObject.GetComponentInChildren<FollowGuidedTour>();

            // set the flag to test all paths
            followGuidedTourScriptInstance.doTestAllPaths = true;
            // enable debug lines
            followGuidedTourScriptInstance.showDebugLines = true;
            followGuidedTourScriptInstance.shuffleSeed = shuffleSeed;
            DebugUtils.DebugLog("Testing Guided Tour locations using shuffle seed " + shuffleSeed);

            // call the Start() method which will initialize the guided tour objects
            if (followGuidedTourScriptInstance != null)
            {
                // use Reflection to get the Start() method in the FollowGuidedTour script
                var method = followGuidedTourScriptInstance.GetType().GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(followGuidedTourScriptInstance, null);
                }
                else
                {
                    Debug.LogError("Start method not found.");
                }
            }
            else
            {
                Debug.LogError("Script instance not found.");
            }
        }
    }

    // clears all debug GuidedTour path lines
    [MenuItem("Cinderella City Project/Guided Tour/Clear Guided Tour Debug Lines", false, 309)]
    public static void ClearGuidedTourDebugLInes()
    {
        DebugUtils.ClearLines();
    }

    [MenuItem("Cinderella City Project/Nav Meshes/Show Guided Tour Camera Position Debug Lines", false, 310)]
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

    /* ---------- MISC SECTION ---------- */

    ///// OCCLUSION CULLING /////
    [MenuItem("Cinderella City Project/Occlusion Culling/Update for All Scenes", false, 400)]
    public static void UpdateOcclusionCulling()
    {
        // load all scenes additively
        ManageEditorScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);

        HoistCurrentEditorSceneContainersUp();

        // compute the static occlusion culling after the scenes are moved to intervals
        StaticOcclusionCulling.Compute();
    }

    ///// THUMBNAIL SCREENSHOTS /////
    [MenuItem("Cinderella City Project/Thumbnail Screenshots/Update for Current Scene", false, 401)]
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

    ///// BATCH OPERATIONS /////
    [MenuItem("Cinderella City Project/Batch Operations/Post Process Scene Update", false, 402)]
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

    /* ---------- UNITY SECTION ---------- */
    [MenuItem("Cinderella City Project/Reopen Project", false, 1000)]
    public static void ReopenProject()
    {
        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
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
