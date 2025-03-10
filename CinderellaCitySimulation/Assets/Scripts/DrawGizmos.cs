using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // debug lines use Gizmos, so provide a lifecycle hook for them to draw
        DebugUtils.OnDrawGizmos();
    }
}