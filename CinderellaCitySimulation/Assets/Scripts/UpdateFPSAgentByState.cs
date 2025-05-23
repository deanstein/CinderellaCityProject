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
        bool? enableFPSAgent = null;

        // for regular gameplay, agent should be on so player won't clip NPCs
        if (!ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused)
        {
            enableFPSAgent = true;
        }

        // agent should be enabled when guided tour is active
        if (ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused)
        {
            enableFPSAgent = true;

            // but not if anti-gravity mode is enabled
            if (ModeState.isAntiGravityModeActive)
            {
                enableFPSAgent = false;
            }

            // agent should be disabled if we're time-travel peeking or periodic time-traveling
            if (ModeState.isTimeTravelPeeking || ModeState.isPeriodicTimeTraveling)
            {
                enableFPSAgent = false;
            }
        }
        else if (ModeState.isGuidedTourPaused)
        {
            // agent should be disabled when guided tour is paused
            enableFPSAgent = false;

            // agent should be disabled when anti-gravity mode is on
            if (ModeState.isAntiGravityModeActive)
            {
                enableFPSAgent = false;
            }
            else
            {
                enableFPSAgent = true;
            }

            // agent should be disabled if we're time-travel peeking or periodic time-traveling
            if (ModeState.isTimeTravelPeeking || ModeState.isPeriodicTimeTraveling)
            {
                enableFPSAgent = false;
            }

            // if the player is taking control during guided tour,
            // enable the agent to ensure no NPC clipping
            if (FollowGuidedTour.GetIsGuidedTourOverrideRequested())
            {
                enableFPSAgent = true;
            }
        }

        // agent should always be off if time-traveling
        if (ModeState.doShowTimeTravelingLabel)
        {
            enableFPSAgent = false;
        }

        // set the agent if the result was defined
        if (enableFPSAgent.HasValue)
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = enableFPSAgent.Value;
        }
    }
}