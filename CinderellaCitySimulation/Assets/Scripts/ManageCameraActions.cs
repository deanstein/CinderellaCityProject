
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

using System.Collections;
using System.IO;

public class ManageCameraActions : MonoBehaviour
{
    public class CameraActionGlobals
    {
        // gets the most recent postProcessingHost
        public static GameObject activeCameraHost;

        // record the highest known camera effect priority
        public static float highestKnownCameraEffectPriority = 0;

        // gets set true when there is a camera effect active
        public static bool isCameraEffectActive = false;

        // gets the currently-active camera effect
        public static string activeCameraEffect;
        
        //
        // define paths and file names
        //

        // define where camera effects like PostProcessProfiles are stored
        public static string cameraEffectsPath = "Effects/"; // in Resources

        // in-game only: the subfolder below the persistent data folder where screenshots will be saved
        public static string inGameScreenshotSubfolder = "/Screenshots/";

        // the two screenshot path options, depending on whether we're in the editor or not
        public static string inGameScreenshotsPath = Application.persistentDataPath + inGameScreenshotSubfolder;
        public static string editorScreenshotsPath = UIGlobals.projectUIPath;

        // the screenshot file format
        public static string screenshotFormat = ".png";
    }

    // get a screenshot path based on context (editor or in-game)
    public static string GetScreenshotPathByContext()
    {
        // if we're not in the editor, set the screenshot path as required
        if (!Application.isEditor)
        {
            string screenshotPath = CameraActionGlobals.inGameScreenshotsPath;

            // if the directory doesn't exist, create it
            if (!Directory.Exists(screenshotPath))
            {
                Directory.CreateDirectory(screenshotPath);
            }

            return screenshotPath;
        }
        // otherwise, we're in the editor, so the screenshot path is different
        else
        {
            string screenshotPath = CameraActionGlobals.editorScreenshotsPath;

            return screenshotPath;
        }
    }

    // get a screenshot file name based on context (editor or in-game)
    public static string GetScreenshotFileNameByContext()
    {
        // if we're not in the editor, set the screenshot path as required
        if (!Application.isEditor)
        {
            string screenshotFileName = GenerateInGameScreenshotFileName();

            return screenshotFileName;
        }
        // otherwise, we're in the editor, so the screenshot path is different
        else
        {
            string screenshotFileName = GenerateEditorScreenshotFileName(CameraActionGlobals.activeCameraHost);

            return screenshotFileName;
        }
    }

    // define how to generate the screenshot file name when taken from in the editor
    public static string GenerateEditorScreenshotFileName(GameObject cameraHost)
    {
        string screenshotName = cameraHost.name + CameraActionGlobals.screenshotFormat;

        return screenshotName;
    }

    // define how to generate the screenshot file name when taken from in-game
    public static string GenerateInGameScreenshotFileName()
    {
        // define the delimeter for screenshots
        string delimeter = " ";

        // define the date stamp format for the screenshot
        string dateStamp = System.DateTime.Now.ToString("yyyy") + System.DateTime.Now.ToString("MM") + System.DateTime.Now.ToString("dd");

        // define the time stamp format for the screenshot
        string timeStamp = System.DateTime.Now.ToString("hh") + System.DateTime.Now.ToString("mm") + System.DateTime.Now.ToString("ss");

        // define the scene-based identify
        string sceneName = SceneManager.GetActiveScene().name;

        // combine everything into a final filename
        string screenshotName = dateStamp + delimeter + timeStamp + delimeter + sceneName + CameraActionGlobals.screenshotFormat;

        return screenshotName;
    }

    // increment the current camera effect priority
    // this ensures that camera effects are always visible
    public static void IncrementCameraEffectPriority(PostProcessVolume cameraEffectVolume)
    {
        // if this priority is higher than the last recorded, then we already have priority
        if (CameraActionGlobals.highestKnownCameraEffectPriority < cameraEffectVolume.priority)
        {
            CameraActionGlobals.highestKnownCameraEffectPriority = cameraEffectVolume.priority;
        }
        // otherwise, increment this higher priority and set this to it
        else
        {
            CameraActionGlobals.highestKnownCameraEffectPriority++;
            cameraEffectVolume.priority = CameraActionGlobals.highestKnownCameraEffectPriority;
        }
    }

    // sets a post processing effects profile on this gameObject's child by the given name
    public static void SetPostProcessProfile(GameObject postProcessVolumeHost, string profileName)
    {
        // set this object as the globally-available postProcessingHost
        CameraActionGlobals.activeCameraHost = postProcessVolumeHost;
        //get the post processing volume from the given object
        PostProcessVolume postProcessVolume = postProcessVolumeHost.GetComponent<PostProcessVolume>();

        // only set the requested effect if no effect is active,
        // or if the requested effect is different than the active effect
        if (CameraActionGlobals.activeCameraEffect == null || CameraActionGlobals.activeCameraEffect != profileName)
        {
            // find the target profile by the given name
            PostProcessProfile targetProfile = Resources.Load(CameraActionGlobals.cameraEffectsPath + profileName) as PostProcessProfile;

            // set the target profile as the current profile
            postProcessVolume.profile = targetProfile;

            // ensure the new volume has priority
            IncrementCameraEffectPriority(postProcessVolume);

            // indicate that an effect is active
            CameraActionGlobals.isCameraEffectActive = true;

            // store the active effect for other cameras to access
            CameraActionGlobals.activeCameraEffect = profileName;
        }
        // if the requested profile is the same as the active profile, return to the default profile
        else if (CameraActionGlobals.activeCameraEffect == profileName)
        {
            // find the default profile for this scene
            PostProcessProfile defaultProfile = Resources.Load(CameraActionGlobals.cameraEffectsPath + GetDefaultPostProcessProfileBySceneName(SceneManager.GetActiveScene().name)) as PostProcessProfile;

            // set the default profile as the current profile
            postProcessVolume.profile = defaultProfile;

            // ensure the new volume has priority
            IncrementCameraEffectPriority(postProcessVolume);

            // indicate that an effect is no longer active
            CameraActionGlobals.activeCameraEffect = GetDefaultPostProcessProfileBySceneName(SceneManager.GetActiveScene().name);
        }
    }

    public static void SetPostProcessTransitionProfile(GameObject postProcessVolumeHost, string profileName)
    {
        PostProcessVolume currentVolume = postProcessVolumeHost.GetComponent<PostProcessVolume>();

        // get the transition profile
        PostProcessProfile flashProfile = Resources.Load(CameraActionGlobals.cameraEffectsPath + profileName) as PostProcessProfile;

        // get the current color grading settings
        PostProcessEffectSettings colorGradingSettings = currentVolume.profile.GetSetting<ColorGrading>();

        // for continuity, remove any color grading settings from Flash, and replace them with the outgoing profile's settings
        flashProfile.RemoveSettings<ColorGrading>();
        flashProfile.AddSettings(colorGradingSettings);

        // set the flash profile
        currentVolume.profile = flashProfile;

        // ensure the new volume has priority
        IncrementCameraEffectPriority(currentVolume);
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
