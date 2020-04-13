using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGlobals
{
    // when a scene change is requested, record the outgoing and upcoming scenes for other scripts to access
    public static string referringScene;
    public static string upcomingScene;

    // list the friendly names of available time periods, in chronologial order (used for UI labels)
    // TODO: add Alt Future when available
    public static string[] availableTimePeriodFriendlyNames = { "1960s-70s", "1980s-90s" };

    // convert the friendly names to Scene names
    // uses CleanString to remove spaces, punctuation, and in the case of years, "19"
    // for example: "1980s-1990s" becomes "80s90s"
    // IMPORTANT: Unity Scene names must match the strings in this list exactly
    public static List<string> availableTimePeriodSceneNames = StringUtils.ConvertFriendlyNamesToSceneNames(new List<string>(availableTimePeriodFriendlyNames));

    // based on the current scene, these are the time period scenes that are disabled
    // used to generate thumbnails for disabled scenes for the Pause Menu
    public static List<string> disabledTimePeriodSceneNames = new List<string>();
}

public static class ManageAvailableScenes
{
    // gets the other (disabled) time period scene names, given the current scene name
    public static List<string> GetDisabledTimePeriodSceneNames(string currentSceneName)
    {
        // first, clear the list of disabled time periods - it's stale now
        SceneGlobals.disabledTimePeriodSceneNames.Clear();

        // iterate through the list and add any scenes that don't match the current scene
        foreach (string timePeriodScenName in SceneGlobals.availableTimePeriodSceneNames)
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
        int currentTimePeriodSceneIndex = SceneGlobals.availableTimePeriodSceneNames.IndexOf(SceneManager.GetActiveScene().name);

        // if the current scene doesn't match any in the list, return with an error
        if (currentTimePeriodSceneIndex == -1)
        {
            Debug.Log("Error: failed to determine the next time period, because the current time period is unknown.");
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
                    newTimePeriodSceneIndex = (SceneGlobals.availableTimePeriodSceneNames.Count - 1);
                }
                else
                {
                    return "null";
                }
            }

            // get the equivalent scene name from the available time periods
            var prevTimePeriodSceneName = SceneGlobals.availableTimePeriodSceneNames[newTimePeriodSceneIndex];
            return prevTimePeriodSceneName;
        }

        // if the next time period is requested
        if (previousOrNext.Contains("next"))
        {
            // increment the time period index
            int newTimePeriodSceneIndex = currentTimePeriodSceneIndex + 1;

            // if we're outside the length of the list, either go to the beginning of the index or do nothing
            if (newTimePeriodSceneIndex > (SceneGlobals.availableTimePeriodSceneNames.Count - 1))
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
            string nextTimePeriodSceneName = SceneGlobals.availableTimePeriodSceneNames[newTimePeriodSceneIndex];
            return nextTimePeriodSceneName;
        }

        else
        {
            return "null";
        }
    }
}

