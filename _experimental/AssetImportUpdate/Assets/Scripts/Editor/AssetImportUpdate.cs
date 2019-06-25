using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Windows;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

public class AssetImportUpdate : AssetPostprocessor {

    // this script only runs when an asset is updated
    // when this happens, find out what file was updated
    // and globally store its name, path, etc. for all functions to access
    static String globalAssetFilePath;
    static String globalAssetFileNameAndExtension;
    static String globalAssetFileName;
    static String globalAssetFileDirectory;
    static String globalAssetTexturesDirectory;

    // get the current scene
    Scene currentScene = SceneManager.GetActiveScene();

    // all incoming models are scaled to this value
    static float globalScale = 1.0f;

    // in some cases, we need to stop processing from happening if it's already been done
    static float prevTime;

    // post processing should happen always, but can be optionally skipped
    static bool postProcessingRequired = true;

    // post-processing seems to repeat itself a lot, so set a max and keep track of how many times
    static int globalMaxPostProcessingHits = 2;
    static List<bool> postProcessingHits = new List<bool>();

    //
    // master list of option flags that any file just changed can receive
    // all option flags should default to false, except global scale
    //

    // pre-processor option flags
    static bool doSetGlobalScale;
    static bool doInstantiateAndPlaceInCurrentScene;
    static bool doSetColliderActive;
    static bool doSetUVActiveAndConfigure;
    static bool doDeleteReimportMaterialsTextures;

    // post-processor option flags
    static bool doSetStatic;
    static bool doSetMaterialEmission;
    static bool doInstantiateProxyReplacements;
    static bool doHideProxyObjects;

    //
    // end master list
    //

    // define how to clear the console
    public static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    // define how to set the scale of the imported model
    // should be called on all imported objects for consistency
    void SetGlobalScale(ModelImporter mi)
    {
        mi.globalScale = globalScale;
    }

    // define how to set colliders for the imported model
    void SetColliderActive(ModelImporter mi)
    {
        mi.addCollider = true;
    }

    // define how to enable UVs and configure them
    void SetUVActiveAndConfigure(ModelImporter mi)
    {
        mi.generateSecondaryUV = true;
        mi.secondaryUVHardAngle = 88;
        mi.secondaryUVPackMargin = 64;
        mi.secondaryUVAngleDistortion = 4;
        mi.secondaryUVAreaDistortion = 4;
    }

    // define how to instantiate the asset (typically an FBX file) in the scene
    void InstantiateAndPlaceAssetAsGameObject(string assetFilePath, Scene scene)
    {
        GameObject gameObjectFromAsset = (GameObject)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(GameObject));

        // if the game object is null, this is a new file... so refresh the asset database and try again
        if (!gameObjectFromAsset)
        {
            AssetDatabase.Refresh();
            gameObjectFromAsset = (GameObject)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(GameObject));
        }

        // skip instantiating if it already exists in this scene
        foreach (GameObject g in Transform.FindObjectsOfType<GameObject>())
        {
            if (g.name == gameObjectFromAsset.name)
            {
                Debug.Log("This object is already present in the model.");
                return;
            }
        }

        // otherwise, instantiate as a prefab with the name of the file
        GameObject prefab = PrefabUtility.InstantiatePrefab(gameObjectFromAsset, scene) as GameObject;
        prefab.name = gameObjectFromAsset.name;
        Debug.Log("This object was instantiated in the model hierarchy.");

    }

    // define how to delete and reimport materials and textures
    public void DeleteReimportMaterialsTextures(string assetFilePath)
    { 
        // initialize ModelImporter
        ModelImporter importer = assetImporter as ModelImporter;

        // set reimport required initially to false
        bool reimportRequired = false;

        // get the material and texture dependencies and delete them
        var prefab = AssetDatabase.LoadMainAssetAtPath(assetFilePath);
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Debug.Log("Dependency path: " + dependencyPathString);

            // if there are materials or textures detected in the path, delete them
            if (dependencyPathString.Contains(".mat") || (dependencyPathString.Contains(".jpg") || (dependencyPathString.Contains(".jpeg") || (dependencyPathString.Contains(".png")))))
            {
                UnityEngine.Windows.File.Delete(dependencyPathString);
                UnityEngine.Windows.File.Delete(dependencyPathString + ".meta");
                Debug.Log("<b>Deleting files and meta files:</b> " + dependencyPathString);
                reimportRequired = true;
                prevTime = Time.time;
            }
            
            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }

        // if materials or textures were deleted, force reimport the model
        if (reimportRequired)
        {
            Debug.Log("Reimport model triggered. Forcing asset update...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("Re-importing materials...");

        // import materials
        importer.importMaterials = true;

        // make sure materials are being stored externally
        // and name them based on incoming names
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;

        // Materials are automatically stored in a Materials folder (Unity default behavior)
        
        // Textures are automatically stored in an ".fbm" folder (Unity default behavior)
        // However, for clean organization, we want to store textures in a "Textures" folder
        // the old .FBM folder will be deleted in the post-processor

        // textures will be extracted to a "Textures" folder next to the asset
        string assetTexturesDirectory = globalAssetFileDirectory + "Textures";
        globalAssetTexturesDirectory = assetTexturesDirectory;

        // re-extract textures
        Debug.Log("Re-importing textures...");
        importer.ExtractTextures(assetTexturesDirectory);

    }

    // define how to enable emission on a material and set its color and texture to emissive
    public static void SetStandardMaterialEmission(string materialFilePath)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // enable emission
        mat.EnableKeyword("_EMISSION");

        // set the material to baked emissive
        var emissiveFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        mat.globalIlluminationFlags = emissiveFlags;

        // set emission color to the material's color
        Color color = mat.GetColor("_Color");
        mat.SetColor("_EmissionColor", color);

        // set the emission map to the material's texture
        Texture texture = mat.GetTexture("_MainTex");
        mat.SetTexture("_EmissionMap", texture);

        Debug.Log("<b>Set standard emission on Material: </b>" + mat);
    }

    // define how to enable custom emission color and intensity on a material
    public static void SetCustomMaterialEmissionIntensity(string materialFilePath, float intensity)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));
        Color color = mat.GetColor("_Color");

        // for some reason, the desired intensity value (set in the UI slider) needs to be modified slightly for proper internal consumption
        float adjustedIntensity = intensity - (0.4169F);

        // redefine the color with intensity factored in - this should result in the UI slider matching the desired value
        color *= Mathf.Pow(2.0F, adjustedIntensity);
        mat.SetColor("_EmissionColor", color);
        Debug.Log("<b>Set custom emission intensity of " + intensity + " (" + adjustedIntensity + " internally) on Material: </b>" + mat);
    }

    // define how to set a custom material emission color
    public static void SetCustomMaterialEmissionColor(string materialFilePath, float R, float G, float B)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // if any of the values are provided as RGB 255 format, remap them between 0 and 1
        if (R > 1 || G > 1 || B > 1)
        {
            R = R / 255;
            G = G / 255;
            B = B / 255;
        }

        mat.color = new Color(R, G, B);
        Debug.Log("<b>Set custom emission color on Material: </b>" + mat);
    }


    // define how to clean up an automatically-created .fbm folder
    public static void CleanUpFBMDirectory(string assetFileDirectory, string assetFileName)
    {
        if (AssetDatabase.IsValidFolder(assetFileDirectory + assetFileName + ".fbm"))
        {
            Debug.Log("<b>Deleting a leftover .FBM folder.</b>");
            //Debug.Log(assetFileDirectory + assetFileName + ".fbm");
            UnityEngine.Windows.Directory.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm");
            UnityEngine.Windows.File.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm.meta");
        }
    }

    // define how to get the max height of a gameObject
    public static float GetMaxHeightOfGameObject(GameObject gameObjectToMeasure)
    {
        // create an array of MeshChunks found within this GameObject so we can get the bounding box size
        MeshRenderer[] gameObjectMeshRendererArray = gameObjectToMeasure.GetComponentsInChildren<MeshRenderer>();

        // create a list to contain heights
        List<float> gameObjectHeightsList = new List<float>();

        // for each MeshRenderer found, get the height and add it to the list
        for (int i = 0; i < gameObjectMeshRendererArray.Length; i++)
        {
            //Debug.Log("Found a MeshChunk to get bounds info from: " + gameObjectMeshRendererArray[i]);

            Bounds bounds = gameObjectMeshRendererArray[i].bounds;
            //Debug.Log("Bounds: " + bounds);
            float height = bounds.extents.y;

            // add this height to the list of heights
            gameObjectHeightsList.Add(height);
        }

        float gameObjectMaxHeight = gameObjectHeightsList.Max();
        //Debug.Log("Max height of " + gameObjectToMeasure + ": " + gameObjectMaxHeight);
        return gameObjectMaxHeight;
    }

    // define how to scale one GameObject to match the height of another
    public static void ScaleToMatchHeight(GameObject gameObjectToScale, GameObject gameObjectToMatch)
    {
        // get the height of the object to be replaced
        float targetHeight = GetMaxHeightOfGameObject(gameObjectToMatch);
        float currentHeight = GetMaxHeightOfGameObject(gameObjectToScale);
        float scaleFactor = (targetHeight / currentHeight) * ((gameObjectToScale.transform.localScale.x + gameObjectToScale.transform.localScale.y + gameObjectToScale.transform.localScale.z) / 3);

        // scale the prefab to match the height of its replacement
        gameObjectToScale.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        Debug.Log("<b>Scaled </b>" + gameObjectToScale + " <b>to match</b> " + gameObjectToMatch + " <b>(" + scaleFactor + " scale factor)</b>");
    }

    //
    // ><><><><><><><><><><><><>
    //
    // these functions must be called in the post-processor
    // some may run multiple times in one import due to Unity post-processor behavior
    //
    // <><><><><><><><><><><><><>
    //

    // define how to get the asset's in-scene gameObject and set it to static
    static void SetAssetAsStaticGameObject(string assetName)
    {
        // get the game object from the global asset name that was changed
        GameObject gameObjectFromAsset = GameObject.Find(globalAssetFileName);
        Transform[] allChildren = gameObjectFromAsset.GetComponentsInChildren<Transform>();

        // set the GameObject itself as static, if it isn't already
        if (gameObjectFromAsset.gameObject.isStatic == false)
        {
            gameObjectFromAsset.isStatic = true;
            Debug.Log("<b>Setting GameObject to static: </b>" + gameObjectFromAsset);
        }
        else
        {
            Debug.Log("GameObject was already static: " + gameObjectFromAsset);
        }

        // also set each of the GameObject's children as static, if they aren't already
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.isStatic == false)
            {
                child.gameObject.isStatic = true;
                //Debug.Log("<b>Setting GameObject to static: </b>" + child.gameObject);
            }
            else
            {
                //Debug.Log("GameObject was already static: " + child.gameObject);
            }
        }
    }

    //define how to look for materials with certain names and add emission to them
    static void SetMaterialEmissionByName()
    {
        // define the asset that was changed as the prefab
        var prefab = AssetDatabase.LoadMainAssetAtPath(globalAssetFilePath);

        // make changes to this prefab's dependencies (materials)
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Debug.Log("Dependency path: " + dependencyPathString);

            //
            // apply general rules
            //

            // all LIGHTs get their color and texture set as emission color/texture
            if (dependencyPathString.Contains("LIGHT"))
            {
                SetStandardMaterialEmission(dependencyPathString);
            }

            //
            // look for certain materials, and modify them further
            //

            if (dependencyPathString.Contains("emission test"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("fluorescent panel"))
            {
                SetCustomMaterialEmissionColor(dependencyPathString, 255, 255, 251);
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            if (dependencyPathString.Contains("green fluor"))
            {
                SetCustomMaterialEmissionColor(dependencyPathString, 253, 255, 240);
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.5F);
            }

            if (dependencyPathString.Contains("high intensity white"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (dependencyPathString.Contains("high intensity green"))
            {
                SetCustomMaterialEmissionColor(dependencyPathString, 253, 255, 240);
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }
    }

    // define how to instantiate proxy replacement objects
    public static void InstantiateProxyReplacements(string assetName)
    {
        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        var transformByAsset = gameObjectByAsset.transform;

        // identify the path of the prefab to replace this object
        string birchPath = "Assets/TreesVariety/birch/birch 5.prefab";

        // get all objects tagged already and delete them
        GameObject[] replacementsToDelete = GameObject.FindGameObjectsWithTag("DeleteMe");
        for (int i = 0; i < replacementsToDelete.Length; i++)
        {
            Debug.Log("<b>Deleted an object with delete tag: </b>" + replacementsToDelete[i].name);
            UnityEngine.Object.DestroyImmediate(replacementsToDelete[i].gameObject);
        }

        // for each of this asset's children, look for any whose name indicates they are proxies to be replaced
        foreach (Transform child in transformByAsset)
        {
            if (child.name.Contains("REPLACE"))
            {
                GameObject gameObjectToBeReplaced = child.gameObject;
                Debug.Log("Found a proxy gameObject to be replaced: " + gameObjectToBeReplaced);

                // ensure the object we want to replace is visible, so we can measure it
                ToggleVisibility.ToggleGameObjectOn(gameObjectToBeReplaced);

                // create a new gameObject for the new asset
                GameObject newObject = (GameObject)AssetDatabase.LoadAssetAtPath(birchPath, typeof(GameObject));

                // create an instance from the prefab
                var instancedPrefab = PrefabUtility.InstantiatePrefab(newObject as GameObject) as GameObject;
                
                // if instancing the prefab was successful, move it to the same location as the proxy
                if (instancedPrefab)
                {
                    Debug.Log("<b>Instanced this prefab: </b>" + instancedPrefab);

                    // give the new prefab the same parent, position, and scale as the proxy
                    instancedPrefab.transform.parent = gameObjectToBeReplaced.transform.parent;
                    instancedPrefab.transform.SetPositionAndRotation(gameObjectToBeReplaced.transform.localPosition, gameObjectToBeReplaced.transform.localRotation);
                    instancedPrefab.transform.localScale = gameObjectToBeReplaced.transform.localScale;
                    ScaleToMatchHeight(instancedPrefab, gameObjectToBeReplaced); // further scale the new object to match the proxy's height

                    // ensure the new prefab rotates about the traditional Z (up) axis to match its proxy 
                    instancedPrefab.transform.localEulerAngles = new Vector3(0, gameObjectToBeReplaced.transform.localEulerAngles.y, 0);

                    // run TagHelper to create the tag if it doesn't exist yet
                    TagHelper.AddTag("DeleteMe");

                    // tag this instanced prefab as a delete candidate for the next import
                    instancedPrefab.gameObject.tag = "DeleteMe";

                    // set the prefab as static
                    GameObject GO = GameObject.Find(instancedPrefab.name);
                    SetAssetAsStaticGameObject(GO.name);
                }
                else
                {
                    Debug.Log("This prefab is null.");
                }

            }
        }
    }

    // define how to turn off the visibility of proxy assets
    public static void HideProxyObjects(string assetName)
    {
        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        var transformByAsset = gameObjectByAsset.transform;

        // for each of this asset's children, look for any whose name indicates they are proxies to be replaced
        foreach (Transform child in transformByAsset)
        {
            if (child.name.Contains("REPLACE"))
            {
                GameObject gameObjectToBeReplaced = child.gameObject;
                //Debug.Log("Found a proxy gameObject to be hide: " + gameObjectToBeReplaced);
                // turn off the visibility of the object to be replaced
                ToggleVisibility.ToggleGameObjectOff(gameObjectToBeReplaced);
            }
        }
    }

    // runs when an asset is updated
    void OnPreprocessModel() {

        // check if the pre-processor just ran, and if so, skip pre-processing
        //Debug.Log("Current time: " + Time.time);
        //Debug.Log("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            //Debug.Log("Skipping pre-processing the model again.");
            return;
        }


        ClearConsole();
        Debug.Log("START PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        ModelImporter importer = assetImporter as ModelImporter;
        String assetFilePath = importer.assetPath.ToLower();
        //Debug.Log(assetFilePath);

        // make the asset path available globally
        globalAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        globalAssetFileNameAndExtension = assetFileNameAndExtension;

        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        globalAssetFileName = assetFileName;

        // get the file's directory only
        String assetFileDirectory = assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        globalAssetFileDirectory = assetFileDirectory;


        /* TODO: use a JSON object to store and read the file import properties */
        /*
        // locate the import settings JSON file
        StreamReader reader = new StreamReader(importSettingsPath);
        string json = reader.ReadToEnd();
        object jsonObject = JsonUtility.FromJson<AssetImportUpdate>(json);
        Debug.Log("Import settings JSON found: " + json);
        //Debug.Log("Get file names: " + jsonObject.fbxFileNames);
        //return JsonUtility.FromJson<AssetImportUpdate>(json);
        */

        //
        // whitelist of files to get modifications
        // only files explicitly mentioned below will get changed
        // each file should state its preference for all available pre- and post-processor flags as defined above
        //

        if (assetFilePath.Contains("material update test.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
        }

        if (assetFilePath.Contains("anchor-broadway.fbx")
            || assetFilePath.Contains("anchor-jcp.fbx")
            || assetFilePath.Contains("anchor-joslins.fbx")
            || assetFilePath.Contains("anchor-mgwards.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-doors-windows-exterior.fbx")
            || assetFilePath.Contains("mall-doors-windows-interior.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-doors-windows-solid.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-flags.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-floor-ceiling-vertical-faceted.fbx")
            || assetFilePath.Contains("mall-interior-detailing-faceted.fbx")
            || assetFilePath.Contains("mall-exterior-detailing-faceted.fbx")
            || assetFilePath.Contains("mall-exterior-walls.fbx")
            || assetFilePath.Contains("store-interior-detailing-L1.fbx")
            || assetFilePath.Contains("store-interior-detailing-L2.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-handrails.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-lights.fbx")
            || assetFilePath.Contains("mall-signage.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-speakers.fbx")
            || assetFilePath.Contains("store-speakers.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("proxy-people.fbx")
            || assetFilePath.Contains("mall-proxy-trees.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
        }

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (doSetGlobalScale)
        {
            SetGlobalScale(importer);
        }

        if (doInstantiateAndPlaceInCurrentScene)
        {
            InstantiateAndPlaceAssetAsGameObject(globalAssetFilePath, currentScene);
        }

        if (doSetColliderActive)
        {
            SetColliderActive(importer);
        }

        if (doSetUVActiveAndConfigure)
        {
            SetUVActiveAndConfigure(importer);
        }

        if (doDeleteReimportMaterialsTextures)
        {
            DeleteReimportMaterialsTextures(globalAssetFilePath);
        }

        Debug.Log("END PreProcessing");
    }

    // runs after pre-processing the model
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // check if there's a leftover .fbm folder, and if so, delete it
        CleanUpFBMDirectory(globalAssetFileDirectory, globalAssetFileName);

        // if post processing isn't required, skip
        if (!postProcessingRequired)
        {
            Debug.Log("Skipping PostProcessing (disabled for this file)");
            return;
        }

        // it seems that a few post processing hits are needed to fully post-process everything
        // any further is probably not necessary
        if (postProcessingHits.Count >= globalMaxPostProcessingHits)
        {
            Debug.Log("Skipping PostProcessing (max allowed reached)");
            return;
        }

        Debug.Log("START PostProcessing...");

        // add to the list of post processing hits, so we know how many times we've been here
        postProcessingHits.Add(true);

        //
        // execute all AssetImportUpdate PostProcessor option flags marked as true
        //

        if (doSetStatic)
        {
            SetAssetAsStaticGameObject(globalAssetFileName);
        }

        if (doSetMaterialEmission)
        {
            SetMaterialEmissionByName();
        }

        if (doInstantiateProxyReplacements)
        {
            InstantiateProxyReplacements(globalAssetFileName);
        }

        if (doHideProxyObjects)
        {
            HideProxyObjects(globalAssetFileName);
        }

        Debug.Log("END PostProcessing");
    }

    public class AssetImportUpdateMono : MonoBehaviour
    {

        //public VersionControl.Asset.States state;

        // Use this for initialization 
        void Start()
        {

            //Debug.Log(state);
            //var gameObjectMaterials = gameObject.GetComponent<Material>();

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}