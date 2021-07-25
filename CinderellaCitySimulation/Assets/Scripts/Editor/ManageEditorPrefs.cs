using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines the syntax of EditorPrefs (stored in the registry) as well as protocols for writing new EditorPrefs
/// </summary>

public static class ManageEditorPrefs
{
    // collect all editor pref globals so we know what's being saved 
    // to the computer registry where the editor is being run
    public class EditorPrefsGlobals
    {
        // prefix for EditorPrefs - typically the name of the project
        public static string editorPrefsPrefix = "CCP.";

        // screenshot mode flag (for reference only, this must be defined where in-game MonoBehaviours can accses)
        public static string screenshotModeFlag = ManageCameraActions.CameraActionGlobals.screenshotModeFlag;

        // iniital scene container position editor pref key
        // defines the original Z-height of the scene container for editor-only operations
        public static string originalContainerYPosKeyID = "SceneContainerOriginalYPos";
    }

    // generate a key name starting with the scene name
    public static string GetEditorPrefKeyBySceneName(string sceneName, string keyDataName)
    {
        string EditorPrefKey = EditorPrefsGlobals.editorPrefsPrefix + sceneName + keyDataName;

        return EditorPrefKey;
    }

    // write the position of this scene container to editor prefs
    // if it doesn't already exist at the right value
    public static void SetCurrentSceneContainerYPosEditorPref()
    {
        // only do something if the current scene name matches one of the available time period scene names
        if (Utils.StringUtils.TestIfAnyListItemContainedInString(SceneGlobals.availableTimePeriodSceneNamesList, SceneManager.GetActiveScene().name))
        {
            // get the current scene container
            GameObject currentSceneContainer = ManageScenes.GetSceneContainerObject(SceneManager.GetActiveScene());

            // get its height
            float currentSceneContainerYPos = currentSceneContainer.transform.position.y;

            // get the editor pref key to use for this scene
            string currentSceneContainerYPosKey = GetEditorPrefKeyBySceneName(SceneManager.GetActiveScene().name, EditorPrefsGlobals.originalContainerYPosKeyID);

            // set the editor pref using the key and position
            // but only if the number hasn't been set or needs to be updated
            if (EditorPrefs.GetFloat(currentSceneContainerYPosKey, -1) == -1)
            {
                EditorPrefs.SetFloat(currentSceneContainerYPosKey, currentSceneContainerYPos);

                Utils.DebugUtils.DebugLog("This scene's container YPos was not recorded previously, so the current YPos was recorded: " + currentSceneContainer.name);
            }
            else if (!Mathf.Approximately(EditorPrefs.GetFloat(currentSceneContainerYPosKey, -1), currentSceneContainerYPos))
            {
                EditorPrefs.SetFloat(currentSceneContainerYPosKey, currentSceneContainerYPos);

                Utils.DebugUtils.DebugLog("This scene's container YPos was already recorded, but didn't match the current YPos, so it's been overwritten: " + currentSceneContainer.name);
            }
            else
            {
                Utils.DebugUtils.DebugLog("This scene's container YPos was already recorded and up to date in EditorPrefs: " + currentSceneContainer.name);
            }
        }
    }

}

