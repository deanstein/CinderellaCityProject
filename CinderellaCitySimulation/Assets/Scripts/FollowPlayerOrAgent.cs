using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    private void Update()
    {
        // guided tour is ON, so FPSController follows agent
        if (ModeState.isGuidedTourActive && 
            !ModeState.isTimeTravelPeeking && 
            !ModeState.isPeriodicTimeTraveling ||
            ModeState.isFPSAgentTraversingMeshLink)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // always make sure the agent is on
                if (!ModeState.isFPSAgentTraversingMeshLink)
                {
                    this.GetComponent<NavMeshAgent>().enabled = true;
                }

                // this is required to be false for NavMeshAgent to not interfere
                this.GetComponent<NavMeshAgent>().updatePosition = false;

                // move the FPSControllerTransform to the NavMeshAgent
                Vector3 nextPosition = this.GetComponent<NavMeshAgent>().nextPosition;

                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position = nextPosition;
            }
        }
        // default behavior: agent follows FPSController
        else
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // if agent gets too far from player, 
                // disable the agent to unlock it from the navmesh
                // (it's possible for the agent to still get separated from the player
                // for example, if the player jumped over a rail)
                if (Vector3.Distance(GetComponent<NavMeshAgent>().transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position) > 1.0f)
                {
                    GetComponent<NavMeshAgent>().enabled = false;
                }
                // otherwise, ensure the agent is enabled
                else
                {
                    GetComponent<NavMeshAgent>().enabled = true;
                }

                this.GetComponent<NavMeshAgent>().updatePosition = true;

                // update the NavmeshAgent's position 
                // to match the player's last known position
                GetComponent<NavMeshAgent>().transform.position = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
            }
        }

        // update gravity on player
        // gravity is always off when guided tour is active or paused
        // to ensure that the agent and player don't have a mismatched height
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (FollowGuidedTour.GetIsGuidedTourOverrideRequested())
            {
                ManageFPSControllers.SetPlayerGravity(true);
            }
            else
            {
                ManageFPSControllers.SetPlayerGravity(false);
            }
        }
        // otherwise, gravity is on when override (mouse or controller) is requested
        // and when anti-gravity mode isn't active
        else if (FollowGuidedTour.GetIsGuidedTourOverrideRequested() && !ModeState.isAntiGravityModeActive)
        {
            ManageFPSControllers.SetPlayerGravity(true);
        }
    }
}