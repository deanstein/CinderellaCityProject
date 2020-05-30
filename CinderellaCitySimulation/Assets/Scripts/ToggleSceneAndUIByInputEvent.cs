using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

// this script should be attached to UI launcher objects that watch for shortcuts to toggle scenes or UI on/off
public class ToggleSceneAndUIByInputEvent : MonoBehaviour {

    // watch for shortcuts in every frame
    private void Update()
    {
        // identify the shortcuts to listen for, and define what they do

        /// time travel shortcuts ///

        // time travel requested - previous time period
        if (Input.GetKeyDown("q") &&
            StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            // get the previous time period scene name
            string previousTimePeriodSceneName = ManageAvailableScenes.GetNextTimePeriodSceneName("previous");

            // toggle to the previous scene with a camera effect transition
            StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(SceneManager.GetActiveScene().name, previousTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform,
                ManageCameraEffects.CameraEffectGlobals.activePostProcessingHost, "FlashWhite", 0.4f));
        }

        // time travel requested - next time period
        if (Input.GetKeyDown("e") &&
            StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            // get the next time period scene name
            string nextTimePeriodSceneName = ManageAvailableScenes.GetNextTimePeriodSceneName("next");

            // then toggle to the next scene with a camera effect transition
            StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(SceneManager.GetActiveScene().name, nextTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform, ManageCameraEffects.CameraEffectGlobals.activePostProcessingHost, "FlashWhite", 0.4f));
        }

        /// UI visiblity shortcuts ///

        // main menu
        // only accessible from time period scenes
        if (Input.GetKeyDown("m") &&
            StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");
        }

        // pause menu
        // only accessible from time period scenes
        if (Input.GetKeyDown(KeyCode.Escape) &&
            StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            // before pausing, we need to capture a screenshot from the active FPSController
            // then update the pause menu background image
            CreateScreenSpaceUIElements.CaptureActiveFPSControllerCamera();
            CreateScreenSpaceUIElements.RefreshObjectImageSprite(UIGlobals.pauseMenuBackgroundImage);

            // toggle to Pause
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "PauseMenu");

            // now capture a screenshot from the inactive scenes' FPSControllers
            // then update the thumbnail sprites
            CreateScreenSpaceUIElements.CaptureDisabledSceneFPSCameras();
            CreateScreenSpaceUIElements.RefreshThumbnailSprites();
        }
        // if we're already in the pause menu, return to the previous scene (referring scene)
        else if (Input.GetKeyDown(KeyCode.Escape)
            && SceneManager.GetActiveScene().name.Contains("PauseMenu"))
        {
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.referringScene);
        }

        // optionally display or hide the under construction label
        if (Input.GetKeyDown(KeyCode.Slash) &&
            StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNames, SceneManager.GetActiveScene().name))
        {
            if (UIGlobals.underConstructionLabelContainer.activeSelf)
            {
                UIGlobals.underConstructionLabelContainer.SetActive(false);
            }
            else if (!UIGlobals.underConstructionLabelContainer.activeSelf)
            {
                UIGlobals.underConstructionLabelContainer.SetActive(true);
            }
        }
    }
}

// utilities for toggling between scenes, and for toggling scene-specific UI elements
public class ToggleSceneAndUI
{

    // toggles all scene objects on, except those tagged with any sort of script host tag
    public static void ToggleSceneObjectsOnExceptScriptHosts(string sceneName)
    {
        // first, turn all the script hosts off
        ToggleScriptHostObjectListOff();

        // turn everything else on
        ToggleSceneObjectsOn(sceneName);
    }

    public static void ToggleScriptHostObjectListOn()
    {
        // disable the script host objects for each of the host types given
        foreach (GameObject[] scriptHostObjectArray in ManageTaggedObjects.TaggedObjectGlobals.scriptHostObjects)
        {
            foreach (GameObject scriptHostObject in scriptHostObjectArray)
            {
                scriptHostObject.SetActive(true);
            }
        }
    }

    public static void ToggleScriptHostObjectListOff()
    {
        // disable the script host objects for each of the host types given
        foreach (GameObject[] scriptHostObjectArray in ManageTaggedObjects.TaggedObjectGlobals.scriptHostObjects)
        {
            foreach (GameObject scriptHostObject in scriptHostObjectArray)
            {
                scriptHostObject.SetActive(false);
            }
        }
    }

    // toggles all scene objects on
    public static void ToggleSceneObjectsOn(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility ON for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make them active if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // make this child active if it's not already
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                //Debug.Log("Toggled visibility ON for: " + child.gameObject.name);
            }
        }
    }

    // toggles all scene objects off
    public static void ToggleSceneObjectsOff(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility OFF for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make them active if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // make this child inactive if it's not already
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                //Debug.Log("Toggled visibility OFF for: " + child.gameObject.name);
            }
        }
    }

    // toggles the "fromScene" off, and toggles the "toScene" on
    public static void ToggleFromSceneToScene(string fromScene, string toScene)
    {
        Debug.Log("Toggling from Scene " + "<b>" + fromScene + "</b>" + " to Scene " + "<b>" + toScene + "</b>");

        // toggle the toScene first, to avoid any gaps in playback
        ToggleSceneObjectsOn(toScene);

        // make the toScene active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(toScene));

        // mark the referring and upcoming scenes globally, for other scripts to access
        SceneGlobals.referringScene = fromScene;
        SceneGlobals.upcomingScene = toScene;

        // now toggle the fromScene scene off
        ToggleSceneObjectsOff(fromScene);
    }

    // toggles scenes, and also relocates the FPSCharacter to match a Camera position
    public static void ToggleFromSceneToSceneRelocatePlayerToCamera(string fromScene, string toScene, string cameraPartialName)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // also relocate and align the FPS controller to the requested camera partial name
        ManageFPSControllers.RelocateAlignFPSControllerToCamera(cameraPartialName);
    }

    // toggles scenes, and also relocates the FPSCharacter to match another FPSCharacter (time-traveling)
    public static void ToggleFromSceneToSceneRelocatePlayerToFPSController(string fromScene, string toScene, Transform FPSControllerTransformToMatch)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // then relocate and align the current FPSController to the referring FPSController
        ManageFPSControllers.RelocateAlignFPSControllerToFPSController(FPSControllerTransformToMatch);
    }

    // toggles scenes and relocates player to another FPSCharacter (time-traveling), with a camera effect transition
    public static IEnumerator ToggleFromSceneToSceneWithTransition(string fromScene, string toScene, Transform FPSControllerTransformToMatch, GameObject postProcessHost, string transitionProfileName, float transitionTime)
    {
        // get the PostProcessing Host's current profile so we can return to it
        string currentProfileName = postProcessHost.GetComponent<PostProcessVolume>().profile.name;

        // first, toggle the flash transition
        ManageCameraEffects.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // reset the profile to the original
        ManageCameraEffects.SetPostProcessProfile(postProcessHost, currentProfileName);

        // toggle to the requested scene
        ToggleFromSceneToSceneRelocatePlayerToFPSController(fromScene, toScene, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);
    }

    // toggles given scenes on in the background, captures the FPSCharacter camera, then toggles the scenes off
    public static void ToggleBackgroundScenesForCameraCapture(string[] scenes)
    {
        // toggle each of the given scenes on
        foreach (string scene in scenes)
        {
            ToggleSceneObjectsOn(scene);

            // relocate and align the current FPSController to the referring FPSController
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);

            // capture the current FPSController camera
            CreateScreenSpaceUIElements.CaptureActiveFPSControllerCamera();
        }
    }
}

