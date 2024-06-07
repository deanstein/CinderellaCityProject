using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads all game Scenes into memory, before anything else
/// This allows the nearly seamless transition between Scenes
/// This must be attached to a LoadingScreenCoroutineHost at the top level of the Loading Screen scene
/// </summary>

// startup config is a .json file with certain options
[System.Serializable]
public class StartupConfig
{
    public bool autoStart;
    public bool autoGuidedTour;
}

public class StartupGlobals
{
    public static string startupConfigPath = Application.persistentDataPath + "/ccp-startup-config.json";
}

public class LoadAllScenesAsync : MonoBehaviour {

    // create a list to keep track of all active async operations progress
    static List<AsyncOperation> asyncOperations = new List<AsyncOperation>();

    // set to true only when all specified scenes are loaded
    bool allLoaded;

    private void Awake()
    {
        // ensure this coroutine host is not destroyed to the config check later can happen
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // limit frame rate to prevent very fast computers from moving objects too quickly
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // load the specified scenes
        ManageScenes.LoadScenesAsync(SceneGlobals.scenesToLoad, asyncOperations);

        // switch to the MainMenu when all scenes are ready
        StartCoroutine(SetActiveSceneAndCheckConfigAfterDelay(SceneManager.GetSceneByName(SceneGlobals.startingSceneName)));
    }
	
	void Update ()
    {
        if (!allLoaded)
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
                    ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(SceneGlobals.scenesToLoad[i], false);
                }
            }

            // if the amount of async operations matches the scene list, and all scenes are loaded,
            // set the flag that all scenes are loaded
            if ((scenesLoaded.Count == SceneGlobals.scenesToLoad.Length) && scenesLoaded.TrueForAll(b => b))
            {
                allLoaded = true;
            }
        }       
    }

    // switches to the given scene when all requested scenes are loaded
    IEnumerator SetActiveSceneWhenReady(Scene startingScene)
    {
        // wait until the flag is set to switch scenes
        yield return new WaitUntil(() => allLoaded == true);

        // set the specified scene as active, once all scenes are loaded
        SceneManager.SetActiveScene(startingScene);
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(startingScene.name, true);

        // then turn off all the Loading Screen's objects
        ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState("LoadingScreen", false);
    }

    IEnumerator SetActiveSceneAndCheckConfigAfterDelay(Scene startingScene)
    {
        yield return StartCoroutine(SetActiveSceneWhenReady(startingScene));

        yield return StartCoroutine(CheckConfigAfterDelay(StartupGlobals.startupConfigPath));
    }

    IEnumerator CheckConfigAfterDelay(string startupConfigPath)
    {
        // check startup config for additional instructions
        if (File.Exists(startupConfigPath))
        {
            Debug.Log("Startup config file found. Checking for configs...");

            string json = File.ReadAllText(startupConfigPath);

            // convert the JSON file to StartupConfig class
            StartupConfig startupConfig = JsonUtility.FromJson<StartupConfig>(json);

            // instantStart skips right to a given scene
            if (startupConfig.autoStart)
            {
                Debug.Log("Startup Config: autoStart is true.");
                yield return new WaitForSeconds(0.5f);
                ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToCamera(SceneManager.GetActiveScene().name, "60s70s", "Camera-Thumbnail-Blue Mall-Highlight");
            }
            // instantGuidedTour sets the guided tour state to true immediately
            if (startupConfig.autoGuidedTour)
            {
                Debug.Log("Startup Config: autoGuidedTour is true.");
                yield return new WaitForSeconds(0.5f);
                ModeState.isGuidedTourActive = true;

                // automatically switch to the next era after some time
                ModeState.toggleToNextEraCoroutine = StartCoroutine(ToggleSceneAndUI.ToggleToNextEraAfterDelay());
            }
        }
    }

}
