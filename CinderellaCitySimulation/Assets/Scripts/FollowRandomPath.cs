using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Finds destinations and paths on a Navigation Mesh for 3D animated people
/// Must be attached to the NavMeshAgent object of the NPC
/// </summary>

[RequireComponent(typeof(NavMeshAgent))]

// this script is for NPCs to randomly find paths on the NavMesh

public class FollowRandomPath : MonoBehaviour
{
    // agent, destination, and path
    public NavMeshAgent thisAgent;
    public UpdateNPCAnimatorByState thisAnimatorUpdateScript;
    public Vector3 initialDestination;
    public NavMeshPath path;

    // variables related to the test for whether the NPC needs to repath
    readonly bool doRepathIfApproachingPlayer = false;
    readonly bool doRepathIfBlockingPlayerCamera = true;
    int numberOfFramesApproachingPlayer = 0;
    readonly int maxNumberOfFramesApproachingPlayer = 60;
    readonly float dotProductAlignThreshold = 0.90f;
    readonly float dotProductPosThreshold = 0.75f;
    readonly float maxDistanceForFarCheck = 2.0f;
    readonly float maxDistanceForCloseCheck = 0.1f;
    // repath if slowing down (likely due to colliding with others)
    readonly bool doRepathIfSlow = true;

    // debugging
    readonly bool doShowDebugMessages = false;

    private void Awake()
    {
        thisAgent = this.GetComponent<NavMeshAgent>();

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

    private void OnEnable()
    {
        NPCControllerGlobals.activeNPCControllersCount++;
        //Utils.DebugUtils.DebugLog("Active NPCControllers following paths: " + NPCControllerGlobals.activeNPCControllers);

        // if a path was previously recorded, use it
        if (path != null)
        {
            this.GetComponent<NavMeshAgent>().path = path;
        }
    }

    private void Start()
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

        path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, initialDestination);
    }

    private void Update()
    {
        if (!thisAgent.pathPending && this.transform.position != null && NPCControllerGlobals.initialNPCPositionsArray != null)
        {
            float currentVelocity = thisAgent.velocity.magnitude;
            Vector3 nextNPCDestination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, NPCControllerGlobals.minDiscardDistance, NPCControllerGlobals.maxDiscardDistance, true);

            // if this NPC is walking the same direction as the player
            // and blocking the player camera, make them repath
            if (doRepathIfBlockingPlayerCamera)
            {
                if (GetIsNPCFacingPlayer(this.gameObject, ManageFPSControllers.FPSControllerGlobals.activeFPSController, true))
                {
                    path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextNPCDestination);
                }
            }

            // if this agent's speed gets too low, it's likely colliding badly with others
            // to prevent a traffic jam, find a different random point from the pool to switch directions
            if (doRepathIfSlow && currentVelocity > 0f && currentVelocity < NPCControllerGlobals.minimumSpeedBeforeRepath)
            {
                path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextNPCDestination);
            }

            // set next destination
            if (thisAgent.remainingDistance <= NPCControllerGlobals.defaultNPCStoppingDistance)
            {
                path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextNPCDestination);
            }
        }
    }

    private void OnDisable()
    {
        // keep track of how many NPCControllers are active and pathfinding
        NPCControllerGlobals.activeNPCControllersCount--;

        // remember the path so this agent can resume it when enabled
        path = this.GetComponent<NavMeshAgent>().path;
    }

    private bool GetIsNPCFacingPlayer(GameObject NPCObject, GameObject FPSController, bool reverseDirection = false /* NPC is facing away from and aligned, in front of the camera */)
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

        if (doShowDebugMessages)
        {
            Debug.Log("NPC forward vector: " + NPCForwardVector);
            Debug.Log("FPSForwardVector: " + FPSForwardVector);
            Debug.Log("Distance: " + distanceFromNPCToPlayer);
            Debug.Log("dotProductPos: " + dotProductPosition);
            Debug.Log("dotProductAlign: " + dotProductAlignment);
        }

        // reverse direction means the NPC is in front of the camera,
        // close to, and aligned with the player
        // so it might block the camera especially during guided tours
        if (reverseDirection)
        {
            // if the NPC is in front of the camera, close, and aligned
            if ((dotProductPosition > dotProductPosThreshold && dotProductAlignment > dotProductAlignThreshold && distanceFromNPCToPlayer < maxDistanceForFarCheck))
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
            else
            {
                numberOfFramesApproachingPlayer = 0;
                return false;
            }
        }
        else
        {
            // if the NPC is looking at and heading towards the player
            if ((dotProductPosition < dotProductAlignThreshold && dotProductAlignment < dotProductAlignThreshold && distanceFromNPCToPlayer < maxDistanceForFarCheck && thisAgent.velocity.magnitude > 0) || (dotProductAlignment < dotProductPosThreshold && distanceFromNPCToPlayer < maxDistanceForCloseCheck))
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
            else
            {
                numberOfFramesApproachingPlayer = 0;
                return false;
            }
        }
    }


}