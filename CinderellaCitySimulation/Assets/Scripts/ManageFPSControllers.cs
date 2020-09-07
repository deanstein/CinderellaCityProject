using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
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

        // the current nav mesh agent - the FPSController should only ever have one of these
        public static NavMeshAgent activeFPSControllerNavMeshAgent;

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
        // update the current FPSController agent
        FPSControllerGlobals.activeFPSControllerNavMeshAgent = FPSControllerGlobals.activeFPSController.GetComponentInChildren<NavMeshAgent>();

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
        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;
        GameObject activeFirstPersonCharacter = activeFPSController.transform.GetChild(0).gameObject;
        NavMeshAgent activeAgent = activeFPSController.GetComponentInChildren<NavMeshAgent>();

        // get the existing post process volume and profile
        PostProcessVolume existingVolume = activeFPSController.GetComponentInChildren<PostProcessVolume>();
        PostProcessProfile existingProfile = existingVolume.profile;

        // create a temporary profile with no motion blur
        PostProcessProfile newProfile = new PostProcessProfile();
        newProfile = existingProfile;
        MotionBlur blur;
        newProfile.TryGetSettings(out blur);
        blur.enabled.Override(false);
        existingVolume.priority++;

        // temporarily override the existing volume to avoid motion blur when relocating
        existingVolume.profile = newProfile;

        // get all the cameras in the scene
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera.name.Contains(cameraPartialName))
            {
                Utils.DebugUtils.DebugLog("Found a matching camera: " + camera.name);

                // need to make sure the camera transform doesn't include a rotation up or down (causes FPSCharacter to tilt)
                Vector3 currentCameraForward = camera.transform.forward;
                Vector3 newCameraForward = new Vector3(currentCameraForward.x, 0, currentCameraForward.z);

                // get the nearest point from the camera to the nav mesh
                // this ensures the FPSController will be standing on the floor, not floating
                Vector3 nearestPointToCameraOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(camera.transform.position, 5);

                // further correct the point, because the nav mesh isn't exactly on the floor surface
                Vector3 nearestPointToCameraOnFloor = Utils.GeometryUtils.GetNearestRaycastPointBelowPos(nearestPointToCameraOnNavMesh, 1.0f);

                // if zero, no good poitn was found, so fall back to the nav mesh point
                if (nearestPointToCameraOnFloor == Vector3.zero)
                {
                    nearestPointToCameraOnFloor = nearestPointToCameraOnNavMesh;
                }

                Vector3 adjustedFPSLocationAboveNavMesh = new Vector3(nearestPointToCameraOnNavMesh.x, nearestPointToCameraOnFloor.y + ((activeFPSController.GetComponent<CharacterController>().height / 2) * activeFPSController.transform.localScale.y), nearestPointToCameraOnNavMesh.z);

                // match the FPSController's position and rotation to the camera
                activeFPSController.transform.SetPositionAndRotation(adjustedFPSLocationAboveNavMesh, Quaternion.LookRotation(newCameraForward, Vector3.up));

                // set the FirstPersonCharacter's camera forward direction
                activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = currentCameraForward;

                // set the FirstPersonCharacter height to the camera height
                activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, camera.transform.position.y, activeFirstPersonCharacter.transform.position.z);

                // reset the FPSController mouse to avoid incorrect rotation due to interference
                activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

                break;
            }
        }

        // restore the existing post process profile
        existingVolume.profile = existingProfile;
    }

    // reposition and realign this FPSController to match the given one
    public static void RelocateAlignFPSControllerToFPSController(Transform FPSControllerTransformToMatch)
    {
        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;
        GameObject activeFirstPersonCharacter = activeFPSController.transform.GetChild(0).gameObject;

        GameObject firstPersonCharacterToMatch = FPSControllerTransformToMatch.transform.GetChild(0).gameObject;

        // match the FPSController's position and rotation to the referring controller's position and rotation
        activeFPSController.transform.SetPositionAndRotation(FPSControllerTransformToMatch.position, FPSControllerTransformToMatch.rotation);

        // set the FirstPersonCharacter's camera forward direction
        activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = firstPersonCharacterToMatch.GetComponent<Camera>().transform.forward;

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

        // set the FirstPersonCharacter height to the camera height
        activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, firstPersonCharacterToMatch.transform.position.y, activeFirstPersonCharacter.transform.position.z);

    }

    // used to allow AI control of the FPSController,
    // for the purposes of positioning or for a guided tour
    public static void ToggleFPSAgentAsParent()
    {
        // get the FPSAgent - there should only be one of these, but its hierarchy may change
        // it will be a child of the FPSController, so search for it there
        NavMeshAgent[] existingFPSAgents = FPSControllerGlobals.activeFPSController.GetComponentsInChildren<NavMeshAgent>();
        NavMeshAgent currentAgentToCopy = existingFPSAgents[0];

        // if there's only one nav mesh agent found, and it's already on the parent, skip
        if (existingFPSAgents.Length == 1 && currentAgentToCopy.transform.name.Contains("FPSController"))
        {
            return;
        }

        // delete the current agent from both possible locations
        foreach (NavMeshAgent FPSAgent in existingFPSAgents)
        {
            Destroy(FPSAgent);
        }

        // copy the agent to the parent
        NavMeshAgent newFPSAgent = FPSControllerGlobals.activeFPSController.AddComponent<NavMeshAgent>();
        CopyAgentSettings(currentAgentToCopy, newFPSAgent);
    }

    // used to give the FPSController full navigation control,
    // ending any guided tours or location adjustment agents
    public static void ToggleFPSAgentAsSibling()
    {
        // get the FPSAgent - there should only be one of these, but its hierarchy may change
        // it will be a child of the FPSController, so search for it there
        NavMeshAgent[] existingFPSAgents = FPSControllerGlobals.activeFPSController.GetComponentsInChildren<NavMeshAgent>();
        NavMeshAgent currentAgentToCopy = existingFPSAgents[0];

        // if there's only one nav mesh agent found, and it's already on the parent, skip
        if (existingFPSAgents.Length == 1 && currentAgentToCopy.transform.name.Contains("FirstPersonAgent"))
        {
            return;
        }

        // delete the current agent from both possible locations
        foreach (NavMeshAgent FPSAgent in existingFPSAgents)
        {
            Destroy(FPSAgent);
        }

        // set the agent on the 2nd sibling, which should be the agent host
        // and copy the settings
        NavMeshAgent newFPSAgent = FPSControllerGlobals.activeFPSController.transform.GetChild(1).gameObject.AddComponent<NavMeshAgent>();
        CopyAgentSettings(currentAgentToCopy, newFPSAgent);
    }

    public static void CopyAgentSettings(NavMeshAgent fromAgent, NavMeshAgent toAgent)
    {
        toAgent.agentTypeID = fromAgent.agentTypeID;
        toAgent.baseOffset = fromAgent.baseOffset;
        toAgent.speed = fromAgent.speed;
        toAgent.angularSpeed = fromAgent.angularSpeed;
        toAgent.acceleration = fromAgent.acceleration;
        toAgent.stoppingDistance = fromAgent.stoppingDistance;
        toAgent.autoBraking = fromAgent.autoBraking;
        toAgent.radius = fromAgent.radius;
        toAgent.height = fromAgent.height;
        toAgent.avoidancePriority = fromAgent.avoidancePriority;
        toAgent.autoTraverseOffMeshLink = fromAgent.autoTraverseOffMeshLink;
        toAgent.autoRepath = fromAgent.autoRepath;
        toAgent.areaMask = fromAgent.areaMask;
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

