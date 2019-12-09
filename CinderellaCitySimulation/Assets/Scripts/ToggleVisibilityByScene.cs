using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleVisibilityByScene : MonoBehaviour {

    public static void ToggleSceneObjectsOn(string sceneName)
    {
        Debug.Log("Toggling scene visibility ON for: " + sceneName + "...");

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
        Debug.Log("Toggling scene visibility OFF for: " + sceneName + "...");

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
}

