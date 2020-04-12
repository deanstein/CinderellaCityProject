using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;


public class ToggleVisibilityByScene : MonoBehaviour {

    // toggles all scene objects on, except those tagged with any sort of script host tag
    public static void ToggleSceneObjectsOnExceptScriptHosts(string sceneName)
    {
        // first, turn all the script hosts off
        ToggleScriptHostObjectListOff();

        // turn everything else on
        ToggleSceneObjectsOn(sceneName);
    }

    public static void ToggleScriptHostObjectListOn()
    {
        // disable the script host objects for each of the host types given
        foreach (GameObject[] scriptHostObjectArray in ManageTags.TagGlobals.scriptHostObjects)
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
        foreach (GameObject[] scriptHostObjectArray in ManageTags.TagGlobals.scriptHostObjects)
        {
            foreach (GameObject scriptHostObject in scriptHostObjectArray)
            {
                scriptHostObject.SetActive(false);
            }
        }
    }

    // toggles all scene objects on
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

    // toggles all scene objects off
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

    // toggles the "fromScene" off, and toggles the "toScene" on
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

    // toggles scenes, and also relocates the FPSCharacter to match a Camera position
    public static void ToggleFromSceneToSceneRelocatePlayerToCamera(string fromScene, string toScene, string cameraPartialName)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // also relocate and align the FPS controller to the requested camera partial name
        ManageFPSControllers.RelocateAlignFPSControllerToCamera(cameraPartialName);
    }

    // toggles scenes, and also relocates the FPSCharacter to match another FPSCharacter (time-traveling)
    public static void ToggleFromSceneToSceneRelocatePlayerToFPSController(string fromScene, string toScene, Transform FPSControllerTransformToMatch)
    {
        // first, switch to the requested scene
        ToggleFromSceneToScene(fromScene, toScene);

        // then relocate and align the current FPSController to the referring FPSController
        ManageFPSControllers.RelocateAlignFPSControllerToFPSController(FPSControllerTransformToMatch);
    }

    // toggles scenes and relocates player to another FPSCharacter (time-traveling), with a camera effect transition
    public static IEnumerator ToggleFromSceneToSceneWithTransition(string fromScene, string toScene, Transform FPSControllerTransformToMatch, GameObject postProcessHost, string transitionProfileName, float transitionTime)
    {
        // get the PostProcessing Host's current profile so we can return to it
        string currentProfileName = postProcessHost.GetComponent<PostProcessVolume>().profile.name;

        // first, toggle the flash transition
        ManageCameraEffects.SetPostProcessTransitionProfile(postProcessHost, transitionProfileName);

        // wait for the transition time
        yield return new WaitForSeconds(transitionTime);

        // reset the profile to the original
        ManageCameraEffects.SetPostProcessProfile(postProcessHost, currentProfileName);

        // toggle to the requested scene
        ToggleFromSceneToSceneRelocatePlayerToFPSController(fromScene, toScene, ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);
    }

    // toggles given scenes on in the background, captures the FPSCharacter camera, then toggles the scenes off
    public static void ToggleBackgroundScenesForCameraCapture(string[] scenes)
    {
        // toggle each of the given scenes on
        foreach (string scene in scenes)
        {
            ToggleSceneObjectsOn(scene);

            // relocate and align the current FPSController to the referring FPSController
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform);

            // capture the current FPSController camera
            CreateScreenSpaceUIElements.CaptureActiveFPSControllerCamera();
        }
    }

}

