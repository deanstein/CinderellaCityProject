
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// this script needs to be attached to the UILauncher in each FPSScene
// in order to properly tabulate the objects with certain tags

public class ManageTags : MonoBehaviour
{
    public class TagGlobals
    {
        // for consistency, keep the prefix for certain tags here
        // these get appended with a specific type by other scripts
        public static string deleteProxyReplacementTagPrefix = "DeleteReplacement";
        public static string scriptHostTagPrefix = "ScriptHost";

        // store all script host objects here
        // we keep these script hosts disabled when capturing a camera in a disabled scene
        public static List<GameObject[]> scriptHostObjects = new List<GameObject[]>();

        // all the script host types we want to disable for performance reasons in some conditions
        public static string[] scriptHostTypes = new string[] { "Speakers" };
    }

    public static void AddTag(string tag)
    {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;     // Tag already present, nothing to do.
                }
            }

            tags.InsertArrayElementAtIndex(0);
            tags.GetArrayElementAtIndex(0).stringValue = tag;
            so.ApplyModifiedProperties();
            Debug.Log("TagHelper added a new tag.");
            so.Update();
        }
    }

    // gets or creates tags for script host objects, by type
    public static string GetOrCreateTagByScriptHostType(string scriptHostType)
    {
        // define the tag that will be used to hide the proxies
        string deleteScriptHostTag = ManageTags.TagGlobals.scriptHostTagPrefix + scriptHostType;

        // run TagHelper to create the hide proxy tag if it doesn't exist yet
        ManageTags.AddTag(deleteScriptHostTag);

        return deleteScriptHostTag;
    }

    // gets or creates tags for proxy objects, by type
    public static string GetOrCreateTagByProxyType(string proxyType)
    {
        // define the tag that will be used to hide the proxies
        string deleteReplacementTag = ManageTags.TagGlobals.deleteProxyReplacementTagPrefix + proxyType;

        // run TagHelper to create the hide proxy tag if it doesn't exist yet
        ManageTags.AddTag(deleteReplacementTag);

        return deleteReplacementTag;
    }

    // finds all objects with tags associated with script host types, and stores them globally
    // TODO: this may need to become a class, if we need to access script host objects by particular type
    public static void FindAndStoreAllScriptHostObjectsByTypes(string[] types)
    {
        foreach(string type in types )
        {
            string tag = TagGlobals.scriptHostTagPrefix + type;
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            TagGlobals.scriptHostObjects.Add(taggedObjects);
        }
    }

    // deletes all objects with the given tag
    public static void DeleteObjectsByTag(string tag)
    {
        // get all objects tagged already and delete them
        GameObject[] replacementsToDelete = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < replacementsToDelete.Length; i++)
        {
            Debug.Log("<b>Deleted an object with delete tag: </b>" + tag);
            UnityEngine.Object.DestroyImmediate(replacementsToDelete[i].gameObject);
        }
    }

    private void Start()
    {
        // needs to run the first time this scene is loaded, for scripts to access downstream
        FindAndStoreAllScriptHostObjectsByTypes(TagGlobals.scriptHostTypes);
    }
}