using UnityEngine;

/// <summary>
/// Enables or disables GameObject Components (AudioSources, scripts...) b
/// ased on their proximity to the player
/// Used for stationary objects like speakers
/// </summary>

// this script should be attached to a parent GameObject containing
// other children with components that need to be disabled when too far from the player

public class ToggleComponentsByProximityToPlayer : MonoBehaviour {

    // these are passed in from AssetImportPipeline
    // these are the component types that will get toggled
    public string[] toggleComponentTypes;

    // may be set by other scripts to except this object temporarily from having its scripts disabled
    public bool isExcepted = false;

    // the maximum distance this object can be from the FPSController before the specified component is disabled
    // this may be overridden by AssetImportPipeline
    public float maxDistance = 50f;

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
                    Utils.DebugUtils.DebugLog("Enabled " + componentType + " on " + this.name);
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
                    Utils.DebugUtils.DebugLog("Disabled " + componentType + " on " + this.name);
                    (this.GetComponent(componentType) as Behaviour).enabled = false;
                }
            }
        }
    }
}

