using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class GlobalSceneVariables
{
    // when a scene change is requested, record the referring scene globally so we can switch back to it
    public static string referringScene;

    // list the friendly names of available time periods, in chronologial order (used for UI labels)
    // TODO: add Alt Future when available
    public static string[] availableTimePeriodFriendlyNames = { "1960s-70s", "1980s-90s" };

    // convert the friendly names to Scene names
    // uses CleanString to remove spaces, punctuation, and in the case of years, "19"
    // for example: "1980s-1990s" becomes "80s90s"
    // IMPORTANT: Unity Scene names must match the strings in this list exactly
    public static List<string> availableTimePeriodSceneNames = StringUtils.ConvertFriendlyNamesToSceneNames(new List<string>(availableTimePeriodFriendlyNames));
}

