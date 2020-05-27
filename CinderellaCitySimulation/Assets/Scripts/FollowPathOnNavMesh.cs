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
            Debug.Log("Agent " + thisAgent.transform.gameObject.name + " failed to find a path.");
        }
    }

    private void Update()
    {
          // Check if we've reached the destination
        if (!thisAgent.pathPending)
        {
            if (thisAgent.remainingDistance <= thisAgent.stoppingDistance)
            {
                if (!thisAgent.hasPath || thisAgent.velocity.sqrMagnitude == 0f)
                {
                    // reached the destination - now set another one
                    thisAgent.SetDestination(Utils.GeometryUtils.GetRandomNavMeshPointWithinRadius(this.gameObject.transform.position, destinationRadius, false));
                }
            }
        }
    }
}