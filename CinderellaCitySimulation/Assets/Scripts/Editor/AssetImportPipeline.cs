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

    // post-processing should only happen after pre-processing
    static bool postProcessingRequired = false;

    // proxy replacement post-processing is needed only until replacements are valid and in the model
    static bool proxyReplacementProcessingRequired = true;

    // instantiate proxy strings
    static string proxyType;
    static string replacementObjectPath;

    // post-processing seems to repeat itself a lot, so set a max and keep track of how many times
    // note that if an object was just instantiated in the scene, this max hit value gets incremented by 1
    static int globalMaxPostProcessingHits = 2;
    static List<bool> postProcessingHits = new List<bool>();

    //
    // master list of option flags that any file just changed can receive
    // all option flags should default to false, except global scale
    //

    // master pre-processor option flags
    static bool doSetGlobalScale;
    static bool doInstantiateAndPlaceInCurrentScene;
    static bool doSetColliderActive;
    static bool doSetUVActiveAndConfigure;
    static bool doDeleteReimportMaterialsTextures;
    static bool doAddBehaviorComponents;

    // master post-processor option flags
    static bool doSetStatic;
    static bool doSetMaterialEmission;
    static bool doSetMaterialSmoothnessMetallic;
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
        mi.importNormals = ModelImporterNormals.Calculate;
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

        // allow additional post processing hits since this model was just instantiated
        globalMaxPostProcessingHits = globalMaxPostProcessingHits + 2;
    }

    // define how to delete and reimport materials and textures
    public void DeleteReimportMaterialsTextures(string assetFilePath)
    {
        // initialize ModelImporter
        ModelImporter importer = assetImporter as ModelImporter;

        // set reimport required initially to false
        bool reimportRequired = false;

        // get the material and texture dependencies and delete them
        //var prefab = AssetDatabase.LoadMainAssetAtPath(assetFilePath);
        //foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        foreach (var dependencyPath in AssetDatabase.GetDependencies(globalAssetFilePath, false))
        {
            //var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Debug.Log("Dependency path: " + dependencyPathString);

            // if there are materials or textures detected in the path, delete them
            // note that we also check that the path includes the file name to avoid other assets from being affected
            if ((dependencyPathString.Contains(".mat") || dependencyPathString.Contains(".jpg") || dependencyPathString.Contains(".jpeg") || dependencyPathString.Contains(".png")) && dependencyPathString.Contains(globalAssetFileName))
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
        importer.materialSearch = ModelImporterMaterialSearch.Local;

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

    // define how to create a greyscale new texture given a scale factor from 0 (black) to 1 (white)
    public static string CreateGreyscaleTexture(float scale)
    {
        // name the metallic map based on its rgb values
        string fileName = "metallicMap-" + scale + "-" + scale + "-" + scale + ".png";

        // define the path where this texture will be stored
        string filePath = globalAssetFileDirectory + "Textures/" + fileName;

        // only make a new texture if it doesn't exist yet
        // note that this texture does not get cleaned up in DeleteReimportMaterialsTextures
        // so it's likely already made and can be reusedused
        if (!AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture)))
        {
            //Debug.Log("No MetallicGlossMap detected. Creating one.");
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

            // write the texture to the file system
            UnityEngine.Windows.File.WriteAllBytes(filePath, (byte[])newTexture.EncodeToPNG());

            newTexture.Apply();
        }
        else
        {
            //Debug.Log("MetallicGlossMap texture already exists.");
        }

        return filePath;
        //return newTexture;
    }


    // define how to set material smoothness
    public static void SetMaterialSmoothness(string materialFilePath, float smoothness)
    {
        Debug.Log("<b>Set smoothness on Material: </b>" + materialFilePath);

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
        string metallicGlossTexturePath = CreateGreyscaleTexture(0.0f);
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
        string metallicGlossTexturePath = CreateGreyscaleTexture(metallic);
        Texture metallicGlossTexture = (Texture)AssetDatabase.LoadAssetAtPath(metallicGlossTexturePath, typeof(Texture));

        // set the MetallicGlossMap as the black texture
        mat.SetTexture("_MetallicGlossMap", metallicGlossTexture);

        Debug.Log("<b>Set metallic on Material: </b>" + mat);
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
    public static float GetMaxGOBoundingBoxDimension(GameObject gameObjectToMeasure)
    {
        // create an array of MeshChunks found within this GameObject so we can get the bounding box size
        MeshRenderer[] gameObjectMeshRendererArray = gameObjectToMeasure.GetComponentsInChildren<MeshRenderer>();
        //Mesh mesh = gameObjectToMeasure.GetComponent(MeshRenderer);

        //float width = GetComponent(MeshFilter).mesh.bounds.extents.x;

        // create a list to contain heights
        List<float> gameObjectMaxDimList = new List<float>();

        // for each MeshRenderer found, get the height and add it to the list
        for (int i = 0; i < gameObjectMeshRendererArray.Length; i++)
        {
            Debug.Log("Found a MeshChunk to get bounds info from: " + gameObjectMeshRendererArray[i]);

            // assume this mesh is valid, and doesn't need to be processed again
            proxyReplacementProcessingRequired = false;

            Bounds bounds = gameObjectMeshRendererArray[i].bounds;

            //Debug.Log("Bounds: " + bounds);
            float dimX = bounds.extents.x;
            float dimY = bounds.extents.y;
            float dimZ = bounds.extents.z;
            Debug.Log("Mesh dimensions for " + gameObjectMeshRendererArray[i] + dimX + "," + dimY + "," +  dimZ);

            List<float> XYZList = new List<float>();
            XYZList.Add(dimX);
            XYZList.Add(dimY);
            XYZList.Add(dimZ);

            float maxXYZ = XYZList.Max();
            Debug.Log("Max XYZ dimension for " + gameObjectMeshRendererArray[i] + ": " + maxXYZ);

            // add this height to the list of heights
            gameObjectMaxDimList.Add(maxXYZ);
            //Debug.Log(gameObjectHeightsList);
        }

        float gameObjectMaxHeight = gameObjectMaxDimList.Max();
        //Debug.Log("Max height of " + gameObjectToMeasure + ": " + gameObjectMaxHeight);

        // if the bounding box is zero, this might not be ready for measuring yet
        // so set the flag to try proxy replacement again
        if (gameObjectMaxHeight == 0)
        {
            Debug.Log("This object couldn't be measured (0 bounding box) and should be tried again next time.");
            proxyReplacementProcessingRequired = true;
        }
        return gameObjectMaxHeight;
    }

    // define how to scale one GameObject to match the height of another
    public static void ScaleToMatchHeight(GameObject gameObjectToScale, GameObject gameObjectToMatch)
    {
        // get the height of the object to be replaced
        float targetHeight = GetMaxGOBoundingBoxDimension(gameObjectToMatch);
        float currentHeight = GetMaxGOBoundingBoxDimension(gameObjectToScale);

        float scaleFactor = (targetHeight / currentHeight) * ((gameObjectToScale.transform.localScale.x + gameObjectToScale.transform.localScale.y + gameObjectToScale.transform.localScale.z) / 3);

        // scale the prefab to match the height of its replacement
        gameObjectToScale.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        Debug.Log("<b>Scaled </b>" + gameObjectToScale + " <b>to match</b> " + gameObjectToMatch + " <b>(" + scaleFactor + " scale factor)</b>");
    }


    // define how to create a component attached to a GameObject, or delete it and create a new one if component already exists
    public static void RemoveComponentIfExisting(GameObject gameObjectForComponent, string componentType)
    {
        // if this object already has a component of this type, delete it
        if (gameObjectForComponent.GetComponent(componentType) as Component)
        {
            Debug.Log("<b>Deleted</b> existing behavior component " + componentType + " on " + gameObjectForComponent + ".");
            Component componentToDelete = gameObjectForComponent.GetComponent(componentType) as Component;
            //AudioSource audioSourceToDelete = gameObjectForComponent.gameObject.GetComponent<AudioSource>();

            // delete the existing component
            UnityEngine.Object.DestroyImmediate(componentToDelete);
        }
    }

    // define how to apply components (behaviors and scripts) to an asset's GameObject in the scene
    public static void AddUnityEngineComponentsByName(String assetName, String componentType)
    {
        // find the associated GameObject by this asset's name, and all of its children objects
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        Transform[] allChildren = gameObjectByAsset.GetComponentsInChildren<Transform>();

        // generic components have a specific type
        Type unityEngineComponentType = Type.GetType("UnityEngine." + componentType + ", UnityEngine");

        // first, remove the component if it exists already
        RemoveComponentIfExisting(gameObjectByAsset, componentType);

        //
        // apply components only to the parent objects
        //

        /*
        // if the name and type match, add the component
        if (gameObjectByAsset.name.Contains("people"))
        {
            Debug.Log("<b>Added</b> " + componentType + " behavior component to " + assetName);
        }
        */

        // 
        // apply components to all the children 
        //

        foreach (Transform child in allChildren)
        {
            // first, remove the component if it exists already
            RemoveComponentIfExisting(child.gameObject, componentType);

            // if the name and type match, add the component
            if (child.name.Contains("speaker-") && componentType.Contains("AudioSource"))
            {
                child.gameObject.AddComponent(unityEngineComponentType);

                // get the audio source for this GameObject
                AudioSource audioSourceComponent = child.gameObject.GetComponent<AudioSource>();
                //Debug.Log("This audio source: " + audioSourceComponent);

                Debug.Log("<b>Added</b> " + componentType + " behavior component to " + child.name);
            }
        }
    }

    // define how to add a scriptable component to a game object
    public static void AddCustomScriptComponentsByName(String assetName, String componentType)
    {
        // custom scripts get a specific type
        Type customScriptComponentType = Type.GetType(componentType + ", Assembly-CSharp");

        // find the associated GameObject by this asset's name, and all of its children objects
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        Transform[] allChildren = gameObjectByAsset.GetComponentsInChildren<Transform>();

        //
        // apply components only to the parent objects
        //

        // first, remove the component if it exists already
        RemoveComponentIfExisting(gameObjectByAsset, componentType);

        // if the name and type match, add the component
        if ((assetName.Contains("proxy-people")) && componentType.Contains("ToggleVisibilityByShortcut"))
        {
            gameObjectByAsset.AddComponent(customScriptComponentType);
            ToggleVisibilityByShortcut ToggleVisibilityScript = gameObjectByAsset.AddComponent<ToggleVisibilityByShortcut>();
            ToggleVisibilityScript.objectType = "people";
            ToggleVisibilityScript.shortcut = "p";
            Debug.Log("<b>Added</b> " + componentType + " behavior component to " + assetName);
        }

        // 
        // apply components to all the children 
        //

        foreach (Transform child in allChildren)
        {
            // first, remove the component if it exists already
            RemoveComponentIfExisting(child.gameObject, componentType);

            // if the name and type match, add the component
            if (child.name.Contains("speaker-") && componentType.Contains("PlayAudioSequencesByName"))
            {
                child.gameObject.AddComponent(customScriptComponentType);
                Debug.Log("<b>Added</b> " + componentType + " behavior component to " + child.name);
            }
        }
    }

    // define how to add certain behavior components to certain GameObjects as defined by their host file name
    public static void AddBehaviorComponents(string fileName)
    {
        Debug.Log("Adding behavior components...");

        // these are the Unity Engine components that will be added to specific GameObjects
        // <!!!> when adding here, remember to also add this behavior and the compatible asset inside the function in the for loop <!!!>
        string[] unityEngineComponentsArray = { "AudioSource" };

        // for each of the Unity Engine components, apply them to this asset
        for (var i = 0; i < unityEngineComponentsArray.Length; i++)
        {
            AddUnityEngineComponentsByName(fileName, unityEngineComponentsArray[i]);
        }

        // these are the custom script components that will be added to specific GameObjects
        // <!!!> when adding here, remember to also add this behavior and the compatible asset inside the function in the for loop <!!!>
        string[] customScriptComponentsArray = { "PlayAudioSequencesByName", "ToggleVisibilityByShortcut" };

        // for each of the custom components, apply them to this asset
        for (var i = 0; i < customScriptComponentsArray.Length; i++)
        {
            AddCustomScriptComponentsByName(fileName, customScriptComponentsArray[i]);
        }
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

            if (dependencyPathString.Contains("wayfinding directory"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("fluorescent panel"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            if (dependencyPathString.Contains("green fluor"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.5F);
            }

            if (dependencyPathString.Contains("high intensity white"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (dependencyPathString.Contains("high intensity green"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (dependencyPathString.Contains("low intensity yellow"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("very low intensity white"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (dependencyPathString.Contains("americana shop"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (dependencyPathString.Contains("store yellowing"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -0.50F);
            }

            if (dependencyPathString.Contains("food court high intensity"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.25F);
            }

            if (dependencyPathString.Contains("food court incandescent"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            if (dependencyPathString.Contains("cinder alley incandescent"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            // temporarily reducing the brightness of these until all signage brightness can be adjusted to affset albedo boost,
            // or if using Baked Lightmap instead of Shadowmask, which will make things brighter without an albedo boost override
            if (dependencyPathString.Contains("store rtc sign") ||
                dependencyPathString.Contains("store fl runner"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }
    }

    // define how to look for materials with certain names and configure their smoothness
    public static void SetMaterialSmoothnessMetallicByName()
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

            if (dependencyPathString.Contains("drywall"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.05F);
            }

            if (dependencyPathString.Contains("glass")
                || dependencyPathString.Contains("mirror"))
            {
                SetMaterialSmoothness(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("metal") && dependencyPathString.Contains(".mat"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.5F);
                SetMaterialMetallic(dependencyPathString, 0.5F);
            }

            //
            // look for certain materials and set their smoothness and metallic values
            //

            if (dependencyPathString.Contains("mall - parquet floor"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.5F);
            }

            if (dependencyPathString.Contains("mall - polished concrete"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.45F);
            }

            if (dependencyPathString.Contains("mall - polished concrete cinder alley")
                || dependencyPathString.Contains("mall - cinder alley scored concrete"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.35F);
            }

            if (dependencyPathString.Contains("mall - shamrock floor brick"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.35F);
                SetMaterialMetallic(dependencyPathString, 0.14F);
            }

            if (dependencyPathString.Contains("mall - shamrock planter brick"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.075F);
            }

            if (dependencyPathString.Contains("mall - stair terrazzo"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.2F);
            }

            if (dependencyPathString.Contains("food court tile"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.2F);
            }

            if (dependencyPathString.Contains("mall - food court ceiling"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.6F);
            }

            if (string.IsNullOrEmpty(dependencyPathString)) continue;
        }
    }

    // define how to instantiate proxy replacement objects
    public static void InstantiateProxyReplacements(string assetName)
    {
        // don't do anything if this isn't required
        if (proxyReplacementProcessingRequired == false)
        {
            Debug.Log("ProxyReplacementProcessing was not required.");
            return;
        }

        if (assetName.Contains("cc-microcosm"))
        {
            proxyType = "Trees";
            Debug.Log("Proxy type: " + proxyType);
        }

        if (assetName.Contains("proxy-trees"))
        {
            proxyType = "Trees";
            Debug.Log("Proxy type: " + proxyType);
        }

        if (assetName.Contains("proxy-people"))
        {
            proxyType = "People";
            Debug.Log("Proxy type: " + proxyType);

            // people aren't ready yet
            return;
        }

        // define the tag that will be used to hide the proxies
        string deleteReplacementTag = "DeleteReplacement" + proxyType;

        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        var transformByAsset = gameObjectByAsset.transform;

        // run TagHelper to create the hide proxy tag if it doesn't exist yet
        TagHelper.AddTag(deleteReplacementTag);

        // get all objects tagged already and delete them
        GameObject[] replacementsToDelete = GameObject.FindGameObjectsWithTag(deleteReplacementTag);
        for (int i = 0; i < replacementsToDelete.Length; i++)
        {
            Debug.Log("<b>Deleted an object with delete tag: </b>" + replacementsToDelete[i].name);
            UnityEngine.Object.DestroyImmediate(replacementsToDelete[i].gameObject);
        }

        Transform[] allChildren = gameObjectByAsset.GetComponentsInChildren<Transform>();

        // for each of this asset's children, look for any whose name indicates they are proxies to be replaced
        foreach (Transform child in allChildren)
        {
            if (child.name.Contains("REPLACE"))
            {
                GameObject gameObjectToBeReplaced = child.gameObject;
                Debug.Log("Found a proxy gameObject to be replaced: " + gameObjectToBeReplaced);

                if (child.name.Contains("tree-center-court"))
                {
                    // identify the path of the prefab to replace this object
                    replacementObjectPath = "Assets/TreesVariety/oak/oak 3.prefab";
                }

                if (child.name.Contains("tree-center-court-small"))
                {
                    // identify the path of the prefab to replace this object
                    replacementObjectPath = "Assets/TreesVariety/birch/birch 2.prefab";
                }

                if (child.name.Contains("tree-shamrock"))
                {
                    // identify the path of the prefab to replace this object
                    replacementObjectPath = "Assets/TreesVariety/birch/birch 5.prefab";
                }

                // ensure the object we want to replace is visible, so we can measure it
                ToggleVisibility.ToggleGameObjectOn(gameObjectToBeReplaced);

                // create a new gameObject for the new asset
                GameObject newObject = (GameObject)AssetDatabase.LoadAssetAtPath(replacementObjectPath, typeof(GameObject));

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

                    // tag this instanced prefab as a delete candidate for the next import
                    instancedPrefab.gameObject.tag = deleteReplacementTag;

                    // set the prefab as static
                    //GameObject GO = GameObject.Find(instancedPrefab.name);
                    //SetAssetAsStaticGameObject(GO.name);
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

        if (assetFilePath.Contains("cc-microcosm.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
        }

        if (assetFilePath.Contains("cc-microcosm-2.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
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
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
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
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
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
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
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
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-furniture.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-floor-ceiling-vertical.fbx")
            || assetFilePath.Contains("mall-floor-ceiling-vertical-faceted.fbx")
            || assetFilePath.Contains("mall-interior-detailing.fbx")
            || assetFilePath.Contains("mall-interior-detailing-faceted.fbx")
            || assetFilePath.Contains("mall-interior-detailing-faceted-L1.fbx")
            || assetFilePath.Contains("mall-interior-detailing-faceted-L2.fbx")
            || assetFilePath.Contains("mall-interior-detailing-faceted-L3.fbx")
            || assetFilePath.Contains("mall-exterior-detailing.fbx")
            || assetFilePath.Contains("mall-exterior-detailing-faceted.fbx")
            || assetFilePath.Contains("mall-exterior-walls.fbx")
            || assetFilePath.Contains("store-interior-detailing.fbx")
            || assetFilePath.Contains("store-interior-detailing-L1.fbx")
            || assetFilePath.Contains("store-interior-detailing-L2.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
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
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-wayfinding"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-lights.fbx")
            || assetFilePath.Contains("mall-signage.fbx")
            || assetFilePath.Contains("site-lights.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("mall-structure.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("proxy-people.fbx")
            || assetFilePath.Contains("proxy-trees.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = true;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
        }

        if (assetFilePath.Contains("site.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("speakers.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        // these are temporary fixes
        if (assetFilePath.Contains("temp-rose-mall-fix.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
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

        if (doAddBehaviorComponents)
        {
            AddBehaviorComponents(globalAssetFileName);
        }

        // since pre-processing is done, mark post-processing as required
        postProcessingRequired = true;
        proxyReplacementProcessingRequired = true;
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
            Debug.Log("Skipping PostProcessing (not required)");
            return;
        }

        // it seems that a few post processing hits are needed to fully post-process everything
        // any further is probably not necessary
        if (!postProcessingRequired || postProcessingHits.Count >= globalMaxPostProcessingHits)
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

        if (doSetMaterialSmoothnessMetallic)
        {
            SetMaterialSmoothnessMetallicByName();
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