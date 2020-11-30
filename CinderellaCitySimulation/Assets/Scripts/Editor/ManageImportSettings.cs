using UnityEditor;

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
            case string assetOrModelName when assetOrModelName.Contains("anchor-broadway-detailing-exterior")
            || assetOrModelName.Contains("anchor-jcp-detailing-exterior")
            || assetOrModelName.Contains("anchor-joslins-detailing-exterior")
            || assetOrModelName.Contains("anchor-mgwards-detailing-exterior")
            || assetOrModelName.Contains("anchor-denver-detailing-exterior")

            || assetOrModelName.Contains("mall-ceilings")
            || assetOrModelName.Contains("mall-detailing-interior")
            || assetOrModelName.Contains("mall-floors-vert")
            || assetOrModelName.Contains("mall-roof")
            || assetOrModelName.Contains("mall-walls-detailing-exterior")
            || assetOrModelName.Contains("mall-walls-interior")
            || assetOrModelName.Contains("store-floors")


            || assetOrModelName.Contains("site-context-buildings")
            || assetOrModelName.Contains("site-curb-gutter-sidewalk-vert")
            || assetOrModelName.Contains("site-detailing")
            || assetOrModelName.Contains("site-parking-surface")
            || assetOrModelName.Contains("site-roads")
            || assetOrModelName.Contains("site-structure")

            || assetOrModelName.Contains("store-ceilings")
            || assetOrModelName.Contains("store-detailing"):
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
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
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

            case string assetOrModelName when assetOrModelName.Contains("mall-water"):
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

            case string assetOrModelName when assetOrModelName.Contains("mall-handrails")
            || assetOrModelName.Contains("site-handrails"):
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

            case string assetOrModelName when assetOrModelName.Contains("mall-structure"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
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

            case string assetOrModelName when assetOrModelName.Contains("proxy-trees"):
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
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
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
            case string name when (name.Contains("doors-exterior")
            || name.Contains("speakers")):
                return 0;
            // only navigation static
            case string name when (name.Contains("windows") 
            || name.Contains("handrails") 
            || name.Contains("wayfinding")
            || name.Contains("proxy-blocker-npc")) 
            && !name.Contains("solid"):
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
            case string name when name.Contains("mall-detailing-interior") 
            || name.Contains("mall-ceilings")
            || name.Contains("mall-walls-detailing-exterior")
            || name.Contains("store-detailing"):
                return 3f;
            case string name when name.Contains("lights") 
            || name.Contains("signage"):
                return 10f;
            case string name when name.Contains("mall-floors-vert")
            || name.Contains("site-curb-gutter-sidewalk-vert"):
                return 2.0f;
            case string name when name.Contains("mall-roof") 
            || name.Contains("site-context-buildings")
            || name.Contains("structure")
            || name.Contains("site-roads")
            || name.Contains("site-parking-surface"):
                return 0.1f;
            case string name when name.Contains("mall-walls-interior") 
            || name.Contains("store-ceilings")
            || name.Contains("store-floors"):
                return 0.1f;
            case string name when name.Contains("experimental-simple"):
                return 22f;
            // if not specified, the default is 1 (no change to global resolution for this asset)
            default:
                return 1f;
        }
    }
}