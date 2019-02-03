using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Windows;
using System.IO;
using System.Reflection;

public class AssetImportUpdate : AssetPostprocessor {

    String assetsFolder = "assets/";
	String texturesFolder = "assets/Textures";
    String materialsFolder = "assets/Materials";
    String importSettingsPath = "assets/importSettings.json";
    static String globalassetFilePath;
    static float prevTime;

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
        
        // get the file path of the asset that just got updated
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
            if (dependencyPathString.Contains(".mat") || (dependencyPathString.Contains("Texture")))
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
            Debug.Log("Reimport model triggered. Forcing asset update:");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("Re-importing materials and setting their location to external...");

        // import materials
        importer.importMaterials = true;

        // make sure materials are being stored externally
        importer.materialLocation = ModelImporterMaterialLocation.External;
        
        // re-extract textures
        Debug.Log("Re-extracting textures...");
        importer.ExtractTextures(texturesFolder);
    }

    // runs when an asset is updated
    void OnPreprocessModel() {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Debug.Log("Current time: " + Time.time);
        //Debug.Log("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            Debug.Log("Skipping pre-processing the model again.");
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

        if (assetFilePath == assetsFolder + "material update test.fbx")
        {
			Debug.Log("Found file to update: " + assetFilePath);

            // delete and reimport materials and textures
            deleteReimportMaterialsTextures(assetFilePath);

            // default importer settings
            Debug.Log("Setting default import options...");
		    importer.globalScale = 100.0F;
            importer.addCollider = true;
            importer.generateSecondaryUV = true;

            globalassetFilePath = assetFilePath;
        }
    }

    // runs after pre-processing the model
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        var prefab = AssetDatabase.LoadMainAssetAtPath(globalassetFilePath);
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Debug.Log("Dependency path: " + dependencyPathString);

            //
            // look for certain materials, and modify them
            //

            var materialName = "LIGHT - yellow.mat";
            if (dependencyPathString.Contains(materialName))
            {
                //Debug.Log("Found a material to update: " + materialName);

                // get the material
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(dependencyPathString, typeof(Material));

                // enable emission and set emission color to the material's color or texture
                Color color = mat.GetColor("_Color");
                Texture texture = mat.GetTexture("_MainTex");
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color);
                mat.SetTexture("_EmissionMap", texture);
                //mat.SetTexture("_Color", texture);


                Debug.Log("<b>Updated material: </b>" + mat);
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