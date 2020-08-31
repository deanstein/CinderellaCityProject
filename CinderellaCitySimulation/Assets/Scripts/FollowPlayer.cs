using UnityEngine;

// attach this script to an object that needs to follow the player

public class FollowPlayer : MonoBehaviour
{
    private void Update()
    {
        if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
        {
            // update this object's position to match the player's last known position
            this.transform.position = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
        }
    }
}