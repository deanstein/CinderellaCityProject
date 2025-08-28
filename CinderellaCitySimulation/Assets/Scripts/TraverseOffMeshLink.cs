using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach this script to a GameObject with an agent
/// that needs to traverse NavMeshLinks using a custom speed
/// (the default speed is way too fast)
/// </summary>

public class TraverseOffMeshLink : MonoBehaviour
{
    public float traversalSpeed = ManageFPSControllers.FPSControllerGlobals.defaultAgentSpeedInside;
    public bool rotateTowardsTarget = true;

    private NavMeshAgent agent;
    // this this agent the player?
    private bool isFPSAgent = false;
    // is this agent traversing an off-mesh link?
    private bool isTraversingLink = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
        isFPSAgent = agent == ManageFPSControllers.FPSControllerGlobals.activeFPSControllerNavMeshAgent;
    }

    void Update()
    {
        if (!isTraversingLink && agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseLink(agent.currentOffMeshLinkData));
        }
    }

    private IEnumerator TraverseLink(OffMeshLinkData linkData)
    {
        isTraversingLink = true;
        if (isFPSAgent)
        {
            ModeState.isFPSAgentTraversingMeshLink = true;
        }

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = linkData.endPos;
        endPos.y = agent.transform.position.y; // Flatten vertical offset

        Vector3 direction = (endPos - startPos);

        if (rotateTowardsTarget)
        {
            // Rotation loop
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = 360f; // degrees per second

            while (Quaternion.Angle(agent.transform.rotation, targetRotation) > 1f)
            {
                float angle = Quaternion.Angle(agent.transform.rotation, targetRotation);
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

            yield return null;
        }

        agent.enabled = true;
        agent.CompleteOffMeshLink();

        isTraversingLink = false;
        if (isFPSAgent)
        {
            ModeState.isFPSAgentTraversingMeshLink = false;
        }
    }
}