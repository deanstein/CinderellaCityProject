using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Retrieves and operates on objects in the scene
/// Provides access to object visibility keywords, top-level object finding, and object visibility checks
/// </summary>

public static class ObjectVisibilityGlobals
{
    // identify key words in order to toggle visibility of several objects at once
    public static string[] anchorStoreObjectKeywords = { "anchor-" };
    public static string[] ceilingObjectKeywords = { "mall-ceilings", "mall-speakers", "store-ceilings" };
    public static string[] exteriorWallObjectKeywords = { "mall-walls-detailing-exterior" };
    public static string[] floorObjectKeywords = { "mall-floors-vert", "store-floors" };
    public static string[] furnitureObjectKeywords = { "mall-furniture" };
    public static string[] historicPhotographObjectKeywords = { "proxy-cameras-photos" };
    public static string[] lightsObjectKeyword = { "mall-lights" };
    public static string[] interiorDetailingObjectKeywords = { "mall-detailing-interior","mall-flags", "store-detailing" };
    public static string[] interiorWallObjectKeywords = { "mall-walls-interior", "store-walls" };
    public static string[] peopleObjectKeywords = { "proxy-people" };
    public static string[] roofObjectKeywords = { "mall-roof" };
    public static string[] signageObjectKeywords = { "mall-signage" };
    public static string[] speakerObjectKeywords = { "speakers" };
    public static string[] vegetationObjectKeywords = { "proxy-trees-veg" };
    public static string[] waterFeatureObjectKeywords = { "proxy-water" };

    // used for updating the checkbox when historic photos are forced to opaque
    public static bool areHistoricPhotosForcedOpaque = false;
    // a list of known historic photo transparency values
    public static List<float> historicPhotoTransparencyValues = new List<float>();
}

public class ObjectVisibility
{
    public static GameObject[] GetTopLevelGameObjectByKeyword(string[] visibilityKeywords)
    {
        // start with an empty list and add to it
        List<GameObject> foundObjectsList = new List<GameObject>();

        foreach (string keyword in visibilityKeywords)
        {
            GameObject[] foundObjects = ManageScenes.GetTopLevelSceneContainerGameObjectsByName(keyword);
            if (foundObjects.Length > 0)
            {
                for (var i = 0; i < foundObjects.Length; i++)
                {
                    foundObjectsList.Add(foundObjects[i]);
                }
            }
        }

        GameObject[] foundObjectsArray = foundObjectsList.ToArray();
        return foundObjectsArray;
    }

    public static bool GetIsObjectVisible(string objectName)
    {
        bool isVisible = false;

        GameObject objectToTest = GameObject.Find(objectName);

        if (objectToTest != null)
        {
            isVisible = objectToTest.activeSelf;
        }

        return isVisible;
    }

    public static bool GetIsAnyChildObjectVisible(GameObject parentObject)
    {
        foreach (Transform child in parentObject.transform)
        {
            if (child.gameObject.activeSelf && !child.GetComponentInChildren<Camera>())// exclude cameras)
            {
                return true;
            }
        }

        return false;
    }

    public static void ToggleHistoricPhotoTransparencies(bool toggleState)
    {
        ObjectVisibilityGlobals.areHistoricPhotosForcedOpaque = toggleState;

        // were the photos disabled to begin with?
        bool setToDisabledWhenComplete = false;

        // get the top-level historic photo gameobject
        GameObject historicPhotoParentObject = GetTopLevelGameObjectByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords)[0];

        Renderer[] historicPhotoRenderers = historicPhotoParentObject.GetComponentsInChildren<Renderer>();

        // if no renderers found, the photos are disabled, 
        // so enable them temporarily to change their properties
        if (historicPhotoRenderers.Length == 0)
        {
            setToDisabledWhenComplete = true;

            ToggleObjects.ToggleGameObjectChildrenVisibility(historicPhotoParentObject);

            // find the renderers again
            historicPhotoRenderers = historicPhotoParentObject.GetComponentsInChildren<Renderer>();
        }

        for (var i = 0; i < historicPhotoRenderers.Length; i++)
        {
            // if toggle is on, record existing alpha then set to 1
            if (toggleState)
            {
                Color color = historicPhotoRenderers[i].material.color;

                // record the existing transparency value so we can reset later if requested
                ObjectVisibilityGlobals.historicPhotoTransparencyValues.Add(color.a);

                color.a = 1.0f;
                historicPhotoRenderers[i].material.color = color;
            }
            // if toggle is off, set new alpha from the recorded alpha
            else
            {
                Color color = historicPhotoRenderers[i].material.color;
                color.a = ObjectVisibilityGlobals.historicPhotoTransparencyValues[i];

                historicPhotoRenderers[i].material.color = color;
            }
        }

        if (!toggleState)
        {
            ObjectVisibilityGlobals.historicPhotoTransparencyValues.Clear();
        }

        // if the historic photos object was disabled, disable it again
        if (setToDisabledWhenComplete)
        {
            ToggleObjects.ToggleGameObjectChildrenVisibility(historicPhotoParentObject);
        }
    }
}

// this was copied from unused code, 
// but is probably useful someday
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
        foreach (string type in types)
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