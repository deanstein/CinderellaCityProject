using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleComponentByProximityToPlayer : MonoBehaviour {

    // these are passed in from AssetImportPipeline
    // the component type should get toggled off
    public string[] toggleComponentTypes;

    // may be set by other scripts to except this object temporarily from having its scripts disabled
    public bool isExcepted = false;

    // this is the FPSController that we measure against to determine proximity
    public GameObject activeFPSController;

    // the maximum distance this object can be from the FPSController before the specified component is disabled
    public float maxDistance = 20f;

    private void Awake()
    {
        // before anything, toggle the requested components off
        foreach (string componentType in toggleComponentTypes)
        {
            // define the monobehavior based on the given type string
            Behaviour componentToToggle = this.GetComponent(componentType) as Behaviour;

            // before anything, toggle the component off
            componentToToggle.enabled = false;
        }
    }

    private void OnEnable()
    {
        // set the current active FPS controller by getting it from the FPSController manager
        activeFPSController = ManageFPSControllers.FPSControllerGlobals.activeFPSController;
    }

    // Update is called once per frame
    void Update()
    {
        // calculate the distance from this object to the FPSController
        float distanceToFPSController = Vector3.Distance(this.gameObject.transform.position, activeFPSController.gameObject.transform.position);

        // if we're within the max distance, ensure the specified components are enabled
        if (!isExcepted && (distanceToFPSController < maxDistance))
        {
            foreach (string componentType in toggleComponentTypes)
            {
                if (!(this.GetComponent(componentType) as Behaviour).enabled)
                {
                    Debug.Log("Enabled " + componentType + " on " + this.name);
                    (this.GetComponent(componentType) as Behaviour).enabled = true;
                }
            }
        }

        // otherwise, disable the specified components
        else if (!isExcepted && (distanceToFPSController > maxDistance))
        {
            foreach (string componentType in toggleComponentTypes)
            {
                if ((this.GetComponent(componentType) as Behaviour).enabled)
                {
                    Debug.Log("Disabled " + componentType + " on " + this.name);
                    (this.GetComponent(componentType) as Behaviour).enabled = false;
                }
            }
        }
    }
}

