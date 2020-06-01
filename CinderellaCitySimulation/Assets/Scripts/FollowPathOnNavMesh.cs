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
    public float destinationRadius = 60;

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

    private void OnDisable()
    {
        NPCControllerGlobals.activeNPCControllers--;

        // keep track of how many NPCControllers are active and pathfinding
        path = this.GetComponent<NavMeshAgent>().path;
    }

    void Start()
    {
        // get this nav mesh agent
        thisAgent = this.GetComponent<NavMeshAgent>();

        // instantiate an empty path that it will follow
        path = new NavMeshPath();

        // set the destination
        destination = Utils.GeometryUtils.GetRandomNavMeshPointWithinRadius(this.transform.position,  destinationRadius, true);

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
                    NavMesh.CalculatePath(this.transform.position, Utils.GeometryUtils.GetRandomNavMeshPointWithinRadius(this.gameObject.transform.position, destinationRadius, false), NavMesh.AllAreas, path);
                    Debug.Log("Agent "  + thisAgent.gameObject.name + " reached its destination.");
                }
            }
        }
    }
}