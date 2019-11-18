using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateScreenSpaceUILayoutByName : MonoBehaviour
{
    // this script needs to be attached to a "launcher" - an empty gameobject in the current scene
    // the correct menu layout, or layout combinations, is then built based on the gameobject's name

    // all menu item backgrounds
    Color32 menuBackgroundColor = new Color32(50, 50, 50, 100);

    // all menu full screen background images
    string[] mainMenuBackgroundSlideShowSequence = { "UI/CCP splash screen 1", "UI/CCP splash screen 2" };

    void Start()
    {
        // build the menu based on the name of the object this script is attached to
        string name = this.name;

        // the main menu
        if (name.Contains("MainMenuLauncher"))
        {
            Debug.Log("Building the Main Menu...");

            // main menu canvas
            GameObject mainMenu = CreateScreenSpaceUIElements.CreateMenuCanvas("MainMenu");

            // the main menu is responsible for preloading the large scenes so level choice is faster
            //LoadSceneByName LoadSceneByNameScript = mainMenu.AddComponent<LoadSceneByName>();

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenBackgroundImageSlideshow(mainMenu, "BackgroundSlideShow", mainMenuBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(mainMenu, "LogoHeader", menuBackgroundColor);

            // time & place picker and container
            GameObject timePlacePicker = CreateScreenSpaceUIElements.CreateTimeAndPlacePicker(mainMenu, logoHeader, menuBackgroundColor);

            // ensure there's an EventSystem
            if (GameObject.Find("EventSystem") == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        // pause menu
        else if (name.Contains("PauseMenuLauncher"))
        {
            Debug.Log("Building the Pause Menu...");

            // pause menu canvas
            GameObject pauseMenu = CreateScreenSpaceUIElements.CreateMenuCanvas("PauseMenu");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenBackgroundImageSlideshow(pauseMenu, "BackgroundSlideShow", mainMenuBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(pauseMenu, "LogoHeader", menuBackgroundColor);

            // ensure there's an EventSystem
            if (GameObject.Find("EventSystem") == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        // otherwise, no UI will be built because the name wasn't found or recognized
        else
        {
            Debug.Log("Unknown UI type! " + name);
        }
    }
}
 
 
 
 
 