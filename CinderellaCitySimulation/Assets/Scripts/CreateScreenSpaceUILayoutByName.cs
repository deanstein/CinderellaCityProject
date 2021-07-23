using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (OverlayUIVisibilityGlobals.isOverlayMenuActive)
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
        GameObject HUDCanvas = CreateScreenSpaceUIElements.CreateMenuCanvas(UILauncher, "HUD");

        // create the time period indicator
        GameObject HUDTimePeriodIndicator = CreateScreenSpaceUIElements.CreateHUDTimePeriodIndicator(HUDCanvas, StringUtils.ConvertSceneNameToFriendlyName(UILauncher.scene.name));

        // create the game version indicator
        GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(HUDCanvas);

        // note that some scenes are under construction
        if (UILauncher.scene.name.Contains("AltFuture"))
        {
            CreateScreenSpaceUIElements.CreateHUDUnderConstructionLabel(HUDCanvas, "/// Under Construction ///");
        }

        return HUDCanvas;
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

        ///
        /// object visibility settings
        /// 

        // create the object visibility toggle group container
        GameObject objectVisibilityToggleGroup = CreateScreenSpaceUIElements.CreateToggleGroupModule(visibilityMenu, titleBarContainer, "Object Visibility");

        // first, create a list of toggles required for each of the object sets
        List<GameObject> toggles = new List<GameObject>();

        // object visibility toggles
        GameObject ceilingsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, objectVisibilityToggleGroup.transform.GetChild(0).gameObject, "Ceilings", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.ceilingObjectKeywords));
        toggles.Add(ceilingsVisibilityToggle);

        GameObject exteriorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, ceilingsVisibilityToggle, "Exterior Walls", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.exteriorWallObjectKeywords));
        toggles.Add(exteriorWallsVisibilityToggle);

        GameObject floorsVertVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, exteriorWallsVisibilityToggle, "Floors", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.exteriorWallObjectKeywords));
        toggles.Add(floorsVertVisibilityToggle);

        GameObject interiorDetailingVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, floorsVertVisibilityToggle, "Interior Detailing", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.interiorDetailingObjectKeywords));
        toggles.Add(interiorDetailingVisibilityToggle);

        GameObject interiorWallsVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorDetailingVisibilityToggle, "Interior Walls", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.interiorWallObjectKeywords));
        toggles.Add(interiorWallsVisibilityToggle);

        GameObject peopleVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, interiorWallsVisibilityToggle, "People", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.peopleObjectKeywords));
        toggles.Add(peopleVisibilityToggle);

        GameObject roofVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, peopleVisibilityToggle, "Roof", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.roofObjectKeywords));
        toggles.Add(roofVisibilityToggle);

        GameObject signageVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, roofVisibilityToggle, "Signage", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.signageObjectKeywords));
        toggles.Add(signageVisibilityToggle);

        GameObject vegetationVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, signageVisibilityToggle, "Vegetation", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.vegetationObjectKeywords));
        toggles.Add(vegetationVisibilityToggle);

        GameObject waterFeatureVisibilityToggle = CreateScreenSpaceUIElements.CreateVisibilityToggleModule(objectVisibilityToggleGroup, vegetationVisibilityToggle, "Water Features", ObjectVisibility.GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.waterFeatureObjectKeywords));
        toggles.Add(waterFeatureVisibilityToggle);

        // now populate the object visibility toggle group container
        CreateScreenSpaceUIElements.PopulateToggleGroup(objectVisibilityToggleGroup, toggles);

        return visibilityMenu;
    }
}
 
 
 
 
 