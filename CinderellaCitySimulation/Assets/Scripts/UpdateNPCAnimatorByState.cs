using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// this script should be attached to an object that has an animator that should change based on speed

public class UpdateNPCAnimatorByState : MonoBehaviour
{
    // the navigation mesh agent on this object
    public NavMeshAgent thisAgent;

    // the previous recorded velocity of this navmesh agent
    public Vector3 lastKnownVelocity = Vector3.zero;

    // determine if we're resuming, and switch to the default controller state
    public bool isResuming = false;

    void Awake()
    {
        // get this NPC's nav mesh agent
        thisAgent = this.GetComponent<NavMeshAgent>();
    }

    private void OnDisable()
    {
        isResuming = true;
    }

    private void Update()
    {
        // if we're just starting, and the navmesh agent hasn't been enabled, set the animator to idle
        if (lastKnownVelocity.magnitude == 0 || !thisAgent.enabled)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetIdleAnimatorControllerByGender(this.name));
        }

        // if we're resuming, use the default controller, instead of resetting to idle
        if (isResuming)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(this.name));

            // set the velocity to the last recorded velocity
            thisAgent.velocity = lastKnownVelocity;

            // reset the isResuming flag
            isResuming = false;
        }

        // otherwise, once the agent is enabled, check the speed to adjust the animator as required
        else if (thisAgent.enabled)
        {
            lastKnownVelocity = thisAgent.velocity;

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
        }

    }

}