using UnityEditor;

// all import params
public class ModelImportParams
{
    // pre-processor option flags
    public bool doSetGlobalScale = false;
    public bool doInstantiateAndPlaceInCurrentScene = false;
    public bool doSetColliderActive = false;
    public bool doSetUVActiveAndConfigure = false;
    public bool doSetCustomLightmapSettings = false;
    public bool doDeleteReimportMaterialsTextures = false;
    public bool doAddBehaviorComponents = false;

    // post-processor option flags
    public bool doSetStaticFlags = false;
    public bool doSetMaterialEmission = false;
    public bool doSetMaterialSmoothnessMetallic = false;
    public bool doInstantiateProxyReplacements = false;
    public bool doHideProxyObjects = false;
    public bool doRebuildNavMesh = false;
}

// return import settings based on the asset name
public class ManageImportSettings
{
    public static ModelImportParams GetModelImportParamsByName(string name)
    {
        ModelImportParams ImportParams = new ModelImportParams();

        switch (name)
        {
            case string assetOrModelName when assetOrModelName.Contains("anchor-broadway")
            || assetOrModelName.Contains("anchor-jcp")
            || assetOrModelName.Contains("anchor-joslins")
            || assetOrModelName.Contains("anchor-mgwards")
            || assetOrModelName.Contains("anchor-denver"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-doors-exterior")
            || assetOrModelName.Contains("mall-doors-windows-interior")
            || assetOrModelName.Contains("mall-windows-exterior"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-doors-windows-solid"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-flags"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-ceiling")
            || assetOrModelName.Contains("mall-detailing-interior")
            || assetOrModelName.Contains("mall-floor-roof-vertical")
            || assetOrModelName.Contains("mall-walls-detailing-exterior")
            || assetOrModelName.Contains("mall-walls-interior")
            || assetOrModelName.Contains("store-ceiling")
            || assetOrModelName.Contains("store-detailing"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-water"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-furniture"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-handrails"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-wayfinding"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-lights")
            || assetOrModelName.Contains("mall-signage")
            || assetOrModelName.Contains("site-lights"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-structure"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-cameras"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-people"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-trees"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;


            case string assetOrModelName when assetOrModelName.Contains("site"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("speakers") || assetOrModelName.Contains("speakers-simple"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            // these are temporary files or experimental models for testing

            case string assetOrModelName when assetOrModelName.Contains("temp-fix"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("experimental-simple"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = true;
                ImportParams.doSetCustomLightmapSettings = true;
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;

            default:
                // pre-processor option flags
                ImportParams.doSetGlobalScale = false;
                ImportParams.doInstantiateAndPlaceInCurrentScene = false;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetStaticFlags = false;
                ImportParams.doSetCustomLightmapSettings = false;
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                ImportParams.doRebuildNavMesh = false;
                return ImportParams;
        }
    }

    // get the appropriate static editor flags given an asset name
    public static StaticEditorFlags GetStaticFlagsByName(string assetName)
    {
        switch (assetName)
        {
            // no static editor flags
            case string name when (name.Contains("doors-exterior")):
                return 0;
            // only navigation static
            case string name when (name.Contains("windows") || name.Contains("handrails") || name.Contains("wayfinding")) && !name.Contains("solid"):
                return StaticEditorFlags.NavigationStatic;
            // if not specified, the default is to get all static editor flags
            default:
                return StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OffMeshLinkGeneration | StaticEditorFlags.ReflectionProbeStatic;
        }
    }

    // get the correct shadow map resolution multiplier given an asset name
    // this is a multiplier of the current global Lightmap Resolution value
    public static float GetShadowMapResolutionMultiplierByName(string assetName)
    {
        // assumes a global resolution of 3.2808 (1 texel per foot)
        switch (assetName)
        {
            case string name when name.Contains("furniture"):
                return 4f;
            case string name when name.Contains("detailing-interior") || name.Contains("store-detailing") || name.Contains("mall-ceiling"):
                return 3f;
            case string name when name.Contains("lights") || name.Contains("signage"):
                return 10f;
            case string name when name.Contains("floor-roof"):
                return 2.0f;
            case string name when name.Contains("site") || name.Contains("structure"):
                return 0.1f;
            case string name when name.Contains("walls-detailing-exterior"):
                return 0.2f;
            case string name when name.Contains("walls-interior") || name.Contains("store-ceiling"):
                return 0.1f;
            case string name when name.Contains("experimental-simple"):
                return 22f;
            // if not specified, the default is 1 (no change to global resolution for this asset)
            default:
                return 1f;
        }
    }
}