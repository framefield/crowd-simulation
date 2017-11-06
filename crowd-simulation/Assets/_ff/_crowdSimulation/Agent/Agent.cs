using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] AgentCategory AgentCategory;
    [SerializeField] Color GizmoColor;
    [SerializeField] float MaxRandomDestinationDistance;
    [SerializeField] float Stamina;

    public Vector3 CurrentDestination
    {
        get
        {
            if (_lockedAttraction != null)
                return _lockedAttraction.transform.position;
            return _currentRandomDestination;
        }
    }

    void Start()
    {
        _interestsForAttractionInstance = new Attractedness();
        _interestsForAttractionInstance.CopyFrom(AgentCategory.Attractedness);

        _agent = GetComponent<NavMeshAgent>();
        GetComponent<Renderer>().material.SetColor("_Color", AgentCategory.AgentColor);

        SetNewRandomDestination();
    }

    void Update()
    {


        var mostAttractiveAttraction = DetermineMostAttractiveAttraction();

        var foundAttraction = mostAttractiveAttraction != null;
        if (!foundAttraction)
        {
            if (CheckIfReachedRandomDestination())
                SetNewRandomDestination();
            return;
        }

        if (mostAttractiveAttraction == _lockedAttraction)
            return;

        _agent.SetDestination(mostAttractiveAttraction.transform.position);
        _lockedAttraction = mostAttractiveAttraction;
    }

    // private void MakeInterestSimulationStep()
    // {
    //     // foreach (var i in _interestsForAttractionInstance)
    //     // {
    //     //     if (i.AttractionCategory == _lockedAttraction.AttractionCategory)
    //     //         i.Attractiveness += Stamina;
    // }


    private Attraction DetermineMostAttractiveAttraction()
    {
        Attraction mostAttractiveAttraction = null;
        var maxFoundAttraction = 0f;

        foreach (var attraction in Simulation.Instance.Attractions)
        {
            var generalAttraction = attraction.GetGeneralAttractivenessAtGlobalPosition(this.transform.position);
            var personalAttraction = generalAttraction * GetCurrentInterest(attraction.AttractionCategory);

            if (personalAttraction > maxFoundAttraction)
            {
                mostAttractiveAttraction = attraction;
                maxFoundAttraction = personalAttraction;
            }
        }
        return mostAttractiveAttraction;
    }

    private float GetCurrentInterest(AttractionCategory attractionCategory)
    {
        if (_interestsForAttractionInstance.ContainsKey(attractionCategory))
            return _interestsForAttractionInstance[attractionCategory];
        else
            return 0f;
    }

    private bool CheckIfReachedRandomDestination()
    {
        var distanceToRandomDestination = Vector3.Distance(this.transform.position, _currentRandomDestination);
        var hasReachedDestination = distanceToRandomDestination < MaxRandomDestinationDistance / 10f;

        return hasReachedDestination;
    }

    private void SetNewRandomDestination()
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

    void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawLine(transform.position, CurrentDestination);
    }

    private NavMeshAgent _agent;
    private Vector3 _currentRandomDestination;
    private Attraction _lockedAttraction;
    public Attractedness _interestsForAttractionInstance;

}
