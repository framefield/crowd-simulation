using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] AgentCategory Category;
    [SerializeField] Color GizmoColor;
    [SerializeField] float MaxRandomDestinationDistance;

    public Vector3 CurrentDestination
    {
        get
        {
            if (_lockedAttractionZone != null)
                return _lockedAttractionZone.transform.position;
            return _currentRandomDestination;
        }
    }


    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        GetComponent<Renderer>().material.SetColor("_Color", Category.AgentColor);

        _currentRandomDestination = GetRandomDestination();
        _agent.SetDestination(_currentRandomDestination);
    }

    void Update()
    {
        AttractionZone closestAttractionZone = null;
        var highestAttraction = 0f;

        foreach (var attractionZone in Simulation.Instance.AllAttractionZones)
        {
            var perceivedAttraction = attractionZone.GetAttractivenessAtGlobalPosition(this.transform.position);

            if (perceivedAttraction > highestAttraction)
            {
                closestAttractionZone = attractionZone;
                highestAttraction = perceivedAttraction;
            }
        }
        if (closestAttractionZone == _lockedAttractionZone && closestAttractionZone != null)
            return;

        if (closestAttractionZone != null)
        {
            _agent.SetDestination(closestAttractionZone.transform.position);
            _lockedAttractionZone = closestAttractionZone;
            return;
        }
        MakeRandomWalk();
    }

    private void MakeRandomWalk()
    {
        var distanceToRandomDestination = Vector3.Distance(this.transform.position, _currentRandomDestination);
        var reachedDestination = distanceToRandomDestination < MaxRandomDestinationDistance / 10f;

        if (reachedDestination)
        {
            _currentRandomDestination = GetRandomDestination();
            _agent.SetDestination(_currentRandomDestination);
        }
    }

    private Vector3 GetRandomDestination()
    {
        var vectorToRandomPosition = Random.insideUnitSphere * MaxRandomDestinationDistance;
        vectorToRandomPosition.Scale(new Vector3(1, 0, 1));
        vectorToRandomPosition += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(vectorToRandomPosition, out hit, MaxRandomDestinationDistance, 1);
        Vector3 closestDestinationOnNavMesh = hit.position;

        return closestDestinationOnNavMesh;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawLine(transform.position, CurrentDestination);
    }

    private NavMeshAgent _agent;
    private Vector3 _currentRandomDestination;
    private AttractionZone _lockedAttractionZone;

}
