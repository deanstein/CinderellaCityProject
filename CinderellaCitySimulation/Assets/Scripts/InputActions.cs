using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Responsible for handling input actions from peripherals like controllers
/// Must be attached to the FPSController in each scene
/// </summary>

public class InputActions : MonoBehaviour
{
    private InputMaster inputMaster;
    public static Vector2 rightStickLook;
    private readonly float lookSpeed = 6f;
    private Camera cam;
    private CharacterController controller;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    // allows start/stop of guided tour from controller
    // for installations like Historic Englewood, this should be false
    // but for other customers, this can be true
    private readonly bool doAllowStartStopGuidedTour = false;

    // TODO / coming soon
    // pause menu interface only supports mouse/keyboard
    public void Pause()
    {
        Debug.Log("Pause invoked.");
    }

    public void StartGuidedTour()
    {
        if (doAllowStartStopGuidedTour)
        {
            FollowGuidedTour.StartGuidedTourMode();
        }
    }

    public void EndGuidedTour()
    {
        if (doAllowStartStopGuidedTour)
        {
            FollowGuidedTour.EndGuidedTourMode();
        }
    }

    public void ToggleGuidedTour()
    {
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            FollowGuidedTour.EndGuidedTourMode();
        } else
        {
            FollowGuidedTour.StartGuidedTourMode();
        }
    }

    public void PreviousPhoto()
    {
        FollowGuidedTour.DecrementGuidedTourIndexAndSetAgentOnPath();
    }

    public void NextPhoto()
    {
        FollowGuidedTour.IncrementGuidedTourIndexAndSetAgentOnPath();
    }

    public void TimeTravelBackward()
    {
        // get the previous time period scene name
        string previousTimePeriodSceneName = ManageScenes.GetNextTimePeriodSceneName("previous");

        // toggle to the previous scene with a camera effect transition
        StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(SceneManager.GetActiveScene().name, previousTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform,
            ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.gameObject, "FlashBlack", 0.2f));
    }

    public void TimeTravelForward()
    {
        // get the next time period scene name
        string nextTimePeriodSceneName = ManageScenes.GetNextTimePeriodSceneName("next");

        // then toggle to the next scene with a camera effect transition
        StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(SceneManager.GetActiveScene().name, nextTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.gameObject, "FlashBlack", 0.2f));
    }

    public void ToggleHistoricPhotos()
    {
        GameObject historicCamerasContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];
        ModeState.areHistoricPhotosRequestedVisible = !ModeState.areHistoricPhotosRequestedVisible;
        ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicCamerasContainer, ModeState.areHistoricPhotosRequestedVisible, false);
    }

    public void ToggleRun()
    {
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<FirstPersonController>().m_WalkSpeed == ManageFPSControllers.FPSControllerGlobals.defaultFPSControllerWalkSpeed)
        {
            ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<FirstPersonController>().m_WalkSpeed = ManageFPSControllers.FPSControllerGlobals.defaultFPSControllerRunSpeed;
        } else
        {
            ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<FirstPersonController>().m_WalkSpeed = ManageFPSControllers.FPSControllerGlobals.defaultFPSControllerWalkSpeed;
        }
    }

    public void TogglePeople()
    {
        GameObject peopleContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.peopleObjectKeywords, true)[0];
        peopleContainer.SetActive(!peopleContainer.activeInHierarchy);
    }

    public void InvertYAxis()
    {
        ModeState.invertYAxis = !ModeState.invertYAxis;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -90F, 90f);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private void Awake()
    {
        inputMaster = new InputMaster();
        inputMaster.Player.Run.performed += ctx => ToggleRun();
        inputMaster.Player.TimeTravelBackward.performed += ctx => TimeTravelBackward();
        inputMaster.Player.TimeTravelForward.performed += ctx => TimeTravelForward();
        inputMaster.Player.TogglePeople.performed += ctx =>
        {
            if (ctx.performed)
            {
                TogglePeople();
            };
        };
        inputMaster.Player.ToggleHistoricPhotos.performed += ctx => ToggleHistoricPhotos();
        inputMaster.Player.InvertYAxis.performed += ctx => InvertYAxis();
        inputMaster.Player.Pause.performed += ctx => Pause();
        inputMaster.Player.StartGuidedTour.performed += ctx => StartGuidedTour();
        inputMaster.Player.EndGuidedTour.performed += ctx => EndGuidedTour();
        inputMaster.Player.ToggleGuidedTour.performed += ctx => ToggleGuidedTour();
        inputMaster.Player.PreviousPhoto.performed += ctx => PreviousPhoto();
        inputMaster.Player.NextPhoto.performed += ctx => NextPhoto();
        inputMaster.Player.Look.performed += ctx => rightStickLook = ctx.ReadValue<Vector2>();
        inputMaster.Player.Look.canceled += ctx => rightStickLook = Vector2.zero;
    }

    private void OnEnable()
    {
        cam = GetComponentInChildren<Camera>();
        controller = GetComponentInChildren<CharacterController>();
        m_CharacterTargetRot = controller.transform.localRotation;
        m_CameraTargetRot = cam.transform.localRotation;

        if (inputMaster != null)
        {
            inputMaster.Enable();
        }
    }

    private void OnDisable()
    {
        if (inputMaster != null)
        {
            inputMaster.Disable();
        }
    }


    private void Update()
    {



        ////////// KEYBOARD ACTIONS //////////
        


        // time travel requested - previous time period
        if (Input.GetKeyDown("q") &&
            Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name))
        {
            TimeTravelBackward();
        }

        // time travel requested - next time period
        if (Input.GetKeyDown("e") &&
            Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name))
        {
            TimeTravelForward();
        }

        // main menu
        // only accessible from time period scenes
        if (Input.GetKeyDown("m") &&
            Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name))
        {
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");
        }

        // pause menu
        // only accessible from time period scenes
        if (Input.GetKeyDown(KeyCode.Escape) &&
            Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name)
            && UIVisibilityGlobals.activeOverlayMenu == null)
        {
            // before pausing, we need to capture a screenshot from the active FPSController
            // then update the pause menu background image
            CreateScreenSpaceUIElements.CaptureActiveFPSControllerCamera();
            CreateScreenSpaceUIElements.RefreshObjectImageSprite(UIGlobals.pauseMenuBackgroundImage);

            // toggle to Pause
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "PauseMenu");

            // set the pausing flag to prevent disabled scenes from adversely affecting hoisting behavior
            ManageFPSControllers.FPSControllerGlobals.isPausing = true;

            // now capture a screenshot from the inactive scenes' FPSControllers
            // then update the thumbnail sprites
            CreateScreenSpaceUIElements.CaptureDisabledSceneFPSCameras();
            CreateScreenSpaceUIElements.RefreshThumbnailSprites();

            // reset the pause flag
            ManageFPSControllers.FPSControllerGlobals.isPausing = false;
        }
        // but if we're already in the pause menu, return to the previous scene (referring scene)
        else if (Input.GetKeyDown(KeyCode.Escape)
            && SceneManager.GetActiveScene().name.Contains("PauseMenu"))
        {
            ManageFPSControllers.FPSControllerGlobals.isTimeTraveling = false;
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.referringSceneName);
        }

        // dismiss any active overlay menu with ESC
        else if (Input.GetKeyDown(KeyCode.Escape) && UIVisibilityGlobals.activeOverlayMenu != null)
        {
            ManageOverlayVisibility.DismissActiveOverlayMenu();
        }

        // visibility menu
        // only accessible from time period scenes
        if (Input.GetKeyDown("v") &&
            (Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name) || (SceneManager.GetActiveScene().name.Contains("Experimental"))))
        {
            if (UIVisibilityGlobals.isOverlayMenuActive)
            {
                ManageOverlayVisibility.DismissActiveOverlayMenu();
            }
            else
            {
                CreateScreenSpaceUILayoutByName.BuildVisualizationMenuOverlay(this.gameObject);
            }
        }

        // audio menu
        // only accessible from time period scenes
        if (Input.GetKeyDown("u") &&
            (Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name) || (SceneManager.GetActiveScene().name.Contains("Experimental"))))
        {
            if (UIVisibilityGlobals.isOverlayMenuActive)
            {
                ManageOverlayVisibility.DismissActiveOverlayMenu();
            }
            else
            {
                CreateScreenSpaceUILayoutByName.BuildAudioMenuOverlay(this.gameObject);
            }
        }

        // optionally display or hide the under construction label
        if (Input.GetKeyDown(KeyCode.Slash) &&
            Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name))
        {
            ToggleSceneAndUI.ToggleUnderConstructionLabel();
        }

        // experimental: fade testing
        if (Input.GetKeyDown(KeyCode.Slash) && SceneManager.GetActiveScene().name.Contains("Experimental"))
        {
            FadeGlobals.frameCount = 0;
            FadeGlobals.startValue = 0.0f;
            FadeGlobals.currentValue = 0.0f;
            FadeGlobals.endValue = 1.0f;
        }
        if (Input.GetKeyDown(KeyCode.Backslash) && SceneManager.GetActiveScene().name.Contains("Experimental"))
        {
            FadeGlobals.frameCount = 0;
            FadeGlobals.startValue = 1.0f;
            FadeGlobals.currentValue = 1.0f;
            FadeGlobals.endValue = 0.0f;
        }



        ////////// XBOX CONTROLLER ACTIONS //////////
        


        // ensure controller right stick inputs move the camera around
        if (rightStickLook.x != 0 || rightStickLook.y != 0 || ModeState.isGuidedTourPaused)
        {
            // initially set the target rotations to the existing so there's no sudden movement
            m_CharacterTargetRot = this.GetComponentInChildren<CharacterController>().transform.localRotation;
            m_CameraTargetRot = this.GetComponentInChildren<Camera>().transform.localRotation;

            float yRot = rightStickLook.x * lookSpeed;
            float xRot = rightStickLook.y * lookSpeed;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(ModeState.invertYAxis ? xRot : -xRot, 0f, 0f);

            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, m_CameraTargetRot,
                    lookSpeed * Time.deltaTime);
            controller.transform.localRotation = Quaternion.Slerp(controller.transform.localRotation, m_CharacterTargetRot,
                    lookSpeed * Time.deltaTime);
        }
    }
}
