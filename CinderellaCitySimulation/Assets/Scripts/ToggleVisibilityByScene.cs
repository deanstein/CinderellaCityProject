using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class GlobalSceneVariables
{
    // when a scene change is requested, record the referring scene globally so we can switch back to it
    public static string referringScene;
}

public class ToggleVisibilityByScene : MonoBehaviour {

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
            // make this child active if it's not already
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                //Debug.Log("Toggled visibility OFF for: " + child.gameObject.name);
            }
        }
    }

    public static void ToggleFromSceneToScene(string fromScene, string toScene, bool makeActive)
    {
        Debug.Log("Toggling from Scene " + "<b>" + fromScene + "</b>" + " to Scene " + "<b>" + toScene + "</b>");

        // toggle the toScene first, to avoid any gaps in playback
        ToggleSceneObjectsOn(toScene);

        // now toggle the fromScene scene off
        ToggleSceneObjectsOff(fromScene);

        // if requested, make the toScene the active Scene
        if (makeActive)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(toScene));
        }

        // mark the referring scene globally, for other scripts to access
        GlobalSceneVariables.referringScene = fromScene;
    }

    public static void ToggleFromSceneToSceneRelocatePlayer(string fromScene, string toScene, bool makeActive, string cameraPartialName)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene, makeActive);
        
        // get all the cameras in the new scene
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

        foreach (Camera camera in cameras)
        {
            if (camera.name.Contains(cameraPartialName))
            {
                Debug.Log("Found a matching camera: " + camera.name);

                // get the current first person character and its character
                GameObject activeFPSController = ManageFPSControllers.FPSControllerGlobals.activeFPSController;

                activeFPSController.transform.position = camera.transform.position;
                activeFPSController.transform.rotation = Quaternion.LookRotation(camera.transform.forward, Vector3.up);
                activeFPSController.transform.GetComponent<FirstPersonController>().MouseReset();
            }
        }
    }
}

