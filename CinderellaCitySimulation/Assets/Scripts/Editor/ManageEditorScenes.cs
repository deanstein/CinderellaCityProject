using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class ManageEditorScenes
{
    // load the given scenes additively in the Unity Editor, starting from a given loading scene
    // closes the current scene if the current scene is not the specified loading scene
    public static void LoadEditorScenesAdditively(string loadingScene, string[] sceneNames)
    {
        // ensure the loadingScene is opened first
        if (SceneManager.GetActiveScene().name != loadingScene)
        {
            string loadingScenePath = SceneGlobals.GetScenePathByName(loadingScene);

            EditorSceneManager.OpenScene(loadingScenePath);
            //Utils.DebugUtils.DebugLog("Opening the loading scene first...");
        }

        // load each specified scene additively
        for (var i = 0; i < sceneNames.Length; i++)
        {
            // convert the scene name to a path
            string scenePath = SceneGlobals.GetScenePathByName(sceneNames[i]);

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            //Utils.DebugUtils.DebugLog("Additively opening scene in editor: " + scenePath);
        }
    }

    public static List<Scene> GetAllOpenScenes()
    {
        int sceneCount = SceneManager.sceneCount;
        List<Scene> sceneList = new List<Scene>();

        for (var i = 0; i < sceneCount; i++)
        {
            sceneList.Add(SceneManager.GetSceneAt(i));
        }

        return sceneList;
    }

    public static List<GameObject> GetAllOpenSceneContainerObjects()
    {
        List<GameObject> allSceneContainerObjects = new List<GameObject>();

        List<Scene> allScenes = ManageEditorScenes.GetAllOpenScenes();

        for (var i = 0; i < allScenes.Count; i++)
        {
            GameObject sceneContainer = ManageScenes.GetSceneContainerObject(allScenes[i]);

            allSceneContainerObjects.Add(sceneContainer);
        }

        return allSceneContainerObjects;
    }

    public static List<GameObject> GetAllTimePeriodSceneContainers()
    {
        List<GameObject> allOpenSceneContainerObjects = GetAllOpenSceneContainerObjects();
        List<GameObject> timePeriodSceneContainerObjects = new List<GameObject>();

        for (var i = 0; i < SceneGlobals.availableTimePeriodSceneNames.Length; i++)
        {
            for (var j = 0; j < allOpenSceneContainerObjects.Count; j++)
            {
                if (allOpenSceneContainerObjects[j].name.Contains(SceneGlobals.availableTimePeriodSceneNames[i]))
                {
                    timePeriodSceneContainerObjects.Add(allOpenSceneContainerObjects[j]);
                }
            }
        }

        return timePeriodSceneContainerObjects;
    }
}

