using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

[RequireComponent(typeof(UnityEngine.UI.Button))]

// holds values other scripts need to access
public class UIGlobals
{
    // all UI sprites stored in a file live here
    public static string projectUIPath = "Assets/Resources/UI/";

    // is used to determine when to generate time-travel specific thumbnails
    public static bool isTimeTravelThumbnail;

    // define the various camera textures
    // these are written to from FPSController cameras during game play
    public static Texture2D outgoingFPSControllerCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController60s70sCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController80s90sCameraTexture = new Texture2D(Screen.width, Screen.height);
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

    // return true if this string is found at all in the given array
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

    // all button colors
    public static Color32 buttonColor = new Color32(20, 20, 20, 220);

    // all nav and container colors
    public static Color32 containerColor = new Color32(50, 50, 50, 100);

    /// sizes ///

    // fonts and text sizes (pixels)
    public static string labelFont = "AvenirNextLTPro-Demi";

    public static int menuTitleLabelSize = 30;

    public static int placeLabelSize = 50;
    public static int timeLabelSize = 50;

    public static int menuTextButtonLabelSize = 35;

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

    public static float textButtonBottomMarginScreenHeightRatio = 0.005f;
    public static float textButtonLeftMarginScreenWidthRatio = 0.01f;

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

    // determine which camera texture to read to or write from, depending on the requesting UI object's name
    public static Texture2D AssociateCameraTextureByName(GameObject imageHostObject)
    {
        switch (imageHostObject.name)
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

    // define what clicking the buttons does, based on the name of the button
    public static void TaskOnClickByName(string buttonName)
    {
        switch (buttonName)
        {
            // handle buttons that lead to menus and exit
            case string name when name.Contains("Resume"):
                ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.referringScene);
                return;
            case string name when name.Contains("MainMenu"):
                ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");
                return;
            case string name when name.Contains("Exit"):
                Application.Quit();
                return;

            // handle buttons that request a time and place
            // check if the button name contains an available time period (scene name)
            case string name when StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, name):
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
                    ToggleVisibilityByScene.ToggleFromSceneToSceneRelocatePlayerToFPSController(SceneManager.GetActiveScene().name, timePeriod, ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform);
                }
                // otherwise, this request includes a specific location in its name, so relocate the player there
                else
                {
                    // switch to the correct scene based on the time period and location in the button name
                    ToggleVisibilityByScene.ToggleFromSceneToSceneRelocatePlayerToCamera(SceneManager.GetActiveScene().name, timePeriod, playerPosition);
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
        Texture2D backgroundTexture = AssociateCameraTextureByName(fullScreenBackground);

        // set the sprite to the given texture
        fullScreenBackgroundImage.sprite = Sprite.Create(backgroundTexture, new Rect(0.0f, 0.0f, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

        // reset the scale before centering and full-screening
        fullScreenBackgroundImage.rectTransform.localScale = new Vector3(1, 1, 1);

        // center and full-screen the image
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(fullScreenBackgroundImage.gameObject);
        TransformScreenSpaceObject.ScaleObjectToFillCamera(fullScreenBackgroundImage.gameObject);

        // if enabled, add the script to refresh the image sprite when the object is re-enabled
        // this allows for using an image that's generated/updated during gameplay
        if (refreshOnEnable)
        {
            fullScreenBackground.AddComponent<RefreshImageSprite>();
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

        // resize the text's bounding box needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position the title text
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(titleLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(titleLabel, titleContainer, menuTitleTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(titleLabel, titleContainer, menuTitleLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(titleContainer, titleLabel, menuTitleBottomMarginScreenHeightRatio);

        // set parent/child hierarchy
        titleContainer.transform.SetParent(parent.transform);
        titleLabel.transform.SetParent(titleContainer.transform);

        return titleContainer;
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
            string combinedPlaceTimeNameSpacelessDashed = StringUtils.CleanString(placeName) + "-" + SceneGlobals.availableTimePeriodSceneNames[i];

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
                Texture2D timeTravelCameraTexture = AssociateCameraTextureByName(timePeriodButton);

                timePeriodButtonImage.sprite = Sprite.Create(timeTravelCameraTexture, new Rect(0.0f, 0.0f, timeTravelCameraTexture.width, timeTravelCameraTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

                // also attach the script to force update the sprite when the Pause Menu is called
                timePeriodButton.AddComponent<RefreshImageSprite>();
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

        // create the main menu button
        GameObject mainMenuButton = CreateTextButton("Main Menu", centralNavContainer, buttonColor);
        // align and position the main menu button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(mainMenuButton, buttonAlignmentObject, 0.0f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(mainMenuButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // exit button
        GameObject exitButton = CreateTextButton("Exit", centralNavContainer, buttonColor);
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




