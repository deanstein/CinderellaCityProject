using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

    // the background image sequence, if required, is created once, then stored here
    Sprite[] backgroundImageSequence = { };

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

    private void OnEnable()
    {
        // mark the current scene's HUD as the globally-available HUD
        // so it can be hidden by other scripts if necessary
        if (this.name.Contains("UILauncher") && UIVisibilityGlobals.activeHUD)
        {
            UIVisibilityGlobals.activeHUD = ManageHUDVisibility.GetActiveHUDCanvas(this.gameObject);
        }

        // create an updated time traveling label
        string upcomingSceneName = ManageScenes.GetUpcomingPeriodSceneName(gameObject.scene.name, "next");
        if (upcomingSceneName != "null")
        {
            string nextTimePeriodLabelText = SceneGlobals.TimePeriods.GetTimePeriodLabelBySceneName(upcomingSceneName);
            // create the time traveling label and set it off initially
            if (UIGlobals.timeTravelingLabelContainer)
            {
                DestroyImmediate(UIGlobals.timeTravelingLabelContainer);
            }
            UIGlobals.timeTravelingLabelContainer = CreateScreenSpaceUIElements.CreateHUDTimeTravelingLabel(UIVisibilityGlobals.activeHUD, "TIME TRAVELING TO " + nextTimePeriodLabelText);
            UIGlobals.timeTravelingLabelContainer.SetActive(false);
        }
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

        // show the time-traveling label if appropriate
        if (UIGlobals.timeTravelingLabelContainer)
        {
            if (ModeState.doShowTimeTravelingLabel)
            {
                UIGlobals.timeTravelingLabelContainer.SetActive(true);
            }
            else
            {
                UIGlobals.timeTravelingLabelContainer.SetActive(false);
            }
        }
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

        UIVisibilityGlobals.isHUDActive = false;
        UIVisibilityGlobals.isOverlayMenuActive = false;
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
            // get all available images in the loading screen backgrounds dir
            backgroundImageSequence = backgroundImageSequence.Length > 0 ? backgroundImageSequence : ArrayUtils.ShuffleArray(Resources.LoadAll<Sprite>(FileDirUtils.ConvertProjectPathToRelativePath(UIGlobals.projectUIPath + UIGlobals.loadingScreenBackgroundsSubdir)));

            BuildLoadingScreen(UILauncher, backgroundImageSequence);
        }

        // the main menu
        else if (this.name.Contains("MainMenuLauncher"))
        {
            // get all available images in the main menu backgrounds dir
            backgroundImageSequence = backgroundImageSequence.Length > 0 ? backgroundImageSequence : ArrayUtils.ShuffleArray(Resources.LoadAll<Sprite>(FileDirUtils.ConvertProjectPathToRelativePath(UIGlobals.projectUIPath + UIGlobals.mainMenuBackgroundsSubdir)));

            BuildMainMenu(UILauncher, backgroundImageSequence);
        }

        // pause menu
        else if (this.name.Contains("PauseMenuLauncher"))
        {
            BuildPauseMenu(UILauncher);
        }

        // the how-to-play screen
        else if (this.name.Contains("HowToPlayScreenLauncher"))
        {
            BuildHowToPlayScreen(UILauncher);
        }

        // the credits screen
        else if (this.name.Contains("CreditsScreenLauncher"))
        {
            BuildCreditsScreen(UILauncher);
        }

        // generic UI launcher in FPSController scenes
        // used here to display persistent HUD UI
        else if (this.name.Contains("UILauncher"))
        {
            BuildTimePeriodHUD(UILauncher);

            // if an overlay menu was active, restore it
            // used when the game window is resized with an overlay menu active
            ManageOverlayVisibility.RestoreLastKnownOverlayMenuByName(UILauncher, UIVisibilityGlobals.lastKnownOverlayMenuName);
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
            DebugUtils.DebugLog("Unknown UI type! " + name);
        }
    }

    //
    // define layouts
    // 

    /*** per-scene UI ***/

    // loading screen
    public static GameObject BuildLoadingScreen(GameObject UILauncher, Sprite[] backgroundSlideshowSequence)
    {
        DebugUtils.DebugLog("Building the loading screen...");

        // the loading screen is responsible for preloading the large scenes so level choice is faster

        // loading screen canvas
        GameObject loadingScreen = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.loadingSceneName);

        // background image slideshow
        GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(loadingScreen, backgroundSlideshowSequence);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(loadingScreen);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(loadingScreen, logoHeader, "Building Cinderella City...", false);

        // create the game version indicator
        CreateScreenSpaceUIElements.CreateBottomTextRow(loadingScreen);

        return loadingScreen;
    }

    // main menu
    public static GameObject BuildMainMenu(GameObject UILauncher, Sprite[] backgroundSlideshowSequence)
    {
        DebugUtils.DebugLog("Building the Main Menu...");

        // main menu canvas
        GameObject mainMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.mainMenuSceneName);

        // background image slideshow
        GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(mainMenu, backgroundSlideshowSequence);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(mainMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(mainMenu, logoHeader, "Choose a time and place:", false);

        // time & place picker and container
        GameObject mainMenuCentralNav = CreateScreenSpaceUIElements.CreateMainMenuCentralNav(mainMenu, titleBarContainer);

        // bottom menu bar
        GameObject bottomMenubar = CreateScreenSpaceUIElements.CreateBottomMenuBar(mainMenu);

        // the buttons in the bottom menu bar
        List<GameObject> menuBarButtons = new List<GameObject>();

        GameObject howToPlayButton = CreateScreenSpaceUIElements.CreateTextButton("How to Play", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio, UIGlobals.bottomMenuBarButtonScreenWidthRatio, UIGlobals.buttonColor);
        menuBarButtons.Add(howToPlayButton);
        howToPlayButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.howToPlaySceneName);

        }); ;

        GameObject creditsButton = CreateScreenSpaceUIElements.CreateTextButton("Credits", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio, UIGlobals.bottomMenuBarButtonScreenWidthRatio, UIGlobals.buttonColor);
        menuBarButtons.Add(creditsButton);
        creditsButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.creditsSceneName);

        }); ;

        // exit button
        GameObject exitButton = CreateScreenSpaceUIElements.CreateTextButton("Exit", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio, UIGlobals.bottomMenuBarButtonScreenWidthRatio, UIGlobals.buttonColor);
        menuBarButtons.Add(exitButton);
        exitButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            Application.Quit();
        });

        // populate the menu bar with the buttons
        CreateScreenSpaceUIElements.PopulateMenuBar(bottomMenubar, menuBarButtons);

        // create the game version indicator
        CreateScreenSpaceUIElements.CreateBottomTextRow(mainMenu);

        return mainMenu;
    }

    // how to play, including shortcuts/controls
    public static GameObject BuildHowToPlayScreen(GameObject UILauncher)
    {
        DebugUtils.DebugLog("Building the How-to-Play Screen...");

        // define the background image path
        string backgroundImagePath = "UI/HowToPlayScreenBackground";

        // canvas
        GameObject howToPlayScreenCanvas = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.howToPlaySceneName);

        // background image slideshow
        GameObject backgroundImage = CreateScreenSpaceUIElements.CreateFullScreenImageBackground(howToPlayScreenCanvas, backgroundImagePath);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(howToPlayScreenCanvas);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(howToPlayScreenCanvas, logoHeader, "How to play:", true);

        // create the game version indicator
        CreateScreenSpaceUIElements.CreateBottomTextRow(howToPlayScreenCanvas);

        return howToPlayScreenCanvas;
    }

    // credits
    public static GameObject BuildCreditsScreen(GameObject UILauncher)
    {
        DebugUtils.DebugLog("Building the Credits Screen...");

        // define the background image path
        string backgroundImagePath = "UI/CreditsScreenBackground";

        // canvas
        GameObject creditsScreenCanvas = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.creditsSceneName);

        // background image
        GameObject backgroundImage = CreateScreenSpaceUIElements.CreateFullScreenImageBackground(creditsScreenCanvas, backgroundImagePath);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(creditsScreenCanvas);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(creditsScreenCanvas, logoHeader, "Credits", true);

        // put all credits content in a horizontal scroll area
        // and central nav container
        GameObject creditsContentHorizontalScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("CreditsContentScrollArea", "horizontal");
        GameObject creditsContentContainer = CreateScreenSpaceUIElements.CreateCentralNavContainer(creditsScreenCanvas, titleBarContainer);
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(creditsContentHorizontalScrollArea, creditsContentContainer);

        // get the list of lists from the CSV file
        List<List<string>> creditsLists = ReadCSV.GetCreditsListsFromCSV();

        // keep track of the created credits group modules
        List<GameObject> createdCreditsGroupModules = new List<GameObject>();

        for (int i = 0; i < creditsLists.Count; i++)
        {
            // the first item gets a different left alignment object
            if (i == 0)
            {
                GameObject creditsGroup = CreateScreenSpaceUIElements.CreateCreditsGroupModule(creditsContentContainer, creditsContentContainer, true, UIGlobals.toggleContainerMaxWidthScreenWidthRatio, creditsLists[i][0]);
                createdCreditsGroupModules.Add(creditsGroup);

                // create a text item for each of the credits in this list
                List<GameObject> createdCreditItems = CreateScreenSpaceUIElements.CreateCreditItemsFromList(creditsLists[i], creditsGroup);

                // for each of the items in the credits list, create a text item
                CreateScreenSpaceUIElements.PopulateContentGroup(creditsGroup, createdCreditItems);
            }
            else
            {
                GameObject creditsGroup = CreateScreenSpaceUIElements.CreateCreditsGroupModule(creditsContentContainer, createdCreditsGroupModules[i - 1], false, UIGlobals.toggleContainerMaxWidthScreenWidthRatio, creditsLists[i][0]);
                createdCreditsGroupModules.Add(creditsGroup);

                // create a text item for each of the credits in this list
                List<GameObject> createdCreditItems = CreateScreenSpaceUIElements.CreateCreditItemsFromList(creditsLists[i], creditsGroup);
                // for each of the items in the credits list, create a text item
                CreateScreenSpaceUIElements.PopulateContentGroup(creditsGroup, createdCreditItems);
            }
        }

        // resize the content within the scroll area to just past the last sub-element
        TransformScreenSpaceObject.ResizeParentContainerToFitLastChild(creditsContentContainer, createdCreditsGroupModules[createdCreditsGroupModules.Count - 1], UIGlobals.toggleContainerPadding, "right");

        // create the game version indicator
        CreateScreenSpaceUIElements.CreateBottomTextRow(creditsScreenCanvas);

        // set parent/child hierarchy
        creditsContentHorizontalScrollArea.transform.SetParent(creditsScreenCanvas.transform);
        creditsContentContainer.transform.SetParent(creditsContentHorizontalScrollArea.transform);

        foreach (GameObject creditsGroupModule in createdCreditsGroupModules)
        {
            CreateScreenSpaceUIElements.SetContentGroupHierarchy(creditsContentContainer, creditsGroupModule);
        }

        return creditsScreenCanvas;
    }

    // pause menu
    public static GameObject BuildPauseMenu(GameObject UILauncher)
    {
        DebugUtils.DebugLog("Building the Pause Menu...");

        // clear the time travel thumbnails, since this may be called multiple times in one session
        UIGlobals.timeTravelThumbnails.Clear();

        // pause menu canvas
        GameObject pauseMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.pauseMenuName);

        // background image
        GameObject backgroundImage = CreateScreenSpaceUIElements.CreateFullScreenImageFromCameraTexture(pauseMenu, true);

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(pauseMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(pauseMenu, logoHeader, "Pause", true);

        // time travel column and pause menu buttons
        GameObject pauseMenuCentralNav = CreateScreenSpaceUIElements.CreatePauseMenuCentralNav(pauseMenu, titleBarContainer);

        // create the game version indicator
        CreateScreenSpaceUIElements.CreateBottomTextRow(pauseMenu);

        return pauseMenu;
    }

    /*** overlay UI ***/

    // the heads-up-display
    public static GameObject BuildTimePeriodHUD(GameObject UILauncher)
    {
        DebugUtils.DebugLog("Building the Heads Up Display...");

        // Heads Up Display canvas
        GameObject HUDCanvasParentObject = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, "HUD");

        UIVisibilityGlobals.activeHUD = HUDCanvasParentObject;
        UIVisibilityGlobals.isHUDActive = true;

        // create the time period indicator
        GameObject HUDTimePeriodIndicator = CreateScreenSpaceUIElements.CreateHUDTimePeriodIndicator(HUDCanvasParentObject, SceneGlobals.TimePeriods.GetTimePeriodLabelBySceneName(UILauncher.scene.name));

        // create the bottom text row, containing the version and the guided tour indicator text
        CreateScreenSpaceUIElements.CreateBottomTextRow(HUDCanvasParentObject);

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
        DebugUtils.DebugLog("Building the Visibility Menu...");

        // visibility menu canvas
        GameObject visibilityMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.visibilityMenuSceneName);
        UIVisibilityGlobals.isOverlayMenuActive = true;
        UIVisibilityGlobals.activeOverlayMenu = visibilityMenu;
        UIVisibilityGlobals.lastKnownOverlayMenuName = visibilityMenu.name;

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(visibilityMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(visibilityMenu, logoHeader, "Visibility Settings", true);

        // put all the possible toggles in a horizontal scroll area
        // and central nav container
        GameObject toggleSetHorizontalScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("VisibilityToggleSet", "horizontal");
        GameObject toggleSetContainer = CreateScreenSpaceUIElements.CreateCentralNavContainer(visibilityMenu, titleBarContainer);
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(toggleSetHorizontalScrollArea, toggleSetContainer);

        ///
        /// object visibility settings
        /// 

        // create the object visibility toggle group container
        GameObject objectVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, toggleSetHorizontalScrollArea, true, 0.2f /* gets resized later */, "OBJECTS");

        // first, create a list of toggles required for each of the object sets
        List<GameObject> visibilityToggles = new List<GameObject>();

        // object visibility toggles
        GameObject anchorStoresVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, objectVisibilityToggleGroup.transform.GetChild(0).gameObject, "Department Stores", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.anchorStoreObjectKeywords));
        visibilityToggles.Add(anchorStoresVisibilityToggle);

        GameObject ceilingsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, anchorStoresVisibilityToggle, "Mall: Ceilings", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.ceilingObjectKeywords));
        visibilityToggles.Add(ceilingsVisibilityToggle);

        GameObject exteriorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, ceilingsVisibilityToggle, "Mall: Exterior Walls", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.exteriorWallObjectKeywords));
        visibilityToggles.Add(exteriorWallsVisibilityToggle);

        GameObject floorsVertVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, exteriorWallsVisibilityToggle, "Mall: Floors", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.floorObjectKeywords));
        visibilityToggles.Add(floorsVertVisibilityToggle);

        GameObject furnitureVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, floorsVertVisibilityToggle, "Mall: Furniture", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.furnitureObjectKeywords));
        visibilityToggles.Add(furnitureVisibilityToggle);

        GameObject interiorDetailingVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, furnitureVisibilityToggle, "Mall: Interior Detailing", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.interiorDetailingObjectKeywords));
        visibilityToggles.Add(interiorDetailingVisibilityToggle);

        GameObject interiorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorDetailingVisibilityToggle, "Mall: Interior Walls", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.interiorWallObjectKeywords));
        visibilityToggles.Add(interiorWallsVisibilityToggle);

        GameObject lightsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorWallsVisibilityToggle, "Mall: Lights", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.lightsObjectKeyword));
        visibilityToggles.Add(lightsVisibilityToggle);

        GameObject roofVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, lightsVisibilityToggle, "Mall: Roof", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.roofObjectKeywords));
        visibilityToggles.Add(roofVisibilityToggle);

        GameObject signageVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, roofVisibilityToggle, "Mall: Signage", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.signageObjectKeywords));
        visibilityToggles.Add(signageVisibilityToggle);

        GameObject wayfindingVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, signageVisibilityToggle, "Mall: Wayfinding", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.wayfindingObjectKeywords));
        visibilityToggles.Add(wayfindingVisibilityToggle);

        GameObject peopleVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, wayfindingVisibilityToggle, "People", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.peopleObjectKeywords));
        visibilityToggles.Add(peopleVisibilityToggle);

        GameObject vegetationVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, peopleVisibilityToggle, "Vegetation", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.vegetationObjectKeywords));
        visibilityToggles.Add(vegetationVisibilityToggle);

        GameObject waterFeatureVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, vegetationVisibilityToggle, "Water Features", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.waterFeatureObjectKeywords));
        visibilityToggles.Add(waterFeatureVisibilityToggle);

        // now populate the object visibility toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(objectVisibilityToggleGroup, visibilityToggles);

        ///
        /// point of interest / historic photograph visibility settings
        /// 

        // create the object visibility toggle group container
        GameObject pointOfInterestVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, objectVisibilityToggleGroup, false, 0.2f /* gets resized later */, "POINTS OF INTEREST");

        // first, create a list of toggles required for each of the object sets
        List<GameObject> pointsOfInterestToggles = new List<GameObject>();

        // historic photograph toggle
        GameObject historicPhotographsToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(pointOfInterestVisibilityToggleGroup, pointOfInterestVisibilityToggleGroup.transform.GetChild(0).gameObject, "Historic Photographs", ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true));
        pointsOfInterestToggles.Add(historicPhotographsToggle);

        // force all historic photos to opaque
        GameObject historicPhotographsOpacityToggle = CreateScreenSpaceUIElements.CreateToggleModule(pointOfInterestVisibilityToggleGroup, historicPhotographsToggle, "Force 100% Photo Opacity");
        // get the actual toggle
        Toggle toggle = historicPhotographsOpacityToggle.GetComponentInChildren<Toggle>();
        toggle.isOn = ObjectVisibilityGlobals.areHistoricPhotosForcedOpaque;
        // set the toggle to invoke changing the visibility of the object
        toggle.onValueChanged.AddListener(delegate {

            // toggle all historic photo transparencies based on the toggle state
            ObjectVisibility.SetHistoricPhotosOpaque(toggle.isOn);

        });
        pointsOfInterestToggles.Add(historicPhotographsOpacityToggle);

        // now populate the object visibility toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(pointOfInterestVisibilityToggleGroup, pointsOfInterestToggles);

        ///
        /// UI visibility settings
        ///

        // create the object visibility toggle group container
        GameObject UIVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, pointOfInterestVisibilityToggleGroup, false, 0.2f /* gets resized later */, "USER INTERFACE");

        // first, create a list of toggles required for each of the object sets
        List<GameObject> UIVisibilityToggles = new List<GameObject>();

        // show heads-up display elements
        GameObject showHUDToggle = CreateScreenSpaceUIElements.CreateToggleModule(UIVisibilityToggleGroup, objectVisibilityToggleGroup.transform.GetChild(0).gameObject, "Show HUD");
        CreateScreenSpaceUIElements.ConfigureTypicalToggle(showHUDToggle, ManageHUDVisibility.ToggleHUDCanvas, UIVisibilityGlobals.isHUDActive);
        UIVisibilityToggles.Add(showHUDToggle);

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(UIVisibilityToggleGroup, UIVisibilityToggles);

        ///
        /// camera settings
        ///

        // create the object visibility toggle group container
        GameObject cameraSettingsToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, UIVisibilityToggleGroup, false, 0.2f /* gets resized later */, "CAMERA SETTINGS");

        // first, create a list of toggles required for each of the object sets
        List<GameObject> cameraSettingsToggles = new List<GameObject>();

        // get the script required for toggling camera effects
        ToggleCameraActionsByInputEvent cameraEffectToggleScript = ManageFPSControllers.FPSControllerGlobals.activeFPSController ? ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponentInChildren<ToggleCameraActionsByInputEvent>() : null;

        // camera mode: normal
        GameObject cameraModeNormalButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Normal Mode", cameraSettingsToggleGroup, UIVisibilityToggleGroup.transform.GetChild(0).gameObject);
        cameraSettingsToggles.Add(cameraModeNormalButton);
        cameraModeNormalButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            cameraEffectToggleScript.SetCameraEffect(ManageCameraActions.GetDefaultPostProcessProfileBySceneName(SceneManager.GetActiveScene().name));

        });

        // camera mode: vaporwave
        GameObject cameraModeVaporwaveButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Vaporwave Mode", cameraSettingsToggleGroup, cameraModeNormalButton);
        cameraSettingsToggles.Add(cameraModeVaporwaveButton);
        cameraModeVaporwaveButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            cameraEffectToggleScript.SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectVaporwave);

        });

        // camera mode: black and white / noir
        GameObject cameraModeNoirButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Noir Mode", cameraSettingsToggleGroup, cameraModeVaporwaveButton);
        cameraSettingsToggles.Add(cameraModeNoirButton);
        cameraModeNoirButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            cameraEffectToggleScript.SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectNoir);

        });

        // camera mode: sepia
        GameObject cameraModeSepiaButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Sepia Mode", cameraSettingsToggleGroup, cameraModeNoirButton);
        cameraSettingsToggles.Add(cameraModeSepiaButton);
        cameraModeSepiaButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            cameraEffectToggleScript.SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectSepia);

        });

        // occlusion culling
        GameObject occlusionCullingToggle = CreateScreenSpaceUIElements.CreateToggleModule(cameraSettingsToggleGroup, cameraModeSepiaButton.gameObject, "Occlusion Culling");
        CreateScreenSpaceUIElements.ConfigureTypicalToggle(occlusionCullingToggle, ManageCameraActions.ToggleCurrentCameraOcclusionCullingState, ManageCameraActions.GetCurrentCameraOcclusionCullingState());
        cameraSettingsToggles.Add(occlusionCullingToggle);

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(cameraSettingsToggleGroup, cameraSettingsToggles);

        ///
        /// camera actions
        ///

        // create the camera actions toggle group container
        GameObject cameraActionsButtonGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, toggleSetContainer, cameraSettingsToggleGroup, false, 0.1f, "CAMERA ACTIONS");

        // first, create a list of buttons required for each of the object sets
        List<GameObject> cameraActionButtons = new List<GameObject>();

        // take screenshot button
        GameObject takeScreenshotButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Take Screenshot", cameraActionsButtonGroup, cameraActionsButtonGroup.transform.GetChild(0).gameObject);
        cameraActionButtons.Add(takeScreenshotButton);
        takeScreenshotButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            CreateScreenSpaceUIElements.CaptureScreenshotButtonAction();

        });


        // save view button
        GameObject saveViewFromClipboardButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Save View", cameraActionsButtonGroup, takeScreenshotButton);
        cameraActionButtons.Add(saveViewFromClipboardButton);
        saveViewFromClipboardButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            CreateScreenSpaceUIElements.SaveViewButtonAction();

        });

        // restore view button
        GameObject restoreViewFromClipboardButton = CreateScreenSpaceUIElements.CreateTextButtonForToggleGroup("Restore View", cameraActionsButtonGroup, saveViewFromClipboardButton);
        cameraActionButtons.Add(restoreViewFromClipboardButton);
        restoreViewFromClipboardButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            CreateScreenSpaceUIElements.RestoreViewButtonAction();

        });

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(cameraActionsButtonGroup, cameraActionButtons);

        // resize the content within the scroll area to just past the last sub-element
        TransformScreenSpaceObject.ResizeParentContainerToFitLastChild(toggleSetContainer, cameraActionsButtonGroup, UIGlobals.toggleContainerPadding, "right");

        // set parent/child hierarchy
        toggleSetHorizontalScrollArea.transform.SetParent(visibilityMenu.transform);
        toggleSetContainer.transform.SetParent(toggleSetHorizontalScrollArea.transform);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, objectVisibilityToggleGroup);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, pointOfInterestVisibilityToggleGroup);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, UIVisibilityToggleGroup);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, cameraSettingsToggleGroup);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, cameraActionsButtonGroup);

        takeScreenshotButton.transform.SetParent(cameraActionsButtonGroup.transform);
        saveViewFromClipboardButton.transform.SetParent(cameraActionsButtonGroup.transform);
        restoreViewFromClipboardButton.transform.SetParent(cameraActionsButtonGroup.transform);

        ManageFPSControllers.DisableCursorLockOnActiveFPSController();

        return visibilityMenu;
    }

    // the audio settings menu
    public static GameObject BuildAudioMenuOverlay(GameObject UILauncher)
    {
        DebugUtils.DebugLog("Building the Audio Menu...");

        // audio menu canvas
        GameObject audioMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, SceneGlobals.visibilityMenuSceneName);
        UIVisibilityGlobals.isOverlayMenuActive = true;
        UIVisibilityGlobals.activeOverlayMenu = audioMenu;
        UIVisibilityGlobals.lastKnownOverlayMenuName = audioMenu.name;

        // project logo and container
        GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(audioMenu);

        // create the title bar container
        GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(audioMenu, logoHeader, "Audio Settings", true);

        // put all the possible toggles in a horizontal scroll area
        // and central nav container
        GameObject toggleSetHorizontalScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("AudioToggleSet", "horizontal");
        GameObject toggleSetContainer = CreateScreenSpaceUIElements.CreateCentralNavContainer(audioMenu, titleBarContainer);
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(toggleSetHorizontalScrollArea, toggleSetContainer);

        ///
        /// mall music actions
        ///

        // create the camera actions toggle group container
        GameObject mallMusicActionsButtonGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(audioMenu, toggleSetContainer, toggleSetHorizontalScrollArea, true, 0.1f, "MALL MUSIC");

        // first, create a list of buttons required for each of the object sets
        List<GameObject> mallMusicActionButtons = new List<GameObject>();

        // mute/unmute button
        GameObject mallMusicMuteUnmuteButton = CreateScreenSpaceUIElements.CreateTextButton("Mute/Unmute", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuTitleBottomMarginScreenHeightRatio, UIGlobals.menuButtonScreenWidthRatio, UIGlobals.containerColor);
        mallMusicMuteUnmuteButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            // get the correct params
            SpeakerParams musicParams = PlayAudioSequencesByName.GetMallMusicSpeakerParamsByScene(SceneManager.GetActiveScene().name);
            // mute
            PlayAudioSequencesByName.MuteSpeakers(musicParams);

        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(mallMusicMuteUnmuteButton, mallMusicActionsButtonGroup.transform.GetChild(0).gameObject, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(mallMusicMuteUnmuteButton, mallMusicActionsButtonGroup);
        mallMusicActionButtons.Add(mallMusicMuteUnmuteButton);

        // previous track button
        GameObject previousTrackButton = CreateScreenSpaceUIElements.CreateTextButton("Previous Track", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio, UIGlobals.menuButtonScreenWidthRatio, UIGlobals.containerColor);
        previousTrackButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            // get the correct params
            SpeakerParams musicParams = PlayAudioSequencesByName.GetMallMusicSpeakerParamsByScene(SceneManager.GetActiveScene().name);
            // previous track
            PlayAudioSequencesByName.PlayPreviousTrack(musicParams);

        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(previousTrackButton, mallMusicMuteUnmuteButton, UIGlobals.toggleContainerPadding);
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(previousTrackButton, mallMusicMuteUnmuteButton);
        mallMusicActionButtons.Add(previousTrackButton);

        // next track button
        GameObject nextTrackButton = CreateScreenSpaceUIElements.CreateTextButton("Next Track", UIGlobals.visibilityMenuTextButtonlabelSize, UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio, UIGlobals.menuButtonScreenWidthRatio, UIGlobals.containerColor);
        nextTrackButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            // get the correct params
            SpeakerParams musicParams = PlayAudioSequencesByName.GetMallMusicSpeakerParamsByScene(SceneManager.GetActiveScene().name);
            // next track
            PlayAudioSequencesByName.PlayNextTrack(musicParams);

        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(nextTrackButton, previousTrackButton, UIGlobals.toggleContainerPadding);
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(nextTrackButton, previousTrackButton);
        mallMusicActionButtons.Add(nextTrackButton);

        // now populate the object camera settings toggle group container
        CreateScreenSpaceUIElements.PopulateContentGroup(mallMusicActionsButtonGroup, mallMusicActionButtons);

        // resize the content within the scroll area to just past the last sub-element
        TransformScreenSpaceObject.ResizeParentContainerToFitLastChild(toggleSetContainer, mallMusicActionsButtonGroup, UIGlobals.toggleContainerPadding, "right");

        // set parent/child hierarchy
        toggleSetHorizontalScrollArea.transform.SetParent(audioMenu.transform);
        toggleSetContainer.transform.SetParent(toggleSetHorizontalScrollArea.transform);

        CreateScreenSpaceUIElements.SetContentGroupHierarchy(toggleSetContainer, mallMusicActionsButtonGroup);

        mallMusicMuteUnmuteButton.transform.SetParent(mallMusicActionsButtonGroup.transform);
        previousTrackButton.transform.SetParent(mallMusicActionsButtonGroup.transform);
        nextTrackButton.transform.SetParent(mallMusicActionsButtonGroup.transform);

        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(mallMusicMuteUnmuteButton, mallMusicActionsButtonGroup);
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(previousTrackButton, mallMusicMuteUnmuteButton);
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(nextTrackButton, previousTrackButton);

        ManageFPSControllers.DisableCursorLockOnActiveFPSController();

        return audioMenu;


    }
}
 
 
 
 
 