using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

// this script needs to be attached to an object that should follow a defined path

public class FollowPathOnNavMesh : MonoBehaviour
{
    public NavMeshAgent thisAgent;
    public Vector3 destination;
    public NavMeshPath path;
    public float destinationRadius = 20f;

    private void OnEnable()
    {
        // keep track of how many NPCControllers are active and pathfinding
        NPCControllerGlobals.activeNPCControllers++;
        //Utils.DebugUtils.DebugLog("Active NPCControllers following paths: " + NPCControllerGlobals.activeNPCControllers);

        // if a path was previously recorded, use it
        if (path != null)
        {
            this.GetComponent<NavMeshAgent>().path = path;
        }

    }

    private void Awake()
    {
        // record this object's position in the pool so others can reference it later
        NPCControllerGlobals.initialNPCPositionsList.Add(this.transform.position);
    }

    void Start()
    {
        // if the NPC positions array hasn't been converted yet, convert it
        if (NPCControllerGlobals.initialNPCPositions == null)
        {
            NPCControllerGlobals.initialNPCPositions = NPCControllerGlobals.initialNPCPositionsList.ToArray();
        }

        // get this nav mesh agent
        thisAgent = this.GetComponent<NavMeshAgent>();

        // instantiate an empty path that it will follow
        path = new NavMeshPath();

        // set the destination
        destination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositions, destinationRadius, true);

        // find a path to the destination
        bool pathSuccess = NavMesh.CalculatePath(this.gameObject.transform.position, destination, NavMesh.AllAreas, path);

        // if a path was created, set this agent to use it
        if (pathSuccess)
        {
            thisAgent.GetComponent<NavMeshAgent>().SetPath(path);
        }
        else
        {
            Utils.DebugUtils.DebugLog("Agent " + thisAgent.transform.gameObject.name + " failed to find a path.");
        }
    }

    private void Update()
    {
        if (path != null && !thisAgent.hasPath)
        {
            thisAgent.path = path;
        }

        if (!thisAgent.pathPending)
        {
            if (thisAgent.remainingDistance <= thisAgent.stoppingDistance)
            {
                if (!thisAgent.hasPath || thisAgent.velocity.sqrMagnitude == 0f)
                {
                    // reached the destination - now set another one
                    NavMesh.CalculatePath(this.transform.position, Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositions, destinationRadius, true), NavMesh.AllAreas, path);
                    Utils.DebugUtils.DebugLog("Agent "  + thisAgent.gameObject.name + " reached its destination.");
                }
            }
        }
    }

    private void OnDisable()
    {
        NPCControllerGlobals.activeNPCControllers--;

        // keep track of how many NPCControllers are active and pathfinding
        path = this.GetComponent<NavMeshAgent>().path;
    }
}