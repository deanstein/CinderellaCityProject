using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVisibilityByShortcut : MonoBehaviour {

    // these are passed in from AssetImportPipeline
    public string objectType;
    public string shortcut;

    // used for console debugging
    public string objectState;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update()
    {
        // loop through all children of this GameObject and make them active or inactive
        foreach (Transform child in this.transform)
        {
            // If key is pressed and children are on, turn them off
            if ((Input.GetKeyDown(shortcut)) && (child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(false);
                objectState = "OFF";
                
            }
            // If key is pressed and children are off, turn them on
            else if ((Input.GetKeyDown(shortcut)) && (!child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(true);
                objectState = "ON";
            }
        }

        // Output the object type and the new state
        if (Input.GetKeyDown(shortcut)) {
            Debug.Log("Turning visibility of " + objectType + " to: " + objectState);
        }

    }
}

