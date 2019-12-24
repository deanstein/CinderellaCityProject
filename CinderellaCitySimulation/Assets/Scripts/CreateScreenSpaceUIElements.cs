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
    public static string[] availableTimePeriods = { "1960s-70s", "1980s-90s" };

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

    // define what clicking the buttons does, based on the name of the button
    public static void TaskOnClickByName(string buttonName)
    {
        switch (buttonName)
        {
            case string name when name.Contains("MainMenu"):
                ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu", true);
                return;
            case string name when name.Contains("Exit"):
                Application.Quit();
                return;
            case string name when name.Contains("60s70s"):
                ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "60s70s", true);
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
        buttonContainer.GetComponent<Button>().onClick.AddListener(() => { TaskOnClickByName(buttonContainer.name); }); ;

        // set the parent/child hierarchy
        buttonContainer.transform.SetParent(parent.transform);
        buttonTextObject.transform.SetParent(buttonContainer.transform);

        // move the button
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(buttonContainer);

        return buttonContainer;
    }

    // create the time label stack
    public static GameObject CreateTimeLabelStack(GameObject parent, GameObject leftAlignmentObject, string[] availableTimePeriods)
    {
        // create a container object
        GameObject timeLabelStackContainer = new GameObject("TimePeriodLabelStack");

        foreach (string timePeriod in availableTimePeriods)
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
        // clear the list of thumbnail objects from the last stack created
        // this list will be used to set parents and as alignment guides for other objects
        placeThumbnailsForAlignment.Clear();

        // make an object to hold the thumbnails
        GameObject thumbnailStack = new GameObject(CleanString(placeName) + "ThumbnailStack");

        // location text
        GameObject placeLabel = new GameObject(CleanString(placeName) + "Label");
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
            string combinedPlaceTimeNameSpacelessDashed = CleanString(placeName) + "-" + CleanString(timePeriodNames[i]);

            Debug.Log(combinedPlaceTimeNameSpacelessDashed);

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
        GameObject timeLabelStack = CreateTimeLabelStack(centralNavContainer, centralNavContainer, availableTimePeriods);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create each place thumbnail stack, and their associated place labels
        GameObject blueMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, timeLabelForAlignment, "Blue Mall", availableTimePeriods);

        GameObject roseMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, blueMallThumbnailStack.transform.GetChild(0).gameObject, "Rose Mall", availableTimePeriods);

        GameObject goldMallThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, roseMallThumbnailStack.transform.GetChild(0).gameObject, "Gold Mall", availableTimePeriods);

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
        GameObject timeLabelStack = CreateTimeLabelStack(centralNavContainer, centralNavContainer, availableTimePeriods);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create the time travel thumbnail container
        GameObject timeTravelThumbnailStack = CreatePlaceTimeThumbnailStack(centralNavContainer, centralNavContainer, timeLabelForAlignment, "Time Travel:", availableTimePeriods);

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




