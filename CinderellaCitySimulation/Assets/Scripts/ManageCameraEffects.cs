
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

using System.Collections;

public class ManageCameraEffects : MonoBehaviour
{
    public class CameraEffectGlobals
    {
        // gets set true when there is a camera effect active
        public static bool isCameraEffectActive = false;

        // gets the currently-active camera effect
        public static string activeCameraEffect;

        // gets the most recent postProcessingHost
        public static GameObject activePostProcessingHost;

        // define where camera effects like PostProcessProfiles are stored (in Resources)
        public static string effectsPath = "Effects/";
    }

    // sets a post processing effects profile on this gameObject's child by the given name, with the given transition time
    public static void SetPostProcessProfile(GameObject postProcessVolumeHost, string profileName)
    {
        // set this object as the globally-available postProcessingHost
        CameraEffectGlobals.activePostProcessingHost = postProcessVolumeHost;
        //get the post processing volume from the given object
        PostProcessVolume postProcessVolume = postProcessVolumeHost.GetComponent<PostProcessVolume>();

        // only set the requested effect if no effect is active,
        // or if the requested effect is different than the active effect
        if (CameraEffectGlobals.activeCameraEffect == null || CameraEffectGlobals.activeCameraEffect != profileName)
        {
            // find the target profile by the given name
            PostProcessProfile targetProfile = Resources.Load(CameraEffectGlobals.effectsPath + profileName) as PostProcessProfile;

            // set the target profile as the current profile
            postProcessVolume.profile = targetProfile;

            // indicate that an effect is active
            CameraEffectGlobals.isCameraEffectActive = true;

            // store the active effect for other cameras to access
            CameraEffectGlobals.activeCameraEffect = profileName;
        }
        // if the requested profile is the same as the active profile, return to the default profile
        else if (CameraEffectGlobals.activeCameraEffect == profileName)
        {
            // find the default profile for this scene
            PostProcessProfile defaultProfile = Resources.Load(CameraEffectGlobals.effectsPath + GetDefaultPostProcessProfileBySceneName(SceneManager.GetActiveScene().name)) as PostProcessProfile;

            // set the default profile as the current profile
            postProcessVolume.profile = defaultProfile;

            // indicate that an effect is no longer active
            CameraEffectGlobals.activeCameraEffect = GetDefaultPostProcessProfileBySceneName(SceneManager.GetActiveScene().name);
        }
    }

    public static void SetPostProcessTransitionProfile(GameObject postProcessVolumeHost, string profileName)
    {
        PostProcessVolume currentVolume = postProcessVolumeHost.GetComponent<PostProcessVolume>();

        // get the transition profile
        PostProcessProfile flashProfile = Resources.Load(CameraEffectGlobals.effectsPath + profileName) as PostProcessProfile;

        // get the current color grading settings
        PostProcessEffectSettings colorGradingSettings = currentVolume.profile.GetSetting<ColorGrading>();

        // for continuity, remove any color grading settings from Flash, and replace them with the outgoing profile's settings
        flashProfile.RemoveSettings<ColorGrading>();
        flashProfile.AddSettings(colorGradingSettings);

        // set the flash profile
        currentVolume.profile = flashProfile;
    }

    public static string GetDefaultPostProcessProfileBySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case string name when sceneName.Contains("60s70s"):
                return "60s70s";
            case string name when sceneName.Contains("80s90s"):
                return "80s90s";
            case string name when sceneName.Contains("AltFuture"):
                return "AltFuture";
            default:
                return null;
        }
    }
}
