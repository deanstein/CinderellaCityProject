using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateScreenSpaceUIElements : MonoBehaviour
{
    // define the available time periods
    // this determines how many rows to make for time and place pickers
    public static string[] availableTimePeriods = { "60s70s", "80s90s" };

    /// colors ///

    // all button colors
    public static Color32 buttonColor = new Color32(20, 20, 20, 220);

    // all nav and container colors
    public static Color32 containerColor = new Color32(50, 50, 50, 100);

    /// sizes ///

    // fonts and text sizes (pixels)
    public static string labelFont = "AvenirNextLTPro-Demi";

    public static int menuTitleLabelSize = 30;
    public static int timePlaceLabelSize = 50;
    public static int menuTextButtonLabelSize = 35;

    // button sizes (ratio relative to screen size)
    public static float menuButtonScreenWidthRatio = 0.15f;
    public static float menuButtonTopBottomPaddingScreenHeightRatio = 0.01f;

    /// spacings ///

    public static float textButtonBottomMarginScreenHeightRatio = 0.005f;
    public static float textButtonLeftMarginScreenWidthRatio = 0.01f;
    
    // remove spaces and punctuation from a string
    public static string CleanString(string messyString)
    {
        // remove spaces
        string cleanString = messyString.Replace(" ", "");

        // remove colons
        cleanString = cleanString.Replace(":", "");

        return cleanString;
    }

    // define what clicking the buttons does
    public static void TaskOnClick()
    {
        ToggleVisibilityByScene.ToggleFromSceneToScene("MainMenu", "60s70s", true);
        Debug.Log("You clicked the button!");
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

    public static GameObject CreateFullScreenBackgroundImageSlideshow(GameObject parent, string[] slideshowSequence)
    {
        // create the background object
        GameObject fullScreenBackgroundImage = new GameObject("BackgroundSlideShow");
        fullScreenBackgroundImage.AddComponent<Image>();

        // set the image
        Image mainMenuBackgroundImage = fullScreenBackgroundImage.GetComponent<Image>();
        mainMenuBackgroundImage.transform.SetParent(parent.transform);

        // this script will sequence, transform, and animate the background images as required
        AnimateScreenSpaceObject AnimateScreenSpaceObjectScript =fullScreenBackgroundImage.AddComponent<AnimateScreenSpaceObject>();
        AnimateScreenSpaceObjectScript.mainMenuBackgroundSlideShowSequence = slideshowSequence;

        return fullScreenBackgroundImage;
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
        TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(logo, 0.15f);

        // position the logo
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(logo, 0.1f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromCameraLeft(logo, 0.1f);

        // position the logo container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(logoContainer, logo, 0.03f);

        // resize the logo container
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(logoContainer, logo, 0.03f);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(logoContainer, logo, 0.03f);

        // set the parent/child hierarchy
        logoContainer.transform.SetParent(parent.transform);
        logo.transform.SetParent(logoContainer.transform);

        return logoContainer;
    }

    public static GameObject CreateMenuTitleBar(GameObject parent, GameObject topAlignmentObject)
    {
        // create the container object
        GameObject titleContainer = new GameObject("MenuTitleBar");
        titleContainer.AddComponent<CanvasRenderer>();
        Image logoContainerColor = titleContainer.AddComponent<Image>();
        logoContainerColor.color = containerColor;

        // position the time and place container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(titleContainer, topAlignmentObject, 0.05f);

        // resize the time and place container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(titleContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(titleContainer, 0.1f);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(titleContainer, 0.1f);

        return titleContainer;
    }

    // create the stacked thumbnails for a place, across time periods
    public static GameObject CreatePlaceTimeThumbnailColumn(GameObject parent, GameObject topAlignmentObject, GameObject leftAlignmentObject, string placeName, string[] timePeriodNames)
    {
        // make an object to hold the thumbnails
        GameObject thumbnailColumn = new GameObject(CleanString(placeName) + "ThumbnailColumn");
        thumbnailColumn.transform.SetParent(parent.transform);

        // location text
        GameObject placeLabel = new GameObject(CleanString(placeName) + "Label");

        Text placeLabelText = placeLabel.AddComponent<Text>();
        placeLabelText.font = (Font)Resources.Load(labelFont);
        placeLabelText.text = placeName;
        placeLabelText.fontSize = timePlaceLabelSize;
        placeLabelText.alignment = TextAnchor.MiddleCenter;

        // get the text's dimensions to match only the space it needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(placeLabelText);

        // position the label
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(placeLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(placeLabel, topAlignmentObject, 0.03f);

        // create an empty list to be filled out by each of the thumbnail objects
        List<GameObject> thumbnailObjects = new List<GameObject>();

        // for each image name provided, make a new thumbnail button
        for (var i = 0; i < timePeriodNames.Length; i++)
        {
            // combine the place name and time period strings
            string combinedPlaceTimeNameSpacelessDashed = CleanString(placeName) + "-" + timePeriodNames[i];

            // create the button
            GameObject timePeriodButton = new GameObject(combinedPlaceTimeNameSpacelessDashed + "Button");
            timePeriodButton.AddComponent<Image>();

            // set the image
            // note this requires a valid image in the Resources folder path below, with a file name that matches combinedPlaceTimeNameSpaceless
            Image timePeriodButtonImage = timePeriodButton.GetComponent<Image>();
            timePeriodButtonImage.sprite = (Sprite)Resources.Load("UI/Camera-Thumbnail-" + combinedPlaceTimeNameSpacelessDashed, typeof(Sprite));
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
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, placeLabel, 0.02f);
            }
            // otherwise, position it below the previous thumbnail
            else
            {
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, thumbnailObjects[i - 1], 0.01f);
            }

            // add this new thumbnail button to the list for tracking
            thumbnailObjects.Add(timePeriodButton);

            // add the button property
            timePeriodButton.AddComponent<Button>();
            timePeriodButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);

            // set the parent/child hierarchy
            timePeriodButton.transform.SetParent(thumbnailColumn.transform);
        }

        // position the place label centered at the first thumbnail
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(placeLabel, thumbnailObjects[0]);

        // set the parent/child hierarchy
        placeLabel.transform.SetParent(thumbnailColumn.transform);

        return thumbnailColumn;
    }

    // create the time and place picker interface
    public static GameObject CreateMainMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        // create the time and place picker container
        GameObject timePlacePickerContainer = new GameObject("TimeAndPlacePicker");
        timePlacePickerContainer.AddComponent<CanvasRenderer>();

        // set the color of the time and place container
        Image timePlacePickerContainerColor = timePlacePickerContainer.AddComponent<Image>();
        timePlacePickerContainerColor.color = containerColor;

        // position the time and place container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePlacePickerContainer, topNeighbor, 0.05f);

        // resize the time and place container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(timePlacePickerContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(timePlacePickerContainer, 0.1f);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(timePlacePickerContainer, 0.1f);
        //TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborLeft(thumbnailContainer, CCPLogoContainer, 0.0f);

        // add the intro message
        GameObject introMessageLabel = new GameObject("IntroMessageLabel");
        Text introMessageLabelText = introMessageLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(labelFont);
        introMessageLabelText.text = "Choose a time and place:";
        introMessageLabelText.fontSize = 20;
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position the intro message
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(introMessageLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(introMessageLabel, timePlacePickerContainer, -0.02f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(introMessageLabel, timePlacePickerContainer, -0.02f);

        // create the time periods labels
        GameObject timePeriodLabel1 = new GameObject("1960s70sLabel");

        Text timePeriodLabel1Text = timePeriodLabel1.AddComponent<Text>();
        timePeriodLabel1Text.font = (Font)Resources.Load(labelFont);
        timePeriodLabel1Text.text = "1960s-70s";
        timePeriodLabel1Text.fontSize = timePlaceLabelSize;
        timePeriodLabel1Text.alignment = TextAnchor.MiddleCenter;

        // get the text's dimensions to match only the space it needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabel1Text);

        // position the time period label
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodLabel1);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel1, timePlacePickerContainer, -0.02f);

        // create each place thumbnail and their respective available time periods
        GameObject BlueMallThumbnailColumn = CreatePlaceTimeThumbnailColumn(timePlacePickerContainer, introMessageLabel, timePeriodLabel1, "Blue Mall", availableTimePeriods);

        GameObject RoseMallThumbnailColumn = CreatePlaceTimeThumbnailColumn(timePlacePickerContainer, introMessageLabel, BlueMallThumbnailColumn.transform.GetChild(0).gameObject, "Rose Mall", availableTimePeriods);

        GameObject GoldMallThumbnailColumn = CreatePlaceTimeThumbnailColumn(timePlacePickerContainer, introMessageLabel, RoseMallThumbnailColumn.transform.GetChild(0).gameObject, "Gold Mall", availableTimePeriods);

        // resize the container to align with the last thumbnail in the column
        int thumbnailCount = BlueMallThumbnailColumn.transform.childCount - 1; // exclude the label
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(timePlacePickerContainer, BlueMallThumbnailColumn.transform.GetChild(thumbnailCount - 1).gameObject, 0.05f);

        // set the parent/child hierarchy
        timePlacePickerContainer.transform.SetParent(parent.transform);
        introMessageLabel.transform.SetParent(timePlacePickerContainer.transform);
        timePeriodLabel1.transform.SetParent(timePlacePickerContainer.transform);

        return timePlacePickerContainer;
    }

    // define what clicking the buttons does
    public static void GoToMainMenu()
    {
        ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu", true);
    }

    public static GameObject CreateTextButton(string text, GameObject parent, Color32 color)
    {
        // create the text label
        GameObject buttonTextObject = new GameObject(CleanString(text) + "ButtonText");
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.font = (Font)Resources.Load(labelFont);
        buttonText.text = text;
        buttonText.fontSize = menuTextButtonLabelSize;
        buttonText.alignment = TextAnchor.MiddleCenter;

        Vector2 textSize = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(buttonText);

        // create the button
        GameObject buttonContainer = new GameObject(CleanString(text) + "Button");
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
        buttonContainer.GetComponent<Button>().onClick.AddListener(GoToMainMenu);

        // set the parent/child hierarchy
        buttonContainer.transform.SetParent(parent.transform);
        buttonTextObject.transform.SetParent(buttonContainer.transform);

        // move the button
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(buttonContainer);

        return buttonContainer;
    }

    public static GameObject CreatePauseMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        string menuTitle = "Pause";
        // create the time and place picker container
        GameObject navContainer = new GameObject("PauseMenuNav");
        navContainer.AddComponent<CanvasRenderer>();

        // set the color of the time and place container
        Image navContainerColor = navContainer.AddComponent<Image>();
        navContainerColor.color = containerColor;

        // position the time and place container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(navContainer, topNeighbor, 0.05f);

        // resize the time and place container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(navContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(navContainer, 0.1f);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(navContainer, 0.1f);
        //TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborLeft(thumbnailContainer, CCPLogoContainer, 0.0f);

        // add the intro message
        GameObject menuNameLabel = new GameObject("MenuNameLabel");
        Text introMessageLabelText = menuNameLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(labelFont);
        introMessageLabelText.text = menuTitle;
        introMessageLabelText.fontSize = 20;
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position the intro message
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(menuNameLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(menuNameLabel, navContainer, -0.02f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(menuNameLabel, navContainer, -0.02f);

        // create the time periods labels
        GameObject timePeriodLabel1 = new GameObject("1960s70sLabel");

        Text timePeriodLabel1Text = timePeriodLabel1.AddComponent<Text>();
        timePeriodLabel1Text.font = (Font)Resources.Load(labelFont);
        timePeriodLabel1Text.text = "1960s-70s";
        timePeriodLabel1Text.fontSize = timePlaceLabelSize;
        timePeriodLabel1Text.alignment = TextAnchor.MiddleCenter;

        // get the text's dimensions to match only the space it needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabel1Text);

        // position the time period label
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(timePeriodLabel1);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel1, navContainer, -0.02f);

        // create the time travel thumbnail container
        GameObject timeTravelThumbnailColumn = CreatePlaceTimeThumbnailColumn(navContainer, menuNameLabel, timePeriodLabel1, "Time Travel:", availableTimePeriods);

        /// buttons ///

        // define a gameObject to align the buttons to
        GameObject buttonAlignmentObject = timeTravelThumbnailColumn.transform.GetChild(0).gameObject;

        // create the main menu button
        GameObject mainMenuButton = CreateTextButton("Main Menu", navContainer, buttonColor);
        // align and position the main menu button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(mainMenuButton, buttonAlignmentObject, 0.0f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(mainMenuButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // exit button
        GameObject exitButton = CreateTextButton("Exit", navContainer, buttonColor);
        // align and position the exit button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(exitButton, mainMenuButton, textButtonBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(exitButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // set the parent/child hierarchy
        navContainer.transform.SetParent(parent.transform);
        menuNameLabel.transform.SetParent(navContainer.transform);
        timePeriodLabel1.transform.SetParent(navContainer.transform);

        return navContainer;
    }
}




