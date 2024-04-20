using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Changes a NavMesh agent's animator to various poses 
/// depending on the speed of the agent
/// </summary>

// this script should be attached to an object that has an animator that should change based on speed
public class UpdateNPCAnimatorByState : MonoBehaviour
{
    // this object's agent and animator
    public NavMeshAgent thisAgent;
    public Animator thisAnimator;

    void Awake()
    {
        // get a random speed for this agent and animator to use
        float randomSpeed = UnityEngine.Random.Range(NPCControllerGlobals.minWalkingSpeed, NPCControllerGlobals.maxWalkingSpeed);

        // get this NPC's nav mesh agent
        thisAgent = this.GetComponent<NavMeshAgent>();
        thisAgent.speed = randomSpeed;

        // get this NPC's animator
        thisAnimator = this.GetComponent<Animator>();
        thisAnimator.speed = randomSpeed;
    }

    private void Update()
    {
        // if the agent hasn't been enabled, give it a walking animation
        // so that walking NPCs look normal as they calculate their nav mesh paths
        if (!thisAgent.enabled)
        {
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetWalkingAnimatorControllerByGender(this.name));

            thisAnimator.speed = thisAgent.velocity.magnitude;
        }
        // otherwise, when the agent is enabled, match the animation speed with the velocity
        else if (thisAgent.enabled)
        {
            // if we're moving, use this agent's default controller
            if (!thisAgent.pathPending && thisAgent.velocity.magnitude > 0)
            {
                thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(this.name));
            }

            thisAnimator.speed = thisAgent.velocity.magnitude;
        }

    }

}