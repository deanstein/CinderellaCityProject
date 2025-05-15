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
    // number of photos to view before periodic time-traveling
    private readonly int periodicTimeTravelAfterPhotoCount = 4;

    // number of seconds to pause and look at a camera
    readonly float pauseAtCameraDuration = 10f;
    // number of seconds to pause before resuming
    readonly float pauseAtEnableOrResumeDuration = 4f;
    // number of seconds to pause for time-travel peeking
    readonly float pauseAtTimeTravelPeekDuration = 6f;
    // guided tour can be stationary only for the durations specified above
    // keep track of the stationary time so we can force an advance if max stationary times are exceeded
    float stationaryTimeActive = 0f;
    float stationaryTimePaused = 0f;

    // should the player camera match the destination camera or simply look toward it?
    readonly bool matchCameraForward = false;
    // distance from end of path where before camera begins looking at destination camera
    readonly float lookToCameraAtRemainingDistance = 8.0f;
    // distance from the end of the path where the photo turns on
    readonly float hidePeopleAtRemainingDistance = 8.0f;
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
    private float calculatedRemainingDistance; // uses remaining path segments

    // set to true briefly to allow IEnumerator time-travel transition
    public static bool isGuidedTourTimeTravelRequested = false;
    // set to true to force an update of historic photo and people visibility
    private bool isProxyObjectVisibilityUpdateRequired = false;

    //
    // DEBUGGING
    //

    // show paths and camera positions as debug lines
    public bool showDebugLines = false;
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
        string thisSceneName = gameObject.scene.name;
        // fill out the Instances dictionary, so each scene has its own instance
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
            calculatedRemainingDistance = NavMeshUtils.GetAgentRemainingDistanceAlongPath(thisAgent);

            // determine if we're at the current photo, within some tolerance
            bool isAtDestination = calculatedRemainingDistance < thisAgent.height;

            // restart the guided tour to ensure new pathfinding happens 
            ModeState.isGuidedTourActive = false;
            ModeState.isGuidedTourPaused = true;

            // set the initial visibility of historic photos and people
            // when peeking, nothing should be visible
            if (ModeState.isTimeTravelPeeking)
            {
                ModeState.areHistoricPhotosRequestedVisible = false;
                ModeState.arePeopleRequestedVisible = false;
            }
            // if we were just previously periodic time-traveling, hide people
            else if (ModeState.isPeriodicTimeTraveling)
            {
                ModeState.arePeopleRequestedVisible = false;
            }
            // the default is to have people visible only
            else
            {
                ModeState.areHistoricPhotosRequestedVisible = false;
                ModeState.arePeopleRequestedVisible = true;
            }

            // force the proxy objects to be updated
            isProxyObjectVisibilityUpdateRequired = true;

            // if we're at the destination when enabled, and peek is not active, proceed to the next photo
            // (this likely happened because we're resuming after time-travel peeking)
            if (!ModeState.isTimeTravelPeeking && isAtDestination)
            {
                stationaryTimePaused = pauseAtEnableOrResumeDuration;
                IncrementGuidedTourIndexAndSetAgentOnPath(thisAgent.gameObject.scene.name);
            }
        }
    }

    private void OnDisable()
    {
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (ModeState.autoTimeTravelPeek)
            {
                ModeState.areHistoricPhotosRequestedVisible = false;
                ModeState.arePeopleRequestedVisible = false;
                isProxyObjectVisibilityUpdateRequired = true;
                UpdateProxyObjectVisibility();
            }
        }
    }

    private void Update()
    {
        // guided tour active, not paused
        if (ModeState.isGuidedTourActive && 
            !ModeState.isGuidedTourPaused && 
            thisAgent.enabled &&
            !ModeState.isPeriodicTimeTraveling &&
            !ModeState.isTimeTravelPeeking)
        {
            // need to calculate whether we've arrived manually
            // since Unity's NavMeshAgent.remainingDistance may return 0 unexpectedly
            calculatedRemainingDistance = NavMeshUtils.GetAgentRemainingDistanceAlongPath(thisAgent); ;

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

                NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex], showDebugLines);
            }
            // otherwise, ensure the agent has a path on the nav mesh if it doesn't already have a path
            // and if its path isn't partial or invalid (those are handled below)
            else if (!thisAgent.hasPath && thisAgent.pathStatus != NavMeshPathStatus.PathPartial && thisAgent.pathStatus != NavMeshPathStatus.PathPartial)
            {
                NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex], showDebugLines);
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
                        Debug.LogWarning("FollowGuidedTour: Path is " + thisAgent.pathStatus + ", will try to repath in a few moments. Current destination: " + guidedTourObjects[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex].name + " at index: " + Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex);

                        // set the immediate destination to the alternate camera in case of partial path
                        NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[partialPathCameraIndex]);

                        // try setting the agent to the original index in a few moments
                        ModeState.setAgentOnPathAfterDelayCoroutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, thisAgent.transform.position, guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex], 10, false, showDebugLines));
                    } else
                    // otherwise, this path simply cannot be used as it continues to be partial
                    // so go to the next index
                    {
                        Debug.LogWarning("FollowGuidedTour: Either the path to the partialPath camera itself is invalid or partial, or the path continues to be " + thisAgent.pathStatus + " and we're now within  " + partialPathCameraClosestDistance + " units, so giving up and incrementing index.");
                        IncrementGuidedTourIndex();
                        Debug.Log("FollowGuidedTour: New destination: " + guidedTourObjects[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                        NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex]);
                    }
                }
            }

            // only update the guided tour vector if the mode is not paused, and we're moving
            if (!ModeState.isGuidedTourPaused && 
            thisAgent.velocity.sqrMagnitude > 0f)
            {
                // store the current camera destination
                currentGuidedTourDestinationCamera = guidedTourObjects[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex].GetComponent<Camera>();
                Vector3 currentGuidedTourCameraPosition = guidedTourCameraPositions[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex];

                // if we're on the last several feet of path, current tour vector is that camera forward dir
                if (calculatedRemainingDistance < lookToCameraAtRemainingDistance && calculatedRemainingDistance > thisAgent.stoppingDistance)
                {
                    // match the camera forward angle 
                    if (matchCameraForward)
                    {
                        currentGuidedTourVector = currentGuidedTourDestinationCamera.transform.forward;
                    }
                    // or simply look at the image, but no y-component
                    else
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
                else if (calculatedRemainingDistance <= thisAgent.stoppingDistance)
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
        }

        // invoke time-travel just once if requested - can happen even on pause
        if (isGuidedTourTimeTravelRequested)
        {
            // immediately set the flag back to false
            isGuidedTourTimeTravelRequested = false;

            // show the time-traveling label
            ModeState.doShowTimeTravelingLabel = true;

            // invoke time-traveling
            string nextTimePeriodSceneName = ManageScenes.GetUpcomingPeriodSceneName(gameObject.scene.name, "next");
            StartCoroutine(ToggleSceneAndUI.ToggleFromSceneToSceneWithTransition(thisAgent.gameObject.scene.name, nextTimePeriodSceneName, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform, ManageCameraActions.CameraActionGlobals.activeCameraHost, "FlashBlack", SceneGlobals.guidedTourTimeTravelTransitionDuration));

            // indicate to the next scene that it needs to recalc its cameras and paths
            SceneGlobals.isGuidedTourTimeTraveling = true;
        }

        // guided tour can only stop for a certain amount of time before we proceed
        // but the pause duration will differ between unpaused and paused states

        // not paused
        if ((calculatedRemainingDistance < thisAgent.height || thisAgent.velocity.sqrMagnitude == 0f) && !ModeState.isGuidedTourPaused && ModeState.isGuidedTourActive)
        {
            stationaryTimeActive += Time.deltaTime;
        }
        else
        {
            stationaryTimeActive = 0f;
        }

        // player has been stationary beyond the duration to pause at camera
        // so decide how to proceed next
        if (stationaryTimeActive >= pauseAtCameraDuration)
        {
            // periodic time travel if it's time
            bool isAtPeriodicInterval = (Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex + 1) % periodicTimeTravelAfterPhotoCount == 0;
            if (ModeState.autoTimeTravelPeriodic && isAtPeriodicInterval)
            {
                isGuidedTourTimeTravelRequested = true;
                stationaryTimeActive = 0f;
                // increment the index so the next time we're in this era, we go to the next photo
                IncrementGuidedTourIndex(thisAgent.gameObject.scene.name);
                ModeState.isPeriodicTimeTraveling = true;
                Debug.Log("Periodic time-traveling!");
            }
            // handle the alternating time-travel mode
            else if (ModeState.autoTimeTravelPeek)
            {
                // if requested, initiate time traveling briefly
                if (!ModeState.isTimeTravelPeeking)
                {
                    Debug.Log("FPSAgent has been stationary with guided tour ACTIVE for " + (pauseAtCameraDuration) + " seconds or more AND time travel peek is requested, so time-traveling briefly to peek at the next era.");
                    stationaryTimeActive = 0f;
                    // request time travel and mark peek active
                    isGuidedTourTimeTravelRequested = true;
                    ModeState.isTimeTravelPeeking = true;
                    // handle photo and people visibility in preparation for returning
                    ModeState.areHistoricPhotosRequestedVisible = false;
                    ModeState.arePeopleRequestedVisible = false;
                }
                else
                {
                    Debug.Log("FPSAgent has been stationary with guided tour ACTIVE for " + (pauseAtCameraDuration) + " seconds or more. Going to next destination.");
                    IncrementGuidedTourIndexAndSetAgentOnPath();
                    stationaryTimeActive = 0f;
                    // in case we were previously periodic time-traveling, reset the flag
                    ModeState.isPeriodicTimeTraveling = false;
                }
            }
            // default behavior is to move on to the next destination
            else
            {
                Debug.Log("FPSAgent has been stationary with guided tour ACTIVE for " + (pauseAtCameraDuration) + " seconds or more. Going to next destination.");
                IncrementGuidedTourIndexAndSetAgentOnPath();
                stationaryTimeActive = 0f;
                // in case we were previously periodic time-traveling, reset the flag
                ModeState.isPeriodicTimeTraveling = false;
            }
        }

        // paused
        if (ModeState.isGuidedTourPaused && !GetIsGuidedTourOverrideRequested() && (thisAgent.velocity.sqrMagnitude == 0f || thisAgent.remainingDistance == Mathf.Infinity))
        {
            stationaryTimePaused += Time.deltaTime;
        }
        else
        {
            stationaryTimePaused = 0f;
        }

        if (stationaryTimePaused >= (ModeState.autoTimeTravelPeek && ModeState.isTimeTravelPeeking ? pauseAtTimeTravelPeekDuration : pauseAtEnableOrResumeDuration))
        {
            // handle alternating time travel and peek being active
            if (ModeState.autoTimeTravelPeek)
            {
                // if we're peeking for time-travel, time travel back
                if (ModeState.isTimeTravelPeeking)
                {
                    Debug.Log("FPSAgent has been stationary with guided tour PAUSED for " + pauseAtTimeTravelPeekDuration + " seconds or more AND alternating time travel is active AND time travel peek is active, so time-traveling back.");
                    stationaryTimePaused = 0f;
                    ModeState.isTimeTravelPeeking = false;
                    isGuidedTourTimeTravelRequested = true;
                }
                // otherwise, resume guided tour
                else
                {
                    Debug.Log("FPSAgent has been stationary with guided tour PAUSED for " + (pauseAtEnableOrResumeDuration) + " seconds or more. Resuming guided tour. Next photo: " + Instances[thisAgent.gameObject.scene.name].guidedTourObjects[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex].name);
                    ModeState.isGuidedTourPaused = false;
                    ModeState.isGuidedTourActive = true;
                    stationaryTimePaused = 0f;
                    // in case we were previously periodic time-traveling, reset the flag
                    ModeState.isPeriodicTimeTraveling = false;
                }
            } 
            // default behavior is to resume guided tour
            else
            {
                Debug.Log("FPSAgent has been stationary with guided tour PAUSED for " + (pauseAtEnableOrResumeDuration) + " seconds or more. Resuming guided tour. Next photo: " + Instances[thisAgent.gameObject.scene.name].guidedTourObjects[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex].name);
                ModeState.isGuidedTourPaused = false;
                ModeState.isGuidedTourActive = true;
                stationaryTimePaused = 0f;
                // in case we were previously periodic time-traveling, reset the flag
                ModeState.isPeriodicTimeTraveling = false;
            }
        }

        // handle override requests
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
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
            NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex]);
        }
        if (Input.GetKeyDown(","))
        {
            DecrementGuidedTourIndex();
            NavMeshUtils.SetAgentOnPath(thisAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(thisAgent.transform.position, thisAgent.height / 2), guidedTourFinalNavMeshDestinations[Instances[thisAgent.gameObject.scene.name].currentGuidedTourDestinationIndex]);
        }

        // during guided tour, historic photos should
        // only be visible when within some distance to the next destination
        // this is possibly expensive, so only do it one frame only when requested
        if (ModeState.isGuidedTourActive && !ModeState.isTimeTravelPeeking && thisAgent.enabled && thisAgent.isOnNavMesh)
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
        if (ModeState.isGuidedTourActive && !ModeState.isTimeTravelPeeking && !ModeState.isPeriodicTimeTraveling && thisAgent.enabled && thisAgent.isOnNavMesh)
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

        // update the people and historic photo visibility
        // this only runs when it needs to
        UpdateProxyObjectVisibility();

        // reset the forced update flag
        isProxyObjectVisibilityUpdateRequired = false;
    }

    public static void StartGuidedTourMode()
    {
        DebugUtils.DebugLog("Starting guided tour mode...");

        // set the mode state
        ModeState.isGuidedTourActive = true;
    }

    public static void EndGuidedTourMode()
    {
        DebugUtils.DebugLog("Ending guided tour mode.");

        // set the mode state
        ModeState.isGuidedTourActive = false;
        ModeState.isGuidedTourPaused = false;
    }

    // increment or start over depending on the current index
    public static void IncrementGuidedTourIndex(string sceneName = null)
    {
        // sceneName may be provided explicitly for certain cases like
        // when called in OnEnable and a stale scene name may be returned by SceneManager
        // if sceneName isn't provided, get the active scene from SceneManager
        if (sceneName == null)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        Instances[sceneName].currentGuidedTourDestinationIndex = (Instances[sceneName].currentGuidedTourDestinationIndex + 1) % Instances[sceneName].guidedTourObjects.Length;
        DebugUtils.DebugLog("Incrementing destination index. New destination: " + Instances[sceneName].guidedTourObjects[Instances[sceneName].currentGuidedTourDestinationIndex].name);
    }

    public static void IncrementGuidedTourIndexAndSetAgentOnPath(string sceneName = null)
    {
        // sceneName may be provided explicitly for certain cases like
        // when called in OnEnable and a stale scene name may be returned by SceneManager
        // if sceneName isn't provided, get the active scene from SceneManager
        if (sceneName == null)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        IncrementGuidedTourIndex(sceneName);
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instances[sceneName].guidedTourFinalNavMeshDestinations[Instances[sceneName].currentGuidedTourDestinationIndex]);
    }

    // decrement or go to the end depending on the current index
    public static void DecrementGuidedTourIndex(string sceneName = null)
    {
        // sceneName may be provided explicitly for certain cases like
        // when called in OnEnable and a stale scene name may be returned by SceneManager
        // if sceneName isn't provided, get the active scene from SceneManager
        if (sceneName == null)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        Instances[sceneName].currentGuidedTourDestinationIndex = (Instances[sceneName].currentGuidedTourDestinationIndex + Instances[sceneName].guidedTourObjects.Length - 1) % Instances[sceneName].guidedTourObjects.Length;
        DebugUtils.DebugLog("Decrementing destination index. New destination: " + Instances[sceneName].guidedTourObjects[Instances[sceneName].currentGuidedTourDestinationIndex].name);
    }

    public static void DecrementGuidedTourIndexAndSetAgentOnPath(string sceneName = null)
    {
        // sceneName may be provided explicitly for certain cases like
        // when called in OnEnable and a stale scene name may be returned by SceneManager
        // if sceneName isn't provided, get the active scene from SceneManager
        if (sceneName == null)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        DecrementGuidedTourIndex(sceneName);
        NavMeshUtils.SetAgentOnPath(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent, Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.height / 2), Instances[sceneName].guidedTourFinalNavMeshDestinations[Instances[sceneName].currentGuidedTourDestinationIndex]);
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

    private void UpdateProxyObjectVisibility()
    {
        // only force visibility if guided tour mode is on, 
        // and the official flag doesn't match the requested flag
        // or if a forced update is required
        if ((ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused) && (ModeState.areHistoricPhotosVisible != ModeState.areHistoricPhotosRequestedVisible || isProxyObjectVisibilityUpdateRequired))
        {
            // enable or disable the historic photos
            GameObject historicCamerasContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];
            ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicCamerasContainer, ModeState.areHistoricPhotosRequestedVisible, false);
            ObjectVisibility.SetHistoricPhotosOpaque(ModeState.areHistoricPhotosRequestedVisible);

            // set the official flag to match so this only runs once
            ModeState.areHistoricPhotosVisible = ModeState.areHistoricPhotosRequestedVisible;
        }

        // similar for people - only adjust their visibility once if requested
        if ((ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused) && (ModeState.arePeopleVisible != ModeState.arePeopleRequestedVisible || isProxyObjectVisibilityUpdateRequired))
        {
            ObjectVisibility.SetPeopleVisibility(ModeState.arePeopleRequestedVisible);

            // set the official flag to match so this only runs once
            ModeState.arePeopleVisible = ModeState.arePeopleRequestedVisible;
        }
    }
}