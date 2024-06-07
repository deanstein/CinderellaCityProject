using UnityEngine;
using UnityEngine.AI;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    private void Update()
    {
        // only follow player if guided tour is NOT active or paused
        if (!ModeState.isGuidedTourActive)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // update this object's position to match the player's last known position
                GetComponent<NavMeshAgent>().nextPosition = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
                this.GetComponent<NavMeshAgent>().updatePosition = true;
            }
            // if guided tour is active, FPSController must follow agent
        } else
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // this is required to be false for NavMeshAgent to not interfere
                this.GetComponent<NavMeshAgent>().updatePosition = false;

                // move the FPSControllerTransform to the NavMeshAgent
                Vector3 nextPosition = this.GetComponent<NavMeshAgent>().nextPosition;

                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position = nextPosition;
            }
        }
    }
}