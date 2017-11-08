using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentWalking : MonoBehaviour
{
    [SerializeField] float MaxRandomDestinationDistance;
    [SerializeField] Color GizmoColor;



    void Start()
    {
        SetNewRandomDestination();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawLine(transform.position, CurrentDestination);
    }
    public Vector3 CurrentDestination
    {
        get
        {
            if (_lockedPointOfInterest != null)
                return _lockedPointOfInterest.transform.position;
            return _currentRandomDestination;
        }
    }
    public bool CheckIfReachedRandomDestination()
    {
        var distanceToRandomDestination = Vector3.Distance(this.transform.position, _currentRandomDestination);
        var hasReachedDestination = distanceToRandomDestination < MaxRandomDestinationDistance / 10f;

        return hasReachedDestination;
    }

    public void SetNewRandomDestination()
    {
        _currentRandomDestination = GenerateRandomDestination();
        _agent.SetDestination(_currentRandomDestination);
    }

    private Vector3 GenerateRandomDestination()
    {
        var vectorToRandomPosition = Random.insideUnitSphere * MaxRandomDestinationDistance;
        vectorToRandomPosition.Scale(new Vector3(1, 0, 1));
        vectorToRandomPosition += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(vectorToRandomPosition, out hit, MaxRandomDestinationDistance, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }


    private NavMeshAgent _agent;
    private Vector3 _currentRandomDestination;
    private PointOfInterest _lockedPointOfInterest;
    public Interests _currentInterests;



}
