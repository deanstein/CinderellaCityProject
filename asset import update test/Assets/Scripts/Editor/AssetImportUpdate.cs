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

    String importSettingsPath = "assets/importSettings.json";

    static String globalAssetFilePath;
    static String globalAssetFileNameAndExtension;
    static String globalAssetFileName;
    static String globalAssetFileDirectory;
    static String globalAssetTexturesDirectory;

    static float prevTime;
    static float globalScale = 1.0f;
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
    public void DeleteReimportMaterialsTextures(string assetFilePath)
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

    // define how to replace proxy objects with their Unity equivalents
    public static void ReplaceProxiesByAsset(string assetName)
    {
        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        var transformByAsset = gameObjectByAsset.transform;

        // identify the path of the prefab to replace this object
        string birchPath = "Assets/TreesVariety/birch/birch 1.prefab";

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
                    instancedPrefab.transform.SetPositionAndRotation(gameObjectToBeReplaced.transform.localPosition, Quaternion.Euler(new Vector3(0, 0, 0)));
                    instancedPrefab.transform.localScale = gameObjectToBeReplaced.transform.localScale;

                    // get the height of the object to be replaced
                    float targetHeight = GetMaxHeightOfGameObject(gameObjectToBeReplaced);
                    float currentHeight = GetMaxHeightOfGameObject(instancedPrefab);
                    float scaleFactor = (targetHeight / currentHeight) * ((gameObjectToBeReplaced.transform.localScale.x + gameObjectToBeReplaced.transform.localScale.y + gameObjectToBeReplaced.transform.localScale.z) / 3);

                    // scale the prefab to match the height of its replacement
                    instancedPrefab.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    Debug.Log("<b>Scaled </b>" + instancedPrefab + " <b>to match</b> " + gameObjectToBeReplaced + " <b>(" + scaleFactor + " scale factor)</b>");

                    // run TagHelper to create the tag if it doesn't exist yet
                    TagHelper.AddTag("DeleteMe");

                    // tag this instanced prefab as a delete candidate for the next import
                    instancedPrefab.gameObject.tag = "DeleteMe";
                }
                else
                {
                    Debug.Log("This prefab is null.");
                }

            }
        }
    }

    // define how to set standard scale, add colliders, generate lightmap UVs (used for most assets)
    void SetStandardScaleColliderUVImportSettings(ModelImporter mi)
    {
        mi.globalScale = globalScale;
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
        mi.globalScale = globalScale;
        mi.addCollider = true;
        mi.generateSecondaryUV = false;
    }

    // define how to set standard scale and UVs, but no colliders (used for flags)
    void SetStandardScaleUVImportSettings(ModelImporter mi)
    {
        mi.globalScale = globalScale;
        mi.addCollider = false;
        mi.generateSecondaryUV = true;
    }

    // define how to set standard scale only, no UVs or colliders (used for windows/doors, people...)
    void SetStandardScaleImportSettings(ModelImporter mi)
    {
        mi.globalScale = globalScale;
        mi.addCollider = false;
        mi.generateSecondaryUV = false;
    }

    // define how to get the asset's in-scene gameObject and set it to static
    static void SetAssetAsStaticGameObject(string assetName)
    {

        //Debug.Log(GameObjectUtility.(gameObjectFromAsset));

        //StaticEditorFlags newFlags = StaticEditorFlags.EverythingStatic;
        //GameObjectUtility.SetStaticEditorFlags(gameObjectFromAsset, -1);


        // nothing here works consistently, so disabling this for now
        bool trySetStatic = false;
        if (trySetStatic)
        {
            // find the game object using the asset name
            GameObject gameObjectFromAsset = GameObject.Find(assetName);
            Debug.Log("TEST!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + gameObjectFromAsset);
            var transform = gameObjectFromAsset.transform;

            // set it as static for lighting to affect it
            //gameObjectFromAsset.isStatic = true;
            StaticEditorFlags setStaticFlags = StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccluderStatic |  StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
            //GameObjectUtility.SetStaticEditorFlags(gameObjectFromAsset, setStaticFlags);
            //GameObjectUtility.SetStaticEditorFlags(transform.gameObject, setStaticFlags);

            // set each child as static
            for (int a = 0; a < transform.childCount; a++)
            {
                // if it's already static, skip
                if (transform.GetChild(a).gameObject.isStatic == true)
                {
                    Debug.Log("Object was already Static: " + transform.GetChild(a).gameObject);
                    break;
                }

                //GameObject gameObjectFromAsset = GameObject.Find(assetName);
                //StaticEditorFlags flags = StaticEditorFlags.LightmapStatic;
                GameObjectUtility.SetStaticEditorFlags(gameObjectFromAsset, setStaticFlags);
                StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(gameObjectFromAsset);
                Debug.Log("Current Flags: " + currentFlags);
                transform.gameObject.isStatic = true;

                // otherwise, set it as static
                transform.GetChild(a).gameObject.isStatic = true;
                Debug.Log("<b>Setting an object as Static: </b>" + transform.GetChild(a).gameObject);

                GameObjectUtility.SetStaticEditorFlags(transform.gameObject, setStaticFlags);

                SerializedObject so = new SerializedObject(gameObjectFromAsset);
                UnityEditor.SerializedProperty sprop = so.FindProperty("m_LightmapParameters");
                //sprop.objectReferenceValue = myParams;

                so.ApplyModifiedProperties();
                //GameObjectUtility.SetStaticEditorFlags(transform).gameObject, setStaticFlags);
            }
        }
        //gameObjectFromAsset.isStatic = true;
        //Debug.Log("Setting '" + gameObjectFromAsset + "' as static...");
        
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
        if (assetFilePath.Contains("anchor_broadway.fbx") 
            || assetFilePath.Contains("mall_floor ceiling vertical.fbx") 
            || assetFilePath.Contains("material update test.fbx") 
            || assetFilePath.Contains("mall_signage.fbx"))
        {
            // delete and reimport materials and textures
            DeleteReimportMaterialsTextures(assetFilePath);

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
            Debug.Log("<b>Found file to update: </b>" + assetFilePath);

            // delete and reimport materials and textures
            DeleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleColliderImportSettings(importer);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files get everything except colliders
        if (assetFilePath.Contains("mall_flags.fbx"))
        {
            Debug.Log("<b>Found file to update: </b>" + assetFilePath);

            // delete and reimport materials and textures
            DeleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleUVImportSettings(importer);

            // set the gameObject as static 
            SetAssetAsStaticGameObject(globalAssetFileName);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files don't get UVs or colliders
        if (assetFilePath.Contains("mall_doors windows interior.fbx") 
            || assetFilePath.Contains("mall_doors windows exterior.fbx") 
            || assetFilePath.Contains("people.fbx"))
        {
            Debug.Log("<b>Found file to update: </b>" + assetFilePath);

            // delete and reimport materials and textures
            DeleteReimportMaterialsTextures(assetFilePath);

            // set importer settings
            SetStandardScaleImportSettings(importer);

            // set a flag that the pre-processing is completed so post-processing starts
            needsPostProcessing = true;
            Debug.Log("Starting post-processing...");
        }

        // these files contain proxies to be are entirely replaced with unity objects
        if (assetFilePath.Contains("mall_trees.fbx") || assetFilePath.Contains("material update test.fbx"))
        {
            Debug.Log("<b>Found file that contains proxies for replacement: <b>" + assetFilePath);
        
            // replace all proxies with their unity objects
            ReplaceProxiesByAsset(globalAssetFileName);
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
        CleanUpFBMDirectory(globalAssetFileDirectory, globalAssetFileName);

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