using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePresence : MonoBehaviour {

    public GameObject targetObjects;
    public GameObject UIElement;

    // Use this for initialization
    void Start () {

        targetObjects = GameObject.FindWithTag("Mall1980s");
        UIElement = GameObject.FindWithTag("UI");

    }
	
	// Update is called once per frame
	void Update () {

        // if key is pressed, turn on or off a target elementf
        if ((Input.GetKeyDown("h")) && (targetObjects.activeSelf)) {
            targetObjects.SetActive(false);
            Debug.Log("Turning OFF objects: " + targetObjects);
        }

        else if ((Input.GetKeyDown("h")) && (!targetObjects.activeSelf)) {
            targetObjects.SetActive(true);
            Debug.Log("Turning ON objects: " + targetObjects);
        }

        }

    /*void OnGUI()
    {
        GUI.
        if (GUI.Button(UIElement, "I am a button"))
        {
            Debug.Log("You clicked the button!");
        }
    }*/

}

