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

    readonly string partialPathCameraName = "Blue Mall 1";
    readonly int pauseAtCameraDuration = 6; // number of seconds to pause and look at a camera
    readonly bool matchCameraForward = false; // player camera: match destination camera or simply look at it?
    readonly float lookToCameraAtRemainingDistance = 10.0f; // distance from end of path where FPC begins looking at camera
    readonly float adjustPosAwayFromCamera = 1.25f; // distance away from camera look vector so when looking at a camera, it's visible
    readonly public float guidedTourRotationSpeed = 0.4f;
    readonly public int guidedTourRestartAfterSeconds = 5; // seconds to wait before un-pausing

    // DEBUGGING
    readonly bool shuffleGuidedTourDestinations = true;
    private int currentGuidedTourDestinationIndex = 0; // optionally start at a specific index
    readonly bool useOverrideDestinations = false; // if true, use a special list for tour objects
    readonly private bool showDebugLines = true; // if true, show path as debug lines

    private void Awake()
    {
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // get and post-process the destinations
        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene(this.gameObject.scene.name);

        // DEBUGGING
        // shuffle the list if requested
        if (shuffleGuidedTourDestinations)
        {
            guidedTourObjects = ArrayUtils.ShuffleArray(guidedTourObjects);
        }
        // use the overrides if requested
        if (useOverrideDestinations)
        {
            guidedTourObjects = new GameObject[] { guidedTourObjects[23], guidedTourObjects[30], guidedTourObjects[3] /* index of partial path alt */ };
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

            // also record the index of the alternate camera destination in case of partial path
            if (objectCamera.name.Contains(partialPathCameraName))
            {
                partialPathCameraIndex = cameraPositionsAdjustedOnNavMeshList.Count - 1;
            }

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
                // if not pausing at camera, we may be just starting, so set path immediately
                if (!isPausingAtCamera)
                {
                    Debug.Log("FollowGuidedTour: Setting agent on first path. Current destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                    NavMeshUtils.SetAgentOnPath(thisAgent, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], showDebugLines);
                    doIncrementIndex = false;
                }

                // increment the next destination index if appropriate
                if (doIncrementIndex && ModeState.setAgentOnPathAfterDelayRoutine == null)
                {
                    // increment the index or start over
                    currentGuidedTourDestinationIndex = (currentGuidedTourDestinationIndex + 1) % guidedTourObjects.Length;

                    Debug.Log("FollowGuidedTour: Incrementing index. New destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                    doIncrementIndex = false;
                }

                // set the agent on a new path after a pause
                if (ModeState.setAgentOnPathAfterDelayRoutine == null && !thisAgent.hasPath)
                {
                    Debug.Log("FollowGuidedTour: Pausing at camera, setting path after delay. Current destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);
                    ModeState.setAgentOnPathAfterDelayRoutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], pauseAtCameraDuration, true, showDebugLines));
                }

                // if the agent doesn't currently have a path, increment the index
                if (!thisAgent.hasPath)
                {
                    doIncrementIndex = true;
                }
            }

            // if the path is partial, try setting it again in a few moments
            // this could due to the path being very long
            if (thisAgent.pathStatus == NavMeshPathStatus.PathPartial && ModeState.setAgentOnPathAfterDelayRoutine == null && partialPathCameraIndex != -1)
            {
                Debug.LogWarning("FollowGuidedTour: Path is partial, will try to repath in a few moments. Current destination: " + guidedTourObjects[currentGuidedTourDestinationIndex].name + " at index: " + currentGuidedTourDestinationIndex);

                // set the immediate destination to the alternate camera in case of partial path
                NavMeshUtils.SetAgentOnPath(thisAgent, guidedTourFinalNavMeshDestinations[partialPathCameraIndex]);

                // try setting the agent to the original index in a few moments
                ModeState.setAgentOnPathAfterDelayRoutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], 10, false, showDebugLines));
            }

            // start pausing at the camera just a bit before destination reached
            if (!thisAgent.pathPending && thisAgent.remainingDistance <= 0.1f && (!thisAgent.hasPath || thisAgent.velocity.sqrMagnitude == 0f))
            {
                isPausingAtCamera = true;
            }
            else
            {
                isPausingAtCamera = false;
            }

            // only update the vector if we're not pausing to look at the current camera
            if (!isPausingAtCamera && !ModeState.isGuidedTourPaused)
            {
                // store the current camera destination
                currentGuidedTourDestinationCamera = guidedTourObjects[currentGuidedTourDestinationIndex].GetComponent<Camera>();
                Vector3 currentGuidedTourCameraPosition = guidedTourCameraPositions[currentGuidedTourDestinationIndex];

                // if we're on the last several feet of path, current tour vector is that camera forward dir
                if (thisAgent.remainingDistance < lookToCameraAtRemainingDistance)
                {
                    // match the camera forward angle 
                    if (matchCameraForward)
                    {
                        currentGuidedTourVector = currentGuidedTourDestinationCamera.transform.forward;
                    }
                    else // or simply look at the image
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
                }
                // otherwise, current path vector is the agent's velocity
                else
                {
                    currentGuidedTourVector = thisAgent.velocity;
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
        if (Input.GetKeyDown(".") || Input.GetKeyDown(","))
        {
            currentGuidedTourDestinationIndex = Random.Range(0, guidedTourObjects.Length - 1);
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
        } else
        {
            // make photos visible if guided tour is not happening
            ModeState.areHistoricPhotosRequestedVisible = true;
        }

        // only force visibility if guided tour mode is on, 
        // and the local flag doesn't match the global flag
        // or if the local flag hasn't been set yet
        if (areHistoricPhotosVisible == null || areHistoricPhotosVisible != ModeState.areHistoricPhotosRequestedVisible)
        {
            // enable or disable the historic photos
            GameObject historicCamerasContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];
            ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicCamerasContainer, ModeState.areHistoricPhotosRequestedVisible, false);
            ObjectVisibility.SetHistoricPhotosOpaque(ModeState.areHistoricPhotosRequestedVisible);

            // set the local flag to match so this only runs once
            areHistoricPhotosVisible = ModeState.areHistoricPhotosRequestedVisible;
        }
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