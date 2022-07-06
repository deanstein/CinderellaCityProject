using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads all game Scenes into memory, before anything else
/// This allows the nearly seamless transition between Scenes
/// </summary>

public class LoadAllScenesAsync : MonoBehaviour {

    // create a list to keep track of all active async operations progress
    static List<AsyncOperation> asyncOperations = new List<AsyncOperation>();

    // set to true only when all specified scenes are loaded
    bool allLoaded;

    // initialization
    void Start ()
    {
        // limit frame rate to prevent very fast computers from moving objects too quickly
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // load the specified scenes
        ManageScenes.LoadScenesAsync(SceneGlobals.scenesToLoad, asyncOperations);

        // switch to the MainMenu when all scenes are ready
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
                ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsOff(SceneGlobals.scenesToLoad[i]);
            }
        }

        // if the amount of async operations matches the scene list, and all scenes are loaded,
        // set the flag that all scenes are loaded
        if ((scenesLoaded.Count == SceneGlobals.scenesToLoad.Length) && scenesLoaded.TrueForAll(b => b))
        {
            allLoaded = true;
        }
    }

    IEnumerator SetActiveSceneWhenReady()
    {
        // wait until the flag is set to switch scenes
        yield return new WaitUntil(() => allLoaded == true);

        // set the specified scene as active, once all scenes are loaded
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneGlobals.startingSceneName));
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsOn(SceneGlobals.startingSceneName);

        // then turn off all the Loading Screen's objects
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsOff("LoadingScreen");
    }
}
