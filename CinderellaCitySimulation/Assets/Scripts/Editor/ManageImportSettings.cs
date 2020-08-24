using UnityEditor;

// return import settings based on the asset name
public class ManageImportSettings
{
    // get the appropriate static editor flags given an asset name
    public static StaticEditorFlags GetStaticFlagsByName(string assetName)
    {
        switch (assetName)
        {
            // only navigation static
            case string name when (name.Contains("windows") || name.Contains("handrails") || name.Contains("wayfinding")) && !name.Contains("solid"):
                return StaticEditorFlags.NavigationStatic;
            // if not specified here, the default is to get all static editor flags
            default:
                return StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OffMeshLinkGeneration | StaticEditorFlags.ReflectionProbeStatic;
        }
    }

    // get the correct shadow map resolution multiplier given an asset name
    // this is a multiplier of the current global Lightmap Resolution value
    public static float GetShadowMapResolutionMultiplierByName(string assetName)
    {
        switch (assetName)
        {
            case string name when name.Contains("furniture"):
                return 5f;
            case string name when name.Contains("lights") || name.Contains("signage"):
                return 10f;
            case string name when name.Contains("site"):
                return 0.1f;
            case string name when name.Contains("experimental-simple"):
                return 22f;
            // if not specified, the default is 1 (no change to global resolution for this asset)
            default:
                return 1f;
        }
    }
}