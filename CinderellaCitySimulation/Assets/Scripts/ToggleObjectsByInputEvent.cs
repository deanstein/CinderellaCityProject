using UnityEngine;

/// <summary>
/// Toggles geometry on/off via certain input events
/// </summary>

// this script should be attached to any object that needs to watch for shortcuts to toggle it, or its children, on/off
// this script also contains general object visibility utils used by other scripts
public class ToggleObjectsByInputEvent : MonoBehaviour {

     // watch for shortcuts every frame
    void Update()
    {
        /// object visibility shortcuts ///

        // people
        if (Input.GetKeyDown("p") && this.name.Contains("people"))
        {
            ToggleObjects.ToggleObjectChildrenVisibility(this.gameObject);
        }
        // reset people to starting location
        if (Input.GetKeyDown("o") && this.name.Contains("people"))
        {
            ManageNPCControllers.ResetAllNPCsToOriginalLocation();
        }
    }
}

public class ToggleObjects
{
    public static void ToggleGameObjectOff(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                //Utils.DebugUtils.DebugLog("Turning off GameObject: " + gameObject);
            }
        }
    }

    public static void ToggleGameObjectOn(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                //Utils.DebugUtils.DebugLog("Turning on GameObject: " + gameObject);
            }
        }
    }

    public static void ToggleObjectChildrenVisibility(GameObject parent)
    {
        // loop through all children of this GameObject and make them active or inactive
        foreach (Transform child in parent.transform)
        {
            // If key is pressed and children are on, turn them off
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);

            }
            // If key is pressed and children are off, turn them on
            else if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}

