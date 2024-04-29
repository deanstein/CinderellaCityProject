using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Finds destinations and paths on a Navigation Mesh for 3D animated people
/// </summary>

[RequireComponent(typeof(NavMeshAgent))]

// this script needs to be attached to an object that should follow a defined path
// for example, an NPC or the player when in guided tour mode

public class FollowPathOnNavMesh : MonoBehaviour
{
    // agent, destination, and path
    public bool isNPC;
    public NavMeshAgent thisAgent;
    public UpdateNPCAnimatorByState thisAnimatorUpdateScript;
    public Vector3 initialDestination;
    public NavMeshPath path;
    private bool showDebugLines = true; // enable for debugging
    private GameObject[] guidedTourObjects;

    // variables related to the test for whether the NPC is on a collision course with the player
    // this is only used for NPCs
    int numberOfFramesApproachingPlayer = 0;
    readonly int maxNumberOfFramesApproachingPlayer = 45;
    readonly float dotProductThresholdFar = -0.96f;
    readonly float dotProductThresholdClose = -0.93f;
    readonly float maxDistanceForFarCheck = 7.0f;
    readonly float maxDistanceForCloseCheck = 2.0f;

    private void OnEnable()
    {
        // keep track of how many NPCControllers are active and pathfinding
        if (isNPC)
        {
            NPCControllerGlobals.activeNPCControllersCount++;
            //Utils.DebugUtils.DebugLog("Active NPCControllers following paths: " + NPCControllerGlobals.activeNPCControllers);
        }

        // if a path was previously recorded, use it
        if (path != null)
        {
            this.GetComponent<NavMeshAgent>().path = path;
        }

    }

    private void Awake()
    {
        thisAgent = this.GetComponent<NavMeshAgent>();

        // determine if this is a non-playable character or not
        isNPC = GetIsNPC();

        if (isNPC)
        {
            thisAnimatorUpdateScript = this.GetComponent<UpdateNPCAnimatorByState>();

            // record this NPC and its position for other scripts to reference later
            NPCControllerGlobals.activeNPCControllersList.Add(this.gameObject);
            NPCControllerGlobals.initialNPCPositionsList.Add(this.transform.position);

            // turn off the mesh collider
            // for some reason, this is now required to avoid a regression in v0.7.4
            // where NPCs would no longer be consistently pushable by the player
            MeshCollider[] meshCollidersInChildren = this.GetComponentsInChildren<MeshCollider>(true);
            if (meshCollidersInChildren.Length > 0)
            {
                meshCollidersInChildren[0].enabled = false;
            }
        }
    }

    void Start()
    {
        if (isNPC)
        {
            // if the NPC arrays haven't been converted yet, convert them
            if (NPCControllerGlobals.initialNPCPositionsArray == null)
            {
                NPCControllerGlobals.initialNPCPositionsArray = NPCControllerGlobals.initialNPCPositionsList.ToArray();
            }
            if (NPCControllerGlobals.activeNPCControllersArray == null)
            {
                NPCControllerGlobals.activeNPCControllersArray = NPCControllerGlobals.activeNPCControllersList.ToArray();
            }

            initialDestination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, 0, NPCControllerGlobals.maxDestinationDistance, true);

            SetAgentOnPath(thisAgent, initialDestination);
        }
    }

    private void Update()
    {
        if (!thisAgent.pathPending)
        {
            // NPCs
            if (isNPC)
            {
                float currentVelocity = thisAgent.velocity.magnitude;
                Vector3 nextNPCDestination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, NPCControllerGlobals.minDiscardDistance, NPCControllerGlobals.maxDiscardDistance, true);

                // if this NPC appears to be on a collision course with the player,
                // get a different destination so the NPC doesn't continue walking into the player
                if (GetIsNPCApproachingPlayer(this.gameObject, ManageFPSControllers.FPSControllerGlobals.activeFPSController))
                {
                    SetAgentOnPath(thisAgent, nextNPCDestination);
                }

                // if this agent's speed gets too low, it's likely colliding badly with others
                // to prevent a traffic jam, find a different random point from the pool to switch directions
                if (currentVelocity > 0f && currentVelocity < NPCControllerGlobals.minimumSpeedBeforeRepath)
                {
                    SetAgentOnPath(thisAgent, nextNPCDestination);
                }

                // set next destination
                if (thisAgent.remainingDistance <= NPCControllerGlobals.defaultNPCStoppingDistance)
                {
                    SetAgentOnPath(thisAgent, nextNPCDestination);
                }
            }
            // FPC (fist person controller)
            else
            {
                if (ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive)
                {
                    // find the guided tour objects if they haven't been loaded and a guided tour is active
                    // for some reason, this can't happen in Start() or Update()
                    if (guidedTourObjects == null)
                    {
                        guidedTourObjects = ManageSceneObjects.ProxyObjects.GetAllHistoricPhotoCamerasInScene();
                    }

                    // store the current camera destination
                    ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationCamera = guidedTourObjects[ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationIndex].GetComponent<Camera>();

                    if (ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationCamera != null && thisAgent.path.corners.Length >= 2)
                    {
                        // if we're on the last path segment, current tour vector is that camera
                        if (thisAgent.path.corners.Length == 2 || thisAgent.steeringTarget == thisAgent.path.corners[thisAgent.path.corners.Length - 1])
                        {
                            ManageFPSControllers.FPSControllerGlobals.currentGuidedTourVector = ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationCamera.transform.forward;
                        }
                        // otherwise, current path vector is the agent's velocity
                        else
                        {
                            ManageFPSControllers.FPSControllerGlobals.currentGuidedTourVector = thisAgent.velocity;
                        }
                    }

                    // set next destination
                    if (thisAgent.remainingDistance <= 1)
                    {
                        // use initial destination if not set already
                        if (initialDestination == Vector3.zero)
                        {
                            // set the initial destination to the first waypoint camera
                            initialDestination = Utils.GeometryUtils.GetNearestPointOnNavMesh(guidedTourObjects[0].transform.position, 5);

                            SetAgentOnPath(thisAgent, initialDestination);
                        }
                        // otherwise, use the next destination
                        else
                        {
                            // increment the index
                            ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationIndex++;

                            // update the next destination
                            Vector3 nextFPCDestination = Utils.GeometryUtils.GetNearestPointOnNavMesh(guidedTourObjects[ManageFPSControllers.FPSControllerGlobals.currentGuidedTourDestinationIndex].transform.position, 5);

                            //Utils.DebugUtils.DebugLog("FPC reached destination." + nextDestination);
                            SetAgentOnPath(thisAgent, nextFPCDestination);
                        }
                    }
                }
            }
        }
    }

    public static IEnumerator SetPathAfterDelay(NavMeshAgent agent, Vector3 destination)
    {
        yield return new WaitForSeconds(0.5f);

        //SetAgentOnPath(agent, destination);
    }

    private void OnDisable()
    {
        if (isNPC)
        {
            // keep track of how many NPCControllers are active and pathfinding
            NPCControllerGlobals.activeNPCControllersCount--;
        }

        // remember the path so this agent can resume it when enabled
        path = this.GetComponent<NavMeshAgent>().path;
    }

    // returns true if this agent is *not* the player
    private bool GetIsNPC()
    {
        if (this.transform.parent.gameObject.name.Contains("FPSController"))
        {
            Utils.DebugUtils.DebugLog("This agent is the player: " + this.name);
            return false;
        }
        else
        {
            return true;
        }
    }

    // returns true if the NPC on a collision course with the player
    private bool GetIsNPCApproachingPlayer(GameObject NPCObject, GameObject FPSController)
    {
        // get the NPC direction and compare it to the vector between the NPC and player
        Vector3 NPCForwardVector = (NPCObject.transform.forward).normalized;
        Vector3 NPCToFPSVector = (NPCObject.transform.position - FPSController.transform.position).normalized;
        Vector3 FPSForwardVector = FPSController.transform.forward.normalized;

        // use the dot product to determine how close the two vectors are
        // or how aligned the NPC is to the player
        float dotProductPosition = Vector3.Dot(NPCForwardVector, NPCToFPSVector);
        float dotProductAlignment = Vector3.Dot(NPCForwardVector, FPSForwardVector);

        // distance between this object and the player
        float distanceFromNPCToPlayer = Vector3.Distance(NPCObject.transform.position, FPSController.transform.position);

        // if the NPC object is looking at, and heading towards, the player
        // or if the NPC is very close to, and mostly looking at, the player
        // start counting the number of frames to determine if this NPC will collide with the player
        if ((dotProductPosition < dotProductThresholdFar && dotProductAlignment < dotProductThresholdFar && distanceFromNPCToPlayer < maxDistanceForFarCheck && thisAgent.velocity.magnitude > 0) || (dotProductAlignment < dotProductThresholdClose && distanceFromNPCToPlayer < maxDistanceForCloseCheck))
        {
            numberOfFramesApproachingPlayer++;
            if (numberOfFramesApproachingPlayer >= maxNumberOfFramesApproachingPlayer)
            {
                numberOfFramesApproachingPlayer = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        // otherwise, the NPC is not on a collision course with the player
        else
        {
            numberOfFramesApproachingPlayer = 0;
            return false;
        }
    }

    // set a given agent's path to a given destination point
    private void SetAgentOnPath(NavMeshAgent agent, Vector3 destinationPoint)
    {
        // instantiate an empty path that it will follow
        path = new NavMeshPath();

        // find a path to the destination
        bool pathSuccess = NavMesh.CalculatePath(this.gameObject.transform.position, destinationPoint, NavMesh.AllAreas, path);

        // if a path was created, set this agent to use it
        if (pathSuccess)
        {
            // optional: visualize the path with a line in the editor
            if (showDebugLines)
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.green, 1000);
                }
            }

            agent.SetPath(path);
        }
        else
        {
            Utils.DebugUtils.DebugLog("Agent " + thisAgent.transform.gameObject.name + " failed to find a path.");
        }
    }
}