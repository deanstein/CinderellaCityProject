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
        if (ModeState.isGuidedTourActive)
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
        // otherwise, guided tour is OFF or PAUSED so agent follows FPSController
        else if (!ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // it's possible for the agent to still get separated from the player
                // for example, if the player jumped over a rail
                // if agent gets too far from player, disable the agent to unlock it from the navmesh
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

                // update this object's position to match the player's last known position
                GetComponent<NavMeshAgent>().transform.position = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
            }
        }
    }
}