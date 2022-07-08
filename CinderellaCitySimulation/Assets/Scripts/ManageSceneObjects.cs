using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Retrieves and operates on objects in the scene
/// Provides access to object visibility keywords, top-level object finding, and object visibility checks
/// </summary>

public static class ManageSceneObjects
{
    // gets the container object for the given scene
    public static GameObject GetSceneContainerObject(Scene sceneWithContainer)
    {
        // get the root objects of the scene
        GameObject[] rootObjects = sceneWithContainer.GetRootGameObjects();

        // this assumes there's only 1 object in the scene: a container for all objects
        GameObject sceneContainer = rootObjects[0];
        return sceneContainer;
    }

    // gets the top-level children in this scene's scene container
    public static GameObject[] GetTopLevelChildrenInSceneContainer(Scene sceneWithContainer)
    {
        // get the current scene's container
        GameObject sceneContainer = ManageSceneObjects.GetSceneContainerObject(SceneManager.GetActiveScene());

        // get all the scene objects
        GameObject[] sceneObjects = ManageSceneObjects.GetAllTopLevelChildrenInObject(sceneContainer);

        return sceneObjects;
    }

    // search the top-level children in a scene container, and returns the first object matching the given name
    // likely cheaper than the default GameObject.Find() function
    public static GameObject[] GetTopLevelSceneContainerGameObjectsByName(string objectName)
    {
        GameObject activeSceneContainer = ManageSceneObjects.GetSceneContainerObject(SceneManager.GetActiveScene());
        List<GameObject> topLevelMatchingObjects = new List<GameObject>();

        foreach (Transform child in activeSceneContainer.transform)
        {
            if (child.name.Contains(objectName))
            {
                topLevelMatchingObjects.Add(child.gameObject);
            }
        }

        GameObject[] topLevelGameObjectArray = topLevelMatchingObjects.ToArray();

        return topLevelGameObjectArray;
    }

    // gets all children in the root object
    public static GameObject[] GetAllTopLevelChildrenInObject(GameObject parentObject)
    {
        List<GameObject> childrenList = new List<GameObject>();

        foreach (Transform trans in parentObject.transform)
        {
            childrenList.Add(trans.gameObject);
        }
        GameObject[] childrenObjects = childrenList.ToArray();

        return childrenObjects;
    }

    // gets all children recursively
    public static GameObject[] GetAllChildrenInObjectRecursively(GameObject parentObject)
    {
        // all transforms recursively 
        Transform[] allTransforms = parentObject.GetComponentsInChildren<Transform>(true /* include inactive */);

        List<GameObject> childrenList = new List<GameObject>();

        foreach (Transform trans in allTransforms)
        {
            // exclude self
            if (trans.gameObject != parentObject)
            {
                childrenList.Add(trans.gameObject);
            }
        }
        GameObject[] childrenObjects = childrenList.ToArray();

        return childrenObjects;
    }

    // gets the UI launcher object for the current scene
    public static GameObject GetUILauncherObject(Scene sceneWithUILauncher)
    {
        GameObject containerObject = ManageSceneObjects.GetSceneContainerObject(sceneWithUILauncher);
        GameObject UILauncherObject = null;

        for (int i = 0; i < containerObject.transform.childCount; i++)
        {
            if (containerObject.transform.GetChild(i).name.Contains("Launcher"))
            {
                UILauncherObject = containerObject.transform.GetChild(i).gameObject;
            }
        }

        return UILauncherObject;

    }

    // various functions to control visibility/active state
    public class ObjectState
    {
        // toggle a gameobject visibility to the opposite state
        // returns the new state
        public static bool ToggleSceneObject(GameObject sceneObject)
        {
            if (sceneObject.activeSelf)
            {
                sceneObject.SetActive(false);
                return false;
            }
            else
            {
                sceneObject.SetActive(true);
                return true;
            }
        }

        public static void ToggleSceneObjectToState(GameObject sceneObject, bool desiredState)
        {
            sceneObject.SetActive(desiredState);
        }

        public static void ToggleSceneObjectsToState(GameObject[] sceneObjects, bool desiredState)
        {
            foreach (GameObject gameObject in sceneObjects)
            {
                gameObject.SetActive(desiredState);
            }
        }

        // toggles all scene objects on
        public static void ToggleAllTopLevelSceneObjectsToState(string sceneName, bool desiredState)
        {
            //Utils.DebugUtils.DebugLog("Toggling Scene object visibility ON for: " + sceneName + "...");

            // find the Scene's container GameObject
            GameObject sceneContainerObject = ManageSceneObjects.GetSceneContainerObject(SceneManager.GetSceneByName(sceneName));

            // loop through all children of the scene's container object and make them
            // the desired state
            foreach (Transform child in sceneContainerObject.transform)
            {
                child.gameObject.SetActive(desiredState);
            }
        }

        // forces all children of an object to a state - recursively
        public static void ToggleChildrenSceneObjectsToStateRecursively(GameObject sceneObject, bool desiredState, bool includeSelf)
        {
            GameObject[] allChildren = ManageSceneObjects.GetAllChildrenInObjectRecursively(sceneObject);

            foreach (GameObject child in allChildren)
            {
                child.SetActive(desiredState);
            }

            if (includeSelf)
            {
                sceneObject.SetActive(desiredState);
            }
        }

        // toggles all top-level children of an object
        // toggle all children but leave the parent alone
        public static void ToggleTopLevelChildrenSceneObjects(GameObject parent)
        {
            // loop through all children of this GameObject and make them active or inactive
            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        // toggles all scene objects on, except those tagged with any sort of script host tag
        // note that this doesn't seem to be used currently
        public static void ToggleAllSceneObjectsOnExceptScriptHosts(string sceneName)
        {
            // first, turn all the script hosts off
            ToggleScriptHostObjectListOff();

            // turn everything else on
            ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(sceneName, true);
        }

        public static void ToggleScriptHostObjectListOn()
        {
            // disable the script host objects for each of the host types given
            foreach (GameObject[] scriptHostObjectArray in TaggedObjects.TaggedObjectGlobals.scriptHostObjects)
            {
                foreach (GameObject scriptHostObject in scriptHostObjectArray)
                {
                    scriptHostObject.SetActive(true);
                }
            }
        }

        public static void ToggleScriptHostObjectListOff()
        {
            // disable the script host objects for each of the host types given
            foreach (GameObject[] scriptHostObjectArray in TaggedObjects.TaggedObjectGlobals.scriptHostObjects)
            {
                foreach (GameObject scriptHostObject in scriptHostObjectArray)
                {
                    scriptHostObject.SetActive(false);
                }
            }
        }
    }

    // specific functions for proxy objects
    public class ProxyObjects
    {
        // get the replacement proxy type based on an asset name
        public static string GetProxyTypeByName(string gameObjectOrAssetName)
        {
            switch (gameObjectOrAssetName)
            {
                case string name when name.Contains("proxy-trees-veg"):
                    return "TreesVeg";
                // for now, legacy cameras are known as just cameras
                // so be sure to not include historic photo cameras
                case string name when name.Contains(ManageCameraActions.CameraActionGlobals.proxyCamerasObjectName) &&
                !name.Contains(ManageCameraActions.CameraActionGlobals.proxyCamerasPhotosObjectName):
                    return "Cameras";
                case string name when name.Contains(ManageCameraActions.CameraActionGlobals.proxyCamerasPhotosObjectName):
                    return "CamerasPhotos";
                case string name when name.Contains("proxy-people"):
                    return "People";
                case string name when name.Contains("water"):
                    return "Water";
                default:
                    return null;
            }
        }

        // define a proxy host list
        public class ProxyHostList
        {
            public List<GameObject> proxyContainerList = new List<GameObject>();
            public List<GameObject> proxyMeshList = new List<GameObject>();
            public List<GameObject> replacementObjectList = new List<GameObject>();
        }

        // get the divided list of proxy mesh/container/replacement objects from the host
        public static ProxyHostList GetProxyHostList(GameObject proxyHost)
        {
            // get all children of the parent recursively
            GameObject[] allChildren = ManageSceneObjects.GetAllChildrenInObjectRecursively(proxyHost);
            // the remaining children after replacements are found - start with all children and remove from there
            List<GameObject> remainingChildren = new List<GameObject>(allChildren);

            // create an empty list object to store the types
            ProxyHostList proxyHostList = new ProxyHostList();

            // look for any objects with a proxy replacement tag
            foreach (GameObject child in allChildren)
            {
                // first, make sure the child transform is enabled so the tag check works
                // then restore its state at the end
                bool isEnabled = child.gameObject.activeSelf;
                child.gameObject.SetActive(true);

                // all proxy objects have this prefix in the tag name
                if (child.tag.Contains(TaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix))
                {
                    //Utils.DebugUtils.DebugLog("Found a replacement: " + child.name);
                    proxyHostList.replacementObjectList.Add(child);

                    // get the rest of the children in this replacement object
                    GameObject[] replacementObjectChildrenTransforms = ManageSceneObjects.GetAllChildrenInObjectRecursively(child);
                    //Debug.Log("Number of children in this replacement object: " + replacementObjectChildrenTransforms.Length);
                    // add the children to the list as well
                    foreach (GameObject childObject in replacementObjectChildrenTransforms)
                    {
                        //Utils.DebugUtils.DebugLog("Found a replacement child: " + childObject.name);
                        proxyHostList.replacementObjectList.Add(childObject);
                    }
                }
                child.gameObject.SetActive(isEnabled);
            }

            // remove all replacement objects from the remaining list
            foreach (GameObject replacementObjectFound in proxyHostList.replacementObjectList)
            {
                remainingChildren.Remove(replacementObjectFound);
            }

            // loop through the remainder of the objects and determine if they are
            // meshes or containers
            foreach (GameObject remainingChild in remainingChildren)
            {
                // add mesh objects to the list only if they have a mesh renderer
                if (remainingChild.gameObject.GetComponent<MeshRenderer>())
                {
                    Utils.DebugUtils.DebugLog("Found a proxy: " + remainingChild.name);
                    proxyHostList.proxyMeshList.Add(remainingChild.gameObject);
                }
                // otherwise, this is a proxy container and should be recorded as such
                else
                {
                    //Utils.DebugUtils.DebugLog("Found a container: " + child.name);
                    proxyHostList.proxyContainerList.Add(remainingChild.gameObject);
                }
            }

            return proxyHostList;
        }

        // enable or disable proxy replacements
        // (will also toggle the proxy meshes to the opposite state)
        public static void ToggleProxyHostReplacementsToState(GameObject proxyHost, bool desiredState, bool toggleMeshesToOppositeState)
        {
            ProxyHostList proxyHostList = GetProxyHostList(proxyHost);

            GameObject[] proxyMeshObjects = proxyHostList.proxyMeshList.ToArray();
            GameObject[] replacementObjects = proxyHostList.replacementObjectList.ToArray();

            ManageSceneObjects.ObjectState.ToggleSceneObjectsToState(replacementObjects, desiredState);
            // only toggle the proxies to the opposite if the flag is set
            if (toggleMeshesToOppositeState)
            {
                ManageSceneObjects.ObjectState.ToggleSceneObjectsToState(proxyMeshObjects, !desiredState);
            }
        }

        // enable or disable proxy meshes
        // (will also toggle the proxy replacements to the opposite state)
        public static void ToggleProxyHostMeshesToState(GameObject proxyHost, bool desiredState, bool toggleReplacementsToOppositeState)
        {
            ProxyHostList proxyHostList = GetProxyHostList(proxyHost);

            GameObject[] proxyMeshObjects = proxyHostList.proxyMeshList.ToArray();
            GameObject[] replacementObjects = proxyHostList.replacementObjectList.ToArray();

            ManageSceneObjects.ObjectState.ToggleSceneObjectsToState(proxyMeshObjects, desiredState);
            // only toggle the proxies to the opposite if the flag is set
            if (toggleReplacementsToOppositeState)
            {
                ManageSceneObjects.ObjectState.ToggleSceneObjectsToState(replacementObjects, !desiredState);
            }
        }

        // get all the thumbnail cameras in this scene
        // these were previously created from geometry-based cameras and tagged appropriately
        // so find them by tag
        public static GameObject[] GetAllThumbnailCamerasInScene()
        {
            GameObject[] allThumbnailCameras = GameObject.FindGameObjectsWithTag(TaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix + "Cameras");

            return allThumbnailCameras;
        }

        // get all historic photograph cameras
        public static GameObject[] GetAllHistoricPhotoCamerasInScene()
        {
            GameObject historicPhotosProxyHost = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(ObjectVisibilityGlobals.historicPhotographObjectKeywords[0])[0];

            ProxyObjects.ProxyHostList proxyHostList = ManageSceneObjects.ProxyObjects.GetProxyHostList(historicPhotosProxyHost);

            GameObject[] cameraObjects = proxyHostList.replacementObjectList.ToArray();

            return cameraObjects;
        }
    }
}

// TODO: put everything below into the above class

public static class ObjectVisibilityGlobals
{
    // identify key words in order to toggle visibility of several objects at once
    public static string[] anchorStoreObjectKeywords = { "anchor-" };
    public static string[] ceilingObjectKeywords = { "mall-ceilings", "mall-speakers", "store-ceilings" };
    public static string[] exteriorWallObjectKeywords = { "mall-walls-detailing-exterior" };
    public static string[] floorObjectKeywords = { "mall-floors-vert", "store-floors" };
    public static string[] furnitureObjectKeywords = { "mall-furniture" };
    public static string[] lightsObjectKeyword = { "mall-lights" };
    public static string[] interiorDetailingObjectKeywords = { "mall-detailing-interior","mall-flags", "store-detailing" };
    public static string[] interiorWallObjectKeywords = { "mall-walls-interior", "store-walls" };
    public static string[] roofObjectKeywords = { "mall-roof" };
    public static string[] signageObjectKeywords = { "mall-signage" };
    public static string[] speakerObjectKeywords = { "speakers" };

    public static string[] peopleObjectKeywords = { "proxy-people" };
    public static string[] blockerObjectKeywords = { "proxy-blocker-npc" };
    public static string[] vegetationObjectKeywords = { "proxy-trees-veg" };
    public static string[] waterFeatureObjectKeywords = { "proxy-water" };
    public static string[] thumbnailCameraObjectKeywords = { "proxy-cameras" };
    public static string[] historicPhotographObjectKeywords = { "proxy-cameras-photos" };

    // used for updating the checkbox when historic photos are forced to opaque
    public static bool areHistoricPhotosForcedOpaque = false;
    // a list of known historic photo transparency values
    public static List<float> historicPhotoTransparencyValues = new List<float>();
}

public class ObjectVisibility
{
    public static GameObject[] GetTopLevelGameObjectsByKeyword(string[] visibilityKeywords)
    {
        // start with an empty list and add to it
        List<GameObject> foundObjectsList = new List<GameObject>();

        foreach (string keyword in visibilityKeywords)
        {
            GameObject[] foundObjects = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(keyword);
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
        GameObject historicPhotoParentObject = GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords)[0];

        Renderer[] historicPhotoRenderers = historicPhotoParentObject.GetComponentsInChildren<Renderer>();

        // if no renderers found, the photos are disabled, 
        // so enable them temporarily to change their properties
        if (historicPhotoRenderers.Length == 0)
        {
            setToDisabledWhenComplete = true;

            ManageSceneObjects.ObjectState.ToggleTopLevelChildrenSceneObjects(historicPhotoParentObject);

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
            ManageSceneObjects.ObjectState.ToggleTopLevelChildrenSceneObjects(historicPhotoParentObject);
        }
    }
}

// this was copied from unused code, 
// but is probably useful someday
public class TaggedObjects : MonoBehaviour
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