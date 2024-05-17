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
    private bool isPausingAtCamera = false; // player will pause to look at camera for some amount of time
    private bool? areHistoricPhotosVisible = null;
    public static bool incrementIndex = false; // set to false if we should start or restart at the previously-known destination
    readonly int pauseAtCameraDuration = 4; // number of seconds to pause and look at a camera
    readonly float lookToCameraAtRemainingDistance = 10.0f; // distance from end of path where FPC begins looking at camera
    readonly float adjustPosAwayFromCamera = 1.25f; // distance away from camera look vector so when looking at a camera, it's visible
    readonly public static float guidedTourRotationSpeed = 0.15f;
    readonly public static int guidedTourRestartAfterSeconds = 5; // seconds to wait before un-pausing
    readonly bool useRandomGuidedTourDestination = false;
    readonly private bool showDebugLines = true; // enable for debugging
    public static int currentGuidedTourDestinationIndex = 0; // optionally start at a specific index
    public static Camera currentGuidedTourDestinationCamera;
    public static Vector3 currentGuidedTourVector;
    // set to true briefly to allow IEnumerator time-travel transition
    public static bool isGuidedTourTimeTravelRequested = false;

    private void Awake()
    {
        thisAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // get and post-process the destinations
        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene();
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

            Debug.Log(objectCamera.name + " is at index " + (cameraPositionsAdjustedOnNavMeshList.Count - 1).ToString());

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
        // if guided tour is active from the previous scene, ensure the historic photos are visible
        if (ModeState.isGuidedTourActive)
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
        // when guided tour is active and not paused, ensure the player is on the navmesh
        if (ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused)
        {
            // if the player is not on the navmesh, move it to the nearest point
            if (!ManageFPSControllers.FPSControllerGlobals.isActiveFPSControllerOnNavMesh)
            {
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position = Utils.GeometryUtils.GetNearestPointOnNavMesh(ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position, 2.0f);
            }

            // we've arrived, new path requested
            if (thisAgent.remainingDistance == 0 && !thisAgent.pathPending)
            {
                // if not pausing at camera, we may be just starting, so set path immediately
                if (!isPausingAtCamera)
                {
                    NavMeshUtils.SetAgentOnPath(thisAgent, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], showDebugLines);
                    incrementIndex = false;
                }

                // increment the next destination index if appropriate
                if (incrementIndex && ModeState.setAgentOnPathAfterDelayRoutine == null)
                {
                    if (useRandomGuidedTourDestination)
                    {
                        int randomIndex = Random.Range(0, guidedTourObjects.Length - 1);
                        // make sure the random number isn't the index already in use
                        if (randomIndex == currentGuidedTourDestinationIndex)
                        {
                            // if so, increment the index, or reset to 0 if already at max
                            randomIndex = randomIndex < guidedTourObjects.Length - 1 ? randomIndex + 1 : 0;
                        }
                        // use a random index from the destination array
                        currentGuidedTourDestinationIndex = randomIndex;
                    }
                    else
                    {
                        // increment the index or start over
                        if (currentGuidedTourDestinationIndex < guidedTourObjects.Length - 1)
                        {
                            currentGuidedTourDestinationIndex++;
                        }
                        else
                        {
                            currentGuidedTourDestinationIndex = 0;
                        }
                    }

                    incrementIndex = false;
                }
            }

            // start pausing at the camera just a bit before destination reached
            if (thisAgent.remainingDistance <= 0.1f)
            {
                isPausingAtCamera = true;

                // set the agent on a new path after a pause
                if (ModeState.setAgentOnPathAfterDelayRoutine == null && !thisAgent.hasPath)
                {
                    ModeState.setAgentOnPathAfterDelayRoutine = StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, guidedTourFinalNavMeshDestinations[currentGuidedTourDestinationIndex], pauseAtCameraDuration, showDebugLines));
                }
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

                // if we're on the last several feet of path, current tour vector is that camera forward dir
                if (thisAgent.remainingDistance < lookToCameraAtRemainingDistance)
                {
                    currentGuidedTourVector = currentGuidedTourDestinationCamera.transform.forward;
                }
                // otherwise, current path vector is the agent's velocity
                else
                {
                    // prevent tilt by eliminating Y component
                    currentGuidedTourVector = new Vector3(thisAgent.velocity.x, 0, thisAgent.velocity.z);
                }
            }

            // start or stop the restart routines if override is requested
            if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
            {
                if (GetIsGuidedTourOverrideRequested())
                {
                    // stop the active countdown coroutine if there is one
                    if (ModeState.restartGuidedTourCoroutine != null)
                    {
                        StopCoroutine(ModeState.restartGuidedTourCoroutine);
                        StopCoroutine(ModeState.toggleToNextEraCoroutine);
                    }
                    // otherwise, start a new countdown to resume the guided tour again
                    // TODO: this happens every frame which is not ideal
                    ModeState.restartGuidedTourCoroutine = StartCoroutine(GuidedTourRestartCountdown());
                    ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
                }
            }

            // if the player is trying to walk,
            // temporarily pause the guided tour
            if (GetIsGuidedTourOverrideRequested())
            {
                ModeState.isGuidedTourActive = false;
                ModeState.isGuidedTourPaused = true;

                // ensure the FPS controller isn't tilted when player takes control again
                // for example, if player takes control while looking up at a camera
                // calculate the forward direction without vertical tilt
                Vector3 cameraForwardNoTilt = new Vector3(ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward.x, 0, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward.z);

                // get the current rotation
                Quaternion currentRotation = ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.rotation;
                // calculate the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(cameraForwardNoTilt, Vector3.up);
                // slerp interpolation
                Quaternion slerpRotation = Quaternion.SlerpUnclamped(currentRotation, targetRotation, Time.deltaTime * guidedTourRotationSpeed);
                // set the new rotation
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.rotation = slerpRotation;

                // set the camera's forward direction to match the character controller
                Vector3 slerpForward = Vector3.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward, ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward = slerpForward;

            }

            // the current guided tour vector is 
            // either the path vector or the camera direction if on last leg of path
            // so set the current FPSController direction and camera to that vector
            if (currentGuidedTourVector != Vector3.zero)
            {
                // set the FirstPersonCharacter's camera forward direction
                Vector3 targetDirection = currentGuidedTourVector;
                Vector3 slerpForward = Vector3.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward, targetDirection, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward = slerpForward;

                // set the camera's forward direction to match the character controller
                Vector3 cameraSlerpForward = Vector3.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward, ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward, Time.deltaTime * guidedTourRotationSpeed);
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

        // provide emergency override to change the next destination
        if (Input.GetKeyDown(".") || Input.GetKeyDown(","))
        {
            int randomIndex = Random.Range(0, guidedTourObjects.Length - 1);
            currentGuidedTourDestinationIndex = randomIndex;
        }

        // during guided tour, historic photos should
        // only be visible when within some distance to the next destination
        // this is possibly expensive so only do it one frame only when requested
        if (ModeState.isGuidedTourActive && thisAgent.remainingDistance < lookToCameraAtRemainingDistance)
        {
            ModeState.areHistoricPhotosRequestedVisible = true;
        }
        else
        {
            ModeState.areHistoricPhotosRequestedVisible = false;
        }

        // only force visibility if the local flag doesn't match the global flag
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
        yield return new WaitForSeconds(0.5f);
        ModeState.isGuidedTourActive = true;
    }
}