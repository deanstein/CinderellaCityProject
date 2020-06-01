using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class ManageFPSControllers : MonoBehaviour {

    // this script needs to be attached to each FPSController in each scene
    public class FPSControllerGlobals
    {
        // all FPSControllers
        public static List<GameObject> allFPSControllers = new List<GameObject>();

        // the current FPSController
        public static GameObject activeFPSController;
        public static Transform activeFPSControllerTransform;
        public static Vector3 activeFPSControllerCameraForward;

        public Texture2D outgoingFPSControllerCameraTexture;
    }

    // add this FPSController to the list of available FPSControllers, if it doesn't exist already
    public void AddToAvailableControllers()
    {
        if (!FPSControllerGlobals.allFPSControllers.Contains(this.gameObject))
        {
            FPSControllerGlobals.allFPSControllers.Add(this.gameObject);
        }
    }

    // update the active controller to this object
    public void UpdateActiveFPSController()
    {
        FPSControllerGlobals.activeFPSController = this.gameObject;
    }

    // update the active controller's transform position
    public void UpdateActiveFPSControllerPositionAndCamera()
    {
        // record the position of the active FPSController for other scripts to access
        FPSControllerGlobals.activeFPSControllerTransform = this.transform;
        // record the forward vector of the active FPSController camera for other scripts to access
        FPSControllerGlobals.activeFPSControllerCameraForward = this.transform.GetChild(0).GetComponent<Camera>().transform.forward;
    }

    // reposition and realign this FPSController to match another camera in the scene
    public static void RelocateAlignFPSControllerToCamera(string cameraPartialName)
    {
        // get all the cameras in the  scene
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera.name.Contains(cameraPartialName))
            {
                Utils.DebugUtils.DebugLog("Found a matching camera: " + camera.name);

                // get the current FPSController
                GameObject activeFPSController = FPSControllerGlobals.activeFPSController;

                // need to make sure the camera transform doesn't include a rotation up or down (causes FPSCharacter to tilt)
                Vector3 currentCameraForward = camera.transform.forward;
                Vector3 newCameraForward = new Vector3(currentCameraForward.x, 0, currentCameraForward.z);

                // match the FPSController's position and rotation to the camera
                activeFPSController.transform.SetPositionAndRotation(camera.transform.position, Quaternion.LookRotation(newCameraForward, Vector3.up));

                // match the CharacterController's camera separately
                activeFPSController.transform.GetChild(0).GetComponent<Camera>().transform.forward = currentCameraForward;

                // reset the FPSController mouse to avoid incorrect rotation due to interference
                activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();
            }
        }
    }

    // reposition and realign this FPSController to match the given one
    public static void RelocateAlignFPSControllerToFPSController(Transform FPSControllerTransformToMatch)
    {
        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;

        // match the FPSController's position and rotation to the referring controller's position and rotation
        activeFPSController.transform.SetPositionAndRotation(FPSControllerTransformToMatch.position, FPSControllerTransformToMatch.rotation);

        // match the CharacterController's camera separately
        activeFPSController.transform.GetChild(0).GetComponent<Camera>().transform.forward = FPSControllerTransformToMatch.transform.GetChild(0).gameObject.GetComponent<Camera>().transform.forward;

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();
    }

    // enables the mouse lock on all FPSControllers
    public static void EnableMouseLockOnAllFPSControllers()
    {
        foreach(GameObject FPSController in FPSControllerGlobals.allFPSControllers)
        {
            FPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(true);
        }
    }

    private void OnEnable()
    {
        // update the active controller as this object
        UpdateActiveFPSController();

        // match position and camera
        if (FPSControllerGlobals.activeFPSControllerTransform)
        {
            this.transform.position = FPSControllerGlobals.activeFPSControllerTransform.position;
            this.transform.rotation = FPSControllerGlobals.activeFPSControllerTransform.rotation;
            this.transform.GetChild(0).GetComponent<Camera>().transform.forward = FPSControllerGlobals.activeFPSControllerCameraForward;
        }

        // add this FPSController to the list of available controllers
        AddToAvailableControllers();

        // lock the cursor so it doesn't display on-screen
        // need to do this on every FPSController - even disabled FPSControllers can keep the cursor visible
        EnableMouseLockOnAllFPSControllers();
    }

    private void OnDisable()
    {
        // unlock the cursor only when the upcoming scene is a menu (not a time period scene)
        if (SceneGlobals.availableTimePeriodSceneNames.IndexOf(SceneGlobals.upcomingScene) == -1)
        {
            FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(false);
        }
    }

    private void Update()
    {
        // record the active FPSController position globally for other scripts to access
        UpdateActiveFPSControllerPositionAndCamera();
    }
}

