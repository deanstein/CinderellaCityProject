using UnityEditor;

/// <summary>
/// Creates new Tags in the project, or returns existing Tags if available
/// </summary>

public class ManageTags
{
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
            //Utils.DebugUtils.DebugLog("TagHelper added a new tag.");
            so.Update();
        }
    }

    // gets or creates tags for script host objects, by type
    public static string GetOrCreateTagByScriptHostType(string scriptHostType)
    {
        // define the tag that will be used to hide the proxies
        string deleteScriptHostTag = TaggedObjects.TaggedObjectGlobals.scriptHostTagPrefix + scriptHostType;

        // run TagHelper to create the hide proxy tag if it doesn't exist yet
        ManageTags.AddTag(deleteScriptHostTag);

        return deleteScriptHostTag;
    }

    // gets or creates tags for proxy objects, by type
    public static string GetOrCreateTagByProxyType(string proxyType)
    {
        // define the tag that will be used to hide the proxies
        string deleteReplacementTag = TaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix + proxyType;

        // run TagHelper to create the hide proxy tag if it doesn't exist yet
        ManageTags.AddTag(deleteReplacementTag);

        return deleteReplacementTag;
    }
}