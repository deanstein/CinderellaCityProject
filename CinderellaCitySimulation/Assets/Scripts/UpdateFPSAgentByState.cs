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
        // agent should be enabled when guided tour is active
        if (ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused)
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = true;

            // but not if anti-gravity mode is enabled
            if (ModeState.isAntiGravityModeActive)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
            }
        }
        else if (ModeState.isGuidedTourPaused)
        {
            // agent should be disabled when guided tour is paused
            gameObject.GetComponent<NavMeshAgent>().enabled = false;

            // agent should be disabled when anti-gravity mode is on
            if (ModeState.isAntiGravityModeActive)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
            }
            else
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = true;
            }

            // agent should be disabled if we're time-travel peeking or periodic time-traveling
            if (ModeState.isTimeTravelPeeking || ModeState.isPeriodicTimeTraveling)
            {
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
            }
        }
    }
}