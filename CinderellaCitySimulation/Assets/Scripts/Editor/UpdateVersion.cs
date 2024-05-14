using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Ensures the version field in Project Settings -> Version is always updated to today's date
/// </summary>

[InitializeOnLoad]
public class VersionUpdater
{
    static VersionUpdater()
    {
        string[] versionParts = PlayerSettings.bundleVersion.Split('.');
        string today = DateTime.Now.ToString("yyyyMMdd");

        // Only update the version if the date part doesn't match today's date
        if (versionParts[3] != today)
        {
            string newVersion = string.Format("{0}.{1}.{2}.{3}", versionParts[0], versionParts[1], versionParts[2], today);
            PlayerSettings.bundleVersion = newVersion;
            Debug.Log("Updated version to: " + newVersion);
        }
    }
}
