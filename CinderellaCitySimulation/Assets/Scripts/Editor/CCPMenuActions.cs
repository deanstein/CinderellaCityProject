using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[ExecuteInEditMode]
public class CCPMenuActions : MonoBehaviour
{
    void Update()
    {
        Debug.Log("EditorUpdate " + StaticOcclusionCulling.isRunning);
    }

    [MenuItem("Cinderella City Project/Update Occlusion Culling")]
    public static void UpdateOcclusionCulling()
    {
        ManageScenes.LoadEditorScenesAdditively(SceneGlobals.loadingSceneName, SceneGlobals.allGameplaySceneNames);
        StaticOcclusionCulling.Compute();
        // TODO: return to the loading screen once occlusion culling is computed
    }

    [MenuItem("Cinderella City Project/Update All Nav Meshes")]
    public static void RebuildAllNavMeshes()
    {
        foreach (string sceneName in SceneGlobals.availableTimePeriodSceneNames)
        {
            // open the scene if it's not open already
            if (sceneName != EditorSceneManager.GetActiveScene().name)
            {
                EditorSceneManager.OpenScene(SceneGlobals.GetScenePathByName(sceneName));

                // build the nav mesh and save the scene
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

                Utils.DebugUtils.DebugLog("Updated the nav mesh in scene: " + sceneName);
            }
            // otherwise, we're already in the request scene, so build the nav mesh
            else
            {
                // build the nav mesh and save the scene
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }
    }
    // TODO: return to the loading screen once all nav meshes are updated
}