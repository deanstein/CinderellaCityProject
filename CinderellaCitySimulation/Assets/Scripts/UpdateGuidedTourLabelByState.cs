using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the guided tour label text and color 
/// depending on whether guided tour is is active or paused
/// </summary>

// this script should be attached to the guided tour label object
public class UpdateGuidedTourLabelByState : MonoBehaviour
{
    // this object's agent and animator
    private Text thisText;

    private void Awake()
    {
        thisText = this.GetComponentInChildren<Text>(); 
    }

    private void Update()
    {
        thisText.text = ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive ? UIGlobals.guidedTourActive : UIGlobals.guidedTourPaused;
        thisText.color = ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive ? Color.green : Color.yellow;
    }

}