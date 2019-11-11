using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateUIMenuByType : MonoBehaviour
{
    public void TaskOnClick()
    {
        Debug.Log("You clicked the button!");
    }

    void Start()
    {
        // build the menu based on the name of the object this script is attached to
        string name = this.name;

        // all menu item backgrounds
        Color32 menuBackgroundColor = new Color32(50, 50, 50, 100);

        // the main menu
        if (name.Contains("MainMenuLauncher"))
        {
            Debug.Log("Building the Main Menu...");

            ////////// main menu canvas //////////
            GameObject mainMenu = new GameObject("MainMenu");
            mainMenu.AddComponent<Canvas>();

            Canvas mainMenuCanvas = mainMenu.GetComponent<Canvas>();
            mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainMenu.AddComponent<CanvasScaler>();
            mainMenu.AddComponent<GraphicRaycaster>();

            ////////// main menu background image //////////
            GameObject mainMenuBackground = new GameObject("MainMenuBackgroundImage");
            mainMenuBackground.AddComponent<Image>();

            Image mainMenuBackgroundImage = mainMenuBackground.GetComponent<Image>();
            mainMenuBackgroundImage.transform.SetParent(mainMenu.transform);
            mainMenuBackgroundImage.sprite = (Sprite)Resources.Load("UI/CCP splash screen 1", typeof(Sprite));
            mainMenuBackgroundImage.preserveAspect = true;
            mainMenuBackgroundImage.SetNativeSize();

            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(mainMenuBackground);
            TransformScreenSpaceObject.ScaleObjectToFillCamera(mainMenuBackground);

            ////////// CCP logo container + background color //////////
            GameObject CCPLogoContainer = new GameObject("CCPLogoContainer");

            CCPLogoContainer.AddComponent<CanvasRenderer>();
            Image CCPLogoContainerColor = CCPLogoContainer.AddComponent<Image>();
            CCPLogoContainerColor.color = menuBackgroundColor;

            ////////// CCP logo //////////
            GameObject CCPLogo = new GameObject("CCPLogo");
            CCPLogo.AddComponent<Image>();

            Image CCPLogoImage = CCPLogo.GetComponent<Image>();
            CCPLogoImage.sprite = (Sprite)Resources.Load("UI/CCP logo", typeof(Sprite));
            CCPLogoImage.preserveAspect = true;
            CCPLogoImage.SetNativeSize();

            // adjust the position of the logo, its container, or both
            //TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(CCPLogoContainer, 0.0f);
            //TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(CCPLogoContainer);

            TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(CCPLogo, 0.15f);

            TransformScreenSpaceObject.PositionObjectByHeightRatioFromCameraTop(CCPLogo, 0.1f);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromCameraLeft(CCPLogo, 0.1f);
            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(CCPLogoContainer, CCPLogo, 0.03f);

            TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(CCPLogoContainer, CCPLogo, 0.03f);
            TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(CCPLogoContainer, CCPLogo, 0.03f);
            //TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborLeft(CCPLogoContainer, CCPLogo, 0.03f);

            ////////// thumbnail container + background color //////////
            GameObject thumbnailContainer = new GameObject("ThumbnailContainer");

            thumbnailContainer.AddComponent<CanvasRenderer>();
            Image thumbnailContainerColor = thumbnailContainer.AddComponent<Image>();
            thumbnailContainerColor.color = menuBackgroundColor;

            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(thumbnailContainer, CCPLogoContainer, 0.05f);

            TransformScreenSpaceObject.ResizeObjectWidthToMatchCamera(thumbnailContainer);
            TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromCameraLeft(thumbnailContainer, 0.1f);
            TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromCameraBottom(thumbnailContainer, 0.1f);
            //TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborLeft(thumbnailContainer, CCPLogoContainer, 0.0f);

            ////////// intro message ////////////
            GameObject introMessageLabel = new GameObject("IntroMessageLabel");

            Text introMessageLabelText = introMessageLabel.AddComponent<Text>();
            introMessageLabelText.font = (Font)Resources.Load("AvenirNextLTPro-Demi");
            introMessageLabelText.text = "Choose a time and place:";
            introMessageLabelText.fontSize = 20;
            introMessageLabelText.alignment = TextAnchor.UpperLeft;

            // get the text's dimensions to match only the space it needs, before any transforms
            TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(introMessageLabel);
            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(introMessageLabel, thumbnailContainer, -0.02f);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(introMessageLabel, thumbnailContainer, -0.02f);

            // Blue Mall text
            GameObject blueMallLabel = new GameObject("BlueMallLabel");

            Text blueMallLabelText = blueMallLabel.AddComponent<Text>();
            blueMallLabelText.font = (Font)Resources.Load("AvenirNextLTPro-Demi");
            blueMallLabelText.text = "Blue Mall";
            blueMallLabelText.fontSize = 50;
            blueMallLabelText.alignment = TextAnchor.MiddleCenter;

            // get the text's dimensions to match only the space it needs, before any transforms
            TransformScreenSpaceObject.ResizeTextExtentsToFitContents(blueMallLabelText);

            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(blueMallLabel);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(blueMallLabel, thumbnailContainer, -0.05f);
            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(blueMallLabel, introMessageLabel, 0.05f);

            ////////// Blue Mall buttons //////////

            // 60s-70s
            GameObject blueMall60s70sButton = new GameObject("BlueMall60s70sButton");

            blueMall60s70sButton.AddComponent<Image>();
            Image BlueMall60s70sButtonImage = blueMall60s70sButton.GetComponent<Image>();
            BlueMall60s70sButtonImage.sprite = (Sprite)Resources.Load("UI/MainMenu-BlueMall80s90s", typeof(Sprite));
            BlueMall60s70sButtonImage.preserveAspect = true;
            BlueMall60s70sButtonImage.SetNativeSize();

            TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(blueMall60s70sButton, 0.2f);

            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(blueMall60s70sButton);
            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(blueMall60s70sButton, blueMallLabel, 0.02f);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(blueMall60s70sButton, thumbnailContainer, -0.02f);

            blueMall60s70sButton.AddComponent<Button>();
            blueMall60s70sButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);

            // 80s-90s
            GameObject blueMall80s90sButton = new GameObject("BlueMall80s90sButton");

            blueMall80s90sButton.AddComponent<Image>();
            Image blueMall80s90sButtonImage = blueMall80s90sButton.GetComponent<Image>();
            blueMall80s90sButtonImage.sprite = (Sprite)Resources.Load("UI/MainMenu-BlueMall80s90s", typeof(Sprite));
            blueMall80s90sButtonImage.preserveAspect = true;
            blueMall80s90sButtonImage.SetNativeSize();

            TransformScreenSpaceObject.ScaleObjectByCameraHeightProportion(blueMall80s90sButton, 0.2f);

            TransformScreenSpaceObject.PositionObjectAtCenterofCamera(blueMall80s90sButton);
            TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(blueMall80s90sButton, blueMall60s70sButton, 0.01f);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(blueMall80s90sButton, blueMall60s70sButton, 0f);

            blueMall80s90sButton.AddComponent<Button>();
            blueMall80s90sButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);

            // organize children and parent relationships for the entire menu
            // must happen after transforms
            CCPLogoContainer.transform.SetParent(mainMenu.transform);
            CCPLogo.transform.SetParent(CCPLogoContainer.transform);

            thumbnailContainer.transform.SetParent(mainMenu.transform);
            introMessageLabel.transform.SetParent(thumbnailContainer.transform);
            blueMallLabel.transform.SetParent(thumbnailContainer.transform);
            blueMall60s70sButton.transform.SetParent(thumbnailContainer.transform);
            blueMall80s90sButton.transform.SetParent(thumbnailContainer.transform);


            // ensure there's an EventSystem
            if (GameObject.Find("EventSystem") == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        else
        {
            Debug.Log("Unknown UI type!");
        }
    }
}
 
 
 
 
 