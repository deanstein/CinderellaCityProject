using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ToggleVisibilityByScene : MonoBehaviour {

    public static void ToggleSceneObjectComponentsOnExcept(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility ON for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make their components inactive if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // get all components of this child object
            Component[] childComponents = child.GetComponents(typeof(Behaviour));

            foreach (Behaviour childComponent in childComponents)
            {
                if (!childComponent.enabled)
                {
                    childComponent.enabled = true;
                }
            }
        }
    }

    public static void ToggleSceneObjectComponentsOffExcept(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility OFF for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make their components inactive if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // get all components of this child object
            Component[] childComponents = child.GetComponents(typeof(Behaviour));

            foreach (Behaviour childComponent in childComponents)
            {
                if (childComponent.enabled)
                {
                    childComponent.enabled = false;
                    Debug.Log(childComponent.enabled);
                }
            }
        }
    }

    public static void ToggleSceneObjectsOn(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility ON for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make them active if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // make this child active if it's not already
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                //Debug.Log("Toggled visibility ON for: " + child.gameObject.name);
            }
        }
    }

    public static void ToggleSceneObjectsOff(string sceneName)
    {
        //Debug.Log("Toggling Scene object visibility OFF for: " + sceneName + "...");

        // each Scene should have a GameObject that contains all of the Scene objects
        // this container should be named after the Scene + "Container"
        string sceneContainerName = sceneName + "Container";

        // find the Scene's container GameObject by name
        GameObject sceneContainerObject = GameObject.Find(sceneContainerName);

        // loop through all children of the scene's container object and make them active if they're not already
        foreach (Transform child in sceneContainerObject.transform)
        {
            // make this child inactive if it's not already
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                //Debug.Log("Toggled visibility OFF for: " + child.gameObject.name);
            }
        }
    }

    public static void ToggleFromSceneToScene(string fromScene, string toScene)
    {
        Debug.Log("Toggling from Scene " + "<b>" + fromScene + "</b>" + " to Scene " + "<b>" + toScene + "</b>");

        // toggle the toScene first, to avoid any gaps in playback
        ToggleSceneObjectsOn(toScene);

        // make the toScene active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(toScene));

        // mark the referring and upcoming scenes globally, for other scripts to access
        SceneGlobals.referringScene = fromScene;
        SceneGlobals.upcomingScene = toScene;

        // now toggle the fromScene scene off
        ToggleSceneObjectsOff(fromScene);
    }

    public static void ToggleFromSceneToSceneRelocatePlayerToCamera(string fromScene, string toScene, string cameraPartialName)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // also relocate and align the FPS controller to the requested camera partial name
        ManageFPSControllers.RelocateAlignFPSControllerToCamera(cameraPartialName);
    }

    public static void ToggleFromSceneToSceneRelocatePlayerToFPSController(string fromScene, string toScene, Transform FPSControllerTransformToMatch)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // then relocate and align the current FPSController to the referring FPSController
        ManageFPSControllers.RelocateAlignFPSControllerToFPSController(FPSControllerTransformToMatch);
    }

}

