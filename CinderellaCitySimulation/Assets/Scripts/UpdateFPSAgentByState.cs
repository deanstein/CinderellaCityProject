using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Responsible for toggling the agent attached to the player on/off to enable the agent to relocate to the player's position when required.
/// </summary>

// attach this script to an object that needs to follow the player
public class UpdateFPSAgentByState : MonoBehaviour
{
    private void Update()
    {
            // handle guided tour state
            if (ModeState.isGuidedTourPaused)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
            }
            else if (ModeState.isGuidedTourActive)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = true;
            }
            else if (!ModeState.isGuidedTourActive)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
            }

        // otherwise, in normal gameplay (non-guided tour related)
        // handle agent rubberbanding from player in certain cases

        // toggle the nav mesh off if the player is not on the nav mesh
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponent<CharacterController>().isGrounded)
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = true;
        } else
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
        }

        if (ModeState.isAntiGravityModeActive)
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
        } else
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = true;
        }
    }
}