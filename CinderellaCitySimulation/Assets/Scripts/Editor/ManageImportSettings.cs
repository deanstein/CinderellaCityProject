using UnityEditor;

/// <summary>
/// Contains import settings for various importable files, used by AssetImportPipeline to apply certain settings to certain files
/// </summary>

// all import params
public class ModelImportParams
{
    // pre-processor option flags
    public bool doSetGlobalScale = false;
    public bool doInstantiateAndPlaceInCurrentScene = false;
    public bool doSetColliderActive = false;
    public bool doSetUVActiveAndConfigure = false;
    public bool doDeleteReimportMaterialsTextures = false;
    public bool doAddBehaviorComponents = false;

    // post-processor option flags
    public bool doSetMaterialEmission = false;
    public bool doSetMaterialSmoothnessMetallic = false;
    public bool doInstantiateProxyReplacements = false;
    public bool doHideProxyObjects = false;

    // these import params are deprecated, for now,
    // because these operations don't fire reliably in the post-processor
    // so they are manually executed from the CCP Menu
    // leaving here for now in case they can be added back to the import process in the future
    public bool doSetStaticFlags = false;
    public bool doRebuildNavMesh = false;
    public bool doSetCustomLightmapSettings = false;
}

// return import settings based on the asset name
public class ManageImportSettings
{
    public static ModelImportParams GetModelImportParamsByName(string name)
    {
        ModelImportParams ImportParams = new ModelImportParams();

        switch (name)
        {
            /// typical building or site elements
            /// will receive all material and collider treatments
            
            case string assetOrModelName when 
            // main mall and anchors
            assetOrModelName.Contains("ceilings")
            || assetOrModelName.Contains("detailing-interior")
            || assetOrModelName.Contains("floors-vert")
            || assetOrModelName.Contains("light-shrouds")
            || assetOrModelName.Contains("roof")
            || assetOrModelName.Contains("structure")
            || assetOrModelName.Contains("walls-detailing-exterior")
            || assetOrModelName.Contains("walls-interior")
            // stores
            || assetOrModelName.Contains("anchor-broadway")
            || assetOrModelName.Contains("store-ceilings")
            || assetOrModelName.Contains("store-detailing")
            || assetOrModelName.Contains("store-floors")
            // site
            || assetOrModelName.Contains("site-context-buildings")
            || assetOrModelName.Contains("site-curb-gutter-sidewalk-vert")
            || assetOrModelName.Contains("site-detailing")
            || assetOrModelName.Contains("site-parking-surface")
            || assetOrModelName.Contains("site-roads"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// special-cased building elements

            case string assetOrModelName when assetOrModelName.Contains("doors-exterior")
            || assetOrModelName.Contains("doors-windows-interior")
            || assetOrModelName.Contains("vents")
            || assetOrModelName.Contains("windows-exterior"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("doors-windows-solid"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
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
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
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
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("handrails"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("lights")
            || assetOrModelName.Contains("signage")
            || assetOrModelName.Contains("wayfinding"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// proxy objects which either get replaced or augmented on import

            case string assetOrModelName when assetOrModelName.Contains("proxy-cameras"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
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
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-trees-veg"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-water"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-blocker-npc"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("speakers") 
            || assetOrModelName.Contains("speakers-simple"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// temporary files or experimental models for testing

            case string assetOrModelName when assetOrModelName.Contains("temp-fix"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
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
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// if the name wasn't mentioned here, we do nothing
            /// files need to be specified above to have any special rules applied

            default:
                // pre-processor option flags
                ImportParams.doSetGlobalScale = false;
                ImportParams.doInstantiateAndPlaceInCurrentScene = false;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;
        }
    }

    // get the appropriate static editor flags given an asset name
    public static StaticEditorFlags GetStaticFlagsByName(string assetName)
    {
        switch (assetName)
        {
            // no static editor flags
            case string name when (name.Contains("light-shrouds")
            || name.Contains("mall-vents")
            || name.Contains("structure-concealed")
            || name.Contains("speakers")
            || name.Contains("trees")
            || name.Contains("water")):
                return 0;
            // only navigation static
            case string name when (name.Contains("handrails")
            || name.Contains("furniture")
            || name.Contains("wayfinding")
            || name.Contains("proxy-blocker-npc")) 
            && !name.Contains("solid"):
                return StaticEditorFlags.NavigationStatic;
            // lightmap static only
            case string name when (name.Contains("doors-exterior")):
                return StaticEditorFlags.LightmapStatic;
            // if not specified, default to all static editor flags
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
            case string name when name.Contains("lights")
            || name.Contains("signage"):
                return 10f;
            case string name when name.Contains("detailing-interior")
            || name.Contains("walls-detailing-exterior"):
                return 5f;
            case string name when name.Contains("detailing-interior")
            || name.Contains("mall-ceilings")
            || name.Contains("store-detailing"):
                return 4f;
            case string name when name.Contains("floors-vert")
            || name.Contains("site-curb-gutter-sidewalk-vert")
            || name.Contains("site-detailing")
            || name.Contains("site-structure")
            || name.Contains("structure-exposed"):
                return 3f;
            case string name when name.Contains("doors")
            || name.Contains("windows"):
                return 2f;
            case string name when name.Contains("walls-interior")
            || name.Contains("store-ceilings")
            || name.Contains("store-floors")
            || name.Contains("site-parking-surface"):
                return 0.5f;
            case string name when name.Contains("roof") 
            || name.Contains("site-context-buildings")
            || name.Contains("site-roads"):
                return 0.1f;
            case string name when name.Contains("experimental-simple"):
                return 22f;
            // if not specified, the default is 1 (no change to global resolution for this asset)
            default:
                return 1f;
        }
    }
}