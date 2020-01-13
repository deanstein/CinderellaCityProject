using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class ManageFPSControllers : MonoBehaviour {

    // this script needs to be attached to all FPSController objects
    public class FPSControllerGlobals
    {
        // the globally-available current FPSController
        public static GameObject activeFPSController;
        public static Vector3 activeFPSControllerTransformPosition;

        // the outgoing FPS controller transform that must be stored and used later for the new FPSController to match
        public static Transform outgoingFPSControllerTransform;
    }

    // set the active controller to this object
    public void SetActiveFPSController()
    {
        FPSControllerGlobals.activeFPSController = this.gameObject;
    }

    // set the active controller's transform position
    public void SetActiveFPSControllerPosition()
    {
        // record the position of the active FPSController for other scripts to access
        FPSControllerGlobals.activeFPSControllerTransformPosition = this.transform.position;
    }

    // set the referring controller to this object
    public void SetOutgoingFPSControllerTransform()
    {
        FPSControllerGlobals.outgoingFPSControllerTransform = FPSControllerGlobals.activeFPSController.transform;
    }

    public static void RelocateAlignFPSControllerToCamera(string cameraPartialName)
    {
        // get all the cameras in the  scene
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera.name.Contains(cameraPartialName))
            {
                Debug.Log("Found a matching camera: " + camera.name);

                // get the current FPSController
                GameObject activeFPSController = FPSControllerGlobals.activeFPSController;
                // also set this as the outgoing FPSController's transform (required to simulate switching to this scene from in-game as a "time traveler"
                FPSControllerGlobals.outgoingFPSControllerTransform = activeFPSController.transform;

                // match the FPSController's position and rotation to the camera
                activeFPSController.transform.SetPositionAndRotation(camera.transform.position, Quaternion.LookRotation(camera.transform.forward, Vector3.up));

                // reset the FPSController mouse to avoid incorrect rotation due to interference
                activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

            }
        }
    }

    public static void RelocateAlignFPSControllerToFPSController(Transform referringControllerTransform)
    {
        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;

        // match the FPSController's position and rotation to the referring controller's position and rotation
        activeFPSController.transform.SetPositionAndRotation(referringControllerTransform.position, referringControllerTransform.rotation);

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();
    }

    private void OnEnable()
    {
        // set the active controller to this object
        SetActiveFPSController();
    }

    private void OnDisable()
    {
        // set the outgoing FPSController transform in order to match for the next FPSController
        SetOutgoingFPSControllerTransform();
    }

    private void Update()
    {
        // record the active FPSController position globally for other scripts to access
        SetActiveFPSControllerPosition();
    }
}

