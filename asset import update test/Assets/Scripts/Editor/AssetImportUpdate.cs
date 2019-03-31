using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Windows;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;

public class AssetImportUpdate : AssetPostprocessor {

    String importSettingsPath = "assets/importSettings.json";

    static String globalAssetFilePath;
    static String globalAssetFileNameAndExtension;
    static String globalAssetFileName;
    static String globalAssetFileDirectory;
    static String globalAssetTexturesDirectory;

    static float prevTime;
    static bool needsPostProcessing = false;

    // define how to clear the console
    public static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    // define how to delete and reimport materials and textures
    public void deleteReimportMaterialsTextures(string assetFilePath)
    {
        // make the asset path available globally
        globalAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        globalAssetFileNameAndExtension = assetFileNameAndExtension;

        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        globalAssetFileName = assetFileName;

        // get the file's directory only
        String assetFileDirectory= assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        globalAssetFileDirectory = assetFileDirectory;

        // initialize ModelImporter
        ModelImporter importer = assetImporter as ModelImporter;

        // assume the active scene is where this object should be instantiated
        Scene scene = SceneManager.GetActiveScene();

        // place the asset in the scene if it's not there already
        InstantiateAndPlaceAssetAsGameObject(assetFilePath, scene);

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
        importer.materialLocation = ModelImporterMaterialLocation.External;

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

    // define how to clean up an automatically-created .fbm folder
    public static void cleanUpFBMDirectory(string assetFileDirectory, string assetFileName)
    {
        if (AssetDatabase.IsValidFolder(assetFileDirectory + assetFileName + ".fbm"))
        {
            Debug.Log("Seeing a leftover .FBM folder, and deleting it...");
            //Debug.Log(assetFileDirectory + assetFileName + ".fbm");
            UnityEngine.Windows.Directory.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm");
            UnityEngine.Windows.File.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm.meta");
        }
    }

    // define how to set standard scale, add colliders, generate lightmap UVs (used for most assets)
    void SetStandardScaleColliderUVImportSettings(ModelImporter mi)
    {
        mi.globalScale = 100.0F;
        mi.addCollider = true;
        mi.generateSecondaryUV = true;
        mi.secondaryUVHardAngle = 88;
        mi.secondaryUVPackMargin = 64;
        mi.secondaryUVAngleDistortion = 4;
        mi.secondaryUVAreaDistortion = 4;
    }

    // define how to set standard scale and add colliders, but no UVs (used for handrails)
    void SetStandardScaleColliderImportSettings(ModelImporter mi)
    {
        mi.globalScale = 100.0F;
        mi.addCollider = true;
        mi.generateSecondaryUV = false;
    }

    // define how to set standard scale and UVs, but no colliders (used for flags)
    void SetStandardScaleUVImportSettings(ModelImporter mi)
    {
        mi.globalScale = 100.0F;
        mi.addCollider = false;
        mi.generateSecondaryUV = true;
    }

    // define how to set standard scale only, no UVs or colliders (used for windows/doors, people...)
    void SetStandardScaleImportSettings(ModelImporter mi)
    {
        mi.globalScale = 100.0F;
        mi.addCollider = false;
        mi.generateSecondaryUV = false;
    }

    // define how to get the asset as a gameObject and set it to static
    void SetAssetAsStaticGameObject(string assetName)
    {
        // instantiate the game object
        GameObject gameObjectFromAsset = GameObject.Find(assetName);

        // set it as static for lighting to affect it
        gameObjectFromAsset.isStatic = true;
        Debug.Log("Setting '" + gameObjectFromAsset + "' as static...");
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

    // define how to enable emission on a material and set its color and texture to emissive
    public static void setEmissionOnMaterial(string materialFilePath)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // enable emission and set emission color to the material's color or texture
        Color color = mat.GetColor("_Color");
        Texture texture = mat.GetTexture("_MainTex");
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color);
        mat.SetTexture("_EmissionMap", texture);

        Debug.Log("<b>Updated emissive material: </b>" + mat);
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

        // get the file path of the asset that just got updated
        ModelImporter importer = assetImporter as ModelImporter;
        String assetFilePath = importer.assetPath.ToLower();
        //Debug.Log(assetFilePath);


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
        // test for specific files
        //

        // these files get the full import treatment: materials/textures, scale, collider, and lightmap UVs
        if ((assetFilePath.Contains("anchor_broadway.fbx") || (assetFilePath.Contains("mall_floor ceiling vertical.fbx")) || (assetFilePath.Contains("material update test.fbx"))))
        {
            Debug.Log("Found file to update: " + assetFilePath);

            // delete and reimport materials and textures
            deleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleColliderUVImportSettings(importer);

            // set the gameObject as static 
            SetAssetAsStaticGameObject(globalAssetFileName);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files get everything except UVs
        if (assetFilePath.Contains("mall_handrails.fbx"))
        {
            Debug.Log("Found file to update: " + assetFilePath);

            // delete and reimport materials and textures
            deleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleColliderImportSettings(importer);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files get everything except colliders
        if (assetFilePath.Contains("mall_flags.fbx"))
        {
            Debug.Log("Found file to update: " + assetFilePath);

            // delete and reimport materials and textures
            deleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleUVImportSettings(importer);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files don't get UVs or colliders
        if ((assetFilePath.Contains("mall_doors windows interior.fbx") || (assetFilePath.Contains("mall_doors windows exterior.fbx")) || (assetFilePath.Contains("people.fbx"))))
        {
            Debug.Log("Found file to update: " + assetFilePath);

            // delete and reimport materials and textures
            deleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleImportSettings(importer);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

    }

    // runs after pre-processing the model
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // don't post-process if the flag isn't set
        if (!needsPostProcessing)
            return;

        // check if there's a leftover .fbm folder, and if so, delete it
        cleanUpFBMDirectory(globalAssetFileDirectory, globalAssetFileName);

        var prefab = AssetDatabase.LoadMainAssetAtPath(globalAssetFilePath);
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Debug.Log("Dependency path: " + dependencyPathString);

            //
            // apply general rules
            //

            // all LIGHTs get color and texture emissives
            var materialPartialName = "LIGHT";
            if (dependencyPathString.Contains(materialPartialName))
            {
                //Debug.Log("Found a material named "LIGHT" and setting emissions...");
                setEmissionOnMaterial(dependencyPathString);
            }

            //
            // look for certain materials, and modify them
            //

            /*
            var materialName = "LIGHT - yellow.mat";
            if (dependencyPathString.Contains(materialName))
            {
                //Debug.Log("Found a material to update: " + materialName);
                setEmissionOnMaterial(dependencyPathString);
            }
            */
            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }
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