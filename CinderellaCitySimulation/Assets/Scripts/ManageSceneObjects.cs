using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Retrieves and operates on objects in the scene
/// Provides access to object visibility keywords, top-level object finding, and object visibility checks
/// </summary>

public static class ManageSceneObjects
{
    // finds a GameObject with name from an array of GameObjects
    // returns its index
    public static int FindGameObjectIndexByName(string name, GameObject[] objectsToSearch, bool exactMatch = false)
    {
        for (int i = 0; i < objectsToSearch.Length; i++)
        {
            GameObject gameObject = objectsToSearch[i];
            if (exactMatch)
            {
                if (gameObject.name == name)
                {
                    return i;
                }
            }
            else
            {
                if (gameObject.name.Contains(name))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    // finds a GameObject with a specific name from an array of gameobjects
    // less expensive than GameObject.Find if a scoped array is known
    public static GameObject FindGameObjectInArrayByName(string name, GameObject[] objectsToSearch, bool exactMatch = false)
    {
        // attempt to find the index
        int foundIndex = -1;
        foundIndex = FindGameObjectIndexByName(name, objectsToSearch, exactMatch);

        if (foundIndex != -1)
        {
            return objectsToSearch[foundIndex];
        }
        else
        {
            Debug.LogError("Failed to find GameObject " + "<i>" + name + "</i>" + " in array.");
            return null;
        }
    }


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
    // THIS MIGHT NEED TO BE USED IN EDITOR ONLY - it fails in Start() in multi-scene
    public static GameObject[] GetTopLevelSceneContainerGameObjectsByName(string objectName, bool exactName = false /* if true uses .Equals, if false uses .Contains when checking by name */)
    {
        GameObject activeSceneContainer = GetSceneContainerObject(SceneManager.GetActiveScene());
        List<GameObject> topLevelMatchingObjects = new List<GameObject>();

        foreach (Transform child in activeSceneContainer.transform)
        {
            // exactName uses .Equals for string comparison
            if (exactName)
            {
                if (child.name.Equals(objectName))
                {
                    topLevelMatchingObjects.Add(child.gameObject);
                }
            } else
            // otherwise, use .Contains for string comparison
            {
                if (child.name.Contains(objectName))
                {
                    topLevelMatchingObjects.Add(child.gameObject);
                }
            }
        }

        GameObject[] topLevelGameObjectArray = topLevelMatchingObjects.ToArray();
        if (topLevelGameObjectArray == null)
        {
            Debug.LogWarning("No top-level objects found with keyword: " + objectName);
        }

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
            //DebugUtils.DebugLog("Toggling Scene object visibility ON for: " + sceneName + "...");

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
                    //DebugUtils.DebugLog("Found a replacement: " + child.name);
                    proxyHostList.replacementObjectList.Add(child);

                    // get the rest of the children in this replacement object
                    GameObject[] replacementObjectChildrenTransforms = ManageSceneObjects.GetAllChildrenInObjectRecursively(child);
                    //Debug.Log("Number of children in this replacement object: " + replacementObjectChildrenTransforms.Length);
                    // add the children to the list as well
                    foreach (GameObject childObject in replacementObjectChildrenTransforms)
                    {
                        //DebugUtils.DebugLog("Found a replacement child: " + childObject.name);
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
                    //DebugUtils.DebugLog("Found a proxy: " + remainingChild.name);
                    proxyHostList.proxyMeshList.Add(remainingChild.gameObject);
                }
                // otherwise, this is a proxy container and should be recorded as such
                else
                {
                    //DebugUtils.DebugLog("Found a container: " + child.name);
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
            // get the proxy type of this host
            // so we can keep track of its visibility state in certain cases
            string proxyType = GetProxyTypeByName(proxyHost.name);
            if (proxyType == "CamerasPhotos")
            {
                // set the mode state so the rest of the app knows historic photos are visible
                ModeState.areHistoricPhotosVisible = desiredState;
            }

            ProxyHostList proxyHostList = GetProxyHostList(proxyHost);
            GameObject[] proxyMeshObjects = proxyHostList.proxyMeshList.ToArray();
            GameObject[] replacementObjects = proxyHostList.replacementObjectList.ToArray();

            ObjectState.ToggleSceneObjectsToState(proxyMeshObjects, desiredState);
            // only toggle the proxies to the opposite if the flag is set
            if (toggleReplacementsToOppositeState)
            {
                ObjectState.ToggleSceneObjectsToState(replacementObjects, !desiredState);
            }
        }

        // get all the thumbnail cameras in this scene
        // these were previously created from geometry-based cameras and tagged appropriately
        // so find them by tag
        public static GameObject[] GetAllThumbnailCamerasInScene()
        {
            // WARNING! this finds all cameras in all scenes, not just the current one
            // TODO: fix this (although doesn't seem to be breaking anything)
            GameObject[] allThumbnailCameras = GameObject.FindGameObjectsWithTag(TaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix + "Cameras");

            return allThumbnailCameras;
        }

        // get all historic photograph cameras
        public static GameObject[] GetAllHistoricPhotoCamerasInScene(string sceneName)
        {
            // we only want the photos in the given scene, so prepare a list for filtered results
            List<GameObject> filteredHistoricCameraObjects = new List<GameObject>();

            // this finds objects in all additively loaded scenes
            GameObject[] allHistoricCameraObjects = GameObject.FindGameObjectsWithTag(TaggedObjects.TaggedObjectGlobals.deleteProxyReplacementTagPrefix + "CamerasPhotos");

            // filter the objects for the current scene only
            // and also exclude certain photos
            foreach (GameObject filteredHistoricCameraObject in allHistoricCameraObjects)
            {
                if (filteredHistoricCameraObject.scene.name == sceneName)
                {
                    // the "Village Inn" photo is behind glass and best for custom tours
                    if (!filteredHistoricCameraObject.name.Contains("Funtastic composite")
                        // the Funtastics photos are too far inside the space
                        && !filteredHistoricCameraObject.name.Contains("Gold Mall - Village Inn"))
                    {
                        filteredHistoricCameraObjects.Add(filteredHistoricCameraObject);
                    }
                }
            }

            return filteredHistoricCameraObjects.ToArray();
        }

        // get both historic photos and thumbnail cameras
        public static GameObject[] GetCombinedCamerasInScene(string sceneName)
        {
            List<GameObject> allCameras = new List<GameObject>();
            allCameras.AddRange(GetAllHistoricPhotoCamerasInScene(sceneName));
            allCameras.AddRange(GetAllThumbnailCamerasInScene());
            return allCameras.ToArray();
        }

        // gets a curated list of historic photos for Guided Tour in RecordingMode
        // by searching a list of uncurated guidedTourObjects for specific names
        public static GameObject[] FindAllCuratedGuidedTourObjects(GameObject[] guidedTourObjects)
        {
            // get the scene from the object
            // GetActiveScene() can't be trusted here for some reason
            string sceneName = guidedTourObjects[0].scene.name;

            // the final array of curated objects
            GameObject[] finalCuratedGuidedTourObjects = new GameObject[0];
            GuidedTourObjectMeta[] curatedObjectMeta = new GuidedTourObjectMeta[0];

            // 60s70s or Experimental scene
            if (sceneName == SceneGlobals.mallEra60s70sSceneName || sceneName == SceneGlobals.experimentalSceneName)
            {
                curatedObjectMeta = new GuidedTourObjectMeta[]
                {
                    // BLUE MALL CENTRAL
                    // Blue Mall Denver exterior entrance
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Denver entrance",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // fountain - straight on
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // fountain - closer
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 2",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Blue Mall trampoline event
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall trampoline",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: true), ///// PERIODIC TIME TRAVEL! /////

                    // BLUE MALL OUTER HALLS
                    // Robin Hood
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Robin Hood interior",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Americana
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall - Americana",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Von Frellick on the stairs
                    new GuidedTourObjectMeta(
                        partialName: "Von Frellick stair",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Rich Burger
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Rich Burger",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Blue hall toward Rose
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 3",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true),
                    // reverse view
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 4",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: true),
                    // 10th anniversary
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Snack Bar",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Earcetera
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall at Rose Mall",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: true), ///// PERIODIC TIME TRAVEL! /////

                    // ROSE MALL
                    // K-G
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall 2",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // City Campus
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall City Campus",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Richman Bros
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Richman",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Cricket
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Hatch's
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Hatch's",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // The Regiment
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Regiment",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Cinema-Neusteters
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Exterior 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: true),
                    // Gano-Downs
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Gano-Downs exterior",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: true), ///// PERIODIC TIME TRAVEL! /////

                    // BLUE MALL EXTERIOR
                    // Leader entrance
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Leader Entrance",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Farrell's
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall - Farrell's 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Blue Mall exterior corner
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Exterior 2",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false), ///// PERIODIC TIME TRAVEL! /////

                    // SHAMROCK MALL
                    // Tommy Wong's
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Mall Tommy Wong's",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Kiddie Shop
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Kiddie Shop",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true), ///// PERIODIC TIME TRAVEL! /////

                    // CINDER ALLEY
                    // Cinder Alley from Penney's
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Candle Makers of Candles II
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley Candles",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true),
                    // store with planter
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley 2",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Cinder Alley looking through gate to alleys
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley gate",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Cinder Alley looking toward Penney's
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley far",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Cinder Alley marketing shot
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley marketing",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // GOLD MALL
                    // Gold Mall colorized
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Colorized",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),
                    // Spencer's
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Horseshoes",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // CA sign at Gold Mall
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall 2",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Stuart's
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Stuart's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // BLUE MALL OUTER HALLS
                    // Karmelkorn
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Karmelkorn",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                    // Hummell's
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Hummel's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // EXTERIOR
                    // Joslins exterior entrance
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Joslins entrance",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // RETURN TO BLUE MALL
                    // Blue Mall mezzanine
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall - mezzanine 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true),
                };

                curatedObjectMeta = new GuidedTourObjectMeta[]
                {
                    // BLUE MALL CENTRAL
                    // Atrium marketing
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall deep",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),

                    // Carousel
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall carousel 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // B&W decay
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),

                    // Wedding
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall wedding 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true),

                    // Pollard decay
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall peek",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),

                    // BLUE MALL OUTER HALLS
                    // Footlocker
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Footlocker",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Pollard west doors
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall west doors",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // ROSE MALL
                    // Lauter
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall - Lauter",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall 1",
                        doTimeTravelPeek: true,
                        doTimeTravelPeriodic: false),

                    // Thom McAn
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Thom McAn",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Woolworth's
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Woolworth's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Stride Right
                    new GuidedTourObjectMeta(
                        partialName: "Rose Mall Stride Right",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // PARKING GARAGE
                    new GuidedTourObjectMeta(
                        partialName: "Parking garage at bank",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // SHAMROCK MALL
                    // Shamrock with debris by escalator
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Mall 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Shamrock escalator toward Broadway
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock escalator",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Pollard Shamrock
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Woolworths",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Waterbeds
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Mall AWF",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Book fair
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Mall book fair",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Sports Fan
                    new GuidedTourObjectMeta(
                        partialName: "Shamrock Mall 2",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // FOOD COURT
                    // Renzios
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 9",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Sbarro
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 8",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Wendy's
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 7",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Paul Wu's
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 6",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Smoking area
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 2",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Orange Julius
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Corn Dog
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 5",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Chick-Fil-A
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 4",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // CINDER ALLEY
                    // Jazzercise
                    new GuidedTourObjectMeta(
                        partialName: "Food Court 3",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Zeezo's
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley Zeezo's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Funtastic's
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley Funtastic's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Abandoned
                    new GuidedTourObjectMeta(
                        partialName: "Cinder Alley 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // GOLD MALL
                    // Cinder Alley sign east
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall CA east stair",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Gold Mall
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall far",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Pollard Gold
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Pollard to Joslins",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Cinder Alley sign west
                    new GuidedTourObjectMeta(
                        partialName: "Gold Mall Penney's",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Hummel's
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Hummels 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // BLUE MALL
                    // Blue Mall Ward's exterior entrance
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall Exterior 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),

                    // Blue Mall mezzanine
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall mezzanine 1",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: true),

                    // Blue Mall from stair landing
                    new GuidedTourObjectMeta(
                        partialName: "Blue Mall 2",
                        doTimeTravelPeek: false,
                        doTimeTravelPeriodic: false),
                };
                finalCuratedGuidedTourObjects = new GameObject[] {
                    // BLUE MALL CENTRAL
                    // Blue Mall Denver exterior entrance
                    FindGameObjectInArrayByName("Blue Mall Denver entrance", guidedTourObjects),
                    // fountain - straight on
                    FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                    // fountain - closer
                    FindGameObjectInArrayByName("Blue Mall 2", guidedTourObjects),
                    // Blue Mall event
                    FindGameObjectInArrayByName("Blue Mall trampoline", guidedTourObjects),

                    // BLUE MALL OUTER HALLS
                    // Robin Hood
                    FindGameObjectInArrayByName("Blue Mall Robin Hood interior", guidedTourObjects),
                    // Americana
                    FindGameObjectInArrayByName("Blue Mall - Americana", guidedTourObjects),
                    // Von Frellick on the stairs
                    FindGameObjectInArrayByName("Von Frellick stair", guidedTourObjects),
                    // Rich Burger
                    FindGameObjectInArrayByName("Blue Mall Rich Burger", guidedTourObjects),

                    // BLUE MALL OUTER HALLS
                    // Blue hall toward Rose
                    FindGameObjectInArrayByName("Blue Mall 3", guidedTourObjects),
                    // reverse view
                    FindGameObjectInArrayByName("Blue Mall 4", guidedTourObjects),
                    // 10th anniversary
                    FindGameObjectInArrayByName("Blue Mall Snack Bar", guidedTourObjects),
                    // earcetera
                    FindGameObjectInArrayByName("Blue Mall at Rose Mall", guidedTourObjects),

                    // ROSE MALL
                    // K-G
                    FindGameObjectInArrayByName("Rose Mall 2", guidedTourObjects),
                    // City Campus
                    FindGameObjectInArrayByName("Rose Mall City Campus", guidedTourObjects),
                    // Richman Bros
                    FindGameObjectInArrayByName("Rose Mall Richman", guidedTourObjects),
                    // Cricket
                    FindGameObjectInArrayByName("Rose Mall 1", guidedTourObjects),
                    // Hatch's
                    FindGameObjectInArrayByName("Rose Mall Hatch's", guidedTourObjects),
                    // The Regiment
                    FindGameObjectInArrayByName("Rose Mall Regiment", guidedTourObjects),
                    // Cinema-Neusteters
                    FindGameObjectInArrayByName("Rose Mall Exterior 1", guidedTourObjects),
                    // Gano-Downs
                    FindGameObjectInArrayByName("Rose Mall Gano-Downs exterior", guidedTourObjects),

                    // BLUE MALL ENTRANCES
                    // Leader entrance
                    FindGameObjectInArrayByName("Rose Mall Leader Entrance", guidedTourObjects),
                    // Farrell's
                    FindGameObjectInArrayByName("Blue Mall - Farrell's 1", guidedTourObjects),
                    // Blue Mall exterior corner
                    FindGameObjectInArrayByName("Blue Mall Exterior 2", guidedTourObjects),

                    // SHAMROCK MALL
                    // Tommy Wong's
                    FindGameObjectInArrayByName("Shamrock Mall Tommy Wong's", guidedTourObjects),
                    // Kiddie Shop
                    FindGameObjectInArrayByName("Shamrock Kiddie Shop", guidedTourObjects),

                    // RETURN TO BLUE MALL
                    // fountain - straight on
                    FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                    // fountain - closer
                    FindGameObjectInArrayByName("Blue Mall 2", guidedTourObjects),

                    // CINDER ALLEY
                    // Cinder Alley from Penney's
                    FindGameObjectInArrayByName("Cinder Alley 1", guidedTourObjects),
                    // Candle Makers of Candles II
                    FindGameObjectInArrayByName("Cinder Alley Candles", guidedTourObjects),
                    // store with planter
                    FindGameObjectInArrayByName("Cinder Alley 2", guidedTourObjects),
                    // Cinder Alley looking through gate to alleys
                    FindGameObjectInArrayByName("Cinder Alley gate", guidedTourObjects),
                    // Cinder Alley looking toward Penney's
                    FindGameObjectInArrayByName("Cinder Alley far", guidedTourObjects),
                    // Cinder Alley marketing shot
                    FindGameObjectInArrayByName("Cinder Alley marketing", guidedTourObjects),

                    // GOLD MALL
                    // Gold Mall colorized
                    FindGameObjectInArrayByName("Gold Mall Colorized", guidedTourObjects),
                    // Spencer's
                    FindGameObjectInArrayByName("Gold Mall Horseshoes", guidedTourObjects),
                    // CA sign at Gold Mall
                    FindGameObjectInArrayByName("Gold Mall 2", guidedTourObjects),
                    FindGameObjectInArrayByName("Gold Mall 1", guidedTourObjects),
                    // Stuart's
                    FindGameObjectInArrayByName("Gold Mall Stuart's", guidedTourObjects),

                    // BLUE MALL OUTER HALLS
                    // Karmelkorn
                    FindGameObjectInArrayByName("Blue Mall Karmelkorn", guidedTourObjects),
                    // Hummell's
                    FindGameObjectInArrayByName("Blue Mall Hummel's", guidedTourObjects),

                    // EXTERIOR
                    // Joslins exterior entrance
                    FindGameObjectInArrayByName("Gold Mall Joslins entrance", guidedTourObjects),
                    // Blue Mall Denver exterior entrance
                    FindGameObjectInArrayByName("Blue Mall Denver entrance", guidedTourObjects),

                    // BLUE MALL CENTRAL
                    // back to fountain - straight on
                    FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                    // Blue Mall mezzanine
                    FindGameObjectInArrayByName("Blue Mall - mezzanine 1", guidedTourObjects)
                };
            }
            // 80s90s scene
            else if (sceneName == SceneGlobals.mallEra80s90sSceneName)
            {
                finalCuratedGuidedTourObjects = new GameObject[] {
                    // BLUE MALL CENTRAL
                    // atrium marketing
                    FindGameObjectInArrayByName("Blue Mall deep", guidedTourObjects),
                    // carousel
                    FindGameObjectInArrayByName("Blue Mall carousel 1", guidedTourObjects),
                    // b&w decay
                    FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                    // wedding
                    FindGameObjectInArrayByName("Blue Mall wedding 1", guidedTourObjects),
                    // Pollard decay
                    FindGameObjectInArrayByName("Blue Mall peek", guidedTourObjects),

                    // BLUE MALL OUTER HALLS
                    // Footlocker
                    FindGameObjectInArrayByName("Blue Mall Footlocker", guidedTourObjects),
                    // Pollard west doors
                    FindGameObjectInArrayByName("Blue Mall west doors", guidedTourObjects),

                    // Rose Mall
                    FindGameObjectInArrayByName("Rose Mall - Lauter", guidedTourObjects),
                    FindGameObjectInArrayByName("Rose Mall 1", guidedTourObjects),
                    // Thom McAn
                    FindGameObjectInArrayByName("Rose Mall Thom McAn", guidedTourObjects),
                    // Woolworth's
                    FindGameObjectInArrayByName("Rose Mall Woolworth's", guidedTourObjects),
                    // Stride Right
                    FindGameObjectInArrayByName("Rose Mall Stride Right", guidedTourObjects),

                    // PARKING GARAGE
                    FindGameObjectInArrayByName("parking garage at bank", guidedTourObjects),

                    // SHAMROCK MALL
                    // Shamrock with debris by escalator
                    FindGameObjectInArrayByName("Shamrock Mall 1", guidedTourObjects),
                    // Shamrock escalator toward Broadway
                    FindGameObjectInArrayByName("Shamrock escalator", guidedTourObjects),
                    // Pollard Shamrock
                    FindGameObjectInArrayByName("Shamrock Woolworths", guidedTourObjects),
                    // waterbeds
                    FindGameObjectInArrayByName("Shamrock Mall AWF", guidedTourObjects),
                    // book fair
                    FindGameObjectInArrayByName("Shamrock Mall book fair", guidedTourObjects),
                    // Sports Fan
                    FindGameObjectInArrayByName("Shamrock Mall 2", guidedTourObjects),

                    // FOOD COURT
                    // Renzios
                    FindGameObjectInArrayByName("Food Court 9", guidedTourObjects),
                    // Sbarro
                    FindGameObjectInArrayByName("Food Court 8", guidedTourObjects),
                    // Wendy's
                    FindGameObjectInArrayByName("Food Court 7", guidedTourObjects),
                    // Paul Wu's
                    FindGameObjectInArrayByName("Food Court 6", guidedTourObjects),
                    // smoking area
                    FindGameObjectInArrayByName("Food Court 2", guidedTourObjects),
                    // Orange Julius
                    FindGameObjectInArrayByName("Food Court 1", guidedTourObjects),
                    // Corn Dog
                    FindGameObjectInArrayByName("Food Court 5", guidedTourObjects),
                    // Chick-Fil-A
                    FindGameObjectInArrayByName("Food Court 4", guidedTourObjects),

                    // RETURN TO BLUE MALL
                    // atrium marketing
                    FindGameObjectInArrayByName("Blue Mall deep", guidedTourObjects),
                    // carousel
                    FindGameObjectInArrayByName("Blue Mall carousel 1", guidedTourObjects),
                    // wedding
                    FindGameObjectInArrayByName("Blue Mall wedding 1", guidedTourObjects),

                    // CINDER ALLEY
                    // jazzercise
                    FindGameObjectInArrayByName("Food Court 3", guidedTourObjects),
                    // Zeezo's
                    FindGameObjectInArrayByName("Cinder Alley Zeezo's", guidedTourObjects),
                    // Funtastic's
                    FindGameObjectInArrayByName("Cinder Alley Funtastic's", guidedTourObjects),
                    // abandoned
                    FindGameObjectInArrayByName("Cinder Alley 1", guidedTourObjects),

                    // GOLD MALL
                    // Cinder Alley sign east
                    FindGameObjectInArrayByName("Gold Mall CA east stair", guidedTourObjects),
                    // Gold Mall
                    FindGameObjectInArrayByName("Gold Mall far", guidedTourObjects),
                    // Pollard Gold
                    FindGameObjectInArrayByName("Gold Mall Pollard to Joslins", guidedTourObjects),
                    // Cinder Alley sign west
                    FindGameObjectInArrayByName("Gold Mall Penney's", guidedTourObjects),
                    // Hummel's
                    FindGameObjectInArrayByName("Blue Mall Hummels 1", guidedTourObjects),

                    // BLUE MALL
                    // Blue Mall Ward's exterior entrance
                    FindGameObjectInArrayByName("Blue Mall Exterior 1", guidedTourObjects),
                    // Blue Mall mezzanine
                    FindGameObjectInArrayByName("Blue Mall mezzanine 1", guidedTourObjects),
                    // Blue Mall from stair landing
                    FindGameObjectInArrayByName("Blue Mall 2", guidedTourObjects)
                };
            }

            // clean the array in case any items weren't found (eliminate nulls)
            // return the cleaned array
            return ArrayUtils.CleanArray(finalCuratedGuidedTourObjects);
        }

        // define and get the index of the "partial path" camera depending on the scene
        // this is the camera that's used as a fallback
        // if the NavMeshAgent's current destination if returning only a partial path
        public static int FindGuidedTourPartialPathCameraIndex(GameObject[] guidedTourObjects)
        {
            // define the partial names of the cameras per era
            string partialPathCameraName60s70s = "Blue Mall 1";
            string partialPathCameraName80s90s = "Blue Mall deep";

            int finalIndex = -1;

            // get the scene from the object
            // GetActiveScene() can't be trusted here for some reason
            string sceneName = guidedTourObjects[0].scene.name;

            // 60s70s or Experimental scene
            if (sceneName == SceneGlobals.mallEra60s70sSceneName || sceneName == SceneGlobals.experimentalSceneName)
            {
                finalIndex = FindGameObjectIndexByName(partialPathCameraName60s70s, guidedTourObjects);
                Debug.Log("<b>Partial path camera found for 60s70s: " + guidedTourObjects[finalIndex].name + " at index " + finalIndex + "</b>");
            }
            else if (sceneName == SceneGlobals.mallEra80s90sSceneName)
            {
                finalIndex = FindGameObjectIndexByName(partialPathCameraName80s90s, guidedTourObjects);
                Debug.Log("<b>Partial path camera found for 80s90s: " + guidedTourObjects[finalIndex].name + " at index " + finalIndex + "</b>");
            }

            return finalIndex;
        }

        // specific guided tour locations for regression testing
        public static GameObject[] FindDebuggingGuidedTourObjects(GameObject[] guidedTourObjects)
        {
            // the final objects
            GameObject[] finalDebuggingGuidedTourObjects = new GameObject[] { guidedTourObjects[0] };
            List<GameObject> finalDebuggingGuidedTourObjectsList = new List<GameObject>();

            // trigger known cases
            // descriptions are below
            bool testCase1 = false;
            bool testCase2 = false;
            bool testCase3 = false;

            switch (true)
            {
                // guided tour would hang on image - 80s90s
                case bool _ when testCase1:
                    finalDebuggingGuidedTourObjects = new GameObject[] {
                        FindGameObjectInArrayByName("Cinder Alley Zeezo's", guidedTourObjects),
                        FindGameObjectInArrayByName("Food Court 8", guidedTourObjects),
                        FindGameObjectInArrayByName("Rose Mall Thom McAn", guidedTourObjects)
                    };
                    break;

                // guided tour would hang on trampoline image - 60s70s
                case bool _ when testCase2:
                    finalDebuggingGuidedTourObjects = new GameObject[] {
                        // fountain
                        FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                        // trampoline
                        FindGameObjectInArrayByName("Blue Mall trampoline", guidedTourObjects),
                    };
                    break;

                // agent approaching camera from behind - camera should not rotate up
                case bool _ when testCase3:
                    finalDebuggingGuidedTourObjects = new GameObject[] {
                        // fountain
                        FindGameObjectInArrayByName("Blue Mall 1", guidedTourObjects),
                        // hallway looking west
                        FindGameObjectInArrayByName("Blue Mall 3", guidedTourObjects),
                        // hallway looking east
                        FindGameObjectInArrayByName("Blue Mall 4", guidedTourObjects)
                    };
                    break;

                default:
                    break;
            }

            // always add the partial path camera to all debugging lists
            int partialPathCameraIndex = FindGuidedTourPartialPathCameraIndex(guidedTourObjects);
            finalDebuggingGuidedTourObjectsList.Add(guidedTourObjects[partialPathCameraIndex]);

            return finalDebuggingGuidedTourObjects;
        }
    }
}

public static class ObjectVisibilityGlobals
{
    // identify key words in order to toggle visibility of several objects at once
    public static string[] anchorStoreObjectKeywords = { "anchor-" };
    public static string[] ceilingObjectKeywords = { "mall-ceilings", "mall-speakers", "store-ceilings" };
    public static string[] exteriorWallObjectKeywords = { "mall-walls-detailing-exterior" };
    public static string[] floorObjectKeywords = { "mall-floors-vert", "store-floors" };
    public static string[] floorNonWalkableKeywords = { "proxy-blocker-npc", "store-floors", "broadway-floors-vert", "cinema-neusteters-floors-vert", "denver-floors-vert", "joslins-floors-vert", "penneys-floors-vert", "wards-floors-vert" };
    public static string[] furnitureObjectKeywords = { "mall-furniture" };
    public static string[] lightsObjectKeyword = { "mall-lights" };
    public static string[] interiorDetailingObjectKeywords = { "mall-detailing-interior", "mall-flags", "store-detailing" };
    public static string[] interiorWallObjectKeywords = { "mall-walls-interior", "store-walls" };
    public static string[] roofObjectKeywords = { "mall-roof" };
    public static string[] signageObjectKeywords = { "mall-signage" };
    public static string[] speakerObjectKeywords = { "speakers" };
    public static string[] wayfindingObjectKeywords = { "mall-wayfinding" };

    public static string[] exteriorObjectKeywords = { "mall-roof", "site-parking-surface", "site-curb-gutter-sidewalk-vert", "site-roads" };
    public static List<string> exteriorObjectKeywordsList = new List<string>(exteriorObjectKeywords);

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
    public static GameObject[] GetTopLevelGameObjectsByKeyword(string[] visibilityKeywords, bool exactName = false)
    {
        // start with an empty list and add to it
        List<GameObject> foundObjectsList = new List<GameObject>();

        foreach (string keyword in visibilityKeywords)
        {
            GameObject[] foundObjects = ManageSceneObjects.GetTopLevelSceneContainerGameObjectsByName(keyword, exactName);
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

    public static void SetPeopleVisibility(bool visible)
    {
        GameObject peopleContainer = GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.peopleObjectKeywords, true)[0];
        peopleContainer.SetActive(visible);
        // set the mode state so the rest of the app knows whether people are visible
        ModeState.arePeopleVisible = visible;
    }

    public static void SetHistoricPhotosOpaque(bool toggleState)
    {
        ObjectVisibilityGlobals.areHistoricPhotosForcedOpaque = toggleState;

        // were the photos disabled to begin with?
        bool setToDisabledWhenComplete = false;

        // get the top-level historic photo gameobject
        GameObject historicPhotoParentObject = GetTopLevelGameObjectsByKeyword(ObjectVisibilityGlobals.historicPhotographObjectKeywords, true)[0];

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
            if (toggleState == true)
            {
                Color color = historicPhotoRenderers[i].material.color;

                // record the existing transparency value so we can reset later if requested
                ObjectVisibilityGlobals.historicPhotoTransparencyValues.Add(color.a);

                color.a = 1.0f;
                historicPhotoRenderers[i].material.color = color;
                MaterialUtils.SetMaterialRenderingMode(historicPhotoRenderers[i].material, MaterialUtils.RenderingMode.Opaque);
            }
            // if toggle is off, set new alpha from the recorded alpha
            else
            {
                Color color = historicPhotoRenderers[i].material.color;
                color.a = ObjectVisibilityGlobals.historicPhotoTransparencyValues[i];
                MaterialUtils.SetMaterialRenderingMode(historicPhotoRenderers[i].material, MaterialUtils.RenderingMode.Transparent);

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
    // WARNING: because this uses FindGameObjectsWithTag, 
    // this must be used IN THE EDITOR ONLY with one scene open
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
    // WARNING: because this uses FindGameObjectsWithTag,
    // this must be used IN THE EDITOR ONLY with one scene open
    public static void DeleteObjectsByTag(string tag)
    {
        // get all objects tagged already and delete them
        GameObject[] replacementsToDelete = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < replacementsToDelete.Length; i++)
        {
            DebugUtils.DebugLog("<b>Deleted an object with delete tag: </b>" + tag);
            UnityEngine.Object.DestroyImmediate(replacementsToDelete[i].gameObject);
        }
    }

    private void Start()
    {
        // needs to run the first time this scene is loaded, for scripts to access downstream
        GetScriptHostObjectsByTypes(TaggedObjectGlobals.scriptHostTypes);
    }
}