using UnityEngine;
using UnityEngine.AI;

// attach this script to an object that needs to follow the player

public class UpdateFPSAgentByState : MonoBehaviour
{
    private void Update()
    {
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
        {
            // determine if this player agent host is
            // at the same position as the player within some tolerance
            bool isFPSAgentAtPlayerPos = Utils.GeometryUtils.GetFastDistance(this.transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position) < 2.0f;

            // if the agent host object and the player are not within some max distance, 
            // disable the nav mesh agent
            if (!isFPSAgentAtPlayerPos)
            {
                this.GetComponent<NavMeshAgent>().enabled = false;
                //Debug.Log("Disabling player nav mesh agent.");
            }

            // only try to re-enable the nav mesh agent if the player is on the nav mesh
            // and if the player agent host is in sync with the player
            if (ManageFPSControllers.FPSControllerGlobals.isActiveFPSControllerOnNavMesh && isFPSAgentAtPlayerPos)
            {
                this.GetComponent<NavMeshAgent>().enabled = true;
                //Debug.Log("Enabling player nav mesh agent.");
            }
        }
    }
}