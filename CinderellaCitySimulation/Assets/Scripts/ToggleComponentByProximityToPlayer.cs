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

    private void Start()
    {
        // add a sphere collider
        SphereCollider sphereCollider = this.gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = maxDistance;
        sphereCollider.isTrigger = true;
    }

    // when the FPSController is within the sphere collider (within range)
    public void OnTriggerEnter(Collider other)
    {
        if (!isExcepted && other.gameObject.name.Contains("FPSController"))
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
    }

    // when the FPSController is exiting the sphere collider (out of range)
    public void OnTriggerExit(Collider other)
    {
        if (!isExcepted && other.gameObject.name.Contains("FPSController"))
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

