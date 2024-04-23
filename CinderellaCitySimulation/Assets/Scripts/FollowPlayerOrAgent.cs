using UnityEngine;
using UnityEngine.AI;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    private void Update()
    {
        // only follow player if guided tour is NOT active
        if (!ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // update this object's position to match the player's last known position
                this.transform.position = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
            }
            // if guided tour is active, FPSController must follow agent
        } else
        {
            // this is required to be false for NavMeshAgent to not interfere
            this.GetComponent<NavMeshAgent>().updatePosition = false;

            // move the CharacterController to the NavMeshAgent's next position
            // using SimpleMove here prevents the agent from changing the height of the controller
            this.transform.parent.GetComponentInChildren<CharacterController>().SimpleMove(this.GetComponent<NavMeshAgent>().velocity);
        }
    }
}