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
    public static GameObject activeOverlayMenu = null;
    public static string lastKnownOverlayMenuName = null;
}

public class ManageOverlayVisibility
{
    public static GameObject RestoreLastKnownOverlayMenuByName(GameObject UILauncher, string overlayMenuName)
    {
        // only attempt to restore if the name is valid, but the object is not
        if (UIVisibilityGlobals.activeOverlayMenu == null && UIVisibilityGlobals.lastKnownOverlayMenuName != null)
        {
            switch (overlayMenuName)
            {
                case string name when name.Contains(SceneGlobals.visibilityMenuName):
                    return CreateScreenSpaceUILayoutByName.BuildVisualizationMenuOverlay(UILauncher);
                default:
                    return null;
            }
        }

        return null;
    }

    public static void DismissActiveOverlayMenu()
    {
        Object.Destroy(UIVisibilityGlobals.activeOverlayMenu);
        UIVisibilityGlobals.isOverlayMenuActive = false;
        UIVisibilityGlobals.lastKnownOverlayMenuName = null;

        ManageFPSControllers.EnableCursorLockOnActiveFPSController();
    }
}

public class ManageHUDVisibility
{
    public static GameObject GetActiveHUDCanvas(GameObject UILauncher)
    {
        GameObject HUD = UILauncher.transform.Find("HUD").gameObject;

        UIVisibilityGlobals.isHUDActive = HUD.activeSelf;

        return HUD;
    }

    public static void ToggleHUDCanvas()
    {
        UIVisibilityGlobals.isHUDActive = ToggleSceneAndUI.ToggleSceneObject(UIVisibilityGlobals.activeHUD);
    }
}