using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides access and management of screen space objects (UI)
/// </summary>

public static class OverlayUIVisibilityGlobals
{
    // a UI launcher can only ever have one overlay menu active, so keep track of it
    public static bool isOverlayMenuActive = false;
    public static string activeOverlayMenuName = null;
}

public class ManageOverlayVisibility
{
    public static void RestoreLastKnownOverlayMenu(GameObject UILauncher)
    {
        if (OverlayUIVisibilityGlobals.activeOverlayMenuName != null)
        {
            ToggleSceneAndUI.ToggleOverlayMenu(UILauncher, OverlayUIVisibilityGlobals.activeOverlayMenuName);
        }
    }
}