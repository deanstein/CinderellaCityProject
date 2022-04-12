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
        if (Input.GetKeyDown("p") && this.name.Contains(ObjectVisibilityGlobals.peopleObjectKeywords[0]))
        {
            ToggleObjects.ToggleGameObjectChildrenVisibility(this.gameObject);
        }
        // reset people to starting location
        if (Input.GetKeyDown("o") && this.name.Contains(ObjectVisibilityGlobals.peopleObjectKeywords[0]))
        {
            ManageNPCControllers.ResetAllNPCsToOriginalLocation();
        }
    }
}

public class ToggleObjects
{
    // toggle just a single object on or off
    public static bool ToggleGameObjectVisibility(GameObject gameObject)
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return false;
        }
        else
        {
            gameObject.SetActive(true);
            return true;
        }
    }

    public static void ToggleGameObjectOff(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                //Utils.DebugUtils.DebugLog("Turning off GameObject: " + child.gameObject.name);
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

    // toggle all children but leave the parent alone
    public static void ToggleGameObjectChildrenVisibility(GameObject parent)
    {
        // loop through all children of this GameObject and make them active or inactive
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}

