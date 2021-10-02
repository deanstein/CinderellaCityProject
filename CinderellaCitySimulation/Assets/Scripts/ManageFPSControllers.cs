using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Monitors the location, rotation, and behavior of FPS Controllers (player) in Scenes
/// this script needs to be attached to each FPSController in each scene
/// </summary>

public class ManageFPSControllers : MonoBehaviour {

    public class FPSControllerGlobals
    {
        // default FPS Controller gravity values
        public static float defaultFPSControllerGravityMultiplier = 2f;
        public static float defaultFPSControllerStickToGroundForce = 10f;

        // marked true when the user is in-game, toggling between eras via shortcut input
        // helps ensure the player doesn't fall when time-traveling to an era 
        // that doesn't have a surface to stand on in that location
        public static bool isTimeTraveling = false;

        // marked true when an FPS controller is requesting the pause menu
        public static bool isPausing = false;

        // all FPSControllers
        public static List<GameObject> allFPSControllers = new List<GameObject>();

        // the current FPSController and its components
        public static GameObject activeFPSController;
        public static Transform activeFPSControllerTransform;
        public static Camera activeFPSControllerCamera;
        public static Vector3 activeFPSControllerCameraForward;
        public static bool isActiveFPSControllerOnNavMesh;
        public static Vector3 activeFPSControllerNavMeshPosition;
        public static FPSControllerRestoreData activeFPSControllerRestoreData;

        // this scene's first FPSController location
        // which is used to calculate turning gravity back on after time traveling
        public static Vector3 initialFPSControllerLocation;
        // the max distance a player can go before gravity is re-enabled after time-travling
        public static float maxDistanceBeforeResettingGravity = 0.5f;

        // the current nav mesh agent - the FPSController should only ever have one of these
        public static NavMeshAgent activeFPSControllerNavMeshAgent;

        public Texture2D outgoingFPSControllerCameraTexture;
    }

    [Serializable]
    public class FPSControllerRestoreData
    {
        public float[] restorePosition = new float[3];
        public float[] restoreRotation = new float[4];
        public float[] restoreScale = new float[3];
        public float[] restoreCameraForward = new float[3];

        public static void WriteFPSControllerRestoreDataToDir(string restoreData)
        {
            string writeDir = ManageCameraActions.CameraActionGlobals.savedViewsPath;

            // if the directory doesn't exist, create it
            if (!Directory.Exists(writeDir))
            {
                Directory.CreateDirectory(writeDir);
            }

            // use the screenshot naming convention here
            // since we're saving a screenshot in this dir at the same time
            string fileName = ManageCameraActions.GetInGameScreenshotFileName();

            // save a screenshot here so that a saved view JSON has some visual reference
            TakeScreenshots.CaptureScreenshotOfCurrentCamera(writeDir);

            // write the JSON
            string JSONFilePath = writeDir + ManageCameraActions.GetInGameScreenshotFileName() + ".json";
            File.WriteAllText(JSONFilePath, restoreData);
        }

        public static ManageFPSControllers.FPSControllerRestoreData ReadFPSControllerRestoreDataFromClipboard()
        {
            // get the clipboard data
            string clipBoardData = GUIUtility.systemCopyBuffer;
            ManageFPSControllers.FPSControllerRestoreData restoreData = new ManageFPSControllers.FPSControllerRestoreData();

            restoreData = JsonUtility.FromJson<ManageFPSControllers.FPSControllerRestoreData>(clipBoardData);

            return restoreData;
        }
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
        // record the camera and forward vector of the camera for other scripts to access
        FPSControllerGlobals.activeFPSControllerCamera = this.GetComponentInChildren<Camera>();
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
        // be sure that the time traveling flag is false to prevent unexpected player hoisting behavior
        FPSControllerGlobals.isTimeTraveling = false;

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

                // if zero, no good point was found, so fall back to the nav mesh point
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

    // serialize the FPSController into a JSON object
    public static string GetSerializedFPSControllerRestoreData(GameObject FPSController)
    {
        FPSControllerRestoreData FPSControllerRestoreData = new FPSControllerRestoreData();
        FPSControllerRestoreData CameraRestoreData = new FPSControllerRestoreData();

        Transform FPSControllerTransform = FPSController.transform;
        Camera FPSControllerCamera = FPSController.GetComponentInChildren<Camera>();
        Transform FPSControllerCameraTransform = FPSControllerCamera.transform;

        FPSControllerRestoreData.restorePosition[0] = FPSControllerTransform.position.x;
        FPSControllerRestoreData.restorePosition[1] = FPSControllerTransform.position.y;
        FPSControllerRestoreData.restorePosition[2] = FPSControllerTransform.position.z;

        FPSControllerRestoreData.restoreCameraForward[0] = FPSControllerCamera.transform.forward.x;
        FPSControllerRestoreData.restoreCameraForward[1] = FPSControllerCamera.transform.forward.y;
        FPSControllerRestoreData.restoreCameraForward[2] = FPSControllerCamera.transform.forward.z;

        string serializedFPSControllerData = JsonUtility.ToJson(FPSControllerRestoreData);
        //Utils.DebugUtils.DebugLog("FPS Controller data: " + serializedFPSControllerData);

        FPSControllerGlobals.activeFPSControllerRestoreData = FPSControllerRestoreData;

        return serializedFPSControllerData;
    }

    // reposition and realign this FPSController to match the given one
    public static void RelocateAlignFPSControllerToMatchRestoreData(FPSControllerRestoreData serializedRestoreDataToMatch)
    {
        // be sure that the time traveling flag is false to prevent unexpected player hoisting behavior
        FPSControllerGlobals.isTimeTraveling = false;

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

        // need to make sure the camera transform doesn't include a rotation up or down (causes FPSCharacter to tilt)
        Vector3 currentCameraForward = new Vector3(serializedRestoreDataToMatch.restoreCameraForward[0], serializedRestoreDataToMatch.restoreCameraForward[1], serializedRestoreDataToMatch.restoreCameraForward[2]);
        Vector3 newCameraForward = new Vector3(currentCameraForward.x, 0, currentCameraForward.z);

        Vector3 newPosition = new Vector3(serializedRestoreDataToMatch.restorePosition[0], serializedRestoreDataToMatch.restorePosition[1], serializedRestoreDataToMatch.restorePosition[2]);

        // match the FPSController's position and rotation to the given restore data
        activeFPSController.transform.SetPositionAndRotation(newPosition, Quaternion.LookRotation(newCameraForward, Vector3.up));

        // set the FirstPersonCharacter's camera forward direction
        activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = currentCameraForward;

        // set the FirstPersonCharacter height to the camera height
        //activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, newPosition.y, activeFirstPersonCharacter.transform.position.z);

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

        // restore the existing post process profile
        existingVolume.profile = existingProfile;
    }

    // reposition and realign this FPSController to match the given one
    public static void RelocateAlignFPSControllerToFPSController(Transform FPSControllerTransformToMatch)
    {
        // mark that the player is time-traveling, so gravity gets temporarily disabled
        // in case the next era has no floor there
        FPSControllerGlobals.isTimeTraveling = true;

        // get the current FPSController
        GameObject activeFPSController = FPSControllerGlobals.activeFPSController;
        GameObject activeFirstPersonCharacter = activeFPSController.transform.GetChild(0).gameObject;

        GameObject firstPersonCharacterToMatch = FPSControllerTransformToMatch.transform.GetChild(0).gameObject;

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

        // match the FPSController's position and rotation to the referring controller's position and rotation
        activeFPSController.transform.SetPositionAndRotation(FPSControllerTransformToMatch.position, FPSControllerTransformToMatch.rotation);

        // set the FirstPersonCharacter's camera forward direction
        activeFirstPersonCharacter.GetComponent<Camera>().transform.forward = firstPersonCharacterToMatch.GetComponent<Camera>().transform.forward;

        // reset the FPSController mouse to avoid incorrect rotation due to interference
        activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();

        // set the FirstPersonCharacter height to the camera height
        activeFirstPersonCharacter.transform.position = new Vector3(activeFirstPersonCharacter.transform.position.x, firstPersonCharacterToMatch.transform.position.y, activeFirstPersonCharacter.transform.position.z);

        // hoist the FPSController to the right height
        HoistSceneObjects.HoistObjectOnSceneChange(activeFPSController);

        // restore the existing post process profile
        existingVolume.profile = existingProfile;
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
    public static void EnableCursorLockOnAllFPSControllers()
    {
        foreach(GameObject FPSController in FPSControllerGlobals.allFPSControllers)
        {
            FPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(true);
        }
    }

    // enables mouse lock only on the active FPS controller
    public static void EnableCursorLockOnActiveFPSController()
    {
        FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(true);
    }

    // disables mouse lock only on the active FPS controller
    public static void DisableCursorLockOnActiveFPSController()
    {
        FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(false);
    }

    // enable and disable the behavior that affects the camera when the cursor moves
    // used when an overlay menu is toggled
    public static void EnableFPSCameraControl()
    {
        FPSControllerGlobals.activeFPSController.GetComponent<FirstPersonController>().enabled = true;
    }
    public static void DisableFPSCameraControl()
    {
        FPSControllerGlobals.activeFPSController.GetComponent<FirstPersonController>().enabled = false;
    }

    public static IEnumerator EnableBlurAfterDelay(GameObject activeFPSController, float delay)
    {
        yield return new WaitForSeconds(delay);

        // get the existing post process volume and profile
        PostProcessVolume existingVolume = activeFPSController.GetComponentInChildren<PostProcessVolume>();
        PostProcessProfile existingProfile = existingVolume.profile;
        MotionBlur blur;
        existingProfile.TryGetSettings(out blur);
        blur.enabled.Override(false);
    }

    // waits until the player moves a given max distance before restoring gravity
    // prevents the player from falling when time-traveling to an era with no floor at that location
    public void UpdateFPSControllerGravityByState(Vector3 initialPosition, float maxDistance)
    {
        // only do something if the time traveling flag is set
        // and if the rigid body is currently set to respect gravity
        if (FPSControllerGlobals.isTimeTraveling && this.GetComponent<Rigidbody>().useGravity)
        {
            // every frame, get the position as the player moves,
            // but lock the Y-axis to prevent falling, just in case there's no floor
            Vector3 newPosition = HoistSceneObjects.AdjustPositionForHoistBetweenScenes(new Vector3(this.transform.position.x, FPSControllerGlobals.initialFPSControllerLocation.y, this.transform.position.z), SceneGlobals.lastKnownTimePeriodSceneName, SceneGlobals.upcomingSceneName);

            this.transform.position = newPosition;

            // keep track of the player's distance from the initial position
            float distance = Vector3.Distance(initialPosition, FPSControllerGlobals.activeFPSControllerTransform.position);

            // when the player has moved a bit, turn off the time traveling flag
            // so the next position update won't have the locked Y axis
            if (distance > maxDistance)
            {
                FPSControllerGlobals.isTimeTraveling = false;
            }
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
        EnableCursorLockOnAllFPSControllers();

        // record this initial position so we can freeze the player's Y-position here when gravity is off
        FPSControllerGlobals.initialFPSControllerLocation = this.transform.position;

        // enable blur on the post-processing profile after a slight delay
        StartCoroutine(EnableBlurAfterDelay(this.gameObject, 1));
    }

    private void OnDisable()
    {
        // record this scene as the last known time period scene
        // but not when pausing, which is only loading background scenes for thumbnail generation
        if (!FPSControllerGlobals.isPausing)
        {
            SceneGlobals.lastKnownTimePeriodSceneName = this.gameObject.scene.name;
        }

        // unlock the cursor only when the upcoming scene is a menu (not a time period scene)
        if (SceneGlobals.availableTimePeriodSceneNamesList.IndexOf(SceneGlobals.upcomingSceneName) == -1)
        {
          FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(false);
        }
    }

    private void Update()
    {
        // record certain FPSController data globally for other scripts to access
        UpdateActiveFPSControllerPositionAndCamera();
        UpdateActiveFPSControllerNavMeshData();

        // when the player is time traveling, temporarily pause their gravity in case there's no floor below
        // or restore their gravity if they've moved after time-traveling
        // (skipped if the time-traveling flag isn't set to true)
        UpdateFPSControllerGravityByState(HoistSceneObjects.AdjustPositionForHoistBetweenScenes(FPSControllerGlobals.initialFPSControllerLocation, SceneGlobals.referringSceneName, SceneGlobals.upcomingSceneName), FPSControllerGlobals.maxDistanceBeforeResettingGravity);
    }
}

