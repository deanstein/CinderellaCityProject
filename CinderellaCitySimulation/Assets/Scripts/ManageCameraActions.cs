using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.IO;

/// <summary>
/// Offers various camera actions, including taking screenshots and applying effects
/// </summary>

public class ManageCameraActions : MonoBehaviour
{
    public class CameraActionGlobals
    {
        // gets the most recent postProcessingHost
        public static GameObject activeCameraHost;

        // all cameras used for UI screenshots
        public static GameObject[] allThumbnailCameras;

        // all cameras used for guided tour waypoints
        public static GameObject[] allPointOfInterestCameras;

        // record the highest known camera effect priority
        public static float highestKnownCameraEffectPriority = 0;

        // gets set true when there is a camera effect active
        public static bool isCameraEffectActive = false;

        // gets the currently-active camera effect
        public static string activeCameraEffect;

        //
        // thumbnail cameras - geometry-based camera objects from FormIt
        //

        public static string proxyCamerasObjectName = "proxy-cameras";
        public static string proxyCamerasPhotosObjectName = "proxy-cameras-photos";
        // FormIt object names to trigger camera generation
        public static string thumbnailCameraKeyword = "Camera-Thumbnail-";
        public static string historicPhotoCameraKeyword = "Photo Object";

        // number of seconds to wait for a thumbnail camera to be captured
        public static int thumbnailCameraCaptureDelay = 1;

        //
        // define paths and file names
        //

        // screenshot mode flag
        public static string screenshotModeFlag = "CCP.IsInScreenshotMode";

        // define where camera effects like PostProcessProfiles are stored
        public static string cameraEffectsPath = "Effects/"; // in Resources

        // in-game only: the subfolder below the persistent data folder where screenshots will be saved
        public static string screenshotsSubfolder = "/Screenshots/";
        public static string savedViewsSubfolder = "/Saved Views/";

        // screenshot and saved views path while in-game (not in editor)
        public static string inGameScreenshotsPath = Application.persistentDataPath + screenshotsSubfolder;
        public static string savedViewsPath = Application.persistentDataPath + savedViewsSubfolder;
        // editor only
        public static string editorScreenshotsPath = UIGlobals.projectUIPath;

        // the screenshot file format
        public static string screenshotFormat = ".png";
    }

    // get a screenshot path based on context (editor or in-game)
    public static string GetScreenshotPathByContext()
    {
        // if we're not in the editor, 
        // set the screenshot path where an end user can find it
        if ((!Application.isEditor))
        {
            string screenshotPath = CameraActionGlobals.inGameScreenshotsPath;

            // if the directory doesn't exist, create it
            if (!Directory.Exists(screenshotPath))
            {
                Directory.CreateDirectory(screenshotPath);
            }

            return screenshotPath;
        }
        // otherwise, we're in the editor capturing batch screenshots
        // so the screenshot path is different
        else
        {
            string screenshotPath = CameraActionGlobals.editorScreenshotsPath;

            return screenshotPath;
        }
    }

    // get a screenshot file name based on context (editor or in-game)
    public static string GetScreenshotFileNameByContext()
    {
        // if we're not in the editor, 
        // set the screenshot name for end users
        if ((!Application.isEditor))
        {
            string screenshotFileName = GetInGameScreenshotFileName();

            return screenshotFileName;
        }
        // otherwise, we're in the editor capturing batch screenshots
        // so the screenshot name is different
        else
        {
            string screenshotFileName = UIGlobals.projectUIPath + (CameraActionGlobals.activeCameraHost);

            return screenshotFileName;
        }
    }

    // get all the thumbnail cameras in this scene
    // these were previously created from geometry-based cameras and tagged appropriately
    // so find them by tag
    public static GameObject[] GetAllThumbnailCamerasInScene()
    {
        GameObject[] allThumbnailCameras = GameObject.FindGameObjectsWithTag(ManageTaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix + "Cameras");

        return allThumbnailCameras;
    }

    // get the correct screenshot file name when taken from in-game
    public static string GetInGameScreenshotFileName()
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
        string screenshotName = dateStamp + delimeter + timeStamp + delimeter + sceneName;

        return screenshotName;
    }

    public static bool GetCurrentCameraOcclusionCullingState()
    {
        if (CameraActionGlobals.activeCameraHost == null)
        {
            return false;
        }

        Camera currentCamera = CameraActionGlobals.activeCameraHost.GetComponent<Camera>();

        if (currentCamera.useOcclusionCulling)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void ToggleCurrentCameraOcclusionCullingState()
    {
        Camera currentCamera = CameraActionGlobals.activeCameraHost.GetComponent<Camera>();

        if (currentCamera.useOcclusionCulling)
        {
            currentCamera.useOcclusionCulling = false;
        }
        else
        {
            currentCamera.useOcclusionCulling = true;
        }
    }

    // copies all scene screenshots from the computer back to resources, replacing the previous
    public static void ReplaceSceneThumbnailsInResources()
    {
#if UNITY_EDITOR
        GameObject[] thumbnailCameras = GetAllThumbnailCamerasInScene();

        // first, delete all existing thumbnail images in the project UI path
        // in case some thumbnails were renamed, we don't want zombies left over in this path
        string[] extensionsToDelete = { "png", "meta" };
        string[] filesToDelete = FileDirUtils.GetAllFilesInDirOfTypes(UIGlobals.projectUIPath + UIGlobals.mainMenuThumbnailsSubdir, extensionsToDelete);

        foreach (string fileToDelete in filesToDelete)
        {
            // only delete screenshots from the current scene
            if (fileToDelete.Contains(SceneManager.GetActiveScene().name))
            {
                UnityEngine.Windows.File.Delete(fileToDelete);
            }
        }

        // for each thumbnail camera, copy the corresponding screenshot
        // from the game path to the resources path
        foreach (GameObject thumbnailCamera in thumbnailCameras)
        {
            // determine the temporary and final (resources) file path based on this camera's name
            string temporaryFilePath = CameraActionGlobals.inGameScreenshotsPath + thumbnailCamera.name + "-" + SceneManager.GetActiveScene().name + CameraActionGlobals.screenshotFormat;
            string resourcesFilePath = UIGlobals.projectUIPath + UIGlobals.mainMenuThumbnailsSubdir + thumbnailCamera.name + "-" + SceneManager.GetActiveScene().name + CameraActionGlobals.screenshotFormat;

            Utils.DebugUtils.DebugLog("<b>Replacing</b> " + temporaryFilePath + " <b>with</b> " + resourcesFilePath);

            FileUtil.ReplaceFile(temporaryFilePath, resourcesFilePath);
        }
#endif
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
