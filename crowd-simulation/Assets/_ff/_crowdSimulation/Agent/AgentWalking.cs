using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentWalking : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    float HasReachedDestinationEpsylon;

    [SerializeField] bool DrawPath;

    [Range(0f, 1f)]
    [SerializeField]
    float PathAlpha;

    void Start()
    {
        _agent = GetComponent<Agent>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        SetNewRandomDestination();
    }

    void OnDrawGizmos()
    {
        if (!DrawPath)
            return;

        Gizmos.color = _agent.AgentCategory.Color * new Color(1, 1, 1, PathAlpha);

        var path = _navMeshAgent.path;
        if (path != null)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }

    private Vector3 CurrentDestination
    {
        set
        {
            _currentDestination = value;
            _navMeshAgent.SetDestination(value);
        }
        get
        {
            return _currentDestination;
        }
    }

    public void SetDestination(Vector3 destination)
    {
        CurrentDestination = destination;
    }

    public void SetNewRandomDestination()
    {
        CurrentDestination = GenerateRandomDestination();
    }

    public bool CheckIfReachedRandomDestination()
    {
        var diffVector = CurrentDestination - transform.position;
        var diffOnXZ = new Vector2(diffVector.x, diffVector.z);
        var squaredDistance = Vector2.Dot(diffOnXZ, diffOnXZ);
        var hasReachedDestination = squaredDistance < HasReachedDestinationEpsylon;
        return hasReachedDestination;
    }

    private Vector3 GenerateRandomDestination()
    {
        var randomPosOnMarker = new Vector3((Random.value - 0.5f) * 60f, 0f, (Random.value - 0.5f) * 60f);

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPosOnMarker, out hit, float.PositiveInfinity, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }

    private NavMeshAgent _navMeshAgent;
    private Agent _agent;
    private Vector3 _currentDestination;
}
