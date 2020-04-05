using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class ToggleCameraEffectsByShortcut : MonoBehaviour {

    private void Awake()
    {
        // when this object is awoken, update the globally-available Post Process host
        ManageCameraEffects.CameraEffectGlobals.activePostProcessingHost = this.gameObject;

        // on awake, set the flash profile, then reset it to the original
        // TODO: why doesn't this work?
        //StartCoroutine(ManageCameraEffects.SetFlashThenTargetProfile(this.gameObject, 0.5f));
    }

    // Update is called once per frame
    void Update()
    {
              
        // determine which effects belong to which shortcut
        if (Input.GetKeyDown("1"))
        {
            StartCoroutine(ManageCameraEffects.SetPostProcessProfile(this.gameObject, "Vaporwave", 0.5f));
        }
        else if (Input.GetKeyDown("2"))
        {
            StartCoroutine(ManageCameraEffects.SetPostProcessProfile(this.gameObject, "B&W", 0.5f));
        }
        else if (Input.GetKeyDown("3"))
        {
            StartCoroutine(ManageCameraEffects.SetPostProcessProfile(this.gameObject, "Sepia", 0.5f));
        }
        else if (Input.GetKeyDown("4"))
        {
            StartCoroutine(ManageCameraEffects.SetPostProcessProfile(this.gameObject, "Dark", 0.5f));
        }

    }
}

