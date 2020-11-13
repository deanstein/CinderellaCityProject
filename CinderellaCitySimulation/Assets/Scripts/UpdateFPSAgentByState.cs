using UnityEngine;
using UnityEngine.AI;

// attach this script to an object that needs to follow the player

public class UpdateFPSAgentByState : MonoBehaviour
{
    private void Update()
    {
        ToggleFPSAgentByProximityToPlayer();
    }

    static void ToggleFPSAgentByProximityToPlayer()
    {
        // if the FPSController and its associated agent are not nearby,
        // disable the agent until the FPSController is on a valid nav mesh location
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
        {
            // get the current nav mesh agent
            // determine if this player agent host is
            // at the same position as the player within some tolerance
            bool isFPSAgentAtPlayerPos = Vector3.Distance(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.nextPosition, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position) < 1.0f;
            //Utils.DebugUtils.DebugLog("Nav Mesh Agent pos: " + ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.nextPosition);
            //Utils.DebugUtils.DebugLog("FPS controller position: " + ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position);
            //Utils.DebugUtils.DebugLog("Is FPS agent at player pos? " + isFPSAgentAtPlayerPos);

            // if the agent host object and the player are not within some max distance, 
            // disable the nav mesh agent
            if (!isFPSAgentAtPlayerPos)
            {
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.enabled = false;
                //Utils.DebugUtils.DebugLog("Disabling player nav mesh agent + " + ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.parent.name);
            }

            // only try to re-enable the nav mesh agent if the player is on the nav mesh
            // and if the player agent host is in sync with the player
            if (ManageFPSControllers.FPSControllerGlobals.isActiveFPSControllerOnNavMesh && isFPSAgentAtPlayerPos)
            {
                ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.enabled = true;
                //Utils.DebugUtils.DebugLog("Enabling player nav mesh agent + " + ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent.transform.parent.name);
            }
        }
    }
}