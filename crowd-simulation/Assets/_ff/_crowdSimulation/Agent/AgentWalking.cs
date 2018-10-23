using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentWalking : MonoBehaviour
{
    [Header("WALKING PARAMETERS")]

    [Range(0f, 1f)]
    [SerializeField]
    float _hasReachedDestinationEpsylon;

    [SerializeField]
    float _randomWalkRadius;

    [Header("TARGET PATH VISUALIZATION")]

    [SerializeField]
    bool _drawPathToTarget;

    [Range(0f, 1f)]
    [SerializeField]
    float _pathToTargetAlpha;

    [Header("LOGGED PATH VISUALIZATION")]

    [SerializeField]
    bool _drawLoggedPath;

    [Range(0f, 1f)]
    [SerializeField]
    float _loggedPathAlpha;

    [SerializeField]
    float _minDistanceBetweenLockedPositions;

    void Start()
    {
        _agent = GetComponent<Agent>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        SetNewRandomDestination();
    }

    void Update()
    {
        LogPath();
    }

    void OnDrawGizmos()
    {
        // if (_drawPathToTarget)
        // {
        //     var navMeshPath = _navMeshAgent.path;
        //     if (navMeshPath != null)
        //     {
        //         var path = navMeshPath.corners;
        //         var color = _agent.AgentCategory.Color * new Color(1, 1, 1, _pathToTargetAlpha);
        //         DrawPath(path, color);
        //     }
        // }
        // if (_drawLoggedPath)
        // {
        //     var color = _agent.AgentCategory.Color * new Color(1, 1, 1, _loggedPathAlpha);
        //     DrawPath(_loggedPath, color);
        // }
    }

    private static void DrawPath(Vector3[] path, Color color)
    {
        Gizmos.color = color;

        if (path != null)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
    private static void DrawPath(List<Vector3> path, Color color)
    {
        Gizmos.color = color;

        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
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
        var hasReachedDestination = squaredDistance < _hasReachedDestinationEpsylon;
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
        var randomUnitSpherePosition = transform.position + Random.insideUnitSphere * _randomWalkRadius;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomUnitSpherePosition, out hit, float.PositiveInfinity, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }

    private void LogPath()
    {
        if (_loggedPath.Count == 0)
        {
            _loggedPath.Add(transform.position);
            return;
        }

        var distanceToLastLockedPosition = Vector3.Distance(transform.position, _loggedPath[_loggedPath.Count - 1]);
        if (distanceToLastLockedPosition < _minDistanceBetweenLockedPositions)
            return;

        _loggedPath.Add(transform.position);
    }

    private List<Vector3> _loggedPath = new List<Vector3>();
    private NavMeshAgent _navMeshAgent;
    private Agent _agent;
    private Vector3 _currentDestination;
}
