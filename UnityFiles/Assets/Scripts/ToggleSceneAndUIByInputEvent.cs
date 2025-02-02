using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Toggles entire Scenes and associated UI via certain input events
/// </summary>

// globals used for fading
public static class FadeGlobals
{
    public static int frameCount = 0;

    public static float startValue = 1.0f;
    public static float endValue = 0.0f;

    public static float currentValue = 1.0f;
}

// utilities for toggling between scenes, and for toggling scene-specific UI elements
public class ToggleSceneAndUI
{
    // toggles the "fromScene" off, and toggles the "toScene" on
    public static void ToggleFromSceneToScene(string fromScene, string toScene)
    {
        DebugUtils.DebugLog("Toggling from Scene " + "<b>" + fromScene + "</b>" + " to Scene " + "<b>" + toScene + "</b>");

        // toggle the toScene first, to avoid any gaps in playback
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(toScene, true);

        // make the toScene active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(toScene));

        // mark the referring and upcoming scenes globally, for other scripts to access
        SceneGlobals.referringSceneName = fromScene;
        SceneGlobals.upcomingSceneName = toScene;

        // now toggle the fromScene scene off
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(fromScene, false);
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
        ManageCameraActions.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // reset the profile to the original
        ManageCameraActions.SetPostProcessProfile(postProcessHost, currentProfileName);

        // toggle to the requested scene
        ToggleFromSceneToSceneRelocatePlayerToFPSController(fromScene, toScene, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);
    }

    public static IEnumerator ToggleToNextEraAfterDelay()
    {
        yield return new WaitForSeconds(600);

        // set the global flag to indicate guided tour time travel has been requested
        FollowGuidedTour.isGuidedTourTimeTravelRequested = true;
    }

    // toggles given scenes on in the background, captures the FPSCharacter camera, then toggles the scenes off
    public static void ToggleBackgroundScenesForCameraCapture(string[] scenes)
    {
        // toggle each of the given scenes on
        foreach (string scene in scenes)
        {
            ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(scene, true);

            // relocate and align the current FPSController to the referring FPSController
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);

            // capture the current FPSController camera
            CreateScreenSpaceUIElements.CaptureActiveFPSControllerCamera();
        }
    }

    // toggle and update the time period notification
    public static IEnumerator ToggleTimePeriodNotificationTemporarily(float displayTime)
    {
        // need to wait just a bit for the UI to get built and stored
        yield return new WaitForEndOfFrame();

        // get the current time period notification container for this scene
        UIGlobals.currentTimePeriodNotificationContainer = CreateScreenSpaceUIElements.GetTimePeriodNotificationContainerByName(SceneManager.GetActiveScene().name);

        // only enable/disable the time period notification container
        // if the player is time traveling, not in a menu, and not pausing
        if (UIGlobals.currentTimePeriodNotificationContainer != null && ManageFPSControllers.FPSControllerGlobals.isTimeTraveling && !ManageFPSControllers.FPSControllerGlobals.isPausing)
        {
            // toggle the time period notification container on
            UIGlobals.currentTimePeriodNotificationContainer.SetActive(true);

            // display the message for a moment
            yield return new WaitForSeconds(displayTime);

            // toggle the time period notification container back off
            UIGlobals.currentTimePeriodNotificationContainer.SetActive(false);
        }
    }

    // toggle the under construction label if it's available for this scene
    public static void ToggleUnderConstructionLabel()
    {
        if (UIGlobals.underConstructionLabelContainer)
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

    // coming soon: ability to tween between values over a given number of frames
    public static void ValueTo(float startValue, float endValue, int numberOfFrames)
    {
        if (FadeGlobals.frameCount != numberOfFrames && FadeGlobals.currentValue != endValue)
        {
            float changeValue = Mathf.Abs(startValue - endValue) / numberOfFrames;

            if (startValue - endValue > 0)
            {

                FadeGlobals.currentValue -= changeValue;
            }
            else
            {
                float addValue = (startValue - endValue) / numberOfFrames;

                FadeGlobals.currentValue += changeValue;
            }

            FadeGlobals.frameCount++;
        }
    }
}

