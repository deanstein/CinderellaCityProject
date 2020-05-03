using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAllScenesAsync : MonoBehaviour {

    // all scene names to load before moving on
    string[] loadSceneNames = { "MainMenu", "PauseMenu", "60s70s", "80s90s" };

    // the name of the scene to set active after all are loaded
    string setActiveSceneName = "MainMenu";

    // create a list to manage all active async operations
    static List<AsyncOperation> asyncOperations = new List<AsyncOperation>();

    // this bool gets set to True only when all scenes are loaded
    bool allLoaded;

    // initialization
    void Start ()
    {
        // start loading each scene asynchronously
        foreach (string sceneName in loadSceneNames)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Debug.Log("Started loading scene: " + sceneName);

            // collect the async operations into a list
            asyncOperations.Add(asyncOperation);
        }

        // start watching for finished signals to load MainMenu when all scenes are ready
        StartCoroutine(SetActiveSceneWhenReady());
    }
	
	// Update is called once per frame
	void Update ()
    {
        // clear and rebuild the list of bools indicating which scenes are loaded
        List<bool> scenesLoaded = new List<bool>();
        foreach (AsyncOperation asyncOperation in asyncOperations)
        {
            scenesLoaded.Add(asyncOperation.isDone);
        }

        for (var i = 0; i < asyncOperations.Count; i++)
        {
            // when the async operation is done, turn off its objects
            if (asyncOperations[i].isDone)
            {
                ToggleSceneAndUI.ToggleSceneObjectsOff(loadSceneNames[i]);
            }
        }

        // if the amount of async operations matches the scene list, and all scenes are loaded,
        // set the flag that all scenes are loaded
        if ((scenesLoaded.Count == loadSceneNames.Length) && scenesLoaded.TrueForAll(b => b))
        {
            allLoaded = true;
        }
    }

    IEnumerator SetActiveSceneWhenReady()
    {
        // wait until the flag is set to switch scenes
        yield return new WaitUntil(() => allLoaded == true);

        // set the specified scene as active, once all scenes are loaded
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(setActiveSceneName));
        ToggleSceneAndUI.ToggleSceneObjectsOn(setActiveSceneName);

        // then turn off all the Loading Screen's objects
        ToggleSceneAndUI.ToggleSceneObjectsOff("LoadingScreen");
    }
}
