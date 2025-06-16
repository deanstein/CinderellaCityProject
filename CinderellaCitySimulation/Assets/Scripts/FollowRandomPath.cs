using System.Linq;
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
    public Vector3 nextDestination;
    public NavMeshPath path;

    // repath if this  NPC is blocking the player's camera
    readonly bool doRepathIfBlockingPlayerCamera = true;
    // any closer than this, check for alignment
    readonly float maxDistanceForAlignmentCheck = 4.5f;
    // alignment between NPC and FPC
    readonly float maxTimeAligned = 1f; // seconds
    float timeAligned = 0f; // seconds
    // amount of the screen (x-dir) to consider for NPC being "in front of" player
    readonly float screenMinX = 0.45f;
    readonly float screenMaxX = 0.55f;

    // proximity between NPC and FPC
    float distanceFromNPCToPlayer;
    readonly float maxTimeProximate = 0.5f; // seconds
    float timeProximate = 0;
    // any closer than this, and repath
    readonly float maxDistanceForProximityCheck = 2.5f;
    // ignore NPCs that are mostly sideways to the player
    readonly float sidewaysThreshold = 0.7f;

    // repath if slowing down (likely due to colliding with others)
    readonly bool doRepathIfSlow = true;
    readonly float maxTimeSlow = 0.5f;
    float timeSlow = 0;

    // these "extend" the camera extents (usualy between 0 and 1), to more quickly enable components
    // when the camera is sweeping around, so the player won't notice the components being enabled
    public float minScreenSpacePoint = -2;
    public float maxScreenSpacePoint = 3;

    // hide NPC when too close to the player
    readonly bool doHideOnProximity = true;
    // this should be slightly larger than the player and NPC radius (0.15 and 0.4)
    readonly float hideAtProximity = 0.8f;
    bool isHidden = false;
    private SkinnedMeshRenderer skinnedMeshRenderer;


    // DEBUGGING
    readonly bool doShowDebugMessages = false;

    private Vector3 GetRandomDestination()
    {
        Vector3 destination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, NPCControllerGlobals.minDiscardDistance, NPCControllerGlobals.maxDiscardDistance, true);
        return destination;
    }

    private Vector3 GetRandomDestinationBehindNPC()
    {
        Vector3 NPCForwardVector = transform.forward.normalized;
        Vector3 currentPosition = transform.position;

        Vector3[] validDestinations = NPCControllerGlobals.initialNPCPositionsArray.Where(dest =>
            Vector3.Dot(NPCForwardVector, (dest - currentPosition).normalized) < -0.3f // Behind threshold
        ).ToArray(); // Convert back to array

        return validDestinations.Length > 0 ? validDestinations[Random.Range(0, validDestinations.Length)] : GetRandomDestination();
    }

    private Vector3 GetRandomDestinationAheadOfNPC()
    {
        Vector3 NPCForwardVector = transform.forward.normalized;
        Vector3 currentPosition = transform.position;

        Vector3[] validDestinations = NPCControllerGlobals.initialNPCPositionsArray.Where(dest =>
            Vector3.Dot(NPCForwardVector, (dest - currentPosition).normalized) > 0.3f // Ahead threshold
        ).ToArray(); // Convert back to array

        return validDestinations.Length > 0 ? validDestinations[Random.Range(0, validDestinations.Length)] : GetRandomDestination();
    }

    private bool IsNPCInView(GameObject NPCObject, GameObject FPSController, float bufferZone = 0.1f)
    {
        bool isInFrame;
        // get the screen space position of this object
        Vector3 screenSpacePoint = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerCamera.WorldToViewportPoint(NPCObject.transform.position);

        isInFrame = screenSpacePoint.z > minScreenSpacePoint && screenSpacePoint.x > minScreenSpacePoint && screenSpacePoint.x < maxScreenSpacePoint && screenSpacePoint.y > minScreenSpacePoint && screenSpacePoint.y < maxScreenSpacePoint;

        return isInFrame;
    }

    private bool GetIsNPCColliding()
    {
        bool isColliding = false;

        if (Mathf.Abs(thisAgent.velocity.magnitude) < NPCControllerGlobals.minimumSpeedBeforeRepath)
        {
            timeSlow += Time.deltaTime;
            if (timeSlow >= maxTimeSlow)
            {
                isColliding = true;
                timeSlow = 0; // Reset after repath
            }
        }
        else
        {
            isColliding = false;
        }

        return isColliding;
    }

    private bool GetIsNPCBlockingPlayerCamera()
    {
        bool isBlocking = false;

        // Get the camera attached to the FPSController
        Camera playerCamera = ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponentInChildren<Camera>();
        if (playerCamera == null) return false;

        // Get NPC screen-space position
        Vector3 screenSpacePoint = playerCamera.WorldToViewportPoint(transform.position);

        // Ensure NPC is **not behind the player** (z > 0)
        if (screenSpacePoint.z <= 0) return false;

        // Check if NPC is within blocking zone
        bool isInBlockingZone = screenSpacePoint.x > screenMinX && screenSpacePoint.x < screenMaxX;

        // Get NPC’s forward direction
        Vector3 NPCForwardVector = transform.forward.normalized;
        Vector3 NPCToPlayerVector = (ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position - transform.position).normalized;

        // Check if NPC is facing **away** from the player
        float dotProductFacing = Vector3.Dot(NPCForwardVector, NPCToPlayerVector);
        bool isFacingAway = dotProductFacing < -sidewaysThreshold;

        if (doShowDebugMessages)
        {
            Debug.Log("NPC Screen Position: " + screenSpacePoint);
            Debug.Log("Is NPC in Blocking Zone? " + isInBlockingZone);
            Debug.Log("Distance: " + distanceFromNPCToPlayer);
            Debug.Log("dotProductFacing: " + dotProductFacing);
            Debug.Log("Is NPC Facing Away? " + isFacingAway);
        }

        // **Proximity Check** - If NPC is within close range and facing away, start counting frames
        if (isFacingAway && distanceFromNPCToPlayer < maxDistanceForProximityCheck)
        {
            timeProximate += Time.deltaTime;
            if (timeProximate >= maxTimeProximate)
            {
                //Debug.Log("BLOCKING VIA PROXIMITY! " + name);
                isBlocking = true;
                timeProximate = 0; // reset after blocking
            }
        }

        // **Alignment Check (Using Screen-Space Coverage)**
        if (isFacingAway && isInBlockingZone && distanceFromNPCToPlayer < maxDistanceForAlignmentCheck)
        {
            timeAligned += Time.deltaTime;
            if (timeAligned >= maxTimeAligned)
            {
                //Debug.Log("BLOCKING VIA SCREEN SPACE! " + name);
                isBlocking = true;
                timeAligned = 0; // reset after blocking
            }
        }

        return isBlocking;
    }

    /*** LIFECYCLE ***/

    private void Awake()
    {
        thisAgent = GetComponent<NavMeshAgent>();

        thisAnimatorUpdateScript = GetComponent<UpdateNPCAnimatorByState>();

        // record this NPC and its position for other scripts to reference later
        NPCControllerGlobals.activeNPCControllersList.Add(gameObject);
        NPCControllerGlobals.initialNPCPositionsList.Add(transform.position);

        // turn off the mesh collider
        // for some reason, this is now required to avoid a regression in v0.7.4
        // where NPCs would no longer be consistently pushable by the player
        MeshCollider[] meshCollidersInChildren = GetComponentsInChildren<MeshCollider>(true);
        if (meshCollidersInChildren.Length > 0)
        {
            meshCollidersInChildren[0].enabled = false;
        }
    }

    private void Start()
    {
        // since NPCs use agents, no need to have a collider
        MeshCollider meshCollider = GetComponentInChildren<MeshCollider>();
        meshCollider.enabled = false;
        // store the mesh renderer for hiding later
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // if the NPC arrays haven't been converted yet, convert them
        if (NPCControllerGlobals.initialNPCPositionsArray == null)
        {
            NPCControllerGlobals.initialNPCPositionsArray = NPCControllerGlobals.initialNPCPositionsList.ToArray();
        }
        if (NPCControllerGlobals.activeNPCControllersArray == null)
        {
            NPCControllerGlobals.activeNPCControllersArray = NPCControllerGlobals.activeNPCControllersList.ToArray();
        }

        // initialize next destination as random destination
        nextDestination = GetRandomDestination();
        path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextDestination);
    }

    private void OnEnable()
    {
        NPCControllerGlobals.activeNPCControllersCount++;
        //DebugUtils.DebugLog("Active NPCControllers following paths: " + NPCControllerGlobals.activeNPCControllers);

        // if a path was previously recorded, use it
        if (path != null)
        {
            this.GetComponent<NavMeshAgent>().path = path;
        }
    }

    private void Update()
    {
        // get distance between NPC and player
        Vector3 playerPosition = transform.position;
        Vector3 NPCPosition = ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.position;
        // zero-out the Y component
        NPCPosition.y = playerPosition.y;
        distanceFromNPCToPlayer = Vector3.Distance(playerPosition, NPCPosition);

        // hide when NPC is close to player if requested
        if (doHideOnProximity)
        {
            // if this agent is too close to the player, hide it
            if (distanceFromNPCToPlayer < hideAtProximity)
            {
                if (!isHidden)
                {
                    skinnedMeshRenderer.enabled = false;
                    isHidden = true;
                    thisAgent.isStopped = true;
                }
            }
            // otherwise, show the NPC
            else
            {
                if (isHidden)
                {
                    skinnedMeshRenderer.enabled = true;
                    isHidden = false;
                    thisAgent.isStopped = false;
                }
            }
        }

        // for alignment and proximity checks, NPC must be in view of player
        if (IsNPCInView(this.gameObject, ManageFPSControllers.FPSControllerGlobals.activeFPSController))
        {
            if (!thisAgent.pathPending && this.transform.position != null && NPCControllerGlobals.initialNPCPositionsArray != null)
            {
                float currentVelocity = thisAgent.velocity.magnitude;

                // if requested, check if this NPC is close to, aligned, and in front of the camera
                // which means it may be blocking the camera
                // if so, repath
                if (doRepathIfBlockingPlayerCamera)
                {
                    if (GetIsNPCBlockingPlayerCamera())
                    {
                        // if possible, get a destination behind the NPC
                        nextDestination = GetRandomDestinationBehindNPC();
                        path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextDestination);
                    }
                }
               

                // if this agent's speed gets too low, it's likely colliding badly with others
                // to prevent a traffic jam, find a different random point from the pool to switch directions
                if (doRepathIfSlow)
                {
                    if (GetIsNPCColliding())
                    {
                        // if possible, get a destination behind the NPC
                        nextDestination = GetRandomDestinationBehindNPC();
                        path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextDestination);
                    }
                }

                // if the agent arrives, set a new destination
                // and record the current destination as the previous
                if (thisAgent.remainingDistance <= NPCControllerGlobals.defaultNPCStoppingDistance)
                {
                    // generate a new next destination ahead of NPC if possible
                    // to avoid sudden turnarounds
                    nextDestination = GetRandomDestination();
                    path = NavMeshUtils.SetAgentOnPath(thisAgent, thisAgent.transform.position, nextDestination);
                }
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
}