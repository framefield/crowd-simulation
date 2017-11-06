using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] AgentCategory AgentCategory;
    [SerializeField] Color GizmoColor;
    [SerializeField] float MaxRandomDestinationDistance;

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
        _currentAttractedness = new Attractedness();
        _currentAttractedness.CopyFrom(AgentCategory.Attractedness);

        _agent = GetComponent<NavMeshAgent>();
        RenderAttractedness();

        SetNewRandomDestination();
    }

    void Update()
    {

        var mostAttractiveAttraction = DetermineMostAttractiveAttraction();

        var foundAttraction = mostAttractiveAttraction != null;
        var wasLockedToAttraction = _lockedAttraction != null;
        if (!foundAttraction)
        {
            if (wasLockedToAttraction)
            {
                _lockedAttraction = null;
                SetNewRandomDestination();
                return;
            }
            else
            {
                if (CheckIfReachedRandomDestination())
                    SetNewRandomDestination();
                return;
            }
        }

        if (mostAttractiveAttraction == _lockedAttraction)
        {
            MakeInterestSimulationStep();
            RenderAttractedness();
            return;
        }

        _agent.SetDestination(mostAttractiveAttraction.transform.position);
        _lockedAttraction = mostAttractiveAttraction;



    }

    private void MakeInterestSimulationStep()
    {
        var key = _lockedAttraction.AttractionCategory;
        var value = _currentAttractedness[key];
        value = Mathf.Max(value + AgentCategory.Stamina, 0f);
        _currentAttractedness[key] = value;
    }

    float _attractednessOnStart = -1f;
    private void RenderAttractedness()
    {
        var culminatedAttractedness = 0f;
        foreach (KeyValuePair<AttractionCategory, float> pair in _currentAttractedness)
            culminatedAttractedness += pair.Value;

        if (_attractednessOnStart == -1f)
        {
            _attractednessOnStart = culminatedAttractedness;
        }

        var brightness = culminatedAttractedness / _attractednessOnStart;
        var color = AgentCategory.AgentColor * new Color(brightness, brightness, brightness, 1);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    private Attraction DetermineMostAttractiveAttraction()
    {
        Attraction mostAttractiveAttraction = null;
        var maxFoundAttraction = 0f;

        foreach (KeyValuePair<AttractionCategory, float> pair in _currentAttractedness)
        {
            var attraction = GetMostAttractiveAttraction(pair.Key);

            float generalAttraction;

            var foundAttraction = attraction != null;

            generalAttraction = foundAttraction
               ? attraction.GetGeneralAttractivenessAtGlobalPosition(this.transform.position)
               : 0f;

            var personalAttraction = generalAttraction * GetCurrentInterest(pair.Key);
            if (personalAttraction > maxFoundAttraction)
            {
                mostAttractiveAttraction = attraction;
                maxFoundAttraction = personalAttraction;
            }
        }
        return mostAttractiveAttraction;
    }

    public Attraction GetMostAttractiveAttraction(AttractionCategory attractionCategory)
    {
        Attraction mostAttractiveAttraction = null;
        var attractionAtAgentsLocation = 0f;
        foreach (var attraction in Simulation.Instance.Attractions[attractionCategory])
        {
            var candidate = attraction.GetGeneralAttractivenessAtGlobalPosition(this.transform.position);
            if (candidate > attractionAtAgentsLocation)
            {
                mostAttractiveAttraction = attraction;
                attractionAtAgentsLocation = candidate;
            }
        }
        return mostAttractiveAttraction;
    }

    public float GetCurrentInterest(AttractionCategory attractionCategory)
    {
        if (_currentAttractedness.ContainsKey(attractionCategory))
            return _currentAttractedness[attractionCategory];
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
    public Attractedness _currentAttractedness;



}
