using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

// this script should be attached to FPSCharacter objects that need to watch for shortcuts to adjust camera effects
public class ToggleCameraEffectsByInputEvent : MonoBehaviour {

    private void OnEnable()
    {
        // optional: start a camera transition when the camera is enabled
        /*
        StartCoroutine(ToggleCameraEffectsByInputEvent.ToggleCameraEffectWithTransition(this.gameObject, ManageCameraEffects.GetDefaultPostProcessProfileBySceneName(this.gameObject.scene.name), "FlashWhite", 0.2f));
        */
    }

    // watch for shortcuts every frame
    void Update()
    {
        // update the globally-available Post Process host
        ManageCameraEffects.CameraEffectGlobals.activePostProcessingHost = this.gameObject;

        // define which effects belong to which shortcut
        if (Input.GetKeyDown("1"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Vaporwave", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("2"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "B&W", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("3"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Sepia", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("4"))
        {
            StartCoroutine(ToggleCameraEffects.ToggleCameraEffectWithTransition(this.gameObject, "Dark", "FlashBlack", 0.4f));
        }
    }
}

public class ToggleCameraEffects
{
    public static IEnumerator ToggleCameraEffectWithTransition(GameObject postProcessHost, string profileName, string transitionProfileName, float transitionTime)
    {
        // first, toggle the flash transition
        ManageCameraEffects.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // set the requested camera effect profile
        ManageCameraEffects.SetPostProcessProfile(postProcessHost, profileName);
    }
}

