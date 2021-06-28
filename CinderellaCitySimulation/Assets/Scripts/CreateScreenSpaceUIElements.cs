using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]

// holds values other scripts need to access
public class UIGlobals
{
    // all UI sprites stored in a file live here
    public static string projectUIPath = "Assets/Resources/UI/";

    // is used to determine when to generate time-travel specific thumbnails
    public static bool isTimeTravelThumbnail;

    // keep track of the UI elements that get dynamically updated when Pause is invoked
    public static GameObject pauseMenuBackgroundImage;
    public static List<GameObject> timeTravelThumbnails = new List<GameObject>();

    // define the various camera textures
    // this is always written when an FPSController is disabled
    public static Texture2D outgoingFPSControllerCameraTexture = new Texture2D(Screen.width, Screen.height);
    // these are the various textures that are used in the UI
    public static Texture2D pauseMenuBackgroundCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController60s70sCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController80s90sCameraTexture = new Texture2D(Screen.width, Screen.height);

    // these are the HUD UI elements that can be hidden/revealed
    public static GameObject underConstructionLabelContainer;
    public static GameObject currentTimePeriodNotificationContainer;
    public static GameObject timePeriodNotificationContainer60s70s;
    public static GameObject timePeriodNotificationContainer80s90s;
    public static GameObject timePeriodNotificationContainerAltFuture;
    public static GameObject timePeriodNotificationContainerExperimental;
}

public class StringUtils
{
    // remove spaces, punctuation, and other characters from a string
    public static string CleanString(string messyString)
    {
        // remove spaces
        string cleanString = messyString.Replace(" ", "");

        // remove colons
        cleanString = cleanString.Replace(":", "");

        // remove dashed
        cleanString = cleanString.Replace("-", "");

        // remove the "19" if used in year syntax
        cleanString = cleanString.Replace("19", "");

        return cleanString;
    }

    // converts an array of friendly UI names into Scene names
    public static List<string> ConvertFriendlyNamesToSceneNames(List<string> friendlyNames)
    {
        List<string> convertedNames = new List<string>();

        foreach (string friendlyName in friendlyNames)
        {
            string convertedName = CleanString(friendlyName);
            convertedNames.Add(convertedName);
        }

        return convertedNames;
    }

    // gets the index of a friendly name given a scene name
    public static string ConvertSceneNameToFriendlyName(string sceneName)
    {
        // get the index of the scene we're in
        int sceneIndex = SceneGlobals.availableTimePeriodSceneNamesList.IndexOf(sceneName);

        if (sceneIndex != -1)
        {
            // now get the associated friendly name
            string friendlyName = SceneGlobals.availableTimePeriodFriendlyNames[sceneIndex];

            return friendlyName;
        }
        else
        {
            return "0000 Experimental";
        }
    }

    // return true if this string is found at all in the given array
    // TODO: move this to Utils?
    public static bool TestIfAnyListItemContainedInString(List<string> listOfStringsToSearchFor, string stringToSearchIn)
    {
        foreach (string searchForString in listOfStringsToSearchFor)
        {
            if (stringToSearchIn.Contains(searchForString))
            {
                return true;
            }
        }
        return false;
    }
}


public class CreateScreenSpaceUIElements : MonoBehaviour
{
    // this is a single stack of place/time thumbnails for aligning time labels to
    public static List<GameObject> placeThumbnailsForAlignment = new List<GameObject>();
    public static List<GameObject> timeLabelsForAlignment = new List<GameObject>();

    /// colors ///

    // text white
    public static Color32 typicalTextColor = new Color32(255, 255, 255, 255);

    // text grey
    public static Color32 subtleTextColor = new Color32(150, 150, 150, 255);

    // all button colors
    public static Color32 buttonColor = new Color32(20, 20, 20, 220);

    // all nav and container colors
    public static Color32 containerColor = new Color32(50, 50, 50, 100);

    // clear color
    public static Color32 clearColor = new Color32(255, 255, 255, 0);

    /// sizes ///

    // fonts and text sizes (pixels)
    public static string labelFont = "AvenirNextLTPro-Demi";

    public static int menuTitleLabelSize = 30;
    public static int HUDTimePeriodLabelSize = 40;
    public static int versionLabelSize = 15;

    public static int placeLabelSize = 50;
    public static int timeLabelSize = 50;

    public static int menuTextButtonLabelSize = 35;

    // the proportion of the text height that the descender is estimated to be
    public static float textDescenderProportion = 0.12f;

    // button sizes (ratio relative to screen size)
    public static float menuButtonScreenWidthRatio = 0.15f;
    public static float menuButtonTopBottomPaddingScreenHeightRatio = 0.01f;

    /// spacings ///

    public static float projectLogoHeightScreenHeightRatio = 0.15f;
    public static float projectLogoLeftMarginScreenWidthRatio = 0.1f;
    public static float projectLogoTopMarginScreenHeightRatio = 0.1f;
    public static float projectLogoContainerTopPaddingScreenHeightRatio = 0.03f;
    public static float projectLogoContainerBottomPaddingScreenHeightRatio = 0.03f;
    public static float projectLogoContainerRightPaddingScreenWidthRatio = 0.03f;

    public static float menuTitleTopMarginScreenHeightRatio = -0.02f;
    public static float menuTitleBottomMarginScreenHeightRatio = 0.02f;
    public static float menuTitleLeftMarginScreenWidthRatio = -0.02f;

    public static float logoHeaderBottomMarginScreenHeightRatio = 0.05f;

    public static float placeLabelTopMarginScreenHeightRatio = -0.02f;
    public static float placeLabelBottomMarginScreenHeightRatio = 0.01f;

    public static float timeLabelLeftMarginScreenWidthRatio = -0.02f;

    public static float timePlaceThumbnailHeightScreenHeightRatio = 0.2f;
    public static float timePlaceThumbnailBottomMarginScreenHeightRatio = 0.01f;

    public static float navContainerTopMarginScreenHeightRatio = 0.01f;
    public static float navContainerLeftMarginScreenWidthRatio = 0.1f;
    public static float navContainerBottomMarginScreenHeightRatio = 0.1f;

    public static float thumbnailStackBottomMarginScreenHeightRatio = 0.02f;

    public static float textButtonBottomMarginScreenHeightRatio = 0.015f;
    public static float textButtonLeftMarginScreenWidthRatio = 0.01f;

    public static float HUDBottomBarTopMarginScreenHeightRatio = 0.9f;
    public static float HUDBottonBarHeightScreenHeightRatio = 0.08f;
    public static float HUDTimePeriodLabelLeftMarginScreenWidthRatio = 0.85f;
    public static float HUDBottomBarBottomMarginScreenHeightRatio = 0.03f;

    public static float versionLabelLeftMarginScreenWidthRatio = 0.008f;
    public static float versionLabelTopMarginScreenHeightRatio = 0.97f;

    // create an empty list of GameObjects that need to be dynamically added to their parent
    // this list will be emptied and populated in UI constructors that make nested sets of objects
    public static List<GameObject> orphanedThumbnailStacks = new List<GameObject>();

    // assigns a list of orphaned objects to a parent
    public static void AssignOrphanedObjectListToParent(List<GameObject> orphanObjects, GameObject parent)
    {
        foreach (GameObject orphan in orphanObjects)
        {
            orphan.transform.SetParent(parent.transform);
        }
    }

    // write a camera's view to a global texture, depending on which FPSController this is
    public static void AssignCameraTextureToVariableByName()
    {

        // use the name of the active FPSController to determine which variable to write the texture to
        switch (ManageFPSControllers.FPSControllerGlobals.activeFPSController.name)
        {
            case string FPSControllerName when FPSControllerName.Contains("60s70s"):
                UIGlobals.FPSController60s70sCameraTexture = UIGlobals.outgoingFPSControllerCameraTexture;
                return;
            case string FPSControllerName when FPSControllerName.Contains("80s90s"):
                UIGlobals.FPSController80s90sCameraTexture = UIGlobals.outgoingFPSControllerCameraTexture;
                return;
        }
    }

    // determine which camera texture to read to or write from, depending on the requesting UI object's name
    public static Texture2D AssociateCameraTextureByName(string name)
    {
        switch (name)
        {
            case string imageHostName when imageHostName.Contains("PauseMenu"):
                return UIGlobals.outgoingFPSControllerCameraTexture;
            case string imageHostName when imageHostName.Contains("60s70s"):
                return UIGlobals.FPSController60s70sCameraTexture;
            case string imageHostName when imageHostName.Contains("80s90s"):
                return UIGlobals.FPSController80s90sCameraTexture;
            default:
                return null;
        }
    }

    // determine which camera texture to use for the pause menu background image
    public static Texture2D GetPauseMenuBackgroundImage(string referringSceneName)
    {
        switch (referringSceneName)
        {
            case string name when name.Contains("60s70s"):
                return UIGlobals.FPSController60s70sCameraTexture;
            case string name when name.Contains("80s90s"):
                return UIGlobals.FPSController80s90sCameraTexture;
            default:
                return UIGlobals.outgoingFPSControllerCameraTexture;
        }
    }

    // get the time travel notification container for each scene
    public static GameObject GetTimePeriodNotificationContainerByName(string sceneName)
    {
        switch (sceneName)
        {
            case string scenePartialName when scenePartialName.Contains("60s70s"):
                return UIGlobals.timePeriodNotificationContainer60s70s;
            case string scenePartialName when scenePartialName.Contains("80s90s"):
                return UIGlobals.timePeriodNotificationContainer80s90s;
            case string scenePartialName when scenePartialName.Contains("AltFuture"):
                return UIGlobals.timePeriodNotificationContainerAltFuture;
            case string scenePartialName when scenePartialName.Contains("Experimental"):
                return UIGlobals.timePeriodNotificationContainerExperimental;
            default:
                return null;
        }
    }

    // set the time travel notification container for each scene (invoked when the UI is built)
    public static GameObject SetTimePeriodNotificationContainerByName(GameObject containerToStore, string sceneName)
    {
        switch (sceneName)
        {
            case string scenePartialName when scenePartialName.Contains("60s70s"):
                UIGlobals.timePeriodNotificationContainer60s70s = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("80s90s"):
                UIGlobals.timePeriodNotificationContainer80s90s = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("AltFuture"):
                UIGlobals.timePeriodNotificationContainerAltFuture = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("Experimental"):
                UIGlobals.timePeriodNotificationContainerExperimental = containerToStore;
                return containerToStore;
            default:
                return null;
        }
    }

    // takes the active FPSController, adds the render camera to image script, and renders the camera
    public static void CaptureActiveFPSControllerCamera()
    {
        // get the camera from the active FPSController
        Camera FPSCamera = ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.GetChild(0).gameObject.GetComponent<Camera>();

        // attach and kick off the render script
        RenderCameraToImageSelfDestruct renderCameraScript = FPSCamera.gameObject.AddComponent<RenderCameraToImageSelfDestruct>();
        // set the override to true, so this will run once in game mode
        renderCameraScript.runInGameMode = true;
        // render the camera
        FPSCamera.Render();
    }

    // captures FPSCharacter cameras, then refreshes their global textures, so the pause menu can be updated
    public static void CaptureDisabledSceneFPSCameras()
    {
        // define the time periods that are considered disabled at this time
        List<string> disabledTimePeriodSceneNames = ManageScenes.GetDisabledTimePeriodSceneNames(SceneGlobals.referringSceneName);
        //Utils.DebugUtils.DebugLog("Disabled time period scene names: " + disabledTimePeriodSceneNames);

        // for each inactive time period, enable it without scripts, take a screenshot, and update textures
        foreach (string disabledSceneName in disabledTimePeriodSceneNames)
        {
            // toggle the scene, but ignore scriptHost objects - so no scripts or behaviors enable
            ToggleSceneAndUI.ToggleSceneObjectsOnExceptScriptHosts(disabledSceneName);

            // adjust the FPSController transform to account for hoisting
            Transform adjustedFPSControllerTransform = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform;
            adjustedFPSControllerTransform.position = HoistSceneObjects.AdjustPositionForHoistBetweenScenes(adjustedFPSControllerTransform.position, SceneGlobals.referringSceneName, disabledSceneName);

            // relocate the disabled FPSController to the correct position for a screenshot
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(adjustedFPSControllerTransform);

            // inherit the sun settings of the disabled scene, in order to generate an accurate screenshot
            Light sunToInherit = ManageSunSettings.GetSunBySceneName(disabledSceneName);
            Light sunToOverwrite = ManageSunSettings.GetSunBySceneName(SceneManager.GetActiveScene().name);
            ManageSunSettings.InheritSunSettingsBySceneName(disabledSceneName, sunToInherit, sunToOverwrite);

            // briefly set this as the active scene to update the skybox settings
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(disabledSceneName));

            // capture the current camera
            // this will also update the global variable with the texture, depending on the scene name
            CaptureActiveFPSControllerCamera();

            // reverse the position adjustment after camera is captured
            adjustedFPSControllerTransform.position = HoistSceneObjects.AdjustPositionForHoistBetweenScenes(adjustedFPSControllerTransform.position, disabledSceneName, SceneGlobals.referringSceneName);

            // relocate the disabled FPSController to the correct position for a screenshot
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(adjustedFPSControllerTransform);

            // turn everything off again
            ToggleSceneAndUI.ToggleSceneObjectsOff(disabledSceneName);

            // return the script hosts to their on state
            ToggleSceneAndUI.ToggleScriptHostObjectListOn();

            // set Pause as active again
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneGlobals.pauseMenu));
        }
    }

    // rebuilds the sprite on this gameObject's image component
    public static void RefreshObjectImageSprite(GameObject imageObject)
    {
        if (imageObject)
        {
            Texture2D thumbnailTexture = AssociateCameraTextureByName(imageObject.name);
            Sprite updatedThumbnailSprite = Sprite.Create(thumbnailTexture, new Rect(0.0f, 0.0f, thumbnailTexture.width, thumbnailTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            imageObject.GetComponent<Image>().sprite = updatedThumbnailSprite;
        }
    }

    // forces a refresh of the time period selection thumbnails, in the Pause Menu
    // this rebuilds a sprite based on a texture, and sets it as the thumbnail image object's sprite
    public static void RefreshThumbnailSprites()
    {
        foreach (GameObject thumbnail in UIGlobals.timeTravelThumbnails)
        {
            RefreshObjectImageSprite(thumbnail);
        }
    }

    // define what clicking the buttons does, based on the name of the button
    public static void TaskOnClickByName(string buttonName)
    {
        switch (buttonName)
        {
            // handle buttons that lead to menus and exit
            case string name when name.Contains("Resume"):
                ManageFPSControllers.FPSControllerGlobals.isTimeTraveling = false;
                ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.referringSceneName);
                return;
            case string name when name.Contains("MainMenu"):
                ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");
                return;
            case string name when name.Contains("Quit"):
                Application.Quit();
                return;

            // handle buttons that request a time and place
            // check if the button name contains an available time period (scene name)
            case string name when StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, name):
                // buttons need to be named with hyphens delimiting important info
                string[] nameSplitByDelimiter = name.Split('-');
                // the place needs to be 1st
                string playerPosition = nameSplitByDelimiter[0];
                // the time period needs to be 2nd
                string timePeriod = nameSplitByDelimiter[1];

                // if the button name indicates a time traveler, don't specify an FPSController location (uses the current FPS location)
                if (name.Contains("TimeTravel"))
                {
                    // switch to the correct scene based on the time period and location in the button name
                    ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToFPSController(SceneManager.GetActiveScene().name, timePeriod, ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform);
                }
                // otherwise, this request includes a specific location in its name, so relocate the player to that camera
                else
                {
                    // switch to the correct scene based on the time period and location in the button name
                    ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToCamera(SceneManager.GetActiveScene().name, timePeriod, playerPosition);
                }
                return;
            default:
                return;
        }
    }

    public static GameObject CreateMenuCanvas(GameObject parent, string name)
    {
        // create the menu object, and make sure its parent is the scene container object
        GameObject menu = new GameObject(name);
        menu.AddComponent<Canvas>();
        menu.transform.SetParent(parent.transform);

        // create the menu's canvas
        Canvas menuCanvas = menu.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menu.AddComponent<CanvasScaler>();
        menu.AddComponent<GraphicRaycaster>();

        return menu;
    }

    public static GameObject CreateFullScreenImageFromCameraTexture(GameObject parent, bool refreshOnEnable)
    {
        // create the background object
        GameObject fullScreenBackground = new GameObject(parent.name + "BackgroundImage");
        fullScreenBackground.AddComponent<Image>();

        // create and configure the image
        Image fullScreenBackgroundImage = fullScreenBackground.GetComponent<Image>();
        fullScreenBackgroundImage.transform.SetParent(parent.transform);
        fullScreenBackgroundImage.preserveAspect = true;
        fullScreenBackgroundImage.SetNativeSize();

        // determine the texture we should use based on the object's name
        Texture2D backgroundTexture = GetPauseMenuBackgroundImage(SceneGlobals.referringSceneName);

        // set the sprite to the given texture
        fullScreenBackgroundImage.sprite = Sprite.Create(backgroundTexture, new Rect(0.0f, 0.0f, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

        // reset the scale before centering and full-screening
        fullScreenBackgroundImage.rectTransform.localScale = new Vector3(1, 1, 1);

        // center and full-screen the image
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(fullScreenBackgroundImage.gameObject);
        TransformScreenSpaceObject.ScaleImageToFillScreen(fullScreenBackgroundImage);

        // if enabled, add the script to refresh the image sprite when the object is re-enabled
        // this allows for using an image that's generated/updated during gameplay
        if (refreshOnEnable)
        {
            RefreshImageSprite refreshImageScript = fullScreenBackground.AddComponent<RefreshImageSprite>();
            refreshImageScript.refreshOnEnable = true;

            // set this image to the global image that needs updating
            UIGlobals.pauseMenuBackgroundImage = fullScreenBackground;
        }

        return fullScreenBackground;
    }

    public static GameObject CreateFullScreenImageSlideshow(GameObject parent, string[] imageSequence)
    {
        // create the background object
        GameObject fullScreenBackgroundSlideShow = new GameObject("BackgroundSlideShow");
        fullScreenBackgroundSlideShow.AddComponent<Image>();

        // set the image
        Image fullScreenBackgroundSlideShowImage = fullScreenBackgroundSlideShow.GetComponent<Image>();
        fullScreenBackgroundSlideShowImage.transform.SetParent(parent.transform);

        // this script will sequence, transform, and animate the background images as required
        AnimateScreenSpaceObject AnimateScreenSpaceObjectScript = fullScreenBackgroundSlideShow.AddComponent<AnimateScreenSpaceObject>();
        AnimateScreenSpaceObjectScript.mainMenuBackgroundSlideShowSequence = imageSequence;

        return fullScreenBackgroundSlideShow;
    }

    public static GameObject CreateLogoHeader(GameObject parent)
    {
        // create the container object
        GameObject logoContainer = new GameObject("LogoHeader");
        logoContainer.AddComponent<CanvasRenderer>();
        Image logoContainerColor = logoContainer.AddComponent<Image>();
        logoContainerColor.color = containerColor;

        // create the logo object
        GameObject logo = new GameObject("ProjectLogo");
        logo.AddComponent<Image>();

        // set the logo image
        Image logoImage = logo.GetComponent<Image>();
        logoImage.sprite = (Sprite)Resources.Load("UI/CCP logo", typeof(Sprite));
        logoImage.preserveAspect = true;
        logoImage.SetNativeSize();

        // adjust the scale of the logo
        TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(logo, projectLogoHeightScreenHeightRatio);

        // position the logo
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(logo, projectLogoTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromCameraLeft(logo, projectLogoLeftMarginScreenWidthRatio);

        // position the logo container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(logoContainer, logo, projectLogoContainerTopPaddingScreenHeightRatio);

        // resize the logo container
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(logoContainer, logo, projectLogoContainerBottomPaddingScreenHeightRatio);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(logoContainer, logo, projectLogoContainerRightPaddingScreenWidthRatio);

        // set the parent/child hierarchy
        logoContainer.transform.SetParent(parent.transform);
        logo.transform.SetParent(logoContainer.transform);

        return logoContainer;
    }

    public static GameObject CreateMenuTitleBar(GameObject parent, GameObject topAlignmentObject, string titleString)
    {
        // create the title container object
        GameObject titleContainer = new GameObject("TitleContainer");
        titleContainer.AddComponent<CanvasRenderer>();
        Image logoContainerColor = titleContainer.AddComponent<Image>();
        logoContainerColor.color = containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(titleContainer, topAlignmentObject, logoHeaderBottomMarginScreenHeightRatio);

        // resize the title container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(titleContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(titleContainer, navContainerLeftMarginScreenWidthRatio);

        // add the title text
        GameObject titleLabel = new GameObject("TitleLabel");
        Text introMessageLabelText = titleLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(labelFont);
        introMessageLabelText.text = titleString;
        introMessageLabelText.fontSize = menuTitleLabelSize;
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(titleLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(titleLabel, titleContainer, menuTitleTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(titleLabel, titleContainer, menuTitleLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(titleContainer, titleLabel, menuTitleBottomMarginScreenHeightRatio);

        // set parent/child hierarchy
        titleContainer.transform.SetParent(parent.transform);
        titleLabel.transform.SetParent(titleContainer.transform);

        return titleContainer;
    }

    public static GameObject CreateHUDTimePeriodIndicator(GameObject parent, string titleString)
    {
        // create the title container object
        GameObject timePeriodContainer = new GameObject("TimePeriodContainer");
        timePeriodContainer.AddComponent<CanvasRenderer>();
        Image timePeriodContainerColor = timePeriodContainer.AddComponent<Image>();
        timePeriodContainerColor.color = containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(timePeriodContainer, HUDBottomBarTopMarginScreenHeightRatio);

        // resize the title container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(timePeriodContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(timePeriodContainer, HUDTimePeriodLabelLeftMarginScreenWidthRatio);

        // add the title text
        GameObject timePeriodLabel = new GameObject("TimePeriodLabel");
        Text timePeriodLabelText = timePeriodLabel.AddComponent<Text>();
        timePeriodLabelText.font = (Font)Resources.Load(labelFont);
        timePeriodLabelText.text = titleString;
        timePeriodLabelText.fontSize = HUDTimePeriodLabelSize;
        timePeriodLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodLabel);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(timePeriodContainer, HUDBottomBarBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel, timePeriodContainer, menuTitleLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(timePeriodContainer, HUDBottomBarBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionTextAtHorizontalCenterlineOfNeighbor(timePeriodLabel, timePeriodContainer);

        // set parent/child hierarchy
        timePeriodContainer.transform.SetParent(parent.transform);
        timePeriodLabel.transform.SetParent(timePeriodContainer.transform);

        return timePeriodContainer;
    }

    // displays the time period in the center of the screen
    // only used when time traveling, to remind the user what time period they've switched to
    public static GameObject CreateHUDTimePeriodNotification(GameObject parent, string notificationText)
    {
        // create the label container
        GameObject timePeriodNotificationContainer = new GameObject("TimePeriodNotificationContainer" + parent.scene.name);
        timePeriodNotificationContainer.AddComponent<CanvasRenderer>();

        SetTimePeriodNotificationContainerByName(timePeriodNotificationContainer, parent.scene.name);

        // image is needed to create a rect transform
        Image timePeriodContainerColor = timePeriodNotificationContainer.AddComponent<Image>();
        timePeriodContainerColor.color = containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodNotificationContainer);

        // add the title text
        GameObject underConstructionLabel = new GameObject("TimePeriodNotificationLabel");
        Text underConstructionLabelText = underConstructionLabel.AddComponent<Text>();
        underConstructionLabelText.font = (Font)Resources.Load(labelFont);
        underConstructionLabelText.text = notificationText;
        underConstructionLabelText.fontSize = menuTitleLabelSize;
        underConstructionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        Vector2 textSize = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(underConstructionLabelText);

        RectTransform rt = timePeriodContainerColor.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(textSize.x, textSize.y);

        Vector2 newSize = TransformScreenSpaceObject.ResizeObjectFromCenterByMargin(timePeriodNotificationContainer, 0.01f, 0.01f);

        // position the title text
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(underConstructionLabel);

        // set parent/child hierarchy
        timePeriodNotificationContainer.transform.SetParent(parent.transform);
        underConstructionLabel.transform.SetParent(timePeriodNotificationContainer.transform);

        // disable initially
        timePeriodNotificationContainer.SetActive(false);

        return timePeriodNotificationContainer;
    }

    public static GameObject CreateHUDUnderConstructionLabel(GameObject parent, string message)
    {
        // create the label container
        UIGlobals.underConstructionLabelContainer = new GameObject("UnderConstructionContainer");
        UIGlobals.underConstructionLabelContainer.AddComponent<CanvasRenderer>();
        // image is needed to create a rect transform
        Image timePeriodContainerColor = UIGlobals.underConstructionLabelContainer.AddComponent<Image>();
        timePeriodContainerColor.color = clearColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(UIGlobals.underConstructionLabelContainer);

        // add the title text
        GameObject underConstructionLabel = new GameObject("UnderConstructionLabel");
        Text underConstructionLabelText = underConstructionLabel.AddComponent<Text>();
        underConstructionLabelText.font = (Font)Resources.Load(labelFont);
        underConstructionLabelText.text = message;
        underConstructionLabelText.fontSize = menuTitleLabelSize;
        underConstructionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(underConstructionLabelText);

        // position the title text
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(underConstructionLabel);

        // set parent/child hierarchy
        UIGlobals.underConstructionLabelContainer.transform.SetParent(parent.transform);
        underConstructionLabel.transform.SetParent(UIGlobals.underConstructionLabelContainer.transform);

        return UIGlobals.underConstructionLabelContainer;
    }

    public static GameObject CreateVersionLabel(GameObject parent)
    {
        string version = "v" + Application.version;

        // create the version container
        GameObject versionLabelContainer = new GameObject("VersionLabelContainer");
        versionLabelContainer.AddComponent<CanvasRenderer>();
        // image is needed to create a rect transform
        Image versionLabelContainerColor = versionLabelContainer.AddComponent<Image>();
        versionLabelContainerColor.color = clearColor;

        // position the version container
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(versionLabelContainer);

        // add the version text
        GameObject versionLabel = new GameObject("VersionLabel");
        Text versionLabelText = versionLabel.AddComponent<Text>();
        versionLabelText.color = subtleTextColor;
        versionLabelText.font = (Font)Resources.Load(labelFont);
        versionLabelText.text = version;
        versionLabelText.fontSize = versionLabelSize;
        versionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(versionLabelText);

        // position the version text
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(versionLabel);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromCameraLeft(versionLabel, versionLabelLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(versionLabel, versionLabelTopMarginScreenHeightRatio);

        // set parent/child hierarchy
        versionLabelContainer.transform.SetParent(parent.transform);
        versionLabel.transform.SetParent(versionLabelContainer.transform);

        return versionLabelContainer;
    }

    public static GameObject CreateCentralNavContainer(GameObject parent, GameObject topAlignmentObject)
    {
        // create the central nav container
        GameObject centralNavContainer = new GameObject("CentralNavContainer");
        centralNavContainer.AddComponent<CanvasRenderer>();

        // set the color of the central nav container
        Image centralNavContainerColor = centralNavContainer.AddComponent<Image>();
        centralNavContainerColor.color = containerColor;

        // position the central nav container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(centralNavContainer, topAlignmentObject, navContainerTopMarginScreenHeightRatio);

        // resize the central nav container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(centralNavContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(centralNavContainer, navContainerLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(centralNavContainer, navContainerBottomMarginScreenHeightRatio);

        return centralNavContainer;
    }

    public static GameObject CreateTextButton(string text, GameObject parent, Color32 color)
    {
        // create the text label
        GameObject buttonTextObject = new GameObject(StringUtils.CleanString(text) + "ButtonText");
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.font = (Font)Resources.Load(labelFont);
        buttonText.text = text;
        buttonText.fontSize = menuTextButtonLabelSize;
        buttonText.alignment = TextAnchor.MiddleCenter;

        Vector2 textSize = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(buttonText);

        // create the button
        GameObject buttonContainer = new GameObject(StringUtils.CleanString(text) + "Button");
        buttonContainer.AddComponent<CanvasRenderer>();
        buttonContainer.AddComponent<RectTransform>();

        // resize the button background to encapsulate the text, plus padding
        RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(Mathf.Round((menuButtonScreenWidthRatio * Screen.width)), Mathf.Round(textSize.y + (2 * (menuButtonTopBottomPaddingScreenHeightRatio * Screen.height))));

        // set the color of the button
        Image buttonContainerColor = buttonContainer.AddComponent<Image>();
        buttonContainerColor.color = color;

        // configure the button
        Button button = buttonContainer.AddComponent<Button>();
        buttonContainer.GetComponent<Button>().onClick.AddListener(() => { TaskOnClickByName(buttonContainer.name); }); ;

        // set the parent/child hierarchy
        buttonContainer.transform.SetParent(parent.transform);
        buttonTextObject.transform.SetParent(buttonContainer.transform);

        // move the button
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(buttonContainer);

        return buttonContainer;
    }

    // create the time label stack
    public static GameObject CreateTimeLabelStack(GameObject parent, GameObject leftAlignmentObject, string[] availableTimePeriodFriendlyNames)
    {
        // create a container object
        GameObject timeLabelStackContainer = new GameObject("TimePeriodLabelStack");

        foreach (string timePeriod in availableTimePeriodFriendlyNames)
        {
            // create the time periods labels
            GameObject timePeriodLabel = new GameObject(timePeriod + "Label");

            Text timePeriodLabelText = timePeriodLabel.AddComponent<Text>();
            timePeriodLabelText.font = (Font)Resources.Load(labelFont);
            timePeriodLabelText.text = timePeriod;
            timePeriodLabelText.fontSize = timeLabelSize;
            timePeriodLabelText.alignment = TextAnchor.MiddleCenter;

            // get the text's dimensions to match only the space it needs, before any transforms
            TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabelText);

            // position the time period label
            // note this only positions it horizontally
            // vertical positioning happens later, after the thumbnails are built
            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodLabel);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel, leftAlignmentObject, timeLabelLeftMarginScreenWidthRatio);

            // add this label to the list of labels for alignment
            timeLabelsForAlignment.Add(timePeriodLabel);

            // set the parent/child hierarchy
            timePeriodLabel.transform.SetParent(timeLabelStackContainer.transform);
        }

        return timeLabelStackContainer;
    }

    // create the stacked thumbnails for a place, across time periods
    public static GameObject CreatePlaceTimeThumbnailStack(GameObject parent, GameObject topAlignmentObject, GameObject leftAlignmentObject, string placeName, string[] timePeriodNames)
    {
        // clear the list of thumbnail objects from the previous stack created
        // this list will be used to set parents and as alignment guides for other objects
        placeThumbnailsForAlignment.Clear();

        // we'll need to generate the thumbnails differently if this stack is intended for "time traveling"
        // NOTE: this requires a match with the user-facing thumbnail stack title UI

        // set the flag if generating a time travel thumbnail
        if (placeName.Contains("Time Travel"))
        {
            UIGlobals.isTimeTravelThumbnail = true;
        }
        else
        {
            UIGlobals.isTimeTravelThumbnail = false;
        }

        // make an object to hold the thumbnails
        GameObject thumbnailStack = new GameObject(StringUtils.CleanString(placeName) + "ThumbnailStack");

        // location text
        GameObject placeLabel = new GameObject(StringUtils.CleanString(placeName) + "Label");
        Text placeLabelText = placeLabel.AddComponent<Text>();
        placeLabelText.font = (Font)Resources.Load(labelFont);
        placeLabelText.text = placeName;
        placeLabelText.fontSize = placeLabelSize;
        placeLabelText.alignment = TextAnchor.MiddleCenter;

        // adjust the text's rectTransform to ensure objects aligning to it fit closely
        RectTransform placeLabelRectTransform = placeLabel.GetComponent<RectTransform>();
        Vector2 sizeVector = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(placeLabelText);
        placeLabelRectTransform.sizeDelta = sizeVector;
        placeLabelText.alignByGeometry = true;

        // position the location text
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(placeLabel, topAlignmentObject, placeLabelTopMarginScreenHeightRatio);

        // for each image name provided, make a new thumbnail button
        for (var i = 0; i < timePeriodNames.Length; i++)
        {
            // combine the place name and time period strings
            string combinedPlaceTimeNameSpacelessDashed = StringUtils.CleanString(placeName) + "-" + SceneGlobals.availableTimePeriodSceneNamesList[i];

            // create the button
            GameObject timePeriodButton = new GameObject(combinedPlaceTimeNameSpacelessDashed + "-Button");
            timePeriodButton.AddComponent<Image>();

            // set the image
            // note this requires a valid image in the Resources folder path below, with a file name that matches combinedPlaceTimeNameSpaceless
            Image timePeriodButtonImage = timePeriodButton.GetComponent<Image>();


            // need to set the sprite differently depending on whether this is a time travel thumbnail, or standard time/place thumbnail

            // time travel sprites need to get a public texture generated by the FPSCameras
            if (UIGlobals.isTimeTravelThumbnail)
            {
                Texture2D timeTravelCameraTexture = AssociateCameraTextureByName(timePeriodButton.name);

                timePeriodButtonImage.sprite = Sprite.Create(timeTravelCameraTexture, new Rect(0.0f, 0.0f, timeTravelCameraTexture.width, timeTravelCameraTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

                // also attach the script to force update the sprite when the Pause Menu is called
                timePeriodButton.AddComponent<RefreshImageSprite>();

                // add this to the global list of time travel thumbnails, so we can update it later
                UIGlobals.timeTravelThumbnails.Add(timePeriodButton);
            }
            // otherwise, standard time periods look for their sprite in the file system
            else
            {
                timePeriodButtonImage.sprite = (Sprite)Resources.Load("UI/Camera-Thumbnail-" + combinedPlaceTimeNameSpacelessDashed, typeof(Sprite));
            }

            timePeriodButtonImage.preserveAspect = true;
            timePeriodButtonImage.SetNativeSize();

            // scale the button to be a proportion of the camera height
            TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(timePeriodButton, 0.2f);

            // position the button
            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodButton);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(timePeriodButton, leftAlignmentObject, 0.02f);

            // if this is the first item, position it below the place label
            if (i == 0)
            {
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, placeLabel, placeLabelBottomMarginScreenHeightRatio);
            }
            // otherwise, position it below the previous thumbnail
            else
            {
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, placeThumbnailsForAlignment[i - 1], timePlaceThumbnailBottomMarginScreenHeightRatio);
            }

            // add this new thumbnail button to the list for tracking
            placeThumbnailsForAlignment.Add(timePeriodButton);

            // add the button property
            timePeriodButton.AddComponent<Button>();
            timePeriodButton.GetComponent<Button>().onClick.AddListener(() => { TaskOnClickByName(timePeriodButton.name); }); ;

        // set the parent/child hierarchy
        timePeriodButton.transform.SetParent(thumbnailStack.transform);
        }

        // position the place label centered at the first thumbnail
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(placeLabel, placeThumbnailsForAlignment[0]);

        // set the parent/child hierarchy
        placeLabel.transform.SetParent(thumbnailStack.transform);

        // due to a problem with parent transforms, 
        // we can't set the parent/child hierarchy of the entire stack yet
        // instead, add this stack to the orphaned object list, and we'll assign its parent later
        orphanedThumbnailStacks.Add(thumbnailStack);

        return thumbnailStack;
    }

    // create the main menu central navigation
    public static GameObject CreateMainMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        // clear lists
        orphanedThumbnailStacks.Clear();
        timeLabelsForAlignment.Clear();

        // create the central nav container
        GameObject centralNavContainer = CreateCentralNavContainer(parent, topNeighbor);

        // create the time label stack
        GameObject timeLabelStack = CreateTimeLabelStack(centralNavContainer, centralNavContainer, SceneGlobals.availableTimePeriodFriendlyNames);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create each place thumbnail stack, and their associated place labels
        GameObject blueMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, timeLabelForAlignment, "Blue Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        GameObject roseMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, blueMallThumbnailStack.transform.GetChild(0).gameObject, "Rose Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        GameObject goldMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, roseMallThumbnailStack.transform.GetChild(0).gameObject, "Gold Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        // resize the container to align with the last thumbnail in the column
        int thumbnailCount = blueMallThumbnailStack.transform.childCount - 1; // exclude the label
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(centralNavContainer, blueMallThumbnailStack.transform.GetChild(thumbnailCount - 1).gameObject, thumbnailStackBottomMarginScreenHeightRatio);

        // position the time labels to align horizontally with the place thumbnails
        TransformScreenSpaceObject.PositionMultiObjectsAtHorizontalCenterlinesOfNeighbors(timeLabelsForAlignment, placeThumbnailsForAlignment);

        // set the parent/child hierarchy
        centralNavContainer.transform.SetParent(parent.transform);
        timeLabelStack.transform.SetParent(centralNavContainer.transform);
        AssignOrphanedObjectListToParent(orphanedThumbnailStacks, centralNavContainer);

        return centralNavContainer;
    }

    public static GameObject CreatePauseMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        // clear lists
        orphanedThumbnailStacks.Clear();
        timeLabelsForAlignment.Clear();

        // create the central nav container
        GameObject centralNavContainer = CreateCentralNavContainer(parent, topNeighbor);

        // create the time label stack
        GameObject timeLabelStack = CreateTimeLabelStack(centralNavContainer, centralNavContainer, SceneGlobals.availableTimePeriodFriendlyNames);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create the time travel thumbnail container
        GameObject timeTravelThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, timeLabelForAlignment, "Time Travel:", SceneGlobals.availableTimePeriodFriendlyNames);

        // resize the container to align with the last thumbnail in the column
        int thumbnailCount = timeTravelThumbnailStack.transform.childCount - 1; // exclude the label
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(centralNavContainer, timeTravelThumbnailStack.transform.GetChild(thumbnailCount - 1).gameObject, thumbnailStackBottomMarginScreenHeightRatio);

        // position the time labels to align horizontally with the place thumbnails
        TransformScreenSpaceObject.PositionMultiObjectsAtHorizontalCenterlinesOfNeighbors(timeLabelsForAlignment, placeThumbnailsForAlignment);

        /// buttons ///

        // define a gameObject to align the buttons to
        GameObject buttonAlignmentObject = timeTravelThumbnailStack.transform.GetChild(0).gameObject;

        // create the resume button
        GameObject resumeButton = CreateTextButton("Resume", centralNavContainer, buttonColor);
        // align and position the main menu button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(resumeButton, buttonAlignmentObject, 0.0f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(resumeButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // create the main menu button
        GameObject mainMenuButton = CreateTextButton("Main Menu", centralNavContainer, buttonColor);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(mainMenuButton, resumeButton, textButtonBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(mainMenuButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // exit button
        GameObject exitButton = CreateTextButton("Quit", centralNavContainer, buttonColor);
        // align and position the exit button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(exitButton, mainMenuButton, textButtonBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(exitButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // set the parent/child hierarchy
        centralNavContainer.transform.SetParent(parent.transform);
        timeLabelStack.transform.SetParent(centralNavContainer.transform);
        AssignOrphanedObjectListToParent(orphanedThumbnailStacks, centralNavContainer);

        return centralNavContainer;
    }
}




