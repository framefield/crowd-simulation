using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentWalking : MonoBehaviour
{
    [Header("PARAMETERS")]

    [Range(0f, 1f)]
    [SerializeField]
    float HasReachedDestinationEpsylon;

    [SerializeField]
    float RandomWalkRadius;

    [Header("VISUALIZATION")]

    [SerializeField]
    bool DrawPath;

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
        NavMeshHit hit;
        NavMesh.SamplePosition(destination, out hit, float.PositiveInfinity, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;
        CurrentDestination = closestDestinationOnNavMesh;
    }


    public void SetNewRandomDestination()
    {
        CurrentDestination = GenerateRandomDestination();
    }


    public bool HasReachedCurrentDestination()
    {
        var squaredDistance = GetSquaredDistanceToCurrentDestination();
        var hasReachedDestination = squaredDistance < HasReachedDestinationEpsylon;
        return hasReachedDestination;
    }

    public float GetSquaredDistanceToCurrentDestination()
    {
        var diffVector = CurrentDestination - transform.position;
        var diffOnXZ = new Vector2(diffVector.x, diffVector.z);
        var squaredDistance = Vector2.Dot(diffOnXZ, diffOnXZ);
        return squaredDistance;
    }


    private Vector3 GenerateRandomDestination()
    {
        var randomUnitSpherePosition = transform.position + Random.insideUnitSphere * RandomWalkRadius;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomUnitSpherePosition, out hit, float.PositiveInfinity, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }


    private NavMeshAgent _navMeshAgent;
    private Agent _agent;
    private Vector3 _currentDestination;
}
