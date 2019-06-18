using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// thanks to LightingBox2
// https://forum.unity.com/threads/access-lighting-window-properties-in-script.328342/
// modified for use in CCP

public static class LightingSettingsHepler
{
    // other properties - use the ones you need
    /*
    SerializedProperty property1 = so.FindProperty("m_LightmapEditorSettings.m_Resolution");
    SerializedProperty property2 = so.FindProperty("m_LightmapEditorSettings.m_BakeResolution");
    SerializedProperty property3 = so.FindProperty("m_LightmapEditorSettings.m_Padding");
    SerializedProperty property4 = so.FindProperty("m_LightmapEditorSettings.m_CompAOExponent");
    SerializedProperty property5 = so.FindProperty("m_LightmapEditorSettings.m_AOMaxDistance");
    SerializedProperty property6 = so.FindProperty("m_LightmapEditorSettings.m_TextureCompression");
    SerializedProperty property7 = so.FindProperty("m_LightmapEditorSettings.m_FinalGather");
    SerializedProperty property8 = so.FindProperty("m_LightmapEditorSettings.m_FinalGatherRayCount");

    SerializedProperty property1 = so.FindProperty("m_GISettings.m_AlbedoBoost");
    SerializedProperty property2 = so.FindProperty("m_GISettings.m_IndirectOutputScale");
    SerializedProperty property3 = so.FindProperty("m_LightmapEditorSettings.m_TextureWidth");
    SerializedProperty property4 = so.FindProperty("m_LightmapEditorSettings.m_LightmapParameters");
    SerializedProperty property5 = so.FindProperty("m_LightmapsMode");

    SerializedProperty property1 = so.FindProperty("m_GISettings.m_BounceScale");
    SerializedProperty property2 = so.FindProperty("m_GISettings.m_TemporalCoherenceThreshold");

    SerializedProperty property1 = so.FindProperty("m_GISettings.m_EnableRealtimeLightmaps");
    SerializedProperty property2 = so.FindProperty("m_GISettings.m_EnableBakedLightmaps");
    */

    public static void SetBounceIntensity(float val)
    {
        SetFloat("m_GISettings.m_IndirectOutputScale", val);
    }

    public static void SetIndirectSamples(int val)
    {
        SetInt("m_LightmapEditorSettings.m_PVRSampleCount", val);
    }

    public static void SetDirectSamples(int val)
    {
        SetInt("m_LightmapEditorSettings.m_PVRDirectSampleCount", val);
    }

    public static void SetIndirectResolution(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_Resolution", val);
    }

    public static void SetAmbientOcclusion(bool val)
    {
        SetBool("m_LightmapEditorSettings.m_AO", val);
    }

    public static void SetAmbientOcclusionDirect(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_CompAOExponentDirect", val);
    }

    public static void SetAmbientOcclusionIndirect(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_CompAOExponent", val);
    }

    public static void SetAmbientOcclusionDistance(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_AOMaxDistance", val);
    }

    public static void SetBakedGiEnabled(bool enabled)
    {
        SetBool("m_GISettings.m_EnableBakedLightmaps", enabled);
    }

    public static void SetFinalGatherEnabled(bool enabled)
    {
        SetBool("m_LightmapEditorSettings.m_FinalGather", enabled);
    }

    public static void SetFinalGatherRayCount(int val)
    {
        SetInt("m_LightmapEditorSettings.m_FinalGatherRayCount", val);
    }

    public static void SetFloat(string name, float val)
    {
        ChangeProperty(name, property => property.floatValue = val);
    }

    public static void SetInt(string name, int val)
    {
        ChangeProperty(name, property => property.intValue = val);
    }

    public static void SetBool(string name, bool val)
    {
        ChangeProperty(name, property => property.boolValue = val);
    }

    public static void SetDirectionalMode(LightmapsMode mode)
    {
        SetInt("m_LightmapEditorSettings.m_LightmapsBakeMode", (int)mode);
    }

    public static void ChangeProperty(string name, Action<SerializedProperty> changer)
    {
        var lightmapSettings = getLighmapSettings();
        var prop = lightmapSettings.FindProperty(name);
        if (prop != null)
        {
            changer(prop);
            lightmapSettings.ApplyModifiedProperties();
        }
        else Debug.Log("lighmap property not found: " + name);
    }

    static SerializedObject getLighmapSettings()
    {
        var getLightmapSettingsMethod = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
        var lightmapSettings = getLightmapSettingsMethod.Invoke(null, null) as Object;
        return new SerializedObject(lightmapSettings);
    }

}