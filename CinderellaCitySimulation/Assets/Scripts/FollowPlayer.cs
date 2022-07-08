using UnityEngine;

// Responsible for making an object's position match the current player's position at all times
//attach this script to an object that needs to follow the player

public class FollowPlayer : MonoBehaviour
{
    private void Update()
    {
        if (!ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // update this object's position to match the player's last known position
                this.transform.position = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
            }
        }
        // make the player follow the agent's location
        else
        {
            ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position = this.transform.position;
        }
    }
}