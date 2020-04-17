using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class ToggleCameraEffectsByShortcut : MonoBehaviour {

    public static IEnumerator ToggleCameraEffectWithTransition(GameObject postProcessHost, string profileName, string transitionProfileName, float transitionTime)
    {
        // first, toggle the flash transition
        ManageCameraEffects.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // set the requested camera effect profile
        ManageCameraEffects.SetPostProcessProfile(postProcessHost, profileName);
    }

    private void OnEnable()
    {
        // optional: start a camera transition when the camera is enabled
        /*
        StartCoroutine(ToggleCameraEffectsByShortcut.ToggleCameraEffectWithTransition(this.gameObject, ManageCameraEffects.GetDefaultPostProcessProfileBySceneName(this.gameObject.scene.name), "FlashWhite", 0.2f));
        */
    }

    // Update is called once per frame
    void Update()
    {
        // update the globally-available Post Process host
        ManageCameraEffects.CameraEffectGlobals.activePostProcessingHost = this.gameObject;

        // define which effects belong to which shortcut
        if (Input.GetKeyDown("1"))
        {
            StartCoroutine(ToggleCameraEffectWithTransition(this.gameObject, "Vaporwave", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("2"))
        {
            StartCoroutine(ToggleCameraEffectWithTransition(this.gameObject, "B&W", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("3"))
        {
            StartCoroutine(ToggleCameraEffectWithTransition(this.gameObject, "Sepia", "FlashBlack", 0.4f));
        }
        else if (Input.GetKeyDown("4"))
        {
            StartCoroutine(ToggleCameraEffectWithTransition(this.gameObject, "Dark", "FlashBlack", 0.4f));
        }
    }
}

