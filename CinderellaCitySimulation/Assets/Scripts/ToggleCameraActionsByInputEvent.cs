using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

/// <summary>
/// Toggles camera operations and effects on certain input events
/// </summary>

// this script should be attached to FPSCharacter objects that need to watch for shortcuts to adjust camera effects
public class ToggleCameraActionsByInputEvent : MonoBehaviour {

    Rigidbody playerRigidBody;
    FirstPersonController playerFPSController;

    private void Awake()
    {
        // add this volume priority to the global priority value
        ManageCameraActions.CameraActionGlobals.highestKnownCameraEffectPriority += this.GetComponent<PostProcessVolume>().priority;
    }

    private void Start()
    {
        // get the player's rigid body and FPS Controller
        playerRigidBody = this.gameObject.transform.parent.GetComponent<Rigidbody>();
        playerFPSController = this.gameObject.transform.parent.GetComponent<FirstPersonController>();

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
        // these should be added to the Visibility Menu too
        if (Input.GetKeyDown("1"))
        {
            SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectVaporwave);
        }
        else if (Input.GetKeyDown("2"))
        {
            SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectNoir);
        }
        else if (Input.GetKeyDown("3"))
        {
            SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectSepia);
        }
        else if (Input.GetKeyDown("4"))
        {
            SetCameraEffect(ManageCameraActions.CameraActionGlobals.cameraEffectDark);
        }
        // toggle blur on/off
        if (Input.GetKeyDown("b"))
        {          
            PostProcessVolume existingVolume = this.GetComponent<PostProcessVolume>();
            PostProcessProfile existingProfile = existingVolume.profile;
            MotionBlur blur;
            existingProfile.TryGetSettings(out blur);
            blur.enabled.Override(!blur.enabled);
        }

        // capture screenshot
        if (Input.GetKeyDown("x"))
        {
            CreateScreenSpaceUIElements.CaptureScreenshotButtonAction();
        }

        // record the current position and camera as a JSON object that can be restored
        if (Input.GetKeyDown("k"))
        {
            CreateScreenSpaceUIElements.SaveViewButtonAction();
        }

        // toggle anti-gravity mode
        if (Input.GetKeyDown("g"))
        {
            // if gravity is on, turn it off, and set the ground force and multiplier to 0
            if (ModeState.isAntiGravityModeActive)
            {
                ModeState.isAntiGravityModeActive = false;
                ManageFPSControllers.SetPlayerGravity(true);
            }
            // if gravity is off, turn it on, and set the ground force and multiplier to their defaults
            else
            {
                ModeState.isAntiGravityModeActive = true;
                ManageFPSControllers.SetPlayerGravity(false);
            }
        }
        // move up
        if (Input.GetKey("r"))
        {
            // only do something if gravity is off
            if (ModeState.isAntiGravityModeActive)
            {
                // the delta to move the player up
                float YPosDelta = 0;

                // use running speed if shift is pressed
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    YPosDelta = Time.fixedDeltaTime * playerFPSController.m_RunSpeed;
                }
                // use walking speed by default
                else
                {
                    YPosDelta = Time.fixedDeltaTime * playerFPSController.m_WalkSpeed;
                }

                Vector3 currentPos = this.transform.parent.position;
                this.transform.parent.position = new Vector3(currentPos.x, currentPos.y + YPosDelta, currentPos.z);
            }
        }
        // move down
        if (Input.GetKey("f"))
        {
            // only do something if gravity is off
            if (ModeState.isAntiGravityModeActive)
            {
                // the delta to move the player up
                float YPosDelta = 0;

                // use running speed if shift is pressed
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    YPosDelta = Time.fixedDeltaTime * playerFPSController.m_RunSpeed;
                }
                // use walking speed by default
                else
                {
                    YPosDelta = Time.fixedDeltaTime * playerFPSController.m_WalkSpeed;
                }

                Vector3 currentPos = this.transform.parent.position;
                this.transform.parent.position = new Vector3(currentPos.x, currentPos.y - YPosDelta, currentPos.z);
            }
        }

        // start the guided tour of available historic photos
        if (Input.GetKeyDown("["))
        {
            Utils.DebugUtils.DebugLog("Starting guided tour mode...");

            // set the mode state
            ModeState.isGuidedTourActive = true;

            // automatically switch to the next era after some time
            ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
        }
        // end the guided tour
        if (Input.GetKeyDown("]"))
        {
            Utils.DebugUtils.DebugLog("Ending guided tour mode.");

            // set the mode state
            ModeState.isGuidedTourActive = false;
            ModeState.isGuidedTourPaused = false;

            // if there was a restart coroutine active, stop it
            if (ModeState.restartGuidedTourCoroutine != null)
            {
                StopCoroutine(ModeState.restartGuidedTourCoroutine);
            }
            if (ModeState.toggleToNextEraCoroutine != null)
            {
                StopCoroutine(ModeState.toggleToNextEraCoroutine);
            }
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

    // create a non-static void wrapper function for
    // calling the camera effects routine from different contexts
    public void SetCameraEffect(string effectName)
    {
        StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, effectName, "FlashBlack", 0.4f));
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
    public static void CaptureScreenshotOfCurrentCamera(string screenshotPath)
    {
        // generate a file name based on the current context
        string screenshotName = ManageCameraActions.GetScreenshotFileNameByContext();

        // take the screenshot and store it at the given location
        ScreenCapture.CaptureScreenshot(screenshotPath + screenshotName + ManageCameraActions.CameraActionGlobals.screenshotFormat);

        Utils.DebugUtils.DebugLog("<b>Saved a screenshot of the current camera at: </b>" + screenshotPath + screenshotName);
    }

    // moves the current player around to all cameras requiring a screenshot
    public static IEnumerator CaptureAllThumbnailScreenshots()
    {
        // get all thumbnail cameras in the current scene
        GameObject[] cameraHostObjects = ManageSceneObjects.ProxyObjects.GetAllThumbnailCamerasInScene();

        // relocate the player to each of these camera host locations, and take a screenshot
        foreach (GameObject cameraHost in cameraHostObjects)
        {
            // the screenshot path is initially stored in a temp directory
            string screenshotPath = ManageCameraActions.CameraActionGlobals.inGameScreenshotsPath;

            // generate a file name based on the current context
            string screenshotName = cameraHost.name + "-" + SceneManager.GetActiveScene().name + ManageCameraActions.CameraActionGlobals.screenshotFormat;

            // move the player to the camera
            ManageFPSControllers.RelocateAlignFPSControllerToCamera(cameraHost.name);

            // wait for the relocation to take effect
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

