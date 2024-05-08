using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]

/// <summary>
/// Applies settings and modifications to imported files (FBX models, audio files, images/graphics...)
/// Runs automatically when a whitelisted file type is imported (or reimported) in the Unity Editor
/// </summary>

public class AssetImportGlobals
{
    // initialize import parameters
    public static ModelImportParams ModelImportParamsByName;
}

// this only runs when an asset is updated in the editor
public class AssetImportUpdate : AssetPostprocessor {

    // get information on the file that was updated
    static String importedAssetFilePath;
    static String importedAssetFileNameAndExtension;
    static String importedAssetFileName;
    static String importedAssetFileDirectory;
    static String importedAssetTexturesDirectory;
    // FBX files will have a corresponding GameObject in the scene
    static GameObject importedAssetGameObject;
    static UnityEngine.Object[] importedAssetGameObjectDependencies;

    // if the model just updated isn't already in the scene, we need to keep track of it
    // in order to maintain parent/child hierarchy in the post-processor
    static GameObject newlyInstantiatedFBXContainer;
    static bool isNewlyInstantiatedFBXContainer;

    // get the current scene
    static Scene currentScene = EditorSceneManager.GetActiveScene();

    // all incoming models are scaled to this value
    static float globalScale = 1.0f;

    // in some cases, we need to stop processing from happening if it's already been done
    // so keep track of the last time processing was done, to help determine whether to skip
    static float prevTime;

    // post-processing should only happen after pre-processing
    static bool postProcessingRequired = false;

    // proxy replacements as required
    static bool proxyReplacementProcessingRequired = false;

    // certain proxies get replaced in a certain way or by a specific prefab
    static string proxyType;

    // keep track of the newly-instantiated objects - like trees and people - for counting and culling
    static List<GameObject> instancedPrefabs = new List<GameObject>();
    // keep track of which newly-instantiated objects were generated from a proxy object
    static List<GameObject> instancedPrefabsFromProxies = new List<GameObject>();
    // keep track of which newly-instantiated objects were generated as a random filler
    static List<GameObject> instancedPrefabsAsRandomFillers = new List<GameObject>();
    // keep track of how many instances had to be deleted because no good random point was found
    static int culledPrefabs = 0;

    // post-processing seems to repeat itself a lot, so set a max and keep track of how many times
    // note that if an object was just instantiated in the scene, this max hit value gets incremented by 1
    static int globalMaxPostProcessingHits = 3;
    static List<bool> postProcessingHits = new List<bool>();

    // set up callbacks
    public AssetImportUpdate()
    {
        EditorSceneManager.activeSceneChangedInEditMode += SceneChangedInEditModeCallback;
    }
    // for some reason, we need to subscribe to this message and update the currentScene name when a different scene is opened in the Editor
    private void SceneChangedInEditModeCallback(Scene previousScene, Scene newScene)
    {
        currentScene = newScene;
        Utils.DebugUtils.DebugLog("Opened a different Scene in the Editor: " + newScene.name);
    }

    // define how to clear the console
    public static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    // get the textures path for an asset given the asset's file path
    static string GetTexturesPathFromMaterialPath(string materialPath)
    {
        // the material path is one folder adjacent to the textures path
        // so split the path by slashes and go up one level
        string[] splitPath = materialPath.Split(new char[] { '/' });
        string[] newSplitPathArray = splitPath.RangeSubset(0, splitPath.Length - 2);

        string newPath = "";

        foreach (string pathSection in newSplitPathArray)
        {
            newPath = newPath + pathSection + "/";
        }

        newPath = newPath + "Textures";

        return newPath;
    }

    // define how to enable UVs and configure them
    void SetUVActiveAndConfigure(ModelImporter mi)
    {
        mi.generateSecondaryUV = true;
        mi.secondaryUVHardAngle = 88;
        mi.secondaryUVPackMargin = 64;
        mi.secondaryUVAngleDistortion = 4;
        mi.secondaryUVAreaDistortion = 4;
        mi.importNormals = ModelImporterNormals.Calculate;
        mi.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
        mi.normalSmoothingAngle = 15.0f;
    }

    // set each mesh renderer in the asset to a certain scale in the lightmap
    public static void SetCustomLightmapSettingsByName(GameObject gameObject)
    {
        // it's possible that the post-processor is running repeatedly (not kicked off by an asset change)
        // if that happens, the gameObject from asset will be null, so skip this to prevent errors
        if (gameObject == null)
        {
            Utils.DebugUtils.DebugLog("This GameObject was null, so skipping set custom lightmapping operation: " + gameObject.name);

            return;
        }

        // the scene name will be checked downstream, 
        // since the 60s70s scene requires additional overrides
        string sceneName = EditorSceneManager.GetActiveScene().name;

        // get all the children transforms
        Renderer[] allChildrenRenderers = gameObject.GetComponentsInChildren<Renderer>();

        //  modify the lightmap settings for each of the renderers
        foreach (Renderer childRenderer in allChildrenRenderers)
        {
            Material mat = childRenderer.sharedMaterial;

            // only try if there's a valid material on this transform
            if (mat != null)
            {
                SerializedObject so = new SerializedObject(childRenderer);

                float existingResolution = so.FindProperty("m_ScaleInLightmap").floatValue;
                float newResolution = ManageImportSettings.GetShadowMapResolutionOverrideByMaterialName(sceneName, gameObject.name, mat.name, ManageImportSettings.GetShadowMapResolutionMultiplierByName(gameObject.name));

                //Utils.DebugUtils.DebugLog("Changing resolution of " + gameObject.name + " child " + child.name + " to " + newResolution);

                // only bother changing the properties if the existing and new res don't match
                if (existingResolution != newResolution)
                {
                    so.FindProperty("m_ScaleInLightmap").floatValue = newResolution;
                    so.ApplyModifiedProperties();
                }

                // need to dispose of the serialized object to avoid runaway RAM usage
                so.Dispose();
            }
        }
    }

    // define how to set audio clip import settings
    void SetClipImportSettings(AudioImporter ai)
    {
        ai.forceToMono = true;
        ai.loadInBackground = true;
        ai.preloadAudioData = true;

        //create a temp variable that contains everything we could apply to the imported AudioClip (possible changes: .compressionFormat, .conversionMode, .loadType, .quality, .sampleRateOverride, .sampleRateSetting)
        AudioImporterSampleSettings aiSampleSettings = ai.defaultSampleSettings;
        aiSampleSettings.loadType = AudioClipLoadType.CompressedInMemory;
        aiSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
        aiSampleSettings.quality = 70f;

        // apply the settings
        ai.defaultSampleSettings = aiSampleSettings;
    }

    // define how to set texture-to-sprite import settings
    void SetTextureToSpriteImportSettings(TextureImporter ti)
    {
        ti.textureType = TextureImporterType.Sprite;
    }

    // define how to instantiate the asset (typically an FBX file) in the scene
    void InstantiateAndPlaceAssetAsGameObject(GameObject gameObjectFromAsset, Scene scene)
    {
        // if the game object is null, this is a new file... so refresh the asset database and try again
        if (!gameObjectFromAsset)
        {
            //Utils.DebugUtils.DebugLog("Game object from asset not valid (yet): " + assetFilePath);
            AssetDatabase.Refresh();
            gameObjectFromAsset = (GameObject)AssetDatabase.LoadAssetAtPath(importedAssetFilePath, typeof(GameObject));
        }

        // skip instantiating if this asset already exists in this scene

        // get the top-level objects in the scene container
        GameObject[] sceneObjects = ManageSceneObjects.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene());
        // test if the asset name matches any of the top-level object names
        foreach (GameObject sceneObject in sceneObjects)
        {
            if (sceneObject.name == gameObjectFromAsset.name)
            {
                Utils.DebugUtils.DebugLog("This object is already present in the model.");
                return;
            }
        }

        // otherwise, instantiate as a prefab with the name of the file
        newlyInstantiatedFBXContainer = PrefabUtility.InstantiatePrefab(gameObjectFromAsset, scene) as GameObject;
        newlyInstantiatedFBXContainer.name = gameObjectFromAsset.name;
        Utils.DebugUtils.DebugLog("This object was instantiated in the model hierarchy.");

        // set the flag that an object was just instantiated so we can fix parent/child hierarchy in post-processor
        isNewlyInstantiatedFBXContainer = true;

        // allow additional post processing hits since this model was just instantiated
        globalMaxPostProcessingHits = globalMaxPostProcessingHits + 2;
    }

    // sets an object as a child of the scene container
    // needs to happen in post-processor
    static void SetObjectAsChildOfSceneContainer(GameObject prefabToModify)
    {
        GameObject sceneContainer = ManageSceneObjects.GetSceneContainerObject(SceneManager.GetActiveScene());

        if (prefabToModify)
        {
            prefabToModify.transform.SetParent(sceneContainer.transform);
        }
    }

    // delete and reimport all materials and textures associated with the asset at the given path
    public void DeleteReimportMaterialsTextures(string assetFilePath)
    {
        // initialize ModelImporter
        ModelImporter importer = assetImporter as ModelImporter;

        // set reimport required initially to false
        bool reimportRequired = false;

        // get the material and texture dependencies and delete them
        //var prefab = AssetDatabase.LoadMainAssetAtPath(assetFilePath);
        //foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        foreach (var dependencyPath in AssetDatabase.GetDependencies(importedAssetFilePath, false))
        {
            //var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            // if there are materials or textures detected in the path, delete them
            // note that we also check that the path includes the file name to avoid other assets from being affected
            if ((dependencyPathString.Contains(".mat") || dependencyPathString.Contains(".jpg") || dependencyPathString.Contains(".jpeg") || dependencyPathString.Contains(".png")) && dependencyPathString.Contains(importedAssetFileName))
            {
                UnityEngine.Windows.File.Delete(dependencyPathString);
                UnityEngine.Windows.File.Delete(dependencyPathString + ".meta");
                Utils.DebugUtils.DebugLog("<b>Deleting files and meta files:</b> " + dependencyPathString);
                reimportRequired = true;
                prevTime = Time.time;
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }

        // if materials or textures were deleted, force reimport the model
        if (reimportRequired)
        {
            Utils.DebugUtils.DebugLog("Reimport model triggered. Forcing asset update...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        Utils.DebugUtils.DebugLog("Re-importing materials...");

        // import materials
        importer.importMaterials = true;

        // make sure materials are being stored externally
        // and name them based on incoming names
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;

        // Materials are automatically stored in a Materials folder (Unity default behavior)

        // Textures are automatically stored in an ".fbm" folder (Unity default behavior)
        // However, for clean organization, we want to store textures in a "Textures" folder
        // the old .FBM folder will be deleted in the post-processor

        // textures will be extracted to a "Textures" folder next to the asset
        string assetTexturesDirectory = importedAssetFileDirectory + "Textures";
        importedAssetTexturesDirectory = assetTexturesDirectory;

        // re-extract textures
        Utils.DebugUtils.DebugLog("Re-importing textures...");
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

        Utils.DebugUtils.DebugLog("<b>Set standard emission on Material: </b>" + mat);
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
        Utils.DebugUtils.DebugLog("<b>Set custom emission intensity of " + intensity + " (" + adjustedIntensity + " internally) on Material: </b>" + mat);
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
        Utils.DebugUtils.DebugLog("<b>Set custom emission color on Material: </b>" + mat);
    }

    // define how to create a greyscale new texture given a scale factor from 0 (black) to 1 (white)
    public static string CreateGreyscaleTexture(string materialFilePath, float scale)
    {
        // name the metallic map based on its rgb values
        string fileName = "metallicMap-" + scale + "-" + scale + "-" + scale + ".png";
        // determine the textures dir for this material and the new texture's file path
        // if this is during an import process, use the known asset import textures dir, otherwise determine the textures dir
        string texturesDir = importedAssetTexturesDirectory != null ? importedAssetTexturesDirectory : AssetImportUpdate.GetTexturesPathFromMaterialPath(materialFilePath);
        string filePath = texturesDir + "/" + fileName;

        // only make a new texture if it doesn't exist yet
        // note that this texture does not get cleaned up in DeleteReimportMaterialsTextures
        // so it's likely already made and can be reused
        if (!AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture)))
        {
            //Utils.DebugUtils.DebugLog("No MetallicGlossMap detected. Creating one.");
            // since the texture is just one color, we don't need a high resolution
            int sizeX = 100;
            int sizeY = 100;

            // convert the given scale factor to a color
            Color color = new Color(scale, scale, scale);

            // create a blank texture at the given size
            Texture2D newTexture = new Texture2D(sizeX, sizeY);

            // fill each pixel the given color
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    newTexture.SetPixel(x, y, color);
                }
            }

            // create the required textures folder before trying to write to it
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }

            // write the texture to the file system
            UnityEngine.Windows.File.WriteAllBytes(filePath, (byte[])newTexture.EncodeToPNG());

            newTexture.Apply();
        }
        else
        {
            //Utils.DebugUtils.DebugLog("MetallicGlossMap texture already exists.");
        }

        return filePath;
    }


    // define how to set material smoothness
    public static void SetMaterialSmoothness(string materialFilePath, float smoothness)
    {
        Utils.DebugUtils.DebugLog("<b>Set smoothness on Material: </b>" + materialFilePath);

        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // enable the keyword so the canvas updates with new gloss values
        mat.EnableKeyword("_METALLICGLOSSMAP");

        // set its smoothness channel to albedo
        mat.SetInt("_SmoothnessTextureChannel", 1);

        // set it to the given smoothness (glossiness) value
        mat.SetFloat("_GlossMapScale", smoothness);

        // once _METALLICGLOSSMAP keyword is enabled, we have to provide a gloss map
        // otherwise metallic gets set to 100% visually in the scene
        // all materials with gloss need get a black (0) gloss map generated for them to negate the metallic effect
        // Materials may get Metallic set to a non-0 value in a separate function

        // create the black texture
        string metallicGlossTexturePath = CreateGreyscaleTexture(materialFilePath, 0.0f);
        Texture blackTexture = (Texture)AssetDatabase.LoadAssetAtPath(metallicGlossTexturePath, typeof(Texture));

        // set the MetallicGlossMap as the black texture
        mat.SetTexture("_MetallicGlossMap", blackTexture);
    }

    // define how to set material metallic
    public static void SetMaterialMetallic(string materialFilePath, float metallic)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // create a greyscale texture based on the specified metallic value
        string metallicGlossTexturePath = CreateGreyscaleTexture(materialFilePath, metallic);
        Texture metallicGlossTexture = (Texture)AssetDatabase.LoadAssetAtPath(metallicGlossTexturePath, typeof(Texture));

        // set the MetallicGlossMap as the black texture
        mat.SetTexture("_MetallicGlossMap", metallicGlossTexture);

        Utils.DebugUtils.DebugLog("<b>Set metallic on Material: </b>" + mat);
    }

    // use the specular shader to set a material to no specular
    // important for non-reflective materials like concrete, asphalt, etc
    // specularValue is applied as RGB, 0-255
    public static void SetMaterialSpecular(string materialFilePath, byte specularValue)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        Shader specularShader = Shader.Find("Standard (Specular setup)");
        mat.shader = specularShader;

        Color32 specularColor = new Color32(specularValue, specularValue, specularValue, 255);

        mat.SetColor("_SpecColor", specularColor);
    }

    // clean up the automatically-created .fbm folder on import
    // this folder is not necessary because we put materials in a "Materials" folder
    public static void DeleteFBMFolderOnImport(string assetFileDirectory, string assetFileName)
    {
        string FBMFolderPath = assetFileDirectory + assetFileName + ".fbm";
        if (AssetDatabase.IsValidFolder(FBMFolderPath))
        {
            Utils.DebugUtils.DebugLog("<b>Deleting a leftover .FBM folder: </b>" + FBMFolderPath);
            //Utils.DebugUtils.DebugLog(assetFileDirectory + assetFileName + ".fbm");
            UnityEngine.Windows.Directory.Delete(importedAssetFileDirectory + importedAssetFileName + ".fbm");
            UnityEngine.Windows.File.Delete(importedAssetFileDirectory + importedAssetFileName + ".fbm.meta");
        }
    }

    // clean up all .fbm folders in the project
    public static void DeleteAllFBMFolders()
    {
        // get all the scene objects
        GameObject[] sceneObjects = ManageSceneObjects.GetTopLevelChildrenInSceneContainer(SceneManager.GetActiveScene());

        bool anyFoldersDeleted = false;

        foreach (GameObject sceneObject in sceneObjects)
        {
            // get the prefab path associated with this scene object
            string prefabPathBySceneObject = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sceneObject);

            // only proceed if the prefab path is non-zero in length
            if (prefabPathBySceneObject.Length != 0)
            {
                // determine the FBM dir, which would be at the same level as the object
                // but with an .fbm extension instead
                string prefabPathWithoutExtension = FileDirUtils.RemoveExtension(prefabPathBySceneObject);
                string FBMFolderPath = prefabPathWithoutExtension + ".fbm";

                // if the folder exists, delete it
                if (AssetDatabase.IsValidFolder(FBMFolderPath))
                {
                    Utils.DebugUtils.DebugLog("<b>Found a leftover .FBM folder to delete: </b>" + FBMFolderPath);
                    UnityEngine.Windows.Directory.Delete(FBMFolderPath);
                    UnityEngine.Windows.File.Delete(FBMFolderPath + ".meta");

                    anyFoldersDeleted = true;
                }
            }
        }

        if (!anyFoldersDeleted)
        {
            Utils.DebugUtils.DebugLog("Found no .FBM folders to delete for the objects found in the scene.");
        }
    }

    // define how to create a component attached to a GameObject, or delete it and create a new one if component already exists
    public static void RemoveComponentIfExisting(GameObject gameObjectForComponent, string componentType)
    {
        bool hasComponent = (gameObjectForComponent != null && gameObjectForComponent.GetComponent(componentType) as Component != null);
        // if this object already has a component of this type, delete it
        if (hasComponent)
        {
            Utils.DebugUtils.DebugLog("<b>Deleted</b> existing behavior component " + componentType + " on " + gameObjectForComponent + ".");
            Component componentToDelete = gameObjectForComponent.GetComponent(componentType) as Component;
            //AudioSource audioSourceToDelete = gameObjectForComponent.gameObject.GetComponent<AudioSource>();

            // delete the existing component
            UnityEngine.Object.DestroyImmediate(componentToDelete);
        }
    }

    // adds Unity Engine components to a GameObject
    public static void AddUnityEngineComponentToGameObject(GameObject gameObjectForComponent, string componentName)
    {
        // define the generic Unity Engine component type
        Type unityEngineComponentType = Type.GetType("UnityEngine." + componentName + ", UnityEngine");

        // first, remove the component if it exists already
        RemoveComponentIfExisting(gameObjectForComponent, componentName);

        // now add the component
        gameObjectForComponent.AddComponent(unityEngineComponentType);

        Utils.DebugUtils.DebugLog("<b>Added</b> " + componentName + " component to " + gameObjectForComponent);
    }

    // adds Unity Engine components to a GameObject's children
    public static void AddUnityEngineComponentToGameObjectChildren(GameObject parentGameObject, string componentName)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            AddUnityEngineComponentToGameObject(childTransform.gameObject, componentName);
        }
    }

    // adds custom components to a GameObject
    public static void AddCustomScriptComponentToGameObject(GameObject gameObjectForComponent, string scriptName)
    {
        if (gameObjectForComponent)
        {
            // define the type of script based on the name
            Type customScriptComponentType = Type.GetType(scriptName + ", Assembly-CSharp");

            // first, remove the component if it exists already
            RemoveComponentIfExisting(gameObjectForComponent, scriptName);

            // now add the component
            Component customScriptComponent = gameObjectForComponent.AddComponent(customScriptComponentType);

            Utils.DebugUtils.DebugLog("<b>Added</b> " + scriptName + " component to " + gameObjectForComponent);
        }
    }

    // adds custom components to a GameObject's children
    public static void AddCustomScriptComponentToGameObjectChildren(GameObject parentGameObject, string scriptName)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            AddCustomScriptComponentToGameObject(childTransform.gameObject, scriptName);
        }
    }

    // add certain behavior components to certain GameObjects as defined by their host file name
    public static void AddBehaviorComponentsByName(string assetName)
    {
        Utils.DebugUtils.DebugLog("Adding behavior components...");

        // find the associated GameObject by this asset's name, and all of its children objects
        GameObject gameObjectByAssetName = GameObject.Find(assetName);

        //
        // set rules based on name
        // 

        // speakers
        if (assetName.Contains("speakers"))
        {
            /* add components to the parent gameObject */

            // add the script to disable components of this object's children by proximity to the player
            AddCustomScriptComponentToGameObject(gameObjectByAssetName, "ToggleChildrenComponentsByProximityToPlayer");
            ToggleChildrenComponentsByProximityToPlayer toggleComponentByProximityScript = gameObjectByAssetName.GetComponent<ToggleChildrenComponentsByProximityToPlayer>();
            toggleComponentByProximityScript.maxDistance = 20f;
            toggleComponentByProximityScript.checkIfInFrame = false;
            toggleComponentByProximityScript.toggleComponentTypes = new string[] { "AudioSource", "PlayAudioSequencesByName" };

            /* add components to children gameObjects */

            // delete the existing script host objects
            string speakerScriptHostDeleteTag = ManageTags.GetOrCreateTagByScriptHostType("Speakers");
            TaggedObjects.DeleteObjectsByTag(speakerScriptHostDeleteTag);

            // get all the children of this object - including empty transforms (instances or groups)
            Transform[] childrenTransforms = gameObjectByAssetName.GetComponentsInChildren<Transform>();
            List<Transform> childrenRequiringAudioSource = new List<Transform>();

            // loop through the transforms and get ones containing a speaker
            foreach (Transform childTransform in childrenTransforms)
            {
                if (childTransform.name.Contains("speaker-"))
                {
                    childrenRequiringAudioSource.Add(childTransform);
                }
            }

            foreach (Transform child in childrenRequiringAudioSource)
            {
                // first, create a host gameobject for all scripts and interactive elements
                GameObject scriptHostObject = new GameObject(child.name + "-ScriptHost");
                // set the script host to nest under the original speaker
                scriptHostObject.transform.parent = child.transform;
                // move the script host to the same position in space - critical for proximity-based scripts!
                scriptHostObject.transform.position = child.transform.position;

                // add the required scripts and behaviors
                AddUnityEngineComponentToGameObject(scriptHostObject, "AudioSource");
                AddCustomScriptComponentToGameObject(scriptHostObject, "PlayAudioSequencesByName");
                AddCustomScriptComponentToGameObject(scriptHostObject, "CanDisableComponents");

                // mark this object as a delete candidate
                scriptHostObject.tag = speakerScriptHostDeleteTag;
            }
        }

        // proxy people
        if (assetName.Contains("proxy-people"))
        {
            /* add components to the parent gameObject */

            // add the script to toggle this entire game object by input event
            AddCustomScriptComponentToGameObject(gameObjectByAssetName, "ToggleObjectsByInputEvent");
            ToggleObjectsByInputEvent ToggleVisibilityScript = gameObjectByAssetName.GetComponent<ToggleObjectsByInputEvent>();

            // add the script to disable components of this object's children by proximity to the player
            AddCustomScriptComponentToGameObject(gameObjectByAssetName, "ToggleChildrenComponentsByProximityToPlayer");
            ToggleChildrenComponentsByProximityToPlayer toggleComponentByProximityScript = gameObjectByAssetName.GetComponent<ToggleChildrenComponentsByProximityToPlayer>();
            toggleComponentByProximityScript.maxDistance = NPCControllerGlobals.maxDistanceBeforeSuspend;
            toggleComponentByProximityScript.checkIfInFrame = true;
            toggleComponentByProximityScript.toggleComponentTypes = new string[] { "NavMeshAgent", "FollowPathOnNavMesh" };

            /* add components to children gameObjects */

        }

        // proxy water
        if (assetName.Contains("water"))
        {
            /* add components to the parent gameObject */

            /* add components to children gameObjects */

            // get all the children of this object - including empty transforms (instances or groups)
            Transform[] childrenTransforms = gameObjectByAssetName.GetComponentsInChildren<Transform>();

            // loop through the transforms and get ones that indicate they will host particle systems
            foreach (Transform childTransform in childrenTransforms)
            {
                if (childTransform.name.Contains("fountain") || childTransform.name.Contains("splash"))
                {
                    childTransform.gameObject.AddComponent<AutoResumeParticleSystem>();
                }
            }
        }
    }


    //
    // <><><><><><><><><><><><><>
    //
    // these functions must be called in the post-processor
    // some may run multiple times in one import due to Unity post-processor behavior
    //
    // <><><><><><><><><><><><><>
    //

    // set every transform in an asset gameObject to specific static editor flags
    public static void SetStaticFlagsByName(GameObject gameObject)
    {
        // it's possible that the post-processor is running repeatedly (not kicked off by an asset change)
        // if that happens, the gameObject from asset will be null, so skip this to prevent errors
        if (!gameObject)
        {
            return;
        }

        // get the appropriate flags for this asset
        var staticFlags = ManageImportSettings.GetStaticFlagsByName(gameObject.name);
        Utils.DebugUtils.DebugLog("Setting static flags for " + gameObject.name);

        // get all children of the asset-based gameObject
        Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>(true); // true to include inactive children

        // ensure that every transform has the correct static settings
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<Renderer>())
            {
                if (child.GetComponent<Renderer>().sharedMaterial)
                {
                    // ensure objects painted with a material indicating non-static don't get static flags
                    if (child.GetComponent<Renderer>().sharedMaterial.name.Contains("non-static"))
                    {
                        GameObjectUtility.SetStaticEditorFlags(child.gameObject, 0);
                    }
                    else
                    {
                        GameObjectUtility.SetStaticEditorFlags(child.gameObject, staticFlags);
                    }
                }            
            }
            else
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, staticFlags);
            }
        }
    }

    // set emission on specific dependent materials in the target object
    public static void SetAllDependentMaterialsEmissionByName(GameObject targetObject)
    {
        // if the target object and the last-updated asset import object are the same,
        // use the global game object dependencies that have already been calculated
        // otherwise, get the dependencies from the provided object
        UnityEngine.Object[] targetDependencies = (targetObject == importedAssetGameObject) ? importedAssetGameObjectDependencies : EditorUtility.CollectDependencies(new UnityEngine.Object[] { targetObject });

        foreach (var dependency in targetDependencies)
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            string dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            // limit changes to materials only
            if (dependencyPathString.Contains(".mat"))
            {
                // all LIGHTs get their color and texture set as emission color/texture
                if (dependencyPathString.Contains("LIGHT - ")
                || dependencyPathString.Contains("artwork - "))
                {
                    SetStandardMaterialEmission(dependencyPathString);

                    // some materials may be overridden with a custom emission value
                    float materialEmissionOverride = ManageImportSettings.GetMaterialEmissionByName(dependencyPathString);

                    if (materialEmissionOverride != 0.001f)
                    {
                        SetCustomMaterialEmissionIntensity(dependencyPathString, materialEmissionOverride);
                    }
                }
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }
    }

    // set smoothness and metallic on specific dependent materials in the target object
    public static void SetAllDependentMaterialsSmoothnessMetallicByName(GameObject targetObject)
    {
        // if the target object and the last-updated asset import object are the same,
        // use the global game object dependencies that have already been calculated
        // otherwise, get the dependencies from the provided object
        UnityEngine.Object[] targetDependencies = (targetObject == importedAssetGameObject) ? importedAssetGameObjectDependencies : EditorUtility.CollectDependencies(new UnityEngine.Object[] { targetObject });

        foreach (var dependency in targetDependencies)
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            string dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            // limit changes to materials only
            if (dependencyPathString.Contains(".mat"))
            {
                float materialMetallic = ManageImportSettings.GetMaterialMetallicByName(dependencyPathString);
                float materialSmoothness = ManageImportSettings.GetMaterialSmoothnessByName(dependencyPathString);

                if (materialSmoothness != -1)
                {
                    SetMaterialSmoothness(dependencyPathString, materialSmoothness);
                }

                if (materialMetallic != -1)
                {
                    SetMaterialMetallic(dependencyPathString, materialMetallic);
                }
            }

            if (string.IsNullOrEmpty(dependencyPathString)) continue;
        }
    }

    // set specular on specific dependent materials in the target object
    public static void SetAllDependentMaterialsSpecularByName(GameObject targetObject)
    {
        // if the target object and the last-updated asset import object are the same,
        // use the global game object dependencies that have already been calculated
        // otherwise, get the dependencies from the provided object
        UnityEngine.Object[] targetDependencies = (targetObject == importedAssetGameObject) ? importedAssetGameObjectDependencies : EditorUtility.CollectDependencies(new UnityEngine.Object[] { targetObject });

        // make changes to this prefab's dependencies (materials)
        foreach (var dependency in targetDependencies)
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            string dependencyPathString = dependencyPath.ToString();

            // get the desired specular value for this material
            int materialSpecular = ManageImportSettings.GetMaterialSpecularByName(dependencyPathString);

            // limit changes to materials only
            if (dependencyPathString.Contains(".mat"))
            {
                if (materialSpecular != -1)
                {
                    SetMaterialSpecular(dependencyPathString, (byte)materialSpecular);
                }
            }
        }
    }

    public static void ApplyAndConfigureNPCAgent(GameObject NPCObject, GameObject proxyObject)
    {
        // only add an agent if the proxy object indicates this NPC is not sitting or a service worker behind a counter
        if (proxyObject && !ManageProxyMapping.GetIsWalking(proxyObject))
        {
            //Debug.Log("Not walking: " + proxyObject.name);
            return;
        }

        // add a navigation mesh agent to this person so it can find its way on the navmesh
        // or act as an obstacle to other agents if this one is idle
        NavMeshAgent thisAgent;
        if (!NPCObject.GetComponent<NavMeshAgent>())
        {
            thisAgent = NPCObject.AddComponent<NavMeshAgent>();
        }
        else
        {
            thisAgent = NPCObject.GetComponent<NavMeshAgent>();
        }

        // configure
        ManageNPCControllers.ConfigureAgentWIthDefaultNPCSettings(thisAgent);
    }

    public static void ConfigureNPCForPathfinding(GameObject NPCObject, GameObject proxyObject)
    {
        // add an agent if required
        ApplyAndConfigureNPCAgent(NPCObject, proxyObject);

        // if a proxy object was specified, check its name before proceeding
        if (proxyObject)
        {
            // add walking logic only if the proxy name indicates this NPC should be walking
            if (ManageProxyMapping.GetIsWalking(proxyObject))
            {
                // add the script to follow a path
                FollowRandomPath followPathOnNavMeshScript = NPCObject.AddComponent<FollowRandomPath>();
                followPathOnNavMeshScript.enabled = false;

                // add the script to update the animation based on the speed
                UpdateNPCAnimatorByState updateAnimatorScript = NPCObject.AddComponent<UpdateNPCAnimatorByState>();

                // randomly rotate the person since it'll be walking toward a random destination anyway
                Utils.GeometryUtils.RandomRotateGameObjectAboutY(NPCObject);
            }
            // otherwise, this NPC was specified specifically as not walking
            // so return out of this entire function
            else
            {
                return;
            }
        }
        // otherwise, this is a filler, and it can be configured to find paths 
        else
        {
            // add the script to follow a path
            FollowRandomPath followPathOnNavMeshScript = NPCObject.AddComponent<FollowRandomPath>();
            followPathOnNavMeshScript.enabled = false;

            // add the script to update the animation based on the speed
            UpdateNPCAnimatorByState updateAnimatorScript = NPCObject.AddComponent<UpdateNPCAnimatorByState>();
        }

        // ensure the agent is moved to a valid location on the navmesh

        // get the distance between the current position, and the nearest navmesh position
        float distanceToNearestNavMeshPoint = Vector3.Distance(NPCObject.transform.position, Utils.GeometryUtils.GetNearestPointOnNavMesh(NPCObject.transform.position, 5));

        //Utils.DebugUtils.DebugLog("Distance from " + NPCObject.name + " to its nearest NavMesh point: " + distanceToNearestNavMeshPoint);

        // if the distance between the current position, and the nearest navmesh position
        // is less than the max distance, use the closest point on the navmesh
        if (distanceToNearestNavMeshPoint < NPCControllerGlobals.maxDistanceForClosestPointAdjustment)
        {
            NPCObject.transform.position = Utils.GeometryUtils.GetNearestPointOnNavMesh(NPCObject.transform.position, 1000);
        }
        // otherwise, this person is probably floating in space
        // so find a totally random location on the navmesh for them to go
        else
        {
            // try to find a random point on this level, within a huge radius
            // returns the origin if the random point couldn't be found
            Vector3 randomPoint = Utils.GeometryUtils.GetRandomPointOnNavMesh(NPCObject.transform.position, 1000, true);

            NPCObject.transform.position = randomPoint;
        }
    }

    // add the typical controller, nav mesh agent, and associated scripts to a gameObject
    public static void ConfigureNPCForAnimationAndPathfinding(GameObject proxyObject, GameObject NPCObject)
    {
        // if there's a proxy object, check the proxy name for a specific animation to use
        if (proxyObject)
        {
            // in order to correctly assign gender and name using string searches,
            // combine the name of the proxy and its replacement NPC object so both sources are covered
            string combinedName = proxyObject.name + "-" + NPCObject.name;

            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(combinedName));

            ConfigureNPCForPathfinding(NPCObject, proxyObject);

            // provide a mesh collider since this won't get an agent
            if (NPCObject.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                SkinnedMeshRenderer meshRend = NPCObject.GetComponentInChildren<SkinnedMeshRenderer>();
                MeshCollider meshCollider = meshRend.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshRend.sharedMesh;
            }
        }

        // otherwise, this is a random filler person and can be configured to walk
        else
        {
            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetWalkingAnimatorControllerByGender(NPCObject.name));

            // TODO: get the initial animation to appear in the editor

            // configure the random filler person for pathfinding
            ConfigureNPCForPathfinding(NPCObject, proxyObject);
        }
    }

    // define how to instantiate proxy replacement objects
    public static void InstantiateProxyReplacements(string assetName)
    {
        // don't do anything if this isn't required
        if (proxyReplacementProcessingRequired == false)
        {
            Utils.DebugUtils.DebugLog("ProxyReplacementProcessing was not required.");
            return;
        }

        // reset the lists and counts
        instancedPrefabs.Clear();
        instancedPrefabsFromProxies.Clear();
        instancedPrefabsAsRandomFillers.Clear();
        culledPrefabs = 0;

        // update the global proxy type variable based on the asset name
        string proxyType = ManageSceneObjects.ProxyObjects.GetProxyTypeByName(assetName);

        // define the delete tag to look for, based on the proxyType, then delete existing proxy replacements
        string proxyReplacementDeleteTag = ManageTags.GetOrCreateTagByProxyType(proxyType);
        TaggedObjects.DeleteObjectsByTag(proxyReplacementDeleteTag);

        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        
        // we might get here, if so we need to return to prevent an error
        if (!gameObjectByAsset)
        {
            Utils.DebugUtils.DebugLog("Couldn't find the GameObject by name: " + assetName);
            postProcessingRequired = false;
            return;
        }

        // get all the children from the gameObject
        Transform[] allChildren = gameObjectByAsset.GetComponentsInChildren<Transform>(true); // true to include inactive children;

        // do something with each of the object's children
        foreach (Transform child in allChildren)
        {
            // these objects are visible proxy objects that need to be replaced with Unity equivalents
            if (child.name.Contains("REPLACE"))
            {
                // replace the proxy object with the replacement prefab
                GameObject instancedPrefab = ManageProxyMapping.ReplaceProxyObjectWithPrefab(child.gameObject, proxyType);

                // only do something if the instanced prefab is valid
                if (instancedPrefab)
                {
                    // add this instanced prefab to the global lists for tracking
                    instancedPrefabs.Add(instancedPrefab);
                    instancedPrefabsFromProxies.Add(instancedPrefab);

                    // special rules if we're replacing proxy people
                    if (child.name.Contains("people"))
                    {
                        // apply animator controllers, agents, and scripts to the new prefab
                        ConfigureNPCForAnimationAndPathfinding(child.gameObject, instancedPrefab);

                        // create additional random filler people around this one
                        for (var i = 0; i < ManageProxyMapping.GetNPCFillerCountBySceneName(SceneManager.GetActiveScene().name); i++)
                        {
                            // create a random point on the navmesh
                            Vector3 randomPoint = Utils.GeometryUtils.GetRandomPointOnNavMesh(child.transform.position, ProxyGlobals.fillerRadius, true);

                            // determine which pool to get people from given the scene name
                            string[] peoplePrefabPoolForCurrentScene = ManageProxyMapping.GetPeoplePrefabPoolBySceneName(SceneManager.GetActiveScene().name);

                            // only do something if the randomPoint is valid
                            if (randomPoint != Vector3.zero)
                            {
                                // instantiate a random person at the point
                                GameObject randomInstancedPrefab = ManageProxyMapping.InstantiateRandomPrefabFromPoolAtPoint(child.gameObject.transform.parent.gameObject, peoplePrefabPoolForCurrentScene, randomPoint);

                                // only do something if the prefab is valid
                                if (randomInstancedPrefab)
                                {
                                    // add this random instanced prefab to the lists for tracking
                                    instancedPrefabs.Add(randomInstancedPrefab);
                                    instancedPrefabsAsRandomFillers.Add(randomInstancedPrefab);

                                    // feed this into the NPC configurator to indicate there is no proxy to match
                                    GameObject nullProxyObject = null;

                                    // apply animator controllers, agents, and scripts to the new random prefab
                                    ConfigureNPCForAnimationAndPathfinding(nullProxyObject, randomInstancedPrefab);

                                    // tag this instanced prefab as a delete candidate for the next import
                                    randomInstancedPrefab.gameObject.tag = proxyReplacementDeleteTag;
                                }
                            }
                            // if we get a zero vector, the random point generation failed
                            else
                            {
                                Utils.DebugUtils.DebugLog("The provided point for a random filler was Vector3.zero, so no filler was instantiated.");
                            }
                        }
                    }

                    else if (child.name.Contains("tree"))
                    {
                        // randomly rotate to add visual variety
                        Utils.GeometryUtils.RandomRotateGameObjectAboutY(instancedPrefab);
                    }             
                    else if (child.name.Contains("shrub"))
                    {
                        // randomly rotate to add visual variety
                        Utils.GeometryUtils.RandomRotateGameObjectAboutY(instancedPrefab);

                        // randomly scale to add visual variety
                        Utils.GeometryUtils.ScaleGameObjectRandomlyWithinRange(instancedPrefab, 0.85f, 1.2f);

                    }
                    else if (child.name.Contains("fountain-main"))
                    {
                        // ensure the main fountain is oriented vertically
                        instancedPrefab.transform.localRotation = new Quaternion(0, 0, 0, 0);

                        // set the particle system settings

                        // main
                        ParticleSystem mainParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var mainSystem = mainParticleSystem.main;
                        mainSystem.duration = 1f;
                        mainSystem.startSpeed = 0.5f;
                        mainSystem.startLifetime = 0.5f;
                        mainSystem.startSize3D = true;
                        mainSystem.startSizeX = 0.5f;
                        mainSystem.startSizeY = 4.25f;
                        mainSystem.startSizeZ = 1f;
                        mainSystem.startColor = new Color(0.7498f, 0.7498f, 0.7498f, 0.2f);
                        mainSystem.gravityModifierMultiplier = 0.09f;
                        mainSystem.simulationSpeed = 0.3f;
                        var shape = mainParticleSystem.shape;
                        shape.rotation = new Vector3(-0.2f, 0, 0);
                        shape.enabled = true;
                        shape.angle = 0.64f;
                        shape.radius = 0f;
                        var mainSystemEmitter = mainParticleSystem.emission;
                        mainSystemEmitter.rateOverTime = 45;
                        //var mainSystemSizeOverLifetime = mainParticleSystem.sizeOverLifetime;
                        //AnimationCurve mainCurve = new AnimationCurve();
                        //mainCurve.AddKey(0.0f, 2.0f);
                        //mainCurve.AddKey(1.0f, 2.0f);
                        //mainSystemSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, mainCurve);

                        // secondary
                        ParticleSystem secondaryParticleSystem = instancedPrefab.transform.GetChild(0).GetComponent<ParticleSystem>();
                        var secondarySystem = secondaryParticleSystem.main;
                        secondarySystem.startSize3D = true;
                        secondarySystem.startSizeX = 0.86f;
                        secondarySystem.startSizeY = 1f;
                        secondarySystem.startSizeZ = 1f;
                        secondarySystem.startColor = new Color(0.725f, 0.9098f, 1.0f, 0.588f);
                        secondarySystem.startLifetime = 2.25f;
                        secondarySystem.maxParticles = 1000;
                        var secondaryShape = secondaryParticleSystem.shape;
                        secondaryShape.position = new Vector3(0, 0, 1.1f);
                        secondaryShape.rotation = new Vector3(-0.2f, 0, 0);
                        secondaryShape.angle = 50f;
                        var secondarySystemEmitter = secondaryParticleSystem.emission;
                        secondarySystemEmitter.rateOverTime = 300;
                        var secondarySystemColorOverLifetime = secondaryParticleSystem.colorOverLifetime;
                        secondarySystemColorOverLifetime.enabled = false;
                        var secondarySystemSizeOverLifetime = secondaryParticleSystem.sizeOverLifetime;
                        AnimationCurve secondaryCurve = new AnimationCurve();
                        secondaryCurve.AddKey(0.0f, 0.0f);
                        secondaryCurve.AddKey(1.0f, 2.4f);
                        secondaryCurve.SmoothTangents(0, 1.0f);
                        secondaryCurve.SmoothTangents(1, 1.0f);
                        secondarySystemSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, secondaryCurve);

                    }
                    else if (child.name.Contains("fountain-secondary"))
                    {
                        // set the particle system settings

                        // main
                        ParticleSystem mainParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var mainSystem = mainParticleSystem.main;
                        mainSystem.startSize3D = true;
                        mainSystem.startSizeX = 0.1f;
                        mainSystem.startSizeY = 0.1f;
                        mainSystem.startSizeZ = 0.1f;
                        mainSystem.startLifetime = 0.7f;
                        mainSystem.startSpeed = 2.0f;
                        mainSystem.startColor = new Color(0.7498f, 0.7498f, 0.7498f, 0.1960f);
                        var mainShape = mainParticleSystem.shape;
                        mainShape.enabled = true;
                        mainShape.rotation = new Vector3(-45, child.transform.eulerAngles.y + 225, 0); // rotate to match the proxy
                        var mainSystemEmitter = mainParticleSystem.emission;
                        mainSystemEmitter.enabled = false;
                        mainSystemEmitter.rateOverTime = 300;

                        // secondary
                        ParticleSystem secondaryParticleSystem = instancedPrefab.transform.GetChild(0).GetComponent<ParticleSystem>();
                        var secondarySystem = secondaryParticleSystem.main;
                        secondarySystem.startSize3D = true;
                        secondarySystem.startSizeX = 0.1f;
                        secondarySystem.startSizeY = 0.1f;
                        secondarySystem.startSizeZ = 0.1f;
                        secondarySystem.startLifetime = 0.7f;
                        secondarySystem.startSpeed = 4.5f;
                        secondarySystem.startColor = new Color(0.725f, 0.9098f, 1.0f, 0.588f);
                        var secondaryShape = secondaryParticleSystem.shape;
                        secondaryShape.position = new Vector3(0, 0, -2);
                        secondaryShape.enabled = true;
                        secondaryShape.rotation = new Vector3(-45, child.transform.eulerAngles.y + 225, 0); // rotate to match the proxy
                        var secondaryEmitter = secondaryParticleSystem.emission;
                        secondaryEmitter.rateOverTime = 900;
                        ParticleSystemRenderer secondarySystemRenderer = instancedPrefab.GetComponent<ParticleSystemRenderer>();
                        secondarySystemRenderer.sortingOrder = 1;
                    }
                    else if (child.name.Contains("splash-main"))
                    {
                        // main
                        ParticleSystem mainParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var mainSystem = mainParticleSystem.main;
                        mainSystem.startSize3D = true;
                        mainSystem.startSizeX = 10f;
                        mainSystem.startSizeY = 10f;
                        mainSystem.startSizeZ = 0f;
                        mainSystem.startLifetime = 1.0f;
                        mainSystem.simulationSpeed = 0.4f;
                        var mainShape = mainParticleSystem.shape;
                        mainShape.radius = 0.001f;
                        var mainShapeEmitter = mainParticleSystem.emission;
                        mainShapeEmitter.rateOverTime = 3.0f;
                        mainShape.position = new Vector3(mainShape.position.x, 0.3f, mainShape.position.z);
                        ParticleSystemRenderer mainParticleSystemRenderer = instancedPrefab.GetComponent<ParticleSystemRenderer>();
                        mainParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                        mainParticleSystemRenderer.sortingOrder = 1;

                        // secondary
                        ParticleSystem secondaryParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var secondarySystem = secondaryParticleSystem.main;
                        secondarySystem.startSize3D = true;
                        secondarySystem.startSizeX = 10f;
                        secondarySystem.startSizeY = 10f;
                        secondarySystem.startSizeZ = 0f;
                        var secondaryShape = secondaryParticleSystem.shape;
                        secondaryShape.radius = 0.001f;
                        secondaryShape.position = new Vector3(secondaryShape.position.x, 0.3f, secondaryShape.position.z);
                        ParticleSystemRenderer secondaryParticleSystemRenderer = instancedPrefab.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
                        secondaryParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                        secondaryParticleSystemRenderer.sortingOrder = 1;

                        // tertiary
                        ParticleSystem tertiaryParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var tertiarySystem = tertiaryParticleSystem.main;
                        tertiarySystem.startSize3D = true;
                        tertiarySystem.startSizeX = 10f;
                        tertiarySystem.startSizeY = 10f;
                        tertiarySystem.startSizeZ = 10f;
                        var tertiaryShape = tertiaryParticleSystem.shape;
                        tertiaryShape.radius = 0.001f;
                        tertiaryShape.position = new Vector3(tertiaryShape.position.x, 0.15f, tertiaryShape.position.z);
                        ParticleSystemRenderer tertiaryParticleSystemRenderer = instancedPrefab.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
                        tertiaryParticleSystemRenderer.enabled = false;
                        tertiaryParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;

                    }
                    else if (child.name.Contains("splash-secondary"))
                    {
                        // main
                        ParticleSystem mainParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var mainSystem = mainParticleSystem.main;
                        mainSystem.simulationSpeed = 0.3f;
                        var mainShape = mainParticleSystem.shape;
                        mainSystem.startSize3D = true;
                        mainSystem.startSizeX = 2f;
                        mainSystem.startSizeY = 2f;
                        mainSystem.startSizeZ = 2f;
                        mainShape.radius = 0.001f;
                        mainShape.position = new Vector3(mainShape.position.x, 0.1f, mainShape.position.z);
                        ParticleSystemRenderer mainParticleSystemRenderer = instancedPrefab.GetComponent<ParticleSystemRenderer>();
                        mainParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                        mainParticleSystemRenderer.sortingOrder = 1;

                        // secondary
                        ParticleSystem secondaryParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var secondarySystem = secondaryParticleSystem.main;
                        var secondaryShape = secondaryParticleSystem.shape;
                        secondaryShape.radius = 0.001f;
                        secondaryShape.position = new Vector3(secondaryShape.position.x, 0.2f, secondaryShape.position.z);
                        ParticleSystemRenderer secondaryParticleSystemRenderer = instancedPrefab.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
                        secondaryParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                        secondaryParticleSystemRenderer.sortingOrder = 1;

                        // tertiary
                        ParticleSystem tertiaryParticleSystem = instancedPrefab.GetComponent<ParticleSystem>();
                        var tertiarySystem = tertiaryParticleSystem.main;
                        var tertiaryShape = tertiaryParticleSystem.shape;
                        tertiaryShape.radius = 0.001f;
                        tertiaryShape.position = new Vector3(tertiaryShape.position.x, 0f, tertiaryShape.position.z);
                        ParticleSystemRenderer tertiaryParticleSystemRenderer = instancedPrefab.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
                        tertiaryParticleSystemRenderer.enabled = false;
                        tertiaryParticleSystemRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;

                    }

                    //Utils.DebugUtils.DebugLog("Position: " + instancedPrefab.transform.GetChild(0).position);

                    // some proxies may have been placed at the origin, meaning we couldn't
                    // find a good random spot for them - so they should be deleted
                    // use the max proxy-to-origin tolerance to ensure values very close to 0 are considered
                    Transform transformToTest = instancedPrefab.transform.childCount > 0 ? instancedPrefab.transform.GetChild(0) : instancedPrefab.transform;
                    if (Math.Abs(transformToTest.position.x) < ProxyGlobals.proxyOriginTolerance
                        && Math.Abs(transformToTest.position.z) < ProxyGlobals.proxyOriginTolerance)
                    {
                        Utils.DebugUtils.DebugLog("<b>Culling </b>" + instancedPrefab.name + " because it was found at the world origin.");

                        // count this as a culled instance
                        culledPrefabs++;
                        // delete the object
                        UnityEngine.GameObject.DestroyImmediate(instancedPrefab);
                    }

                    // nest the instanced prefab as a child of the original proxy object,
                    // but only if it wasn't culled already
                    // this needs to happen at the very end because otherwise the culling fails due to transform interference
                    if (instancedPrefab)
                    {
                        instancedPrefab.transform.parent = child.transform;
                    }
                }

                // ensure this process only runs once for each proxy object
                // by setting the flag to false
                proxyReplacementProcessingRequired = false;
            }

            // camera thumbnail objects - these are used to generate thumbnails
            // also import cameras for historic photographs so we can jump to them
            else if (child.name.Contains(ManageCameraActions.CameraActionGlobals.thumbnailCameraKeyword) || child.name.Contains(ManageCameraActions.CameraActionGlobals.historicPhotoCameraKeyword))
            {
                // get the current scene
                Scene currentScene = EditorSceneManager.GetActiveScene();

                // need to manipulate the default FormIt Group/Instance name to remove the digits at the end

                // first, remove the characters after the last hyphen
                string tempName = child.name.Remove(child.name.LastIndexOf("-"), child.name.Length - child.name.LastIndexOf("-"));
                // then remove the characters after the last hyphen again
                string cameraName = tempName.Remove(tempName.LastIndexOf("-"), tempName.Length - tempName.LastIndexOf("-"));

                // create a new object to host the camera
                // include the name of the current scene (assumes reimporting into the correct scene)
                GameObject cameraObject = new GameObject(cameraName);

                // create a camera
                var camera = cameraObject.AddComponent<Camera>();

                // configure the camera to work with PostProcessing
                camera.renderingPath = RenderingPath.DeferredShading;
                camera.allowMSAA = false;
                camera.useOcclusionCulling = false;

                // match the position and rotation
                cameraObject.transform.SetPositionAndRotation(child.transform.position, child.transform.rotation);
                // make the camera a sibling of the original camera geometry
                cameraObject.transform.parent = child.transform.parent;

                // tag this camera object as a delete candidate for the next import
                cameraObject.tag = proxyReplacementDeleteTag;

                // make the camera look at the plane
                // this assumes the only child of the camera is a plane (a FormIt Group with LCS at the center of the plane)
                cameraObject.transform.LookAt(child.GetChild(1).transform.position);

                // copy the PostProcessing effects from the Main Camera
                // this only applies to "real" FPS scenes, not the main menu, so exclude main menu
                if (SceneManager.GetActiveScene().name != "MainMenu")
                {
                    PostProcessVolume existingVolume = Camera.main.GetComponent<PostProcessVolume>();
                    PostProcessLayer existingLayer = Camera.main.GetComponent<PostProcessLayer>();
                    CopyComponent<PostProcessVolume>(existingVolume, cameraObject);
                    CopyComponent<PostProcessLayer>(existingLayer, cameraObject);
                }

                // disable the camera to prevent performance issues
                camera.enabled = false;
            }
        }

        // log how many prefabs were successfully instanced
        Utils.DebugUtils.DebugLog("Number of successfully instanced prefabs: " + (instancedPrefabs.Count - culledPrefabs) + " (" + culledPrefabs + " culled), including " + instancedPrefabsFromProxies.Count + " from proxy objects, and " + instancedPrefabsAsRandomFillers.Count + " as random fillers.");
    }

    // copies a component and all its settings from one GameObject to another
    static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    // configure and bake the nav mesh
    public static void RebuildNavMesh()
    {
        // TODO: none of this seems to work - fix it or remove it
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
    }

    // runs when a texture/image asset is updated
    void OnPreprocessTexture()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            Utils.DebugUtils.DebugLog("Skipping pre-processing texture because the pre-processor just ran.");
            return;
        }

        //ClearConsole();
        Utils.DebugUtils.DebugLog("START Texture PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        TextureImporter textureImporter = assetImporter as TextureImporter;
        String assetFilePath = textureImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog("Modified file: " + assetFilePath);

        // make the asset path available globally
        importedAssetFilePath = assetFilePath;

        // get texture import params based on name
        TextureImportParams importParams = ManageImportSettings.GetTextureImportParamsByPath(assetFilePath);

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (importParams.doSetTextureToSpriteImportSettings)
        {
            SetTextureToSpriteImportSettings(textureImporter);
        }
    }

    // runs when an audio asset is updated
    void OnPreprocessAudio()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            Utils.DebugUtils.DebugLog("Skipping pre-processing audio because the pre-processor just ran.");
            return;
        }

        ClearConsole();
        Utils.DebugUtils.DebugLog("START Audio PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        AudioImporter audioImporter = assetImporter as AudioImporter;
        String assetFilePath = audioImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog(assetFilePath);

        // make the asset path available globally
        importedAssetFilePath = assetFilePath;

        // get audio import params based on name
        AudioImportParams importParams = ManageImportSettings.GetAudioImportParamsByPath(assetFilePath);

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (importParams.doSetClipImportSettings)
        {
            SetClipImportSettings(audioImporter);
        }

    }

    // runs when a model asset is updated
    void OnPreprocessModel()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            Utils.DebugUtils.DebugLog("Skipping pre-processing model because the pre-processor just ran.");
            return;
        }

        // check if there's a leftover .fbm folder, and if so, delete it
        DeleteFBMFolderOnImport(importedAssetFileDirectory, importedAssetFileName);

        //ClearConsole();
        Utils.DebugUtils.DebugLog("START Model PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        ModelImporter modelImporter = assetImporter as ModelImporter;
        String assetFilePath = modelImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog("Modified file: " + assetFilePath);

        // make the asset path available globally
        importedAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        importedAssetFileNameAndExtension = assetFileNameAndExtension;
        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        importedAssetFileName = assetFileName;
        // get the file's directory only
        String assetFileDirectory = assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        importedAssetFileDirectory = assetFileDirectory;

        //
        // get the model import parameters based on the name
        // 

        // get the model import settings 
        AssetImportGlobals.ModelImportParamsByName = ManageImportSettings.GetModelImportParamsByName(importedAssetFilePath);

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (AssetImportGlobals.ModelImportParamsByName.doSetGlobalScale)
        {
            modelImporter.globalScale = globalScale;
        }

        if (AssetImportGlobals.ModelImportParamsByName.doInstantiateAndPlaceInCurrentScene)
        {
            InstantiateAndPlaceAssetAsGameObject(importedAssetGameObject, currentScene);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doSetColliderActive)
        {
            modelImporter.addCollider = true;
        }
        else
        {
            modelImporter.addCollider = false;
        }

        if (AssetImportGlobals.ModelImportParamsByName.doSetUVActiveAndConfigure)
        {
            SetUVActiveAndConfigure(modelImporter);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doDeleteReimportMaterialsTextures)
        {
            DeleteReimportMaterialsTextures(importedAssetFilePath);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doAddBehaviorComponents)
        {
            AddBehaviorComponentsByName(importedAssetFileName);
        }

        // since pre-processing is done, mark post-processing as required
        postProcessingRequired = true;
        proxyReplacementProcessingRequired = AssetImportGlobals.ModelImportParamsByName.doInstantiateProxyReplacements;

        // mark the scene as dirty, in case something changed that needs to be saved
        // TODO: create a flag that's set to true when something is really changed, so we only mark dirty when necessary
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // runs after pre-processing the model
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // check if there's a leftover .fbm folder, and if so, delete it
        DeleteFBMFolderOnImport(importedAssetFileDirectory, importedAssetFileName);

        // if post processing isn't required, skip
        if (!postProcessingRequired)
        {
            Utils.DebugUtils.DebugLog("Skipping PostProcessing (not required)");
            return;
        }

        // it seems that a few post processing hits are needed to fully post-process everything
        // any further is probably not necessary
        if (!postProcessingRequired || postProcessingHits.Count >= globalMaxPostProcessingHits)
        {
            Utils.DebugUtils.DebugLog("Skipping PostProcessing attempt " + postProcessingHits.Count + " (max allowed: " + globalMaxPostProcessingHits + ")");
            return;
        }

        Utils.DebugUtils.DebugLog("START PostProcessing...");

        // add to the list of post processing hits, so we know how many times we've been here
        postProcessingHits.Add(true);

        // get the game object and its dependencies
        // for some reason this needs to happen in the post-processor
        importedAssetGameObject = (GameObject)AssetDatabase.LoadAssetAtPath(importedAssetFilePath, typeof(GameObject));
        importedAssetGameObjectDependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { importedAssetGameObject });

        //
        // execute all AssetImportUpdate PostProcessor option flags marked as true
        //

        if (AssetImportGlobals.ModelImportParamsByName.doSetMaterialEmission)
        {
            SetAllDependentMaterialsEmissionByName(importedAssetGameObject);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doSetMaterialSmoothnessMetallic)
        {
            SetAllDependentMaterialsSmoothnessMetallicByName(importedAssetGameObject);
        }

        // turn off specular on certain materials like concrete, asphalt, etc
        SetAllDependentMaterialsSpecularByName(importedAssetGameObject);

        if (AssetImportGlobals.ModelImportParamsByName.doInstantiateProxyReplacements)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            InstantiateProxyReplacements(importedAssetFileName);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doHideInitially)
        {
            ManageSceneObjects.ObjectState.ToggleSceneObjectToState(importedAssetGameObject, false);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doHideProxyObjects)
        {
            ManageSceneObjects.ProxyObjects.ToggleProxyHostMeshesToState(importedAssetGameObject, false, false);
        }

        // these never worked reliably, so they've been deprecated (now invoked manually via CCP menu)
        // but keeping them around if they can be used again in the future
        if (AssetImportGlobals.ModelImportParamsByName.doRebuildNavMesh)
        {
            RebuildNavMesh();
        }

        if (AssetImportGlobals.ModelImportParamsByName.doSetCustomLightmapSettings)
        {
            SetCustomLightmapSettingsByName(importedAssetGameObject);
        }

        if (AssetImportGlobals.ModelImportParamsByName.doSetStaticFlags)
        {
            SetStaticFlagsByName(importedAssetGameObject);
        }

        // newly-instantiated objects need to be set as a child of the scene container
        // NOTE: this assumes each scene only has 1 top-level object: a "Container" that holds all Scene objects
        if (isNewlyInstantiatedFBXContainer)
        {
            SetObjectAsChildOfSceneContainer(newlyInstantiatedFBXContainer);
        }

        Utils.DebugUtils.DebugLog("END PostProcessing");
    }
}