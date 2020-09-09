using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using System.Collections;

// this script should be attached to FPSCharacter objects that need to watch for shortcuts to adjust camera effects
public class ToggleCameraActionsByInputEvent : MonoBehaviour {

    private void Awake()
    {
        // add this volume priority to the global priority value
        ManageCameraActions.CameraActionGlobals.highestKnownCameraEffectPriority += this.GetComponent<PostProcessVolume>().priority;
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        // if we're in screenshot mode, update all the cameras in this scene
        if (EditorPrefs.GetBool(ManageCameraActions.CameraActionGlobals.screenshotModeFlag))
        {
            StartCoroutine(TakeScreenshots.CaptureAllThumbnailScreenshots());
        }
#endif
        // optional: start a camera transition when the camera is enabled
        /*
        StartCoroutine(ToggleCameraActionsByInputEvent.ToggleCameraEffectWithTransition(this.gameObject, ManageCameraEffects.GetDefaultPostProcessProfileBySceneName(this.gameObject.scene.name), "FlashWhite", 0.2f));
        */
    }

    // watch for shortcuts every frame
    void Update()
    {
        // update the globally-available camera host
        ManageCameraActions.CameraActionGlobals.activeCameraHost = this.gameObject;

        // define which effects belong to which shortcut
        if (Input.GetKeyDown("1"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Vaporwave", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("2"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "B&W", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("3"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Sepia", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("4"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Dark", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("x"))
        {
            TakeScreenshots.CaptureScreenshotOfCurrentCamera();
        }
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        // if in screenshot mode, copy the newly-created screenshots to their proper destination
        if (EditorPrefs.GetBool(ManageCameraActions.CameraActionGlobals.screenshotModeFlag))
        {
            ManageCameraActions.ReplaceSceneThumbnailsInResources();
        }

        EditorPrefs.SetBool(ManageCameraActions.CameraActionGlobals.screenshotModeFlag, false);
#endif
    }
}

public class ToggleCameraEffects
{
    public static IEnumerator ToggleCameraEffectWithTransition(GameObject postProcessHost, string profileName, string transitionProfileName, float transitionTime)
    {
        // first, toggle the flash transition
        ManageCameraActions.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // set the requested camera effect profile
        ManageCameraActions.SetPostProcessProfile(postProcessHost, profileName);
    }
}

public class TakeScreenshots
{
    public static void CaptureScreenshotOfCurrentCamera()
    {
        // get the correct screenshot path based on the current context
        string screenshotPath = ManageCameraActions.GetScreenshotPathByContext();

        // generate a file name based on the current context
        string screenshotName = ManageCameraActions.GetScreenshotFileNameByContext();

        // take the screenshot and store it at the given location
        ScreenCapture.CaptureScreenshot(screenshotPath + screenshotName);

        Utils.DebugUtils.DebugLog("<b>Saved a screenshot of the current camera at: </b>" + screenshotPath + screenshotName);
    }

    // moves the current player around to all cameras requiring a screenshot
    public static IEnumerator CaptureAllThumbnailScreenshots()
    {
        // get all thumbnail cameras in the current scene
        GameObject[] cameraHostObjects = ManageCameraActions.GetAllThumbnailCamerasInScene();

        // relocate the player to each of these camera host locations, and take a screenshot
        foreach (GameObject cameraHost in cameraHostObjects)
        {
            // get the correct screenshot path based on the current context
            string screenshotPath = ManageCameraActions.CameraActionGlobals.inGameScreenshotsPath;

            // generate a file name based on the current context
            string screenshotName = cameraHost.name + ManageCameraActions.CameraActionGlobals.screenshotFormat;

            // move the player to the camera
            ManageFPSControllers.RelocateAlignFPSControllerToCamera(cameraHost.name);

            // wait for the relocation to take effet
            yield return new WaitForSeconds(1);

            // capture a screenshot without UI to the approprite path
            ScreenCapture.CaptureScreenshot(screenshotPath + screenshotName);

            // wait for the capture to complete
            yield return new WaitForSeconds(1);
        }
#if UNITY_EDITOR
        // exit when complete
        EditorApplication.ExitPlaymode();
#endif
    }
}

