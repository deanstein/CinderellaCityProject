using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AssetImportUpdate : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        String name = importer.assetPath.ToLower();

        if (name == "material update test.fbx")
        {
            Debug.Log("Found file to update: " + name);
            //importer.globalScale = 1.0F;
            //importer.generateAnimations = ModelImporterGenerateAnimations.None;
        }
    }
}