using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TraverseOffMeshLink : MonoBehaviour
{
    public float traversalSpeed = ManageFPSControllers.FPSControllerGlobals.defaultAgentSpeedInside;
    public bool rotateTowardsTarget = false;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        if (!ModeState.isTraversingNavMeshLink && agent.isOnOffMeshLink)
        {
            Debug.Log("Agent is on OffMeshLink. Starting traversal...");
            StartCoroutine(TraverseLink(agent.currentOffMeshLinkData));
        }
    }

    private IEnumerator TraverseLink(OffMeshLinkData linkData)
    {
        ModeState.isTraversingNavMeshLink = true;

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = linkData.endPos;
        endPos.y = agent.transform.position.y; // Flatten vertical offset

        Debug.Log($"TraverseLink started. Start: {startPos}, End: {endPos}, Speed: {traversalSpeed}");
        Vector3 direction = (endPos - startPos);

        if (rotateTowardsTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = 360f; // degrees per second

            while (Quaternion.Angle(agent.transform.rotation, targetRotation) > 1f)
            {
                float angle = Quaternion.Angle(agent.transform.rotation, targetRotation);
                Debug.Log($"Rotating... Angle: {angle}");
                agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            agent.transform.rotation = targetRotation; // Snap to final rotation
        }

        // Movement loop
        Vector3 currentPos = startPos;

        while (Vector3.Distance(currentPos, endPos) > 0.1f)
        {
            currentPos += direction * traversalSpeed * Time.deltaTime;
            agent.transform.position = currentPos;

            Debug.Log($"Moving... Position: {currentPos}, Target: {endPos}, Distance: {Vector3.Distance(currentPos, endPos)}");
            yield return null;
        }

        Debug.Log("Traversal complete. Re-enabling agent control.");

        agent.enabled = true;
        agent.CompleteOffMeshLink();

        ModeState.isTraversingNavMeshLink = false;
    }
}