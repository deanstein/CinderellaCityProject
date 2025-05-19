using UnityEngine;
using UnityEngine.AI;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    private void Update()
    {
        // guided tour is ON, so FPSController follows agent
        if (ModeState.isGuidedTourActive && !ModeState.isTimeTravelPeeking && !ModeState.isPeriodicTimeTraveling)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // always make sure the agent is on
                this.GetComponent<NavMeshAgent>().enabled = true;

                // this is required to be false for NavMeshAgent to not interfere
                this.GetComponent<NavMeshAgent>().updatePosition = false;

                // move the FPSControllerTransform to the NavMeshAgent
                Vector3 nextPosition = this.GetComponent<NavMeshAgent>().nextPosition;

                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position = nextPosition;
            }
        }
        // default during guided tour (paused or not)
        // the agent follows the FPSController
        else if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // if agent gets too far from player, 
                // disable the agent to unlock it from the navmesh
                // (it's possible for the agent to still get separated from the player
                // for example, if the player jumped over a rail)
                bool isOnNavMesh = Utils.GeometryUtils.GetIsOnNavMeshWithinTolerance(new Vector3(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.x, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.y - ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.baseOffset / 2, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.z), 1.0f);

                if (!ManageFPSControllers.FPSControllerGlobals.activeFPSController.GetComponentInChildren<CharacterController>().isGrounded || !isOnNavMesh || Vector3.Distance(GetComponent<NavMeshAgent>().transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position) > 1.0f)
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
        if (!ModeState.isTimeTravelPeeking && !ModeState.isPeriodicTimeTraveling && !ModeState.doShowTimeTravelingLabel)
        {

        }
    }
}