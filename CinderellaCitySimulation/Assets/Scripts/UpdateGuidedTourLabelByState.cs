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

    // colors for active and paused
    Color activeColor = new Color(0.25f, 0.55f, 0.25f);
    Color pausedColor = new Color(0.45f, 0.45f, 0.25f);

    private void Awake()
    {
        guidedTourText = GetComponentInChildren<Text>();
        guidedTourLabel = guidedTourText.gameObject;
    }

    private void Update()
    {
        // ensure that this label is always shown if guided tour is active
        if (ModeState.isGuidedTourActive)
        {
            guidedTourLabel.SetActive(true);
            // text and color depend on whether tour is paused
            guidedTourText.text = UIGlobals.guidedTourActive;
            guidedTourText.color = activeColor;
        }
        // update text and color if paused
        else if (ModeState.isGuidedTourPaused)
        {
            // text and color depend on whether tour is paused
            guidedTourText.text = UIGlobals.guidedTourPaused;
            guidedTourText.color = pausedColor;
        }
        // otherwise, it should be hidden entirely
        else
        {
            guidedTourLabel.SetActive(false);
        }
    }

}