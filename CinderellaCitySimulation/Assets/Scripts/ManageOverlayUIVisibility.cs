using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides access and management of screen space objects (UI)
/// </summary>

public static class UIVisibilityGlobals
{
    // each scene has a layer of HUD that can be disabled when required
    // HUD is active by default
    public static bool isHUDActive = true;
    public static GameObject activeHUD = null;

    // a UI launcher can only ever have one overlay menu active, so keep track of it
    public static bool isOverlayMenuActive = false;
    public static string activeOverlayMenuName = null;
}

public class ManageOverlayVisibility
{
    public static void RestoreLastKnownOverlayMenu(GameObject UILauncher)
    {
        if (UIVisibilityGlobals.activeOverlayMenuName != null)
        {
            ToggleSceneAndUI.ToggleOverlayMenu(UILauncher, UIVisibilityGlobals.activeOverlayMenuName);
        }
    }
}

public class ManageHUDVisibility
{
    public static void ToggleHUDCanvas()
    {
        UIVisibilityGlobals.isHUDActive = ToggleSceneAndUI.ToggleSceneObject(UIVisibilityGlobals.activeHUD);
    }
}