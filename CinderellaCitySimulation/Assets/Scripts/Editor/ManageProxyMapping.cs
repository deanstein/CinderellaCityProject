using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ProxyGlobals
{
    // the average height of proxy objects, used by fillers to get a rough height mapping
    public static float averageProxyHeight = 0;

    // the number of random filler people to be generated per imported proxy person
    public static int fillerNPCsToGenerateExperimental = 0;
    public static int fillerNPCsToGenerate60s70s = 1;
    public static int fillerNPCsToGenerate80s90s = 1;

    // the radius for filler people from the original person's position
    public static int fillerRadius = 10;

    // the folder path to find people prefabs
    public static string peoplePrefabFolderPath = "Assets/Citizens PRO/People Prefabs";

    // define the pool of people prefabs available to each scene
    public static string[] peoplePrefabPool60s70s = new string[] {
        // female
        peoplePrefabFolderPath + "/Female/Summer/business02_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/business03_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/business05_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual01_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual04_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual08_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual09_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual10_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual20_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_11.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_22.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_33.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_44.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_66.prefab",
        peoplePrefabFolderPath + "/Female/Summer/granny01.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual17_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual25_f_highpoly.prefab",
        // POC - may be intentionally duplicated here to increase likelihood of being chosen
        peoplePrefabFolderPath + "/Female/Winter/casual16_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual23_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual23_f_highpoly.prefab",

        // male
        peoplePrefabFolderPath + "/Male/Summer/business01_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business02_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business04_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business06_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business07_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual09_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual17_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual18_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual23_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual28_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/casual29_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_33.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_44.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_55.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_66.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_88.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_99.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual08_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual14_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual26_m_highpoly.prefab",
        // POC - may be intentionally duplicated here to increase likelihood of being chosen
        peoplePrefabFolderPath + "/Male/Summer/business05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual21_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual21_m_highpoly.prefab",
    };
    public static string[] peoplePrefabPool80s90s = new string[] {
        // female
        peoplePrefabFolderPath + "/Female/Summer/business04_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual05_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual11_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual12_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual14_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual15_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/casual21_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_55.prefab",
        peoplePrefabFolderPath + "/Female/Summer/Girl_77.prefab",
        peoplePrefabFolderPath + "/Female/Summer/granny02.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual02_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual03_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual18_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual23_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual26_f_highpoly.prefab",
        // POC - may be intentionally duplicated here to increase likelihood of being chosen
        peoplePrefabFolderPath + "/Female/Winter/casual16_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual24_f_highpoly.prefab",
        peoplePrefabFolderPath + "/Female/Winter/casual24_f_highpoly.prefab",

        // male
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
        peoplePrefabFolderPath + "/Male/Summer/Man_22.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_111.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual06_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual10_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual11_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual16_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual19_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual20_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual31_m_highpoly.prefab",
        // POC - may be intentionally duplicated here to increase likelihood of being chosen
        peoplePrefabFolderPath + "/Male/Summer/Man_11.prefab",
        peoplePrefabFolderPath + "/Male/Summer/Man_11.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual07_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Winter/casual07_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business05_m_highpoly.prefab",
        peoplePrefabFolderPath + "/Male/Summer/business05_m_highpoly.prefab",
    };
}

public class ManageProxyMapping
{
    // determine whether navigation/pathfinding components should be applied to the proxy object
    public static bool GetIsWalking(GameObject proxyObject)
    {
        bool isWalking = (!proxyObject.name.Contains("talking") && !proxyObject.name.Contains("idle") && !proxyObject.name.Contains("sitting") && !proxyObject.name.Contains("listening"));

        return isWalking;
    }

    // get the number of fillers to generate based on the scene
    public static int GetNPCFillerCountBySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case string name when name.Contains("Experimental"):
                return ProxyGlobals.fillerNPCsToGenerateExperimental;
            case string name when name.Contains("60s70s"):
                return ProxyGlobals.fillerNPCsToGenerate60s70s;
            case string name when name.Contains("80s90s"):
                return ProxyGlobals.fillerNPCsToGenerate80s90s;
            default:
                return 0;
        }
    }

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

    // get a random prefab from the correct scene pool
    public static string GetRandomPersonPrefabFilePathBySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case string name when name.Contains("60s70s"):
                return ProxyGlobals.peoplePrefabPool60s70s[Random.Range(0, ProxyGlobals.peoplePrefabPool60s70s.Length)];
            case string name when name.Contains("80s90s"):
                return ProxyGlobals.peoplePrefabPool80s90s[Random.Range(0, ProxyGlobals.peoplePrefabPool80s90s.Length)];
            case string name when name.Contains("Experimental"):
                return ProxyGlobals.peoplePrefabPool60s70s[Random.Range(0, ProxyGlobals.peoplePrefabPool60s70s.Length)];
            default:
                return null;
        }
    }

    // define a replacement path by a name
    public static string GetProxyReplacementPathByName(string objectName)
    {
        //
        // TREES
        //
        if (objectName.Contains("tree"))
        {
            switch (objectName)
            {
                case string name when name.Contains("blue-mall-palm"):
                    return "Assets/TreesVariety/plants/grape_plant dark.prefab";

                case string name when name.Contains("blue-mall-spike"):
                    return "Assets/TreesVariety/birch/birch 2.prefab";

                case string name when name.Contains("blue-mall-woody"):
                    return "Assets/TreesVariety/plants/cotton_plant 1.prefab";

                case string name when name.Contains("center-court") && !objectName.Contains("center-court-small"):
                    return "Assets/TreesVariety/oak/oak 3.prefab";

                case string name when name.Contains("center-court-small"):
                    return "Assets/TreesVariety/birch/birch 2.prefab";

                case string name when name.Contains("shamrock"):
                    return "Assets/TreesVariety/birch/birch 5.prefab";

                default:
                    return null;
            }
        }

        //
        // VEGETATION
        //
        if (objectName.Contains("shrub"))
        {
            switch (objectName)
            {
                case string name when name.Contains("shrub-1"):
                    return "Assets/TreesVariety/plants/cotton_plant 1.prefab";

                case string name when name.Contains("shrub-2"):
                    return "Assets/TreesVariety/plants/grape_plant dark.prefab"; ;

                case string name when name.Contains("hanging-planter-1"):
                    return "Assets/TreesVariety/plants/potato_plant.prefab";

                default:
                    return null;
            }
        }

        // 
        // PEOPLE
        //
        if (objectName.Contains("people"))
        {
            switch (objectName)
            {
                // associate specific people with specific prefab models
                case string name when name.Contains("aaron"):
                    return "Assets/Citizens PRO/People Prefabs/Male/Winter/casual11_m_highpoly.prefab";

                case string name when name.Contains("dale"):
                    return "Assets/Citizens PRO/People Prefabs/Male/Summer/casual24_m_highpoly.prefab";

                case string name when name.Contains("denise"):
                    return "Assets/Citizens PRO/People Prefabs/Female/Summer/casual14_f_highpoly.prefab";

                case string name when name.Contains("ginger-marcus"):
                    return "Assets/Citizens PRO/People Prefabs/Female/Winter/casual18_f_highpoly.prefab";

                case string name when name.Contains("jason-morin"):
                    return "Assets/Citizens PRO/People Prefabs/Male/Summer/casual23_m_highpoly.prefab";

                case string name when name.Contains("leda-thaler"):
                    return "Assets/Citizens PRO/People Prefabs/Female/Summer/casual04_f_highpoly.prefab";

                case string name when name.Contains("lindsey"):
                    return "Assets/Citizens PRO/People Prefabs/Female/Winter/casual03_f_highpoly.prefab";

                case string name when name.Contains("matt"):
                    return "Assets/Citizens PRO/People Prefabs/Male/Winter/casual20_m_highpoly.prefab";

                case string name when name.Contains("melanie"):
                    return "Assets/Citizens PRO/People Prefabs/Female/Summer/casual14_f_highpoly.prefab";

                case string name when name.Contains("stu-goldstein"):
                    return "Assets/Citizens PRO/People Prefabs/Male/Summer/business01_m_highpoly.prefab";

                // if the name isn't specified,
                // replace this object with a random person appropriate to this scene
                default:
                    return GetRandomPersonPrefabFilePathBySceneName(SceneManager.GetActiveScene().name);
            }
        }

        // otherwise, no proxy replacement path has been specified
        else
        {
            return null;
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

        // if instancing the prefab was successful, adjust its position, scale, and parent
        if (instancedPrefab)
        {
            Utils.DebugUtils.DebugLog("<b>Instanced this prefab: </b>" + instancedPrefab);

            // move the prefab to the specified location
            instancedPrefab.transform.position = destinationPoint;

            // scale the prefab to match a globally-available average height
            Utils.GeometryUtils.ScaleGameObjectToMaxHeight(instancedPrefab, ProxyGlobals.averageProxyHeight);

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
        Utils.DebugUtils.DebugLog("Found a proxy gameObject to be replaced: " + gameObjectToBeReplaced);

        // define the proxy replacement path by the proxyObject object's name
        string replacementObjectPath = GetProxyReplacementPathByName(proxyObject.name);
        if (replacementObjectPath == null)
        {
            Utils.DebugUtils.DebugLog("No replacement path was specified for this proxy, so it has not been replaced: " + proxyObject.name);
            return null;
        }

        // ensure the object we want to replace is visible, so we can measure it
        ToggleObjects.ToggleGameObjectOn(gameObjectToBeReplaced);

        // create a new gameObject for the new asset
        GameObject newObject = (GameObject)AssetDatabase.LoadAssetAtPath(replacementObjectPath, typeof(GameObject));

        // create an instance from the prefab
        instancedPrefab = PrefabUtility.InstantiatePrefab(newObject as GameObject) as GameObject;

        // if instancing the prefab was successful, move it to the same location as the proxy
        if (instancedPrefab)
        {
            Utils.DebugUtils.DebugLog("<b>Instanced this prefab: </b>" + instancedPrefab);

            // give the new prefab the same parent, position, and scale as the proxy
            instancedPrefab.transform.parent = gameObjectToBeReplaced.transform.parent;
            instancedPrefab.transform.position = gameObjectToBeReplaced.transform.position;
            instancedPrefab.transform.localScale = gameObjectToBeReplaced.transform.localScale;
            //Utils.DebugUtils.DebugLog("GameObject to be replaced position " + gameObjectToBeReplaced.transform.position);
            //Utils.DebugUtils.DebugLog("Instanced prefab position: " + instancedPrefab.transform.position);

            // further scale the new object to match the proxy's height
            Utils.GeometryUtils.ScaleToMatchHeight(instancedPrefab, gameObjectToBeReplaced);

            // record the scale used for filler replacements to use
            // assume that if this is zero, this is the first proxy, so start the average at this height
            if (ProxyGlobals.averageProxyHeight == 0)
            {
                ProxyGlobals.averageProxyHeight = Utils.GeometryUtils.GetMaxGOBoundingBoxDimension(proxyObject);
            }
            // otherwise, account for this proxy's height in the overall average
            else
            {
                ProxyGlobals.averageProxyHeight = (ProxyGlobals.averageProxyHeight + Utils.GeometryUtils.GetMaxGOBoundingBoxDimension(proxyObject)) / 2;
            }

            // ensure the instanced prefab rotates about the vertical axis
            // to match the orientation of the object that's being replaced
            instancedPrefab.transform.eulerAngles = new Vector3(0,  gameObjectToBeReplaced.transform.eulerAngles.y - 270, 0);

            // tag this instanced prefab as a delete candidate for the next import
            instancedPrefab.gameObject.tag = proxyReplacementDeleteTag;
        }
        else
        {
            Utils.DebugUtils.DebugLog("Failed to instantiate this prefab. Is the path valid? " + replacementObjectPath);
        }
        return instancedPrefab;
    }
}

