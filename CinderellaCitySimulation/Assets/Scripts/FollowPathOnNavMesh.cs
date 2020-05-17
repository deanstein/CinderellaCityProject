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
        destination = RandomNavmeshLocation(destinationRadius);

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
        // if we've stopped moving, change to an idle controller
        if (thisAgent.remainingDistance <= thisAgent.stoppingDistance && thisAgent.pathPending)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetIdleAnimatorControllerByGender(this.name));
        }
        // if we're moving, use this agent's default controller
        else if (!thisAgent.pathPending)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(this.name));
        }

        // if we're colliding with another agent, set the animation to talking
        if (thisAgent.velocity.sqrMagnitude <= 0.1f)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(NPCControllerGlobals.animatorControllerFilePathTalking1);
        }
        else
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(this.name));
        }

        // Check if we've reached the destination
        if (!thisAgent.pathPending)
        {
            if (thisAgent.remainingDistance <= thisAgent.stoppingDistance)
            {
                if (!thisAgent.hasPath || thisAgent.velocity.sqrMagnitude == 0f)
                {
                    // reached the destination - now set another one
                    thisAgent.SetDestination(RandomNavmeshLocation(destinationRadius));
                }
            }
        }
    }

    // get a random point on the navmesh
    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;

        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            // the hit position could wind up on the roof
            // so clamp the y (up) value to ~the 2nd floor height (~10 meters) at all times
            if (hit.position.y > 10)
            {
                finalPosition = new Vector3(hit.position.x, 10, hit.position.z);
            }
            else
            {
                finalPosition = hit.position;
            }

            // optional: visualize the location and a connecting line
            //Debug.DrawLine(this.gameObject.transform.position, finalPosition, Color.red, 100f);
            //Debug.DrawLine(finalPosition, new Vector3(finalPosition.x, finalPosition.y + 1, finalPosition.z), Color.red, 100f);
        }

        return finalPosition;
    }
}

public class GeometryUtils
{
    public static float angle = 20.0f; // Maximum angle offset for new point 
    float speed = 8.0f; // Units per second 
    private float pos = 0.0f;
    private float segLength = 2.0f;
    public static List<float> path = new List<float>();

    // get a point on a mesh
    public static List<Vector3> GetPointOnMesh(GameObject meshParent)
    {

        MeshFilter[] meshes = meshParent.GetComponentsInChildren<MeshFilter>();

        Vector3 meshPos = new Vector3(0, 0, 0);
        List<Vector3> gameObjectMeshPosList = new List<Vector3>();

        foreach (MeshFilter mesh in meshes)
        {
            Vector3[] vertices = mesh.mesh.vertices;
            meshPos = meshParent.transform.TransformPoint(vertices[0]);
            Debug.Log(meshPos);
            gameObjectMeshPosList.Add(meshPos);
        }

        return gameObjectMeshPosList;

    }
}