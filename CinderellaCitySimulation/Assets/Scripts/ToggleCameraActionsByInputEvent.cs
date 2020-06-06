using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

using System.Collections;
using System.IO;

// this script should be attached to FPSCharacter objects that need to watch for shortcuts to adjust camera effects
public class ToggleCameraActionsByInputEvent : MonoBehaviour {

    private void Awake()
    {
        // add this volume priority to the global priority value
        ManageCameraActions.CameraActionGlobals.highestKnownCameraEffectPriority += this.GetComponent<PostProcessVolume>().priority;
    }

    private void OnEnable()
    {
        // optional: start a camera transition when the camera is enabled
        /*
        StartCoroutine(ToggleCameraActionsByInputEvent.ToggleCameraEffectWithTransition(this.gameObject, ManageCameraEffects.GetDefaultPostProcessProfileBySceneName(this.gameObject.scene.name), "FlashWhite", 0.2f));
        */
    }

    // watch for shortcuts every frame
    void Update()
    {
        // update the globally-available Post Process host
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

        Utils.DebugUtils.DebugLog("Captured a screenshot of the current camera at: " + screenshotPath + screenshotName);
    }
}

