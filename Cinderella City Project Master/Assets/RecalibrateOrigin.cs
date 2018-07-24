using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOriginToBoundingBoxCenter : MonoBehaviour {

    public GameObject speakerObjects;

    // Use this for initialization
    void Start () {

        // get objects with the correct tag
        speakerObjects = GameObject.FindWithTag("speakers");
        Debug.Log("Found these objects tagged as speaker objects: " + speakerObjects);

        for (int i = 0; i < 5; i++)
        {
            Debug.Log("Turning OFF objects: " + speakerObjects);
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
