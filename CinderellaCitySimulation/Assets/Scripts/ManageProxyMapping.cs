using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ProxyGlobals
{
    // the number of random filler people to be generated per imported proxy person
    public static int numberOfFillersToGenerate = 5;

    // the radius for filler people from the original person's position
    public static int fillerRadius = 60;

    // the folder path to find people prefabs
    public static string peoplePrefabFolderPath = "Assets/Citizens PRO/People Prefabs";

    // define the pool of people prefabs available to each scene
    public static string[] peoplePrefabPool60s70s = new string[] {
        peoplePrefabFolderPath + "/Female/Summer/business02_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual01_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual04_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual08_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual09_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual10_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_11.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_22.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_33.prefab",
        peoplePrefabFolderPath + "/Female/Summer/granny01.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual16_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual25_f_highpoly.prefab",

        peoplePrefabFolderPath + "/Male/Summer/business01_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business06_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual09_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual17_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual18_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual23_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual28_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual29_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_33.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_44.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_55.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_99.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual08_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual14_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual21_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual26_m_highpoly.prefab",
    };
    public static string[] peoplePrefabPool80s90s = new string[] {
        peoplePrefabFolderPath + "/Female/Summer/business03_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/business04_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual11_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual12_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual14_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual15_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/granny02.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual02_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual03_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual16_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual18_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual23_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual24_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual26_f_highpoly.prefab",

        peoplePrefabFolderPath + "/Male/Summer/business02_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business03_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual02_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual04_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual12_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual13_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual15_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual17_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual22_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual24_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual25_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual27_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual31_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business07_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual06_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual07_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual11_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual16_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual20_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual32_m_highpoly.prefab",
    };
}

public class ManageProxyMapping
{
    // get the proxy pool based on the scene
    public static string[] GetPeoplePrefabPoolBySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case string name when name.Contains("60s70s"):
                return ProxyGlobals.peoplePrefabPool60s70s;
            case string name when name.Contains("80s90s"):
                return ProxyGlobals.peoplePrefabPool80s90s;
            case string name when name.Contains("Experimental"):
                return ProxyGlobals.peoplePrefabPool60s70s;
            default:
                return null;
        }
    }

    // define a replacement path by a name
    public static string GetProxyReplacementPathByName(string objectName)
    {
        string replacementObjectPath = "";

        //
        // TREES
        //

        if (objectName.Contains("tree-center-court"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/TreesVariety/oak/oak 3.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("tree-center-court-small"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/TreesVariety/birch/birch 2.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("tree-shamrock"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/TreesVariety/birch/birch 5.prefab";

            return replacementObjectPath;
        }

        // 
        // PEOPLE
        //

        if (objectName.Contains("people-aaron"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/Citizens PRO/People Prefabs/Male/Winter/casual11_m_highpoly.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("people-ginger-marcus"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/Citizens PRO/People Prefabs/Female/Winter/casual18_f_highpoly.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("people-lindsey"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/Citizens PRO/People Prefabs/Female/Winter/casual03_f_highpoly.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("people-denise"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/Citizens PRO/People Prefabs/Female/Summer/casual14_f_highpoly.prefab";

            return replacementObjectPath;
        }

        if (objectName.Contains("people-dale"))
        {
            // identify the path of the prefab to replace this object
            replacementObjectPath = "Assets/Citizens PRO/People Prefabs/Male/Summer/casual24_m_highpoly.prefab";

            return replacementObjectPath;
        }

        else
        {
            return "";
        }
    }

    // instantiate a random prefab from a pool at a specific point 
    public static GameObject InstantiateRandomPrefabFromPoolAtPoint(GameObject parent, string[] prefabFilePathPool, Vector3 destinationPoint)
    {
        // get a random item from the prefab path pool
        string prefabFilePath = prefabFilePathPool[Random.Range(0, prefabFilePathPool.Length)];

        // create a new gameObject for the new asset
        GameObject newObject = (GameObject)AssetDatabase.LoadAssetAtPath(prefabFilePath, typeof(GameObject));

        // create an instance from the prefab
        GameObject instancedPrefab = PrefabUtility.InstantiatePrefab(newObject as GameObject) as GameObject;

        // if instancing the prefab was successful, adjust its position and parent
        if (instancedPrefab)
        {
            Debug.Log("<b>Instanced this prefab: </b>" + instancedPrefab);

            // move the prefab to the specified location
            instancedPrefab.transform.position = destinationPoint;

            // nest the prefab below the given parent
            instancedPrefab.transform.parent = parent.transform;
        }

        return instancedPrefab;
    }

    // replace a proxy with a prefab
    public static GameObject ReplaceProxyObjectWithPrefab(GameObject proxyObject, string proxyType)
    {
        // define the delete tag to look for, based on the proxyType, then delete existing proxy replacements
        string proxyReplacementDeleteTag = ManageTags.GetOrCreateTagByProxyType(proxyType);

        // instantiate the instanced prefab
        GameObject instancedPrefab;

        GameObject gameObjectToBeReplaced = proxyObject.gameObject;
        Debug.Log("Found a proxy gameObject to be replaced: " + gameObjectToBeReplaced);

        // define the proxy replacement path by the proxyObject object's name
        string replacementObjectPath = GetProxyReplacementPathByName(proxyObject.name);

        // ensure the object we want to replace is visible, so we can measure it
        ToggleObjects.ToggleGameObjectOn(gameObjectToBeReplaced);

        // create a new gameObject for the new asset
        GameObject newObject = (GameObject)AssetDatabase.LoadAssetAtPath(replacementObjectPath, typeof(GameObject));

        // create an instance from the prefab
        instancedPrefab = PrefabUtility.InstantiatePrefab(newObject as GameObject) as GameObject;

        // if instancing the prefab was successful, move it to the same location as the proxy
        if (instancedPrefab)
        {
            Debug.Log("<b>Instanced this prefab: </b>" + instancedPrefab);

            // give the new prefab the same parent, position, and scale as the proxy
            instancedPrefab.transform.parent = gameObjectToBeReplaced.transform.parent;
            instancedPrefab.transform.SetPositionAndRotation(gameObjectToBeReplaced.transform.localPosition, gameObjectToBeReplaced.transform.localRotation);
            instancedPrefab.transform.localScale = gameObjectToBeReplaced.transform.localScale;
            // further scale the new object to match the proxy's height
            Utils.GeometryUtils.ScaleToMatchHeight(instancedPrefab, gameObjectToBeReplaced);

            // ensure the new prefab rotates about the traditional Z (up) axis to match its proxy 
            instancedPrefab.transform.localEulerAngles = new Vector3(0, gameObjectToBeReplaced.transform.localEulerAngles.y, 0);

            // tag this instanced prefab as a delete candidate for the next import
            instancedPrefab.gameObject.tag = proxyReplacementDeleteTag;
        }
        else
        {
            Debug.Log("This prefab is null.");
        }
        return instancedPrefab;
    }
}

