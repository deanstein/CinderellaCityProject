using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// this script should be attached to any object that needs to watch for shortcuts to toggle it, or its children, on/off
// this script also contains general object visibility utils used by other scripts
public class ToggleObjectsByInputEvent : MonoBehaviour {

    // these are passed in from AssetImportPipeline
    public string objectType;
    public string shortcut;

    // used for console debugging
    public string objectState;

     // watch for shortcuts every frame
    void Update()
    {
        /// object visibility shortcuts ///

        // people
        if (Input.GetKeyDown("p") && this.name.Contains("people"))
        {
            ToggleObjects.ToggleObjectChildrenVisibility(this.gameObject);
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
                Debug.Log("Turning off GameObject: " + gameObject);
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
                Debug.Log("Turning on GameObject: " + gameObject);
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

