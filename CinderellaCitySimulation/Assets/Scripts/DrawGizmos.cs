using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Only execute this code in the Unity Editor
        DebugUtils.OnDrawGizmos();
    }
#endif
}