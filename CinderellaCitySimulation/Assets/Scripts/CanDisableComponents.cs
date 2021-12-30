using UnityEngine;

/// <summary>
/// Get and set whether components of the attached object can be disabled by its parent or not
/// </summary>

// this script should be attached to any object whose parent can disable all children
// but some children may be exempt from disablement - for example, master audiosource objects
public class CanDisableComponents : MonoBehaviour {

    // by default, allow disabling all components
    // this may be overridden by other scripts for certain scenarios
    public bool canDisableComponents = true;
}