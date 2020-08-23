using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SceneGlobals
{
    // when a scene change is requested, record the outgoing and upcoming scenes for other scripts to access
    public static string referringScene;
    public static string upcomingScene;

    // construct a path to a scene from its name
    public static string GetScenePathByName(string sceneName)
    {
        string scenePath = "Assets/Scenes/" + sceneName + ".unity";
        return scenePath;
    }

    // all scene names to load for the full simulation experience
    public static string[] allGameplaySceneNames = { "MainMenu", "PauseMenu", "60s70s", "80s90s" };

    // only the first-person (non-menu) scenes
    public static string[] availableTimePeriodSceneNames = { "60s70s", "80s90s" };

    // list the friendly names of available time periods, in chronologial order (used for UI labels)
    // TODO: add Alt Future when available
    public static string[] availableTimePeriodFriendlyNames = { "1960s-70s", "1980s-90s" };

    // the name of the scene that should be displayed while other scenes are loading
    public static string loadingSceneName = "LoadingScreen";

    // the name of the scene to set active after all other scenes are loaded
    public static string startingSceneName = "MainMenu";

    // convert the friendly names to Scene names
    // uses CleanString to remove spaces, punctuation, and in the case of years, "19"
    // for example: "1980s-1990s" becomes "80s90s"
    // IMPORTANT: Unity Scene names must match the strings in this list exactly
    public static List<string> availableTimePeriodSceneNamesList = StringUtils.ConvertFriendlyNamesToSceneNames(new List<string>(availableTimePeriodFriendlyNames));

    // based on the current scene, these are the time period scenes that are disabled
    // used to generate thumbnails for disabled scenes for the Pause Menu
    public static List<string> disabledTimePeriodSceneNames = new List<string>();
}

public static class ManageScenes
{
    // load a series of scenes asyncrhonously while in play mode
    public static void LoadScenesAsync(string[] sceneNames, List<AsyncOperation> asyncOperations)
    {
        // start loading each scene asynchronously
        foreach (string sceneName in sceneNames)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            // collect the async operations into a list
            asyncOperations.Add(asyncOperation);

            Utils.DebugUtils.DebugLog("Started loading scene asynchronously: " + sceneName);
        }
    }

    // load the given scenes additively in the Unity Editor, starting from a given loading scene
    // closes the current scene if the current scene is not the specified loading scene
    public static void LoadEditorScenesAdditively(string loadingScene, string[] sceneNames)
    {
        // ensure the loadingScene is opened first
        if (SceneManager.GetActiveScene().name != loadingScene)
        {
            EditorSceneManager.OpenScene(loadingScene);
            Utils.DebugUtils.DebugLog("Opening the loading scene first...");
        }

        // load each specified scene additively
        for (var i = 0; i < sceneNames.Length; i++)
        {
            // convert the scene name to a path
            string scenePath = SceneGlobals.GetScenePathByName(sceneNames[i]);

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            Utils.DebugUtils.DebugLog("Additively opening scene in editor: " + scenePath);
        }
    }

    // gets the disabled time period scene names, given the current scene name
    public static List<string> GetDisabledTimePeriodSceneNames(string currentSceneName)
    {
        // first, clear the list of disabled time periods - it's stale now
        SceneGlobals.disabledTimePeriodSceneNames.Clear();

        // iterate through the list and add any scenes that don't match the current scene
        foreach (string timePeriodScenName in SceneGlobals.availableTimePeriodSceneNamesList)
        {
            if (timePeriodScenName != currentSceneName)
            {
                SceneGlobals.disabledTimePeriodSceneNames.Add(timePeriodScenName);
            }
        }

        return SceneGlobals.disabledTimePeriodSceneNames;
    }

    // gets the next or previous time period scene name to switch to while in-game
    public static string GetNextTimePeriodSceneName(string previousOrNext)
    {
        // allow defaulting to the other end of index once at index bounds
        bool allowLooping = true;

        // first, figure out which time period index the current scene is
        int currentTimePeriodSceneIndex = SceneGlobals.availableTimePeriodSceneNamesList.IndexOf(SceneManager.GetActiveScene().name);

        // if the current scene doesn't match any in the list, return with an error
        if (currentTimePeriodSceneIndex == -1)
        {
            Utils.DebugUtils.DebugLog("Error: failed to determine the next time period, because the current time period is unknown.");
            return "null";
        }

        // if the next time period is requested
        if (previousOrNext.Contains("previous"))
        {
            // increment the time period index
            int newTimePeriodSceneIndex = currentTimePeriodSceneIndex - 1;

            // if we're outside the length of the list, either go to the end of the index or do nothing
            if (newTimePeriodSceneIndex < 0)
            {
                if (allowLooping)
                {
                    newTimePeriodSceneIndex = (SceneGlobals.availableTimePeriodSceneNamesList.Count - 1);
                }
                else
                {
                    return "null";
                }
            }

            // get the equivalent scene name from the available time periods
            var prevTimePeriodSceneName = SceneGlobals.availableTimePeriodSceneNamesList[newTimePeriodSceneIndex];
            return prevTimePeriodSceneName;
        }

        // if the next time period is requested
        if (previousOrNext.Contains("next"))
        {
            // increment the time period index
            int newTimePeriodSceneIndex = currentTimePeriodSceneIndex + 1;

            // if we're outside the length of the list, either go to the beginning of the index or do nothing
            if (newTimePeriodSceneIndex > (SceneGlobals.availableTimePeriodSceneNamesList.Count - 1))
            {
                if (allowLooping)
                {
                    newTimePeriodSceneIndex = 0;
                }
                else
                {
                    return "null";
                }
            }

            // get the equivalent scene name from the available time periods
            string nextTimePeriodSceneName = SceneGlobals.availableTimePeriodSceneNamesList[newTimePeriodSceneIndex];
            return nextTimePeriodSceneName;
        }

        else
        {
            return "null";
        }
    }
}

