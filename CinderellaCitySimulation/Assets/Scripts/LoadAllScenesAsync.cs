﻿using System.Collections;
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
    public bool autoStart; // skips the main menu and goes to a specific scene
    public bool autoGuidedTour; // starts the guided tour immediately
    public bool autoTimeTravelPeriodic; // time travels automatically after 10 minutes
    public bool autoTimeTravelPeek; // time travels after player arrives at each photo
    public bool shuffleDestinations; // uses a specific order of scenes for recording a video
}
/*** EXAMPLE START CONFIG ***
*** STORE AT C:/Users/<user>/AppData/LocalLow/The Cinderella City Project/ccp-startup-config.json ***

{
    "autoStart": true,
    "autoGuidedTour": true,
    "autoTimeTravelPeriodic": false,
    "autoTimeTravelPeek": true,
    "shuffleDestinations": true
}

*** END EXAMPLE ***/


public class StartupGlobals
{
    public static string startupConfigFile = "ccp-startup-config.json";
    public static string startupConfigPath = null;
    public static StartupConfig startupConfig = null;
}

public class LoadAllScenesAsync : MonoBehaviour {

    // create a list to keep track of all active async operations progress
    static List<AsyncOperation> asyncOperations = new List<AsyncOperation>();
    // set to true only when all specified scenes are loaded
    bool allLoaded;

    // reads the startup config json file and updates ModeState as required
    public static void ReadStartupConfig()
    {
        // persistent data path must be called in Awake() or Start()
        StartupGlobals.startupConfigPath = Application.persistentDataPath + "/" + StartupGlobals.startupConfigFile;

        // check startup config for additional instructions
        if (File.Exists(StartupGlobals.startupConfigPath))
        {
            Debug.Log("Startup config file found. Checking for configs...");

            // read the JSON file
            string json = File.ReadAllText(StartupGlobals.startupConfigPath);

            // convert the JSON file to StartupConfig class
            StartupConfig foundConfig = JsonUtility.FromJson<StartupConfig>(json);
            // make the found config available publicly
            StartupGlobals.startupConfig = foundConfig;

            //
            // handle individual config settings
            //

            if (foundConfig.autoStart)
            {
                Debug.Log("Startup Config: <b>autoStart is TRUE</b>");
                ModeState.autoStart = true;
            }
            else
            {
                Debug.Log("Startup Config: <b>autoStart is FALSE</b>");
            }

            if (foundConfig.autoGuidedTour)
            {
                Debug.Log("Startup Config: <b>autoGuidedTour is TRUE</b>");
                ModeState.isGuidedTourActive = true;
            }
            else
            {
                Debug.Log("Startup Config: <b>autoGuidedTour is FALSE</b>");
            }

            if (foundConfig.autoTimeTravelPeriodic)
            {
                Debug.Log("Startup Config: <b>autoTimeTravelPeriodic is TRUE</b>");
                ModeState.autoTimeTravelPeriodic = true;
            }
            else
            {
                Debug.Log("Startup Config: <b>autoTimeTravelPeriodic is FALSE</b>");
            }

            if (foundConfig.autoTimeTravelPeek)
            {
                Debug.Log("Startup Config: <b>autoTimeTravelPeek is TRUE</b>");
                ModeState.autoTimeTravelPeek = true;
            }
            else
            {
                Debug.Log("Startup Config: <b>autoTimeTravelPeek is FALSE</b>");
            }

            if (foundConfig.shuffleDestinations)
            {
                DebugUtils.DebugLog("Startup Config: <b>shuffleDestinations is TRUE</b>");
                ModeState.shuffleDestinations = true;
            }
            else
            {
                Debug.Log("Startup Config: <b>shuffleDestinations is FALSE</b>");
            }
        }
        else
        {
            DebugUtils.DebugLog("No startup config file found.");
        }
    }

    private void Awake()
    {
        // read the startup config file if it exists
        ReadStartupConfig();
    }

    void Start()
    {
        // limit framerate to prevent very fast computers from moving objects too quickly
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // load the specified scenes
        ManageScenes.LoadScenesAsync(SceneGlobals.scenesToLoad, asyncOperations);

        // switch to the MainMenu when all scenes are ready
        // also check for post-launch config settings
        StartCoroutine(SetActiveSceneWhenReady(SceneManager.GetSceneByName(SceneGlobals.startingSceneName)));
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
        // wait until all scenes are loaded to switch to the given starting scene
        yield return new WaitUntil(() => allLoaded == true);

        // if the starting scene is one of the mall eras,
        // we need to relocate the player to a good initial spot
        if (startingScene.name == SceneGlobals.mallEra60s70sSceneName || startingScene.name == SceneGlobals.mallEra80s90sSceneName || startingScene.name == SceneGlobals.experimentalSceneName)
        {
            ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToCamera(SceneManager.GetActiveScene().name, startingScene.name, "Camera-Thumbnail-Blue Mall-Highlight");
        }
        // otherwise, we assume we're switching to a menu scene, like Main Menu
        else
        {
            SceneManager.SetActiveScene(startingScene);
            ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState(startingScene.name, true);
            // then turn off all the Loading Screen's objects
            ManageSceneObjects.ObjectState.ToggleAllTopLevelSceneObjectsToState("LoadingScreen", false);
        }
    }
}
