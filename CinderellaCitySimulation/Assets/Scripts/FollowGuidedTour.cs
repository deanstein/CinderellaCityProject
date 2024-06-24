using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Responsible for making the FPSController walk a path between meaningful points
/// Must be attached to the NavMeshAgent object of the FPSController
/// </summary>

public class FollowGuidedTour : MonoBehaviour
{
    // this script needs to be a singleton
    public static FollowGuidedTour Instance { get; private set; }

    readonly string partialPathCameraName60s70s = "Blue Mall 1";
    readonly string partialPathCameraName80s90s = "Blue Mall deep";
    readonly int pauseAtCameraDuration = 6; // number of seconds to pause and look at a camera
    readonly bool matchCameraForward = false; // player camera: match destination camera or simply look at it?
    readonly float lookToCameraAtRemainingDistance = 10.0f; // distance from end of path where FPC begins looking at camera
    readonly float adjustPosAwayFromCamera = 1.15f; // distance away from camera look vector so when looking at a camera, it's visible
    readonly public float guidedTourRotationSpeed = 0.4f;
    readonly public int guidedTourRestartAfterSeconds = 5; // seconds to wait before un-pausing
    readonly public float partialPathCameraClosestDistance = 20; // get this close to the partial path camera before giving up

    private NavMeshAgent thisAgent;
    private GameObject[] guidedTourObjects;
    private Vector3[] guidedTourCameraPositions; // raw camera positions
    private Vector3[] guidedTourCameraPositionsAdjusted; // adjusted backward
    private Vector3[] guidedTourFinalNavMeshDestinations; // destinations on NavMesh
    public Camera currentGuidedTourDestinationCamera;
    public Vector3 currentGuidedTourVector;
    private bool isPausingAtCamera = false; // player will pause to look at camera for some amount of time
    private bool? areHistoricPhotosVisible = null;
    public static bool doIncrementIndex = false; // set to false if we should start or restart at the previously-known destination
    public static bool isGuidedTourTimeTravelRequested = false; // set to true briefly to allow IEnumerator time-travel transition
    private bool isResumeRequiredAfterOverride = false;
    private int partialPathCameraIndex = -1; // index of the camera at the name defined below

    // DEBUGGING
    readonly bool shuffleGuidedTourDestinations = true;
    private static int currentGuidedTourDestinationIndex = 0; // optionally start at a specific index
    readonly bool useOverrideDestinations = false; // if true, use a special list for tour objects
    readonly private bool showDebugLines = true; // if true, show path as debug lines
    readonly bool doTestAllPaths = false; // if true, attempt to find paths between all destinations

    private void Awake()
    {
        Instance = this;
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // get and post-process the destinations
        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(this.gameObject.scene.name);

        // log the original indices - this is helpful for next runs
        for (int i = 0; i < guidedTourObjects.Length - 1; i++)
        {
            Debug.Log(guidedTourObjects[i].name + " is at original index " + i);
        }

        // DEBUGGING
        // shuffle the list if requested
        if (shuffleGuidedTourDestinations && !useOverrideDestinations)
        {
            guidedTourObjects = ArrayUtils.ShuffleArray(guidedTourObjects);
        }
        // use the overrides if requested
        if (useOverrideDestinations)
        {
            // USEFUL OVERRIDE PRESETS
            /*** issue where guided tour would hang on image due to since-removed logic (use 80s90s scene) ***/
            //guidedTourObjects = new GameObject[] { guidedTourObjects[13] /* Cinder Alley Zeezo's */, guidedTourObjects[21] /* food court 8 */, guidedTourObjects[31], /*Rose Mall Thom McAn */ /* REQUIRED: index of partial path alt */ guidedTourObjects[3] };
            //guidedTourObjects = new GameObject[] { /*guidedTourObjects[13],*/ guidedTourObjects[21], guidedTourObjects[31], /* REQUIRED: index of partial path alt */ guidedTourObjects[3] };
            // guidedTourObjects = new GameObject[] { guidedTourObjects[3], guidedTourObjects[21], guidedTourObjects[31]  };
            /*** issue where guided tour would hang on tampoline image (use 60s70s scene) ***/
            //guidedTourObjects = new GameObject[] { guidedTourObjects[9], guidedTourObjects[5], guidedTourObjects[15], guidedTourObjects[21], /* REQUIRED: index of partial path alt */ guidedTourObjects[3] };
            /*** issue where partial path happens and index gets incremented like crazy when at partial alt camera ***/
            /*** start FPSController in Rose Mall 80s90s near Woolworth's ***/
            //guidedTourObjects = new GameObject[] { guidedTourObjects[25], guidedTourObjects[3] };
            /*** test whether approaching a camera from the "opposite" way results in bad camera transition ***/
            guidedTourObjects = new GameObject[] { guidedTourObjects[33], guidedTourObjects[3] };
        }

        List<Vector3> cameraPositionsList = new List<Vector3>();
        List<Vector3> cameraPositionsAdjustedList = new List<Vector3>();
        List<Vector3> cameraPositionsAdjustedOnNavMeshList = new List<Vector3>();

        foreach (GameObject guidedTourObject in guidedTourObjects)
        {
            Camera objectCamera = guidedTourObject.GetComponent<Camera>();
            // record the positions of the cameras
            Vector3 objectCameraPos = objectCamera.transform.position;
            cameraPositionsList.Add(objectCameraPos);
            // also record the adjusted positions
            Vector3 objectCameraPosAdjusted = Utils.GeometryUtils.AdjustPositionAwayFromCamera(objectCamera.transform.position, objectCamera, adjustPosAwayFromCamera);
            cameraPositionsAdjustedList.Add(objectCameraPosAdjusted);
            // project the adjusted positions down onto the NavMesh
            Vector3 objectCameraPosAdjustedOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(objectCameraPosAdjusted, 5);
            cameraPositionsAdjustedOnNavMeshList.Add(objectCameraPosAdjustedOnNavMesh);

            Debug.Log(objectCamera.name + " is at index " + (cameraPositionsAdjustedOnNavMeshList.Count - 1).ToString() + " at adjusted position: " + objectCameraPosAdjustedOnNavMesh);

            // also record the index of the alternate camera destination 
            // for use when path is partial
            if (this.gameObject.scene.name == "60s70s" || SceneManager.GetActiveScene().name == SceneGlobals.experimentalSceneName)
            {
                if (objectCamera.name.Contains(partialPathCameraName60s70s))
                {
                    partialPathCameraIndex = cameraPositionsAdjustedOnNavMeshList.Count - 1;
                    Debug.Log("<b>Partial path camera found for 60s70s: " + objectCamera.name + " at index " + partialPathCameraIndex + "</b>");
                }
            } else if (this.gameObject.scene.name == "80s90s")
            {
                if (objectCamera.name.Contains(partialPathCameraName80s90s))
                {
                    partialPathCameraIndex = cameraPositionsAdjustedOnNavMeshList.Count - 1;
                    Debug.Log("<b>Partial path camera found for 80s90s: " + objectCamera.name + " at index " + partialPathCameraIndex + "</b>");
                }
            }

            // start with historic photos on
            ModeState.areHistoricPhotosRequestedVisible = true;

            // DEBUGGING
            // draw debug lines if requested
            if (showDebugLines)
            {
                Debug.DrawLine(objectCameraPos, objectCameraPosAdjusted, Color.red, 1000);
                Debug.DrawLine(objectCameraPosAdjusted, objectCameraPosAdjustedOnNavMesh, Color.blue, 1000);
            }
        }

        // convert the lists into arrays and store them
        guidedTourCameraPositions = cameraPositionsList.ToArray();
        guidedTourCameraPositionsAdjusted = cameraPositionsAdjustedList.ToArray();
        guidedTourFinalNavMeshDestinations = cameraPositionsAdjustedOnNavMeshList.ToArray();

        // DEBUGGING
        // run through the list of destinations if requested
        if (doTestAllPaths)
        {
            for (int i = 0; i < guidedTourObjects.Length - 1; i++)
            {
                GameObject currentObject = guidedTourObjects[i];
                Vector3 currentObjectClosestNavPos = guidedTourFinalNavMeshDestinations[i];
                GameObject nextObject = guidedTourObjects[i + 1];
                Vector3 nextObjectClosestNavPos = guidedTourFinalNavMeshDestinations[i + 1];

                NavMeshPath path = new NavMeshPath();
                bool pathSuccess = NavMesh.CalculatePath(currentObjectClosestNavPos, nextObjectClosestNavPos, NavMesh.AllAreas, path);

                Debug.Log("PATH DEBUGGING: Path success is <b>" + pathSuccess + "</b> between destination " + currentObject.name + " and " + nextObject.name);
            }
        }
    }

    private void OnEnable()
    {
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            // restart the guided tour to ensure new pathfinding happens 
            StartCoroutine(ToggleGuidedTourOnEnable());

            // only spend a certain amount of time in each era
            StopCoroutine(ModeState.toggleToNextEraCoroutine);
            ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
        }
    }

    private void Update()
    {
        // guided tour active, not paused
        if (ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused && thisAgent.enabled)
        {
            // if the player is not on the navmesh, move it to the nearest point
            if (!ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.isOnNavMesh)
            {
                Vector3 nearestHorizontalPoint = Utils.GeometryUtils.GetNearestNavMeshPointHorizontally(thisAgent);

                // use warp to ensure the agent is placed correctly given the point
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.Warp(nearestHorizontalPoint);
            }

            // we've arrived, new path requested
            if (!thisAgent.pathPending && thisAgent.remainingDistance <= thisAgent.stoppingDistance && (!thisAgent.hasPath || thisAgent.velocity.sqrMagnitude == 0f))
            {
                // increment the next destination index if appropriate
                if (doIncrementIndex && ModeState.setAgentOnPathAfterDelayRoutine == null && ModeState.restartGuidedTourCoroutine == null || Vector3.Distance(Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]) < thisAgent.stoppingDistance)
                {
                    // increment the index or start over
                    IncrementGuidedTourIndex();

                    Debug.Log("FollowGuidedTour: Incrementing index. New destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                    doIncrementIndex = false;
                }

                // set the agent on a new path after a pause
                if ((thisAgent.velocity == Vector3.zero && !isPausingAtCamera) || 
                    (thisAgent.velocity == Vector3.zero && ModeState.setAgentOnPathAfterDelayRoutine == null && !thisAgent.hasPath) || 
                    (thisAgent.velocity == Vector3.zero && ModeState.setAgentOnPathAfterDelayRoutine == null && thisAgent.remainingDistance <= thisAgent.stoppingDistance))
                {
                    Debug.Log("FollowGuidedTour: Pausing at camera OR starting/resuming, setting path after delay. Next destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                    // the exact delay depends on whether we're atually pausing at a camera
                    // or merely starting fresh or resuming after time-traveling
                    float delayDuration = isPausingAtCamera ? pauseAtCameraDuration : 0.25f;
                    Debug.Log("Requested delay: " + delayDuration);
                    ModeState.setAgentOnPathAfterDelayRoutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], delayDuration, true, showDebugLines));
                }
            }

            // if the path is partial, try setting it again in a few moments
            // this could due to the path being very long
            if ((thisAgent.pathStatus == NavMeshPathStatus.PathPartial || thisAgent.pathStatus == NavMeshPathStatus.PathInvalid) && ModeState.setAgentOnPathAfterDelayRoutine == null)
            {
                // log an error if the camera index isn't valid
                if (partialPathCameraIndex == -1)
                {
                    Debug.LogError("FollowGuidedTour: Path is partial, and we need to go to the partial camera temporarily, but the provided partial camera index was not found!");
                }
                else
                {
                    // check that the temporary path to the partialPath camera itself is valid
                    NavMeshPath temporaryPath = new NavMeshPath();
                    bool temporaryPathValid = NavMesh.CalculatePath(Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[partialPathCameraIndex], NavMesh.AllAreas, temporaryPath);

                    // if we're far enough away from the partial camera,
                    // and if the path to the partial path camera itself is valid,
                    // keep trying
                    if (Vector3.Distance(Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[partialPathCameraIndex]) > partialPathCameraClosestDistance && temporaryPathValid && temporaryPath.status == NavMeshPathStatus.PathComplete)
                    {
                        Debug.LogWarning("FollowGuidedTour: Path is " + thisAgent.pathStatus + ", will try to repath in a few moments. Current destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);

                        // set the immediate destination to the alternate camera in case of partial path
                        NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[partialPathCameraIndex]);

                        // try setting the agent to the original index in a few moments
                        ModeState.setAgentOnPathAfterDelayRoutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], 10, false, showDebugLines));
                    } else
                    // otherwise, this path simply cannot be used as it continues to be partial
                    // so go to the next index
                    {
                        Debug.LogWarning("FollowGuidedTour: Either the path to the partialPath camera itself is invalid or partial, or the path continues to be " + thisAgent.pathStatus + " and we're now within  " + partialPathCameraClosestDistance + " units, so giving up and incrementing index.");
                        IncrementGuidedTourIndex();
                        Debug.Log("FollowGuidedTour: New destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                        NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);
                    }
                }
            }


            // start pausing at the camera when we're less than the stopping distance away
            if (!thisAgent.pathPending && thisAgent.remainingDistance < 0.1f)
            {
                isPausingAtCamera = true;
            }
            else
            {
                isPausingAtCamera = false;
            }

            // only update the vector if we're not pausing at a camera
            if (!isPausingAtCamera && !ModeState.isGuidedTourPaused)
            {
                // store the current camera destination
                currentGuidedTourDestinationCamera = guidedTourObjects[currentGuidedTourDestinationIndex].GetComponent<Camera>();
                Vector3 currentGuidedTourCameraPosition = guidedTourCameraPositions[currentGuidedTourDestinationIndex];

                // if we're on the last several feet of path, current tour vector is that camera forward dir
                if (thisAgent.remainingDistance < lookToCameraAtRemainingDistance && thisAgent.remainingDistance > thisAgent.stoppingDistance)
                {
                    // match the camera forward angle 
                    if (matchCameraForward)
                    {
                        currentGuidedTourVector = currentGuidedTourDestinationCamera.transform.forward;
                    }
                    else // or simply look at the image, but no y-component
                    {
                        // distance along camera plane to look to
                        // this should match the FormIt camera distance in the Match Photo plugin
                        float distanceToPlane = 5f;

                        // Define the viewport center point (0.5, 0.5 is the center in viewport coordinates)
                        Vector3 cameraViewportCenter = new Vector3(0.5f, 0.5f, distanceToPlane);

                        // Convert the center point from viewport space to world space
                        Vector3 cameraViewportCenterWorld = currentGuidedTourDestinationCamera.ViewportToWorldPoint(cameraViewportCenter);

                        Vector3 tempVector = cameraViewportCenterWorld - thisAgent.transform.position;
                        Vector3 tempVectorNoY = new Vector3(tempVector.x, 0, tempVector.z);

                        currentGuidedTourVector = tempVectorNoY;
                    }
                }
                // if the remaining distance is less than the stopping distance, look directly at the camera
                else if (thisAgent.remainingDistance <= thisAgent.stoppingDistance)
                {
                    // distance along camera plane to look to
                    // this should match the FormIt camera distance in the Match Photo plugin
                    float distanceToPlane = 5f;

                    // Define the viewport center point (0.5, 0.5 is the center in viewport coordinates)
                    Vector3 cameraViewportCenter = new Vector3(0.5f, 0.5f, distanceToPlane);

                    // Convert the center point from viewport space to world space
                    Vector3 cameraViewportCenterWorld = currentGuidedTourDestinationCamera.ViewportToWorldPoint(cameraViewportCenter);

                    currentGuidedTourVector = cameraViewportCenterWorld - thisAgent.transform.position;
                }
                // otherwise, current path vector is the agent's velocity,
                // but with no vertical component
                else
                {
                    currentGuidedTourVector = new Vector3(thisAgent.velocity.x, 0, thisAgent.velocity.z);
                }
            }


            // the current guided tour vector is 
            // either the path vector or the camera direction if on last leg of path
            // so set the current FPSController direction and camera to that vector
            if (currentGuidedTourVector != Vector3.zero)
            {
                // controller gets the camera forward but with no y-value to avoid tilt
                Vector3 currentControllerDirection = new Vector3(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCameraForward.x, 0, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCameraForward.z);
                // determine the target controller and camera directions
                Vector3 targetControllerDirection = new Vector3(currentGuidedTourVector.x, 0, currentGuidedTourVector.z);

                Vector3 currentCameraDirection = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCameraForward;
                Vector3 targetCameraDirection = currentGuidedTourVector;

                // adjust the controller direction with slerp
                Vector3 slerpControllerForward = Vector3.Slerp(currentControllerDirection, targetControllerDirection, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward = slerpControllerForward;

                // adjust the camera direction with slerp
                Vector3 cameraSlerpForward = Vector3.Slerp(currentCameraDirection, targetCameraDirection, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward = cameraSlerpForward;

                // reset the FPSController mouse to avoid incorrect rotation due to interference
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();
            }

            // auto-time travel during guided tour
            if (isGuidedTourTimeTravelRequested)
            {
                // immediately set the flag back to false
                isGuidedTourTimeTravelRequested = false;

                string nextTimePeriodSceneName = ManageScenes.GetNextTimePeriodSceneName("next");

                StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(SceneManager.GetActiveScene().name, nextTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform, ManageCameraActions.CameraActionGlobals.activeCameraHost, "FlashBlack", 0.2f));

                // indicate to the next scene that it needs to recalc its cameras and paths
                SceneGlobals.isGuidedTourTimeTraveling = true;
            }
        }

        // start or stop the restart routines if override is requested
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (GetIsGuidedTourOverrideRequested())
            {
                // record that the override has been requested
                isResumeRequiredAfterOverride = true;

                // stop the active countdown coroutine if there is one
                if (ModeState.restartGuidedTourCoroutine != null)
                {
                    StopCoroutine(ModeState.restartGuidedTourCoroutine);
                }
                if (ModeState.toggleToNextEraCoroutine != null)
                {
                    StopCoroutine(ModeState.toggleToNextEraCoroutine);
                }
            }

            // if override is no longer requested, but override was previously requested
            // then ensure the restart coroutines happen now, just one time
            if (!GetIsGuidedTourOverrideRequested() && isResumeRequiredAfterOverride)
            {
                ModeState.restartGuidedTourCoroutine = StartCoroutine(GuidedTourRestartCountdown());
                ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());

                // indicate that we've restarted the coroutines since last request
                isResumeRequiredAfterOverride = false;
            }

            // if the player is trying to walk,
            // temporarily pause the guided tour
            if (GetIsGuidedTourOverrideRequested())
            {
                ModeState.isGuidedTourActive = false;
                ModeState.isGuidedTourPaused = true;
            }
        }

        // provide emergency override to change the next destination
        if (Input.GetKeyDown("."))
        {
            IncrementGuidedTourIndex();
            NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);
        }
        if (Input.GetKeyDown(","))
        {
            DecrementGuidedTourIndex();
            NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);
        }

        // during guided tour, historic photos should
        // only be visible when within some distance to the next destination
        // this is possibly expensive so only do it one frame only when requested
        if (ModeState.isGuidedTourActive && thisAgent.enabled && thisAgent.isOnNavMesh)
        {
            if (thisAgent.remainingDistance < lookToCameraAtRemainingDistance)
            {
                ModeState.areHistoricPhotosRequestedVisible = true;
            }
            else
            {
                ModeState.areHistoricPhotosRequestedVisible = false;
            }
        }

        // only force visibility if guided tour mode is on, 
        // and the local flag doesn't match the global flag
        // or if the local flag hasn't been set yet
        if (ModeState.isGuidedTourActive && areHistoricPhotosVisible == null || areHistoricPhotosVisible != ModeState.areHistoricPhotosRequestedVisible)
        {
            // enable or disable the historic photos
            GameObject historicCamerasContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];
            ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicCamerasContainer, ModeState.areHistoricPhotosRequestedVisible, false);
            ObjectVisibility.SetHistoricPhotosOpaque(ModeState.areHistoricPhotosRequestedVisible);

            // set the local flag to match so this only runs once
            areHistoricPhotosVisible = ModeState.areHistoricPhotosRequestedVisible;
        }
    }

    public static void StartGuidedTourMode()
    {
        Utils.DebugUtils.DebugLog("Starting guided tour mode...");

        // set the mode state
        ModeState.isGuidedTourActive = true;

        // automatically switch to the next era after some time
        ModeState.toggleToNextEraCoroutine = Instance.StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
    }

    public static void EndGuidedTourMode()
    {
        Utils.DebugUtils.DebugLog("Ending guided tour mode.");

        // set the mode state
        ModeState.isGuidedTourActive = false;
        ModeState.isGuidedTourPaused = false;

        // if there was a restart coroutine active, stop it
        if (ModeState.restartGuidedTourCoroutine != null)
        {
            Instance.StopCoroutine(ModeState.restartGuidedTourCoroutine);
        }
        if (ModeState.toggleToNextEraCoroutine != null)
        {
            Instance.StopCoroutine(ModeState.toggleToNextEraCoroutine);
        }
    }

    // increment or start over depending on the current index
    public static void IncrementGuidedTourIndex()
    {
        currentGuidedTourDestinationIndex = (currentGuidedTourDestinationIndex + 1) % Instance.guidedTourObjects.Length;
    }

    public static void IncrementGuidedTourIndexAndSetAgentOnPath()
    {
        IncrementGuidedTourIndex();
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instance.guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);
    }

    // decrement or go to the end depending on the current index
    public static void DecrementGuidedTourIndex()
    {
        currentGuidedTourDestinationIndex = (currentGuidedTourDestinationIndex + Instance.guidedTourObjects.Length - 1) % Instance.guidedTourObjects.Length;
    }

    public static void DecrementGuidedTourIndexAndSetAgentOnPath()
    {
        DecrementGuidedTourIndex();
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instance.guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);
    }

    // when guided tour is active, this checks if the user is trying to override control
    public bool GetIsGuidedTourOverrideRequested()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || InputActions.rightStickLook.x != 0 || InputActions.rightStickLook.y != 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    // before un-pausing guided tour mode,
    // wait a few seconds
    IEnumerator GuidedTourRestartCountdown()
    {
        yield return new WaitForSeconds(guidedTourRestartAfterSeconds);
        ModeState.isGuidedTourActive = true;
        ModeState.isGuidedTourPaused = false;
    }

    IEnumerator ToggleGuidedTourOnEnable()
    {
        ModeState.isGuidedTourActive = false;
        ModeState.isGuidedTourPaused = true;
        yield return new WaitForSeconds(1f);
        ModeState.isGuidedTourActive = true;
        ModeState.isGuidedTourPaused = false;
    }
}