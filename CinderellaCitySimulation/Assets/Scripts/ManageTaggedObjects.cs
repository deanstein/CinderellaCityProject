
using UnityEngine;

using System.Collections.Generic;

// this script needs to be attached to the UILauncher in each FPSScene
// in order to properly tabulate the objects with certain tags

public class ManageTaggedObjects : MonoBehaviour
{
    public class TaggedObjectGlobals
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

    // finds all objects with tags associated with script host types, and stores them globally
    public static void GetScriptHostObjectsByTypes(string[] types)
    {
        foreach(string type in types )
        {
            string tag = TaggedObjectGlobals.scriptHostTagPrefix + type;
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            TaggedObjectGlobals.scriptHostObjects.Add(taggedObjects);
        }
    }

    // deletes all objects with the given tag
    public static void DeleteObjectsByTag(string tag)
    {
        // get all objects tagged already and delete them
        GameObject[] replacementsToDelete = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < replacementsToDelete.Length; i++)
        {
            Utils.DebugUtils.DebugLog("<b>Deleted an object with delete tag: </b>" + tag);
            UnityEngine.Object.DestroyImmediate(replacementsToDelete[i].gameObject);
        }
    }

    private void Start()
    {
        // needs to run the first time this scene is loaded, for scripts to access downstream
        GetScriptHostObjectsByTypes(TaggedObjectGlobals.scriptHostTypes);
    }
}