using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentWalking : MonoBehaviour
{
    [SerializeField] [Range(10, 500)] float MaxRandomDestinationDistance;
    [SerializeField] Color GizmoColor;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        SetNewRandomDestination();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;

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
        var distanceToRandomDestination = Vector3.Distance(this.transform.position, CurrentDestination);

        var hasReachedDestination = distanceToRandomDestination < MaxRandomDestinationDistance / 5f;
        return hasReachedDestination;
    }

    private Vector3 GenerateRandomDestination()
    {
        var randomPosOnMarker = new Vector3((Random.value - 0.5f) * 60f, 0f, (Random.value - 0.5f) * 60f);

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPosOnMarker, out hit, MaxRandomDestinationDistance, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }

    private NavMeshAgent _navMeshAgent;

    private Vector3 _currentDestination;
}
