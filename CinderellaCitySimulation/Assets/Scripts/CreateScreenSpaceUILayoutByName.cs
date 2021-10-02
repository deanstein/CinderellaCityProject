using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Creates bespoke UI layouts, depending on the name of its host object 
/// for example, the Main Menu, Pause Menu, and Loading Screen host objects
/// will launch their respective UI element layouts
/// </summary>

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateScreenSpaceUILayoutByName : MonoBehaviour
{
    // this script needs to be attached to a "launcher" - an empty gameobject in the appropriate scene
    // then, the correct menu layout or layout combinations will be built based on the launcher's name

    // define the background image sequence for each menu or screen
    // TODO: increase variety and randomize
    string[] loadingScreenBackgroundSlideShowSequence = { "UI/LoadingScreenBackground1", "UI/LoadingScreenBackground2" };
    string[] mainMenuBackgroundSlideShowSequence = { "UI/MainMenuBackground1", "UI/MainMenuBackground2" };

    // the current screen resolution will be checked against the last known resolution
    // to determine whether the UI needs to be rebuilt
    int lastScreenWidth = Screen.width;
    int lastScreenHeight = Screen.height;

    // define how many frames can elapse before the resolution should be checked for changes
    int maxFramesBetweenCheck = 7;
    int currentFrameCount = 0;

    private void Start()
    {
        // every scene gets its UI built at the start
        BuildCurrentSceneUI();
    }

    void Update()
    {
        // UI needs to be rebuilt if the screen size changes
        // so check the resolution every n frames, then only build the scene UI if it's new or changed
        if ((lastScreenWidth != Screen.width || lastScreenHeight != Screen.height) && (currentFrameCount > maxFramesBetweenCheck))
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ClearCurrentSceneUI();
            BuildCurrentSceneUI();

            // reset the frame count
            currentFrameCount = 0;
        }

        // increment the frame count
        currentFrameCount++;
    }

    // delete all the UI objects below the UI launcher host
    // must do this as two separate for loops and lists
    // because of a timing quirk when using a traditional for loop
    public void ClearCurrentSceneUI()
    {
        List<GameObject> sceneUIObjects = new List<GameObject>();

        foreach (Transform child in this.transform)
        {
            sceneUIObjects.Add(child.gameObject);
        }

        sceneUIObjects.ForEach(child => DestroyImmediate(child));
    }

    // build UI based on the name of the current scene
    public void BuildCurrentSceneUI()
    {

        // ensure there's always an EventSystem
        if (GameObject.Find("EventSystem") == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // menu UI will be nested under the launcher object
        GameObject UILauncher = this.transform.gameObject;

        /*** per-scene UI ***/

        // loading screen
        if (this.name.Contains("LoadingScreenLauncher"))
        {
            BuildLoadingScreen(UILauncher, loadingScreenBackgroundSlideShowSequence);
        }

        // the main menu
        else if (this.name.Contains("MainMenuLauncher"))
        {
            BuildMainMenu(UILauncher, mainMenuBackgroundSlideShowSequence);
        }

        // pause menu
        else if (this.name.Contains("PauseMenuLauncher"))
        {
            BuildPauseMenu(UILauncher);
        }

        // generic UI launcher in FPSController scenes
        // used here to display persistent HUD UI
        else if (this.name.Contains("UILauncher"))
        {
            BuildTimePeriodHUD(UILauncher);

            // if an overlay menu was active, restore it
            if (UIVisibilityGlobals.isOverlayMenuActive)
            {
                ManageOverlayVisibility.RestoreLastKnownOverlayMenu(UILauncher);
            }
        }

        /*** overlay UI ***/

        // visibility toggle menu
        // this is typically an overlay menu, but is listed here for testing purposes
        // when invoked from its own scene
        else if (this.name.Contains("VisibilityMenuLauncher"))
        {
            BuildVisualizationMenuOverlay(UILauncher);
        }

        // otherwise, no UI will be built because the name wasn't found or recognized
        else
        {
            Utils.DebugUtils.DebugLog("Unknown UI type! " + name);
        }
    }

    //
    // define layouts
    // 

    /*** per-scene UI ***/

    // loading screen
    public static GameObject BuildLoadingScreen(GameObject UILauncher, string[] backgroundSlideshowSequence)
    {
        Utils.DebugUtils.DebugLog("Building the loading screen...");

        // the loading screen is responsible for preloading the large scenes so level choice is faster

        // loading screen canvas
        GameObject loadingScreen = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.loadingSceneName);

        // background image slideshow
        GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(loadingScreen, backgroundSlideshowSequence);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(loadingScreen);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(loadingScreen, logoHeader, "Building Cinderella City...");

        // create the game version indicator
        GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(loadingScreen);

        return loadingScreen;
    }

    // main menu
    public static GameObject BuildMainMenu(GameObject UILauncher, string[] backgroundSlideshowSequence)
    {
        Utils.DebugUtils.DebugLog("Building the Main Menu...");

        // main menu canvas
        GameObject mainMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.mainMenuSceneName);

        // background image slideshow
        GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(mainMenu, backgroundSlideshowSequence);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(mainMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(mainMenu, logoHeader, "Choose a time and place:");

        // time & place picker and container
        GameObject mainMenuCentralNav = CreateScreenSpaceUIElements.CreateMainMenuCentralNav(mainMenu, titleBarContainer);

        // create the game version indicator
        GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(mainMenu);

        return mainMenu;
    }

    // pause menu
    public static GameObject BuildPauseMenu(GameObject UILauncher)
    {
        Utils.DebugUtils.DebugLog("Building the Pause Menu...");

        // clear the time travel thumbnails, since this may be called multiple times in one session
        UIGlobals.timeTravelThumbnails.Clear();

        // pause menu canvas
        GameObject pauseMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.pauseMenuName);

        // background image
        GameObject backgroundImage = CreateScreenSpaceUIElements.CreateFullScreenImageFromCameraTexture(pauseMenu, true);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(pauseMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(pauseMenu, logoHeader, "Pause");

        // time travel column and pause menu buttons
        GameObject pauseMenuCentralNav = CreateScreenSpaceUIElements.CreatePauseMenuCentralNav(pauseMenu, titleBarContainer);

        // create the game version indicator
        GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(pauseMenu);

        return pauseMenu;
    }

    /*** overlay UI ***/

    // the heads-up-display
    public static GameObject BuildTimePeriodHUD(GameObject UILauncher)
    {
        Utils.DebugUtils.DebugLog("Building the Heads Up Display...");

        // Heads Up Display canvas
        GameObject HUDCanvasParentObject = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, "HUD");
        UIVisibilityGlobals.activeHUD = HUDCanvasParentObject;

        // create the time period indicator
        GameObject HUDTimePeriodIndicator = CreateScreenSpaceUIElements.CreateHUDTimePeriodIndicator(HUDCanvasParentObject, Utils.StringUtils.ConvertSceneNameToFriendlyName(UILauncher.scene.name));

        // create the game version indicator
        GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(HUDCanvasParentObject);

        // note that some scenes are under construction
        if (UILauncher.scene.name.Contains("AltFuture"))
        {
            CreateScreenSpaceUIElements.CreateHUDUnderConstructionLabel(HUDCanvasParentObject, "/// Under Construction ///");
        }

        return HUDCanvasParentObject;
    }

    // the visualization menu
    public static GameObject BuildVisualizationMenuOverlay(GameObject UILauncher)
    {
        Utils.DebugUtils.DebugLog("Building the Visibility Menu...");

        // visibility menu canvas
        GameObject visibilityMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.visibilityMenuName);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(visibilityMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(visibilityMenu, logoHeader, "Visibility Settings");

        // put all the possible toggles in a horizontal scroll area
        // and central nav container
        GameObject toggleSetHorizontalScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("VisibilityToggleSet", "horizontal");
        GameObject toggleSetContainer = CreateScreenSpaceUIElements.CreateCentralNavContainer(visibilityMenu, titleBarContainer);
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(toggleSetHorizontalScrollArea, toggleSetContainer);

        ///
        /// object visibility settings
        /// 

        // create the object visibility scroll area
        GameObject objectVisibilityScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("ObjectVisibility", "vertical");

        // create the object visibility toggle group container
        GameObject objectVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, toggleSetHorizontalScrollArea, true, "OBJECTS");

        // configure scroll area to fit the toggle group
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(objectVisibilityScrollArea, objectVisibilityToggleGroup);

        // first, create a list of toggles required for each of the object sets
        List<GameObject> visibilityToggles = new List<GameObject>();

        // object visibility toggles
        GameObject anchorStoresVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, objectVisibilityToggleGroup.transform.GetChild(0).gameObject, "Department Stores", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.anchorStoreObjectKeywords));
        visibilityToggles.Add(anchorStoresVisibilityToggle);

        GameObject ceilingsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, anchorStoresVisibilityToggle, "Mall: Ceilings", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.ceilingObjectKeywords));
        visibilityToggles.Add(ceilingsVisibilityToggle);

        GameObject exteriorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, ceilingsVisibilityToggle, "Mall: Exterior Walls", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.exteriorWallObjectKeywords));
        visibilityToggles.Add(exteriorWallsVisibilityToggle);

        GameObject floorsVertVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, exteriorWallsVisibilityToggle, "Mall: Floors", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.floorObjectKeywords));
        visibilityToggles.Add(floorsVertVisibilityToggle);

        GameObject interiorDetailingVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, floorsVertVisibilityToggle, "Mall: Interior Detailing", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.interiorDetailingObjectKeywords));
        visibilityToggles.Add(interiorDetailingVisibilityToggle);

        GameObject interiorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorDetailingVisibilityToggle, "Mall: Interior Walls", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.interiorWallObjectKeywords));
        visibilityToggles.Add(interiorWallsVisibilityToggle);

        GameObject roofVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorWallsVisibilityToggle, "Mall: Roof", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.roofObjectKeywords));
        visibilityToggles.Add(roofVisibilityToggle);

        GameObject signageVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, roofVisibilityToggle, "Mall: Signage", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.signageObjectKeywords));
        visibilityToggles.Add(signageVisibilityToggle);

        GameObject peopleVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, signageVisibilityToggle, "People", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.peopleObjectKeywords));
        visibilityToggles.Add(peopleVisibilityToggle);

        GameObject vegetationVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, peopleVisibilityToggle, "Vegetation", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.vegetationObjectKeywords));
        visibilityToggles.Add(vegetationVisibilityToggle);

        GameObject waterFeatureVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, vegetationVisibilityToggle, "Water Features", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.waterFeatureObjectKeywords));
        visibilityToggles.Add(waterFeatureVisibilityToggle);

        // now populate the object visibility toggle group container
        CreateScreenSpaceUIElements.PopulateToggleGroup(objectVisibilityToggleGroup, visibilityToggles);

        ///
        /// UI visibility settings
        ///

        // create the object visibility scroll area
        GameObject UIVisibilityScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("UIVisibilitySettings", "vertical");

        // create the object visibility toggle group container
        GameObject UIVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, objectVisibilityToggleGroup, false, "USER INTERFACE");

        // configure scroll area to fit the toggle group
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(UIVisibilityScrollArea, UIVisibilityToggleGroup);

        // first, create a list of toggles required for each of the object sets
        List<GameObject> UIVisibilityToggles = new List<GameObject>();

        // show heads-up display elements
        GameObject showHUDToggle = CreateScreenSpaceUIElements.CreateToggleModule(UIVisibilityToggleGroup, objectVisibilityToggleGroup.transform.GetChild(0).gameObject, "Show HUD");
        CreateScreenSpaceUIElements.ConfigureTypicalToggle(showHUDToggle, ManageHUDVisibility.ToggleHUDCanvas, UIVisibilityGlobals.isHUDActive);
        UIVisibilityToggles.Add(showHUDToggle);

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateToggleGroup(UIVisibilityToggleGroup, UIVisibilityToggles);

        ///
        /// camera settings
        ///

        // create the object visibility scroll area
        GameObject cameraSettingsScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("CameraSettings", "vertical");

        // create the object visibility toggle group container
        GameObject cameraSettingsToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, UIVisibilityToggleGroup, false, "CAMERA SETTINGS");

        // configure scroll area to fit the toggle group
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(cameraSettingsScrollArea, cameraSettingsToggleGroup);

        // first, create a list of toggles required for each of the object sets
        List<GameObject> cameraSettingsToggles = new List<GameObject>();

        // occlusion culling
        GameObject occlusionCullingToggle = CreateScreenSpaceUIElements.CreateToggleModule(cameraSettingsToggleGroup, UIVisibilityToggleGroup.transform.GetChild(0).gameObject, "Occlusion Culling");
        CreateScreenSpaceUIElements.ConfigureTypicalToggle(occlusionCullingToggle, ManageCameraActions.ToggleCurrentCameraOcclusionCullingState, ManageCameraActions.GetCurrentCameraOcclusionCullingState());
        cameraSettingsToggles.Add(occlusionCullingToggle);

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateToggleGroup(cameraSettingsToggleGroup, cameraSettingsToggles);

        ///
        /// camera actions
        ///

        // create the object visibility scroll area
        GameObject cameraActionsScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("CameraActions", "vertical");

        // create the object visibility toggle group container
        GameObject cameraActionsButtonGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, cameraSettingsToggleGroup, false, "CAMERA ACTIONS");

        // configure scroll area to fit the toggle group
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(cameraActionsScrollArea, cameraActionsButtonGroup);

        // first, create a list of buttons required for each of the object sets
        List<GameObject> cameraActionButtons = new List<GameObject>();

        // take screenshot button
        GameObject takeScreenshotButton = CreateScreenSpaceUIElements.CreateTextButton("Take Screenshot", cameraActionsButtonGroup, UIGlobals.visibilitymenuTextButtonlabelSize, UIGlobals.containerColor);
        takeScreenshotButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            // take the screenshot
            TakeScreenshots.CaptureScreenshotOfCurrentCamera(ManageCameraActions.CameraActionGlobals.inGameScreenshotsPath);

        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(takeScreenshotButton, cameraSettingsToggleGroup.transform.GetChild(1).gameObject, 0.0f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(takeScreenshotButton, cameraActionsButtonGroup, 0.01f);

        //restoreViewFromClipboardButton.
        cameraActionButtons.Add(takeScreenshotButton);

        // restore view button
        GameObject restoreViewFromClipboardButton = CreateScreenSpaceUIElements.CreateTextButton("Restore View", cameraActionsButtonGroup, UIGlobals.visibilitymenuTextButtonlabelSize, UIGlobals.containerColor);
        restoreViewFromClipboardButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            // get the restore data from the clipboard
            ManageFPSControllers.FPSControllerRestoreData restoreData = ManageFPSControllers.FPSControllerRestoreData.ReadFPSControllerRestoreDataFromClipboard();

            ManageFPSControllers.RelocateAlignFPSControllerToMatchRestoreData(restoreData);
        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(restoreViewFromClipboardButton, takeScreenshotButton, 0.01f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(restoreViewFromClipboardButton, cameraActionsButtonGroup, 0.01f);

        //restoreViewFromClipboardButton.
        cameraActionButtons.Add(restoreViewFromClipboardButton);

        // now populate the object camera settings toggle group container
        //CreateScreenSpaceUIElements.PopulateToggleGroup(cameraActionsButtonGroup, cameraActionButtons);

        // set parent/child hierarchy
        toggleSetHorizontalScrollArea.transform.SetParent(visibilityMenu.transform);
        toggleSetContainer.transform.SetParent(toggleSetHorizontalScrollArea.transform);

        objectVisibilityScrollArea.transform.parent = toggleSetContainer.transform;
        objectVisibilityToggleGroup.transform.SetParent(objectVisibilityScrollArea.transform);

        UIVisibilityScrollArea.transform.parent = toggleSetContainer.transform;
        UIVisibilityToggleGroup.transform.SetParent(UIVisibilityScrollArea.transform);

        cameraSettingsScrollArea.transform.parent = toggleSetContainer.transform;
        cameraSettingsToggleGroup.transform.SetParent(cameraSettingsScrollArea.transform);

        cameraActionsScrollArea.transform.parent = toggleSetContainer.transform;
        cameraActionsButtonGroup.transform.SetParent(cameraActionsScrollArea.transform);

        return visibilityMenu;
    }
}
 
 
 
 
 