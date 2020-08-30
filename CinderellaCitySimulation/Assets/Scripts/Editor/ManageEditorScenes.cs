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
            Utils.DebugUtils.DebugLog("Opening the loading scene first...");
        }

        // load each specified scene additively
        for (var i = 0; i < sceneNames.Length; i++)
        {
            // convert the scene name to a path
            string scenePath = SceneGlobals.GetScenePathByName(sceneNames[i]);

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            Utils.DebugUtils.DebugLog("Additively opening scene in editor: " + scenePath);
        }
    }

}

