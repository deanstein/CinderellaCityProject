using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

// this script should be attached to an object that has an animator that should change based on speed

public class UpdateAnimatorBySpeed : MonoBehaviour
{
    // the navigation mesh agent on this object
    public NavMeshAgent thisAgent;
    // the previous recorded velocity of this navmesh agent
    public float lastVelocity = 0;

    void Start()
    {
        // get this nav mesh agent
        thisAgent = this.GetComponent<NavMeshAgent>();

    }

    private void Update()
    {
        // if we're just starting, and the navmesh agent hasn't been enabled, set the animator to idle
        if (lastVelocity == 0 && !thisAgent.enabled)
        {
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetIdleAnimatorControllerByGender(this.name));
        }

        // once the agent is enabled, check the speed to adjust the animator as required
        if (thisAgent.enabled)
        {
            lastVelocity = thisAgent.velocity.magnitude;

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