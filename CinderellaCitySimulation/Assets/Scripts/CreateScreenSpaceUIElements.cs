using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateScreenSpaceUIElements : MonoBehaviour
{
    private void Update()
    {
        
    }
    // define what clicking the buttons does
    public static void TaskOnClick()
    {
        //SceneManager.LoadScene("80s-90s");
        Debug.Log("You clicked the button!");
    }

    public static GameObject CreateMenuCanvas(string name)
    {
        GameObject menu = new GameObject(name);
        menu.AddComponent<Canvas>();

        Canvas menuCanvas = menu.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menu.AddComponent<CanvasScaler>();
        menu.AddComponent<GraphicRaycaster>();

        return menu;
    }

    public static GameObject CreateFullScreenBackgroundImageSlideshow(GameObject parent, string name, string[] slideshowSequence)
    {
        // create the background object
        GameObject fullScreenBackgroundImage = new GameObject(name);
        fullScreenBackgroundImage.AddComponent<Image>();

        // set the image
        Image mainMenuBackgroundImage = fullScreenBackgroundImage.GetComponent<Image>();
        mainMenuBackgroundImage.transform.SetParent(parent.transform);

        // this script will sequence, transform, and animate the background images as required
        AnimateScreenSpaceObject AnimateScreenSpaceObjectScript =fullScreenBackgroundImage.AddComponent<AnimateScreenSpaceObject>();
        AnimateScreenSpaceObjectScript.mainMenuBackgroundSlideShowSequence = slideshowSequence;


        return fullScreenBackgroundImage;
    }

    public static GameObject CreateLogoHeader(GameObject parent, string name, Color32 containerColor)
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

    // create an array of gameobjects to be filled out by the following thumbnails

    // create the stacked thumbnails for a place, across time periods
    public static GameObject CreatePlaceTimeThumbnailColumn(GameObject parent, GameObject topAlignmentObject, GameObject leftAlignmentObject, string placeName, string[] timePeriodNames)
    {
        // remove the spaces from the name
        string placeNameSpaceless = placeName.Replace(" ", "");

        // make an object to hold the thumbnails
        GameObject thumbnailColumn = new GameObject(placeNameSpaceless + "ThumbnailColumn");
        thumbnailColumn.transform.SetParent(parent.transform);

        // location text
        GameObject placeLabel = new GameObject(placeNameSpaceless + "Label");

        Text placeLabelText = placeLabel.AddComponent<Text>();
        placeLabelText.font = (Font)Resources.Load("AvenirNextLTPro-Demi");
        placeLabelText.text = placeName;
        placeLabelText.fontSize = 50;
        placeLabelText.alignment = TextAnchor.MiddleCenter;

        // get the text's dimensions to match only the space it needs, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(placeLabelText);

        // position the label
        TransformScreenSpaceObject.PositionObjectAtCenterofCamera(placeLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(placeLabel, topAlignmentObject, 0.03f);

        // create an empty list to be filled out by each of the thumbnail objects
        List<GameObject> thumbnailObjects = new List<GameObject>();

        // for each image name provided, make a new thumbnail button
        //foreach (string timePeriod in timePeriodNames)
        for (var i = 0; i < timePeriodNames.Length; i++)
        {
            // combine the place name and time period strings
            string combinedPlaceTimeNameSpacelessDashed = placeNameSpaceless + "-" + timePeriodNames[i];

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
    public static GameObject CreateTimeAndPlacePicker(GameObject parent, GameObject topNeighbor, Color32 containerColor)
    {
        // define the available time periods for each place
        string[] availableTimePeriods = { "60s70s", "80s90s" };

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
        introMessageLabelText.font = (Font)Resources.Load("AvenirNextLTPro-Demi");
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
        timePeriodLabel1Text.font = (Font)Resources.Load("AvenirNextLTPro-Demi");
        timePeriodLabel1Text.text = "1960s-70s";
        timePeriodLabel1Text.fontSize = 50;
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
}




