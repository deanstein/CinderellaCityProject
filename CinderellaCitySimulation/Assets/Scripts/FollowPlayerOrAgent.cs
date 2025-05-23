using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    bool isOnNavMeshWithinTolerance;

    private void Update()
    {
        if (ModeState.isGuidedTourActive || ModeState.isGuidedTourPaused)
        {
            // determine if the player is on the nav mesh within some tolerance
            // tolerance is roughly the height of a single stair
            float tolerance = ManageFPSControllers.FPSControllerGlobals.defaultAgentStepOffset;
            isOnNavMeshWithinTolerance = Utils.GeometryUtils.GetIsOnNavMeshWithinTolerance(
                new Vector3(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.x, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.y - ManageFPSControllers.FPSControllerGlobals.defaultFPSControllerHeight / 2, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position.z),
                tolerance, 
                false);

            // if player is on the nav mesh, we can turn gravity off
            // this prevents the player and the agent from being slightly misaligned
            // when toggling between active and paused guided tour
            if (isOnNavMeshWithinTolerance)
            {
                ManageFPSControllers.SetPlayerGravity(false);
            }
            // otherwise, need to prevent the player from getting too far from the nav mesh,
            // so turn gravity on if farther than the tolerance
            else
            {
                ManageFPSControllers.SetPlayerGravity(true);
            }
        }
        // gravity is always on if guided tour is not active or paused
        else
        {
            ManageFPSControllers.SetPlayerGravity(true);
        }

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
        // default behavior: agent follows FPSController
        else
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // if agent gets too far from player, 
                // disable the agent to unlock it from the navmesh
                // (it's possible for the agent to still get separated from the player
                // for example, if the player jumped over a rail)
                if (!isOnNavMeshWithinTolerance || Vector3.Distance(GetComponent<NavMeshAgent>().transform.position, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position) > 1.0f)
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
    }
}