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
    private bool isPausingAtCamera = false; // player will pause to look at camera for some amount of time
    readonly int pauseAtCameraDuration = 4; // number of seconds to pause and look at a camera
    readonly float lookToCameraAtRemainingDistance = 10.0f; // distance from end of path where FPC begins looking at camera
    readonly float adjustPosAwayFromCamera = 4.0f; // distance away from camera look vector so when looking at a camera, it's visible
    readonly bool useRandomGuidedTourDestination = false;
    bool isResettingPosition = false; // if true, the coroutine to find an alternate position has started and can't be started again until it expires
    readonly private bool showDebugLines = true; // enable for debugging

    public static int currentGuidedTourDestinationIndex = 0;
    public static Camera currentGuidedTourDestinationCamera;
    public static Vector3 currentGuidedTourVector;
    // set to true briefly to allow IEnumerator time-travel transition
    public static bool isGuidedTourTimeTravelRequested = false;
    readonly public static float guidedTourRotationSpeed = 0.1f;
    readonly public static int guidedTourRestartAfterSeconds = 5; // seconds to wait before un-pausing

    private void Awake()
    {
        thisAgent = this.GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {

        // if guided tour is active from the previous scene, ensure the historic photos are visible
        if (ModeState.isGuidedTourActive)
        {
            // enable the historic photos
            GameObject historicCamerasContainer = ObjectVisibility.GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];
            ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(historicCamerasContainer, true, false);
            ObjectVisibility.SetHistoricPhotosOpaque(true);

            // restart the guided tour to ensure new pathfinding happens 
            StartCoroutine(ToggleGuidedTourOnEnable());

            // only spend a certain amount of time in each era
            ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
        }
    }

    private void Start()
    {
        // get and post-process the destinations - a mix of historic cameras and thumbnail cameras
        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetCombinedCamerasInScene();
        List<Vector3> cameraPositionsList = new List<Vector3>();
        List<Vector3> cameraPositionsAdjustedList = new List<Vector3>();
        foreach(GameObject guidedTourObject in guidedTourObjects)
        {
            Camera objectCamera = guidedTourObject.GetComponent<Camera>();
            // record the positions of the cameras
            Vector3 objectCameraPos = objectCamera.transform.position;
            cameraPositionsList.Add(objectCameraPos);
            // also record the adjusted positions
            Vector3 objectCameraPosAdjusted = NavMeshUtils.AdjustPositionAwayFromCameraOnNavMesh(objectCamera.transform.position, objectCamera, 4.0f);
            cameraPositionsAdjustedList.Add(objectCameraPosAdjusted);
            if (showDebugLines)
            {
                Debug.DrawLine(objectCameraPos, objectCameraPosAdjusted, Color.red, 1000);
            }
        }
        guidedTourCameraPositions = cameraPositionsList.ToArray();
        guidedTourCameraPositionsAdjusted = cameraPositionsAdjustedList.ToArray();
    }

    private void Update()
    {
        if (ModeState.isGuidedTourActive)
        {
            // if the current path isn't valid, move to a valid location and try again
            if (thisAgent.pathStatus == NavMeshPathStatus.PathPartial && !isResettingPosition)
            {
                Debug.Log("PARTIAL PATH!");
                //StartCoroutine(ResetFPCPosition());
            }

            // what happens when current destination is reached
            if (thisAgent.remainingDistance <= 1)
            {
                isPausingAtCamera = true;

                if (!thisAgent.isStopped)
                {

                    // go to a random camera or the next one?
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

                    Vector3 nextFPCDestination = Utils.GeometryUtils.GetNearestPointOnNavMesh(guidedTourObjects[currentGuidedTourDestinationIndex].transform.position, 5);

                    // adjust the point so the FPC stands behind the camera a bit
                    Vector3 adjustedNextFPCDestination = NavMeshUtils.AdjustPositionAwayFromCameraOnNavMesh(nextFPCDestination, guidedTourObjects[0].GetComponentInChildren<Camera>(), 4.0f);

                    Vector3 adjustedNextFPCDestinationOnNavMesh = Utils.GeometryUtils.GetNearestPointOnNavMesh(adjustedNextFPCDestination, 5);

                    // pause to let the photo show for a few seconds without movement
                    StartCoroutine(NavMeshUtils.SetAgentOnPathAfterDelay(thisAgent, adjustedNextFPCDestinationOnNavMesh, pauseAtCameraDuration /* seconds */, true /*show debug lines*/));
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

            // start or stop the restart routine if override is requested
            if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
            {
                if (GetIsGuidedTourOverrideRequested())
                {
                    // stop the active countdown coroutine if there is one
                    if (ModeState.restartGuidedTourCoroutine != null)
                    {
                        StopCoroutine(ModeState.restartGuidedTourCoroutine);
                    }
                    // otherwise, start a new countdown to resume the guided tour again
                    // TODO: this happens every frame which is not ideal
                    ModeState.restartGuidedTourCoroutine = StartCoroutine(GuidedTourRestartCountdown());
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
                Quaternion currentRotation = ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.rotation;
                // calculate the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(cameraForwardNoTilt, Vector3.up);
                // slerp interpolation
                Quaternion slerpRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * guidedTourRotationSpeed);
                // set the new rotation
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.rotation = slerpRotation;

                // set the camera's forward direction to match the character controller
                Vector3 slerpForward = Vector3.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward, ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.transform.forward = slerpForward;

            }

            // the current guided tour vector is 
            // either the path vector or the camera direction if on last leg of path
            // so set the current FPSController direction and camera to that vector
            if (currentGuidedTourVector != Vector3.zero)
            {
                // make sure the camera looks at the upcoming camera
                Quaternion targetRotation = Quaternion.LookRotation(currentGuidedTourVector, Vector3.up);
                Quaternion SlerpRotation = Quaternion.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.rotation, targetRotation, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.rotation = SlerpRotation;

                // set the FirstPersonCharacter's camera forward direction
                Vector3 targetDirection = currentGuidedTourVector;
                Vector3 slerpForward = Vector3.Slerp(ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward, targetDirection, Time.deltaTime * guidedTourRotationSpeed);
                ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().transform.forward = slerpForward;

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
    }

    // when guided tour is active, this checks if the user is trying to override control
    public bool GetIsGuidedTourOverrideRequested()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
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