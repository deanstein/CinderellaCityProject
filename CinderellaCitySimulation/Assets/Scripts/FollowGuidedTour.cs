using System;
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
    // the two instances of this script need to be held in a dictionary
    // then accessed by scene name, to avoid "crossing the wires" between singleton eras
    public static Dictionary<string, FollowGuidedTour> Instances { get; private set; } = new Dictionary<string, FollowGuidedTour>();

    // get the partial path index of the current partial path camera in the list of cameras
    private int partialPathCameraIndex = -1;

    // number of seconds to pause and look at a camera
    readonly float pauseAtCameraDuration = 10f;
    // seconds to wait before un-pausing
    readonly float guidedTourRestartAfterSeconds = 4f;
    // guided tour can be stationary only for the durations specified above
    // keep track of the stationary time so we can force an advance if max stationary times are exceeded
    float stationaryTimeActive = 0f;
    float stationaryTimePaused = 0f;

    // should the player camera match the destination camera or simply look toward it?
    readonly bool matchCameraForward = false;
    // distance from end of path where before camera begins looking at destination camera
    readonly float lookToCameraAtRemainingDistance = 10.0f;
    // distance from the end of the path where the photo turns on
    readonly float hidePeopleAtRemainingDistance = 3.0f;
    // distance (m) away from camera look vector so when looking at a camera, it's visible
    readonly float adjustPosAwayFromCamera = 1.15f; 
    readonly public float guidedTourRotationSpeed = 0.4f;
    // get this close to the partial path camera before giving up
    readonly public float partialPathCameraClosestDistance = 20;

    private NavMeshAgent thisAgent;
    private GameObject[] guidedTourObjects;
    private Vector3[] guidedTourCameraPositions; // raw camera positions
    private Vector3[] guidedTourCameraPositionsAdjusted; // adjusted backward
    private Vector3[] guidedTourFinalNavMeshDestinations; // destinations on NavMesh
    public Camera currentGuidedTourDestinationCamera;
    public Vector3 currentGuidedTourVector;

    // object visibility during guided tour
    // these are initially set in ModeState.cs
    private bool? areHistoricPhotosVisible = null;
    private bool? arePeopleVisible = null;

    // set to true briefly to allow IEnumerator time-travel transition
    public static bool isGuidedTourTimeTravelRequested = false; 
    private bool isResumeRequiredAfterOverride = false;

    //
    // DEBUGGING
    //

    // show paths and camera positions as debug lines
    public bool showDebugLines = false;
    readonly int debugLineDuration = 10; // seconds
    // shuffle the destinations if requested
    readonly bool shuffleGuidedTourDestinations = true && ModeState.shuffleDestinations && !useDebuggingDestinations;
    // used for debugging shuffled path results
    public int? shuffleSeed = null;
    // start the guided tour at this index
    private int currentGuidedTourDestinationIndex = 0;
    // use a special destination list for debugging
    static readonly bool useDebuggingDestinations = false; // if true, use a special list for tour objects
    public bool doTestAllPaths = false; // if true, attempt to find paths between all destinations

    private void Awake()
    {
        // fill out the Instances dictionary, so each scene has its own instance
        string thisSceneName = gameObject.scene.name;
        // check if an instance already exists for this scene
        if (Instances.ContainsKey(thisSceneName))
        {
            // if an instance already exists, destroy this one
            Destroy(gameObject);
        }
        else
        {
            // if no instance exists for this scene, add this one to the dictionary
            Instances.Add(thisSceneName, this);
            DontDestroyOnLoad(gameObject);
        }

        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // this code may be called from CCPMenuActions for debugging purposes
        // in which case we need to define the agent if not defined already
        if (!thisAgent)
        {
            thisAgent = GetComponent<NavMeshAgent>();
        }

        // get and post-process the destinations
        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(this.gameObject.scene.name);

        // log the original indices - this is helpful for next runs
        for (int i = 0; i < guidedTourObjects.Length; i++)
        {
            Debug.Log(guidedTourObjects[i].name + " is at original index " + i);
        }

        //
        // OPTIONAL OVERRIDES
        //

        // shuffle the list if requested
        if (shuffleGuidedTourDestinations && !useDebuggingDestinations)
        {
            // use the seed if it's provided
            if (shuffleSeed != null)
            {
                // for some reason, this is required
                // to avoid a C# error about shuffleSeed maybe not being defined
                int seed = shuffleSeed ?? DateTime.Now.Millisecond;
                guidedTourObjects = ArrayUtils.ShuffleArray(guidedTourObjects, seed);
            } else
            {
                // otherwise shuffle without a seed
                guidedTourObjects = ArrayUtils.ShuffleArray(guidedTourObjects);
            }
            
        }

        // use the debugging overrides if requested
        if (useDebuggingDestinations)
        {
            guidedTourObjects = ManageSceneObjects.ProxyObjects.FindDebuggingGuidedTourObjects(guidedTourObjects);
        }

        // if we're not shuffling destinations, get the curated list
        if (!ModeState.shuffleDestinations)
        {
            // get the list of curated guided tour objects for RecordingMode
            guidedTourObjects = ManageSceneObjects.ProxyObjects.FindAllCuratedGuidedTourObjects(guidedTourObjects);
        }

        // by now, the final guidedTourObjects are in the right order
        // so get the partial path camera index
        partialPathCameraIndex = ManageSceneObjects.ProxyObjects.FindGuidedTourPartialPathCameraIndex(guidedTourObjects);

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
            Vector3 objectCameraPosAdjustedOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(objectCameraPosAdjusted, thisAgent.height);
            cameraPositionsAdjustedOnNavMeshList.Add(objectCameraPosAdjustedOnNavMesh);

            Debug.Log(objectCamera.name + " is at index " + (cameraPositionsAdjustedOnNavMeshList.Count - 1).ToString() + " at adjusted position: " + objectCameraPosAdjustedOnNavMesh);

            // start with historic photos on
            ModeState.areHistoricPhotosRequestedVisible = true;
            // start with people visible
            ModeState.arePeopleRequestedVisible = true;

            // DEBUGGING
            // draw debug lines if requested
            if (showDebugLines)
            {
                DebugUtils.DrawLine(objectCameraPos, objectCameraPosAdjusted, Color.red);
                DebugUtils.DrawLine(objectCameraPosAdjusted, objectCameraPosAdjustedOnNavMesh, Color.blue);
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
            for (int i = 0; i < guidedTourObjects.Length; i++)
            {
                GameObject currentObject = guidedTourObjects[i];
                Vector3 currentObjectClosestNavPos = guidedTourFinalNavMeshDestinations[i];
                GameObject nextObject = guidedTourObjects[(i + 1) % guidedTourObjects.Length];
                Vector3 nextObjectClosestNavPos = guidedTourFinalNavMeshDestinations[(i + 1) % guidedTourObjects.Length];

                NavMeshPath path = new NavMeshPath();
                bool isValidPath = NavMesh.CalculatePath(currentObjectClosestNavPos, nextObjectClosestNavPos, NavMesh.AllAreas, path);
                NavMeshPathStatus pathStatus = path.status;

                if (isValidPath)
                {
                    // optional: visualize the path with a line in the editor
                    if (showDebugLines)
                    {
                        // draw complete paths in green
                        if (pathStatus == NavMeshPathStatus.PathComplete)
                        {
                            DebugUtils.DrawDebugPathGizmo(path, Color.green);
                            Debug.Log("PATH DEBUGGING: Path success is <b>" + isValidPath + "</b>" + " with path status:" + pathStatus + " between destination " + currentObject.name + " and " + nextObject.name);
                        }
                        // draw partial or invalid paths in yellow (assuming there are any segments)
                        else
                        {
                            DebugUtils.DrawDebugPathGizmo(path, Color.yellow);
                            Debug.LogWarning("PATH DEBUGGING: Path success is <b>" + isValidPath + "</b>" + " with path status:" + pathStatus + " between destination " + currentObject.name + " and " + nextObject.name);
                        }
                    }
                }
                else
                {
                    Debug.LogError("PATH DEBUGGING: Path success is <b>" + isValidPath + "</b>" + " with path status:" + pathStatus + " between destination " + currentObject.name + " and " + nextObject.name);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            // restart the guided tour to ensure new pathfinding happens 
            ModeState.isGuidedTourActive = false;
            ModeState.isGuidedTourPaused = true;

            // only spend a certain amount of time in each era
            if (ModeState.autoTimeTravel)
            {
                StopCoroutine(ModeState.toggleToNextEraCoroutine);
                ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
            }
        }
    }

    private void Update()
    {
        // need to calculate whether we've arrived manually
        // since Unity's NavMeshAgent.remainingDistance may return 0 unexpectedly
        float calculatedRemainingDistance = Vector3.Distance(thisAgent.nextPosition, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex]);

        // guided tour active, not paused
        if (ModeState.isGuidedTourActive && 
            !ModeState.isGuidedTourPaused && 
            thisAgent.enabled)
        {
            // if the player is not on the navmesh, move it to the nearest point
            if (!thisAgent.isOnNavMesh)
            {
                Debug.LogWarning("Agent was found not on NavMesh. Relocating...");
                Vector3 nearestHorizontalPoint = Utils.GeometryUtils.GetNearestNavMeshPointHorizontally(thisAgent);
                Vector3 nearestHorizontalNavMeshPointAtControllerHeight = new Vector3(nearestHorizontalPoint.x, ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position.y, nearestHorizontalPoint.z);

                // move the FPSController and the agent to the closest point
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position = nearestHorizontalNavMeshPointAtControllerHeight;
                // use warp for the agent
                thisAgent.Warp(nearestHorizontalNavMeshPointAtControllerHeight);

                NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], showDebugLines);
            }
            // otherwise, ensure the agent has a path on the nav mesh if it doesn't already have apath
            // and if its path isn't partial or invalid (those are handled below)
            else if (!thisAgent.hasPath && thisAgent.pathStatus != NavMeshPathStatus.PathPartial && thisAgent.pathStatus != NavMeshPathStatus.PathPartial)
            {
                NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], showDebugLines);
            }  

            // if the path is partial, try setting it again in a few moments
            // this could due to the path being very long
            if ((thisAgent.pathStatus == NavMeshPathStatus.PathPartial || 
                thisAgent.pathStatus == NavMeshPathStatus.PathInvalid))
            {
                // log an error if the camera index isn't valid
                if (partialPathCameraIndex == -1)
                {
                    Debug.LogError("FollowGuidedTour: Path is partial, and we need to go to the partial camera temporarily, but the provided partial camera index was not found!");
                }
                // camera is valid
                else
                {
                    // check that the temporary path to the partialPath camera itself is valid
                    NavMeshPath temporaryPath = new NavMeshPath();
                    bool temporaryPathValid = NavMesh.CalculatePath(thisAgent.transform.position, guidedTourFinalNavMeshDestinations[partialPathCameraIndex], NavMesh.AllAreas, temporaryPath);

                    // if we're far enough away from the partial camera,
                    // and if the path to the partial path camera itself is valid,
                    // keep trying
                    if (Vector3.Distance(thisAgent.transform.position, guidedTourFinalNavMeshDestinations[partialPathCameraIndex]) > partialPathCameraClosestDistance && 
                        temporaryPathValid && temporaryPath.status == NavMeshPathStatus.PathComplete)
                    {
                        Debug.LogWarning("FollowGuidedTour: Path is " + thisAgent.pathStatus + ", will try to repath in a few moments. Current destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);

                        // set the immediate destination to the alternate camera in case of partial path
                        NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[partialPathCameraIndex]);

                        // try setting the agent to the original index in a few moments
                        ModeState.setAgentOnPathAfterDelayCoroutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], 10, false, showDebugLines));
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

            // only update the guiided tour vector if the mode is not paused, and we're moving
            if (!ModeState.isGuidedTourPaused && thisAgent.velocity.sqrMagnitude > 0f)
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
                // slerp between the current and target directions
                Vector3 controllerSlerpForward = Vector3.Slerp(currentControllerDirection, targetControllerDirection, Time.deltaTime * guidedTourRotationSpeed);
                // apply the slerp
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward = controllerSlerpForward;

                // get the current and target camera rotations
                Quaternion currentCameraRotation = Quaternion.LookRotation(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCameraForward);
                Quaternion targetCameraRotation = Quaternion.LookRotation(currentGuidedTourVector);
                // slerp between the current and target rotations
                Quaternion slerpedRotation = Quaternion.Slerp(currentCameraRotation, targetCameraRotation, Time.deltaTime * guidedTourRotationSpeed);
                // apply the slerp
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.rotation = slerpedRotation;

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

        // guided tour can only stop for a certain amount of time before we proceed
        // but the pause duration will differ between unpaused and paused states

        // not paused
        if ((thisAgent.remainingDistance < 0.01f || thisAgent.velocity.magnitude < 0.01f) && !ModeState.isGuidedTourPaused && ModeState.isGuidedTourActive)
        {
            stationaryTimeActive += Time.deltaTime;
        }
        else
        {
            stationaryTimeActive = 0f;
        }
        if (stationaryTimeActive >= pauseAtCameraDuration)
        {
            Debug.Log("FPSAgent has been stationary with guided tour ACTIVE for " + (pauseAtCameraDuration) + " seconds or more. Going to next destination.");
            IncrementGuidedTourIndexAndSetAgentOnPath();
            stationaryTimeActive = 0f;
        }

        // paused
        if (ModeState.isGuidedTourPaused && !GetIsGuidedTourOverrideRequested() && (thisAgent.velocity.magnitude < 0.01f || thisAgent.remainingDistance == Mathf.Infinity))
        {
            stationaryTimePaused += Time.deltaTime;
        }
        else
        {
            stationaryTimePaused = 0f;
        }

        if (stationaryTimePaused >= guidedTourRestartAfterSeconds)
        {
            Debug.Log("FPSAgent has been stationary with guided tour PAUSED for " + (guidedTourRestartAfterSeconds) + " seconds or more. Resuming guided tour.");
            ModeState.isGuidedTourPaused = false;
            ModeState.isGuidedTourActive = true;
            stationaryTimePaused = 0f;
        }

        // start or stop the restart routines if override is requested
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (GetIsGuidedTourOverrideRequested())
            {
                if (ModeState.autoTimeTravel)
                {
                    // record that the override has been requested
                    isResumeRequiredAfterOverride = true;

                    if (ModeState.toggleToNextEraCoroutine != null)
                    {
                        // stop the coroutine so it starts over
                        StopCoroutine(ModeState.toggleToNextEraCoroutine);
                    }
                }
            }

            // if override is no longer requested, but override was previously requested
            // then ensure the restart coroutines happen now, just one time
            if (!GetIsGuidedTourOverrideRequested() && isResumeRequiredAfterOverride && ModeState.autoTimeTravel)
            {
                // start the coroutine again
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

        // provide a keyboard override to change the next destination
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
        // this is possibly expensive, so only do it one frame only when requested
        if (ModeState.isGuidedTourActive && thisAgent.enabled && thisAgent.isOnNavMesh)
        {
            if (calculatedRemainingDistance < lookToCameraAtRemainingDistance)
            {
                ModeState.areHistoricPhotosRequestedVisible = true;
            }
            else
            {
                ModeState.areHistoricPhotosRequestedVisible = false;
            }
        }

        // similarly, people should be hidden when within some distance to next destination
        // this is possibly expensive, so only do it one frame when requested
        if (ModeState.isGuidedTourActive && thisAgent.enabled && thisAgent.isOnNavMesh)
        {
            if (calculatedRemainingDistance < hidePeopleAtRemainingDistance)
            {
                ModeState.arePeopleRequestedVisible = false;
            }
            else
            {
                ModeState.arePeopleRequestedVisible = true;
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

        // similar for people - only adjust their visibility once if requested
        if (ModeState.isGuidedTourActive && arePeopleVisible == null || arePeopleVisible != ModeState.arePeopleRequestedVisible)
        {
            ObjectVisibility.SetPeopleVisibility(ModeState.arePeopleRequestedVisible);

            // set the local flag to match so this only runs once
            arePeopleVisible = ModeState.arePeopleRequestedVisible;
        }
    }

    public static void StartGuidedTourMode()
    {
        DebugUtils.DebugLog("Starting guided tour mode...");

        // set the mode state
        ModeState.isGuidedTourActive = true;

        // automatically switch to the next era after some time
        if (ModeState.autoTimeTravel)
        {
            ModeState.toggleToNextEraCoroutine = Instances[SceneManager.GetActiveScene().name].StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
        }
    }

    public static void EndGuidedTourMode()
    {
        DebugUtils.DebugLog("Ending guided tour mode.");

        // set the mode state
        ModeState.isGuidedTourActive = false;
        ModeState.isGuidedTourPaused = false;

        if (ModeState.toggleToNextEraCoroutine != null && ModeState.autoTimeTravel)
        {
            Instances[SceneManager.GetActiveScene().name].StopCoroutine(ModeState.toggleToNextEraCoroutine);
        }
    }

    // increment or start over depending on the current index
    public static void IncrementGuidedTourIndex()
    {
        Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex = (Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex + 1) % Instances[SceneManager.GetActiveScene().name].guidedTourObjects.Length;
        DebugUtils.DebugLog("Incrementing destination index. New destination: " + Instances[SceneManager.GetActiveScene().name].guidedTourObjects[Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex].name);
    }

    public static void IncrementGuidedTourIndexAndSetAgentOnPath()
    {
        IncrementGuidedTourIndex();
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instances[SceneManager.GetActiveScene().name].guidedTourFinalNavMeshDestinations[Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex]);
    }

    // decrement or go to the end depending on the current index
    public static void DecrementGuidedTourIndex()
    {
        Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex = (Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex + Instances[SceneManager.GetActiveScene().name].guidedTourObjects.Length - 1) % Instances[SceneManager.GetActiveScene().name].guidedTourObjects.Length;
        DebugUtils.DebugLog("Decrementing destination index. New destination: " + Instances[SceneManager.GetActiveScene().name].guidedTourObjects[Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex].name);
    }

    public static void DecrementGuidedTourIndexAndSetAgentOnPath()
    {
        DecrementGuidedTourIndex();
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instances[SceneManager.GetActiveScene().name].guidedTourFinalNavMeshDestinations[Instances[SceneManager.GetActiveScene().name].currentGuidedTourDestinationIndex]);
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
}