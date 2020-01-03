using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleVisibilityByShortcut : MonoBehaviour {

    // these are passed in from AssetImportPipeline
    public string objectType;
    public string shortcut;

    // used for console debugging
    public string objectState;

    // Update is called once per frame
    void Update()
    {
        // identify the shortcuts to listen for, and define what they do

        /// time travel shortcuts ///

        // time travel requested - previous time period
        if (Input.GetKeyDown("q") &&
    StringUtils.TestIfAnyListItemContainedInString(GlobalSceneVariables.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            // get the previous time period scene name
            string previousTimePeriodSceneName = ManageAvailableScenes.GetNextTimePeriodSceneName("previous");
            // then toggle to it from the active scene
            ToggleVisibilityByScene.ToggleFromSceneToSceneRelocatePlayerToFPSController(SceneManager.GetActiveScene().name, previousTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.outgoingFPSControllerTransform);
        }

        // time travel requested - next time period
        if (Input.GetKeyDown("e") &&
            StringUtils.TestIfAnyListItemContainedInString(GlobalSceneVariables.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            // get the next time period scene name
            string nextTimePeriodSceneName = ManageAvailableScenes.GetNextTimePeriodSceneName("next");
            // then toggle to it from the active scene
            ToggleVisibilityByScene.ToggleFromSceneToSceneRelocatePlayerToFPSController(SceneManager.GetActiveScene().name, nextTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.outgoingFPSControllerTransform);
        }

        /// UI visiblity shortcuts ///

        // main menu
        // only accessible from time period scenes
        if (Input.GetKeyDown("m") &&
            StringUtils.TestIfAnyListItemContainedInString(GlobalSceneVariables.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");
        }

        // pause menu
        // only accessible from time period scenes
        if (Input.GetKeyDown(KeyCode.Escape) &&
            StringUtils.TestIfAnyListItemContainedInString(GlobalSceneVariables.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "PauseMenu");
        }
        // if we're already in the pause menu, return to the previous scene (referring scene)
        else if (Input.GetKeyDown(KeyCode.Escape)
            && SceneManager.GetActiveScene().name.Contains("PauseMenu"))
        {
            ToggleVisibilityByScene.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, GlobalSceneVariables.referringScene);

        }

        /// object visibility shortcuts ///

        // people
        if (Input.GetKeyDown("p") && objectType.Contains("people"))
        {
            ToggleObjectChildrenVisibility(this.gameObject);
        }

    }

    void ToggleObjectChildrenVisibility(GameObject parent)
    {
        // loop through all children of this GameObject and make them active or inactive
        foreach (Transform child in parent.transform)
        {
            // If key is pressed and children are on, turn them off
            if ((Input.GetKeyDown(shortcut)) && (child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(false);
                objectState = "OFF";

            }
            // If key is pressed and children are off, turn them on
            else if ((Input.GetKeyDown(shortcut)) && (!child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(true);
                objectState = "ON";
            }
        }

        // Output the object type and the new state
        if (Input.GetKeyDown(shortcut))
        {
            Debug.Log("Turning visibility of " + objectType + " to: " + objectState);
        }
    }
}

