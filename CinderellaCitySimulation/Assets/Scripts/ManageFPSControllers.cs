using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
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
        public static bool isActiveFPSControllerOnNavMesh;
        public static Vector3 activeFPSControllerNavMeshPosition;

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

    // update the active controller's nav mesh data
    public void UpdateActiveFPSControllerNavMeshData()
    {
        // determine if the controller is close enough to a nav mesh point 
        // to be considered on the nav mesh
        if (Utils.GeometryUtils.GetIsOnNavMeshWithinTolerance(FPSControllerGlobals.activeFPSControllerTransform.gameObject, 5.0f))
        {
            FPSControllerGlobals.isActiveFPSControllerOnNavMesh = true;
        }
        else
        {
            FPSControllerGlobals.isActiveFPSControllerOnNavMesh = false;
        }

        // get the nearest point on the nav mesh to the controller
        // this will be the world origin if a point is not found
        FPSControllerGlobals.activeFPSControllerNavMeshPosition = Utils.GeometryUtils.GetNearestPointOnNavMesh(FPSControllerGlobals.activeFPSControllerTransform.position, 5.0f);
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
                GameObject activeFirstPersonCharacter = activeFPSController.transform.GetChild(0).gameObject;
                NavMeshAgent activeAgent = activeFPSController.GetComponent<NavMeshAgent>();

                // need to make sure the camera transform doesn't include a rotation up or down (causes FPSCharacter to tilt)
                Vector3 currentCameraForward = camera.transform.forward;
                Vector3 newCameraForward = new Vector3(currentCameraForward.x, 0, currentCameraForward.z);

                // get the nearest point from the camera to the nav mesh
                // this ensures the FPSController will be standing on the floor, not floating
                Vector3 nearestPointToCameraOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(camera.transform.position, 5);
                Vector3 adjustedFPSLocationAboveNavMesh = new Vector3(nearestPointToCameraOnNavMesh.x, nearestPointToCameraOnNavMesh.y + ((activeAgent.height / 2) * activeFPSController.transform.localScale.y), nearestPointToCameraOnNavMesh.z);

                // disable the nav mesh agent temporarily to prevent positioning issues
                activeAgent.enabled = false;

                // match the FPSController's position and rotation to the camera
                activeFPSController.transform.SetPositionAndRotation(adjustedFPSLocationAboveNavMesh, Quaternion.LookRotation(newCameraForward, Vector3.up));

                // set the FirstPersonCharacter's camera forward direction
                activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = currentCameraForward;

                // set the FirstPersonCharacter height to the camera height
                activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, camera.transform.position.y, activeFirstPersonCharacter.transform.position.z);

                // reset the FPSController mouse to avoid incorrect rotation due to interference
                activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

                // enable the agent again
                activeAgent.enabled = true;
            }
        }
    }

    // reposition and realign this FPSController to match the given one
    public static void RelocateAlignFPSControllerToFPSController(Transform FPSControllerTransformToMatch)
    {
        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;
        GameObject activeFirstPersonCharacter = activeFPSController.transform.GetChild(0).gameObject;
        NavMeshAgent activeAgent = activeFPSController.GetComponent<NavMeshAgent>();

        GameObject firstPersonCharacterToMatch = FPSControllerTransformToMatch.transform.GetChild(0).gameObject;

        // disable the nav mesh agent temporarily to prevent positioning issues
        activeAgent.enabled = false;

        // match the FPSController's position and rotation to the referring controller's position and rotation
        activeFPSController.transform.SetPositionAndRotation(FPSControllerTransformToMatch.position, FPSControllerTransformToMatch.rotation);

        // set the FirstPersonCharacter's camera forward direction
        activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = firstPersonCharacterToMatch.GetComponent<Camera>().transform.forward;

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

        // set the FirstPersonCharacter height to the camera height
        activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, firstPersonCharacterToMatch.transform.position.y, activeFirstPersonCharacter.transform.position.z);

        // enable the agent again
        activeAgent.enabled = true;
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
        if (SceneGlobals.availableTimePeriodSceneNamesList.IndexOf(SceneGlobals.upcomingScene) == -1)
        {
            FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(false);
        }
    }

    private void Update()
    {
        // record certain FPSController data globally for other scripts to access
        UpdateActiveFPSControllerPositionAndCamera();
        UpdateActiveFPSControllerNavMeshData();
    }
}

