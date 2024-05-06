using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the guided tour label text and color 
/// depending on whether guided tour is is active or paused
/// </summary>

// this script should be attached to the guided tour label object
public class UpdateGuidedTourLabelByState : MonoBehaviour
{
    // the label is used for visibility, the text for styling
    private GameObject guidedTourLabel;
    private Text guidedTourText;

    private void Awake()
    {
        guidedTourText = GetComponentInChildren<Text>();
        guidedTourLabel = guidedTourText.gameObject;
    }

    private void Update()
    {
        // ensure that this label is always shown if guided tour is active
        if (ManageFPSControllers.FPSControllerGlobals.isGuidedTourActive)
        {
            guidedTourLabel.SetActive(true);
            // text and color depend on whether tour is paused
            guidedTourText.text = UIGlobals.guidedTourActive;
            guidedTourText.color = Color.green;
        }
        // update text and color if paused
        else if (ManageFPSControllers.FPSControllerGlobals.isGuidedTourPaused)
        {
            // text and color depend on whether tour is paused
            guidedTourText.text = UIGlobals.guidedTourPaused;
            guidedTourText.color = Color.yellow;
        }
        // otherwise, it should be hidden entirely
        else
        {
            guidedTourLabel.SetActive(false);
        }
    }

}