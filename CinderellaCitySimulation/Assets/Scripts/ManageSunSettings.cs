
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// this script needs to be attached to the Sun object in each FPSController scene

public class ManageSunSettings : MonoBehaviour
{
    public class SunGlobals
    {
        // define the pause menu sun, which is to be overwritten
        public static Light sunPauseMenu;

        // define the suns for FPS Controller scenes
        public static Light sun60s70s;
        public static Light sun80s90s;
        public static Light sunAltFuture;
    }

    public void Start()
    {
        // get all the scene Suns
        AssignSunByParentName();
    }

    // defines the Sun (Light) for each FPS Controller scene, as well as the Pause Menu (required for disabled scene screenshots)
    public void AssignSunByParentName()
    {
        if (this.transform.parent.name.Contains("60s70s"))
        {
            SunGlobals.sun60s70s = this.GetComponent<Light>();
        }
        else if (this.transform.parent.name.Contains("80s90s"))
        {
            SunGlobals.sun80s90s = this.GetComponent<Light>();
        }
        else if (this.transform.parent.name.Contains("AltFuture"))
        {
            SunGlobals.sunAltFuture = this.GetComponent<Light>();
        }
        // pause menu sun gets overwritten for consistency in disabled scene screenshots
        else if (this.transform.parent.name.Contains("PauseMenu"))
        {
            SunGlobals.sunPauseMenu = this.GetComponent<Light>();
        }
    }

    // returns the Sun (Light) by the given scene name
    public static Light GetSunBySceneName(string sceneName)
    {
        if (sceneName.Contains("60s70s"))
        {
            return SunGlobals.sun60s70s;
        }
        else if (sceneName.Contains("80s90s"))
        {
            return SunGlobals.sun80s90s;
        }
        else if (sceneName.Contains("AltFuture"))
        {
            return SunGlobals.sunAltFuture;
        }
        else if (sceneName.Contains("PauseMenu"))
        {
            return SunGlobals.sunPauseMenu;
        }
        else
        {
            Debug.Log("Failed to find a sun for the given Scene name.");
            return null;
        }
    }

    // used by PauseMenu to inherit sun settings from a given Scene
    public static void InheritSunSettingsBySceneName(string sceneName, Light sunToInherit, Light sunToModify)
    {
        // only used for the PauseMenu
        if (SceneManager.GetActiveScene().name.Contains("PauseMenu"))
        {
            // inherit the sun's transform
            sunToModify.transform.position = sunToInherit.transform.position;
            sunToModify.transform.rotation = sunToInherit.transform.rotation;

            // inherit the sun's properties
            sunToModify.type = sunToInherit.type;
            sunToModify.color = sunToInherit.color;
            //sunToModify.lightmapBakeType = sunToInherit.lightmapBakeType; // seems to be Editor-only
            sunToModify.intensity = sunToInherit.intensity;
            sunToModify.bounceIntensity = sunToInherit.bounceIntensity;
            sunToModify.shadows = sunToInherit.shadows;
            //sunToModify.shadowAngle = sunToInherit.shadowAngle; // seems to be Editor-only
        }
    }

}
