using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePeople : MonoBehaviour {

    public string objectType = "people";
    public string objectState = "unknown";

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update()
    {
        // Place this script on a parent, and then loop through all children and disable them
        foreach (Transform child in this.transform)
        {
            // If key is pressed and children are on, turn them off
            if ((Input.GetKeyDown("p")) && (child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(false);
                objectState = "OFF";
                
            }
            // If key is pressed and children are off, turn them on
            else if ((Input.GetKeyDown("p")) && (!child.gameObject.activeSelf))
            {
                child.gameObject.SetActive(true);
                objectState = "ON";
            }
        }

        // Output the object type and the new state
        if (Input.GetKeyDown("p")) {
            Debug.Log("Turning visibility of " + objectType + " to: " + objectState);
        }

    }
}

