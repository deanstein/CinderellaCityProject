using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Opens, loads, and manages lists of Scenes 
/// including any active Scenes, inactive Scenes, 
/// and the order of "time-traveling" scene sequence 
/// </summary>

public class SceneGlobals
{
    /* individual scenes */
    // experimental scene - used for testing
    public static string experimentalSceneName = "Experimental";
    // the name of the scene that should be displayed while other scenes are loading
    public static string loadingSceneName = "LoadingScreen";
    // menu scene names
    public static string mainMenuSceneName = "MainMenu";
    public static string pauseMenuName = "PauseMenu";
    public static string visibilityMenuSceneName = "VisibilityMenu";
    // static screen scene names
    public static string howToPlaySceneName = "HowToPlayScreen";
    public static string creditsSceneName = "CreditsScreen";
    // mall scene names
    public static string mallEra60s70sSceneName = "60s70s";
    public static string mallEra80s90sSceneName = "80s90s";

    // scene to jump to if autoStart is true in startupConfig
    public static string autoStartSceneName = mallEra60s70sSceneName;
    // scene to set active after all other scenes are loaded
    public static string startingSceneName = StartupGlobals.startupConfig.autoStart ? autoStartSceneName : mainMenuSceneName;
    // when a scene change is requested, record the outgoing and upcoming scenes for other scripts to access
    public static string referringSceneName;
    public static string upcomingSceneName;
    // the pause menu will record the last known time period scene name when Pause was invoked
    public static string lastKnownTimePeriodSceneName;
    // determines what the last era was doing when a new era was requested
    public static bool isGuidedTourTimeTraveling;

    // each time period is represented by a scene name that must match an existing scene,
    // and a label that appears on screen in the HUD when this scene is active
    public class TimePeriod
    {
        public string sceneName;
        public string label;
    }
    // define properties of all available time periods
    // TODO: add Alt Future when available
    public class TimePeriods
    {
        public static List<TimePeriod> timePeriods = new List<TimePeriod>
        {
            new TimePeriod
            {
                sceneName = "60s70s",
                label = "1968-1978 "
            },
            new TimePeriod
            {
                sceneName = "80s90s",
                label = "1987-1997 "
            }
        };

        // used for testing only, and not included in the real list of time periods
        public static TimePeriod exterimentalTimePeriod = new TimePeriod
        {
            sceneName = "Experimental",
            label = "1987-1997"
        };

        public static string[] GetAllTimePeriodSceneNames()
        {
            return timePeriods.Select(tp => tp.sceneName).ToArray();
        }

        public static string[] GetAllTimePeriodLabels()
        {
            return timePeriods.Select(tp => tp.label).ToArray();
        }

        public static string GetTimePeriodLabelBySceneName(string sceneName)
        {
            var timePeriod = timePeriods.FirstOrDefault(tp => String.Equals(tp.sceneName, sceneName, StringComparison.OrdinalIgnoreCase));
            return timePeriod != null ? timePeriod.label : exterimentalTimePeriod.label;
        }

    }
    TimePeriods timePeriods = new TimePeriods();

    // all scene names to load for the full simulation experience
    public static string[] allGameplaySceneNames = new string[] { mainMenuSceneName, pauseMenuName, howToPlaySceneName, creditsSceneName }.Concat(TimePeriods.GetAllTimePeriodSceneNames()).ToArray();

    // only the menu scenes - used for testing menus without overhead of loading first-person scenes
    public static string[] allMenuSceneNames = { mainMenuSceneName, pauseMenuName, howToPlaySceneName, creditsSceneName };
    // only the first-person (non-menu) scenes
    public static string[] availableTimePeriodSceneNames = TimePeriods.GetAllTimePeriodSceneNames();
    // list the friendly names of available time periods, in chronologial order (used for UI labels)
    public static string[] availableTimePeriodLabels = TimePeriods.GetAllTimePeriodLabels();
    // for testing purposes, set this to true to skip loading heavy first-person scenes
    public static bool loadMenusOnly = false;
    // the scenes to load when the game starts will differ depending on the above flag
    public static string[] scenesToLoad = loadMenusOnly ? allMenuSceneNames : allGameplaySceneNames;
    // IMPORTANT: Unity Scene names must match the strings in this list exactly
    public static List<string> availableTimePeriodSceneNamesList = new List<string>(availableTimePeriodSceneNames);
    // based on the current scene, these are the time period scenes that are disabled
    // used to generate thumbnails for disabled scenes for the Pause Menu
    public static List<string> disabledTimePeriodSceneNames = new List<string>();

    // construct a path to a scene from its name
    public static string GetScenePathByName(string sceneName)
    {
        string scenePath = "Assets/Scenes/" + sceneName + ".unity";
        return scenePath;
    }
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

            DebugUtils.DebugLog("Started loading scene asynchronously: " + sceneName);
        }
    }

    // determines if the current scene is an FPS/time period scene
    public static bool GetIsActiveSceneTimePeriodScene()
    {
        return Utils.StringUtils.TestIfAnyListItemContainedInString(new List<string>(SceneGlobals.availableTimePeriodSceneNames), SceneManager.GetActiveScene().name) || SceneManager.GetActiveScene().name == SceneGlobals.experimentalSceneName;
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
            DebugUtils.DebugLog("Error: failed to determine the next time period, because the current time period is unknown.");
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

