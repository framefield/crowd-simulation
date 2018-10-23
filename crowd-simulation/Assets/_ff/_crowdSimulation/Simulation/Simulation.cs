using System;
using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<InterestCategory, List<AttractionZone>> PointsOfInterest = new Dictionary<InterestCategory, List<AttractionZone>>();

    [Header("NAVMESH PARAMETERS")]

    [SerializeField]
    int PathfindingIterationsPerFrame;

    [SerializeField]
    [Range(0.5f, 20)]
    float AvoidancePredictionTime;

    [Header("SIMULATION PARAMETERS")]

    [SerializeField]
    bool EnableSocialInteraction;

    [Header("INTERNAL - DO NOT TOUCH")]

    [SerializeField]
    TMPro.TMP_Text Log;

    public event Action<Agent> OnAgentSpawned;
    public event Action<Agent> OnAgentRemoved;

    void Start()
    {
        var attractionsInScene = FindObjectsOfType<AttractionZone>();
        foreach (var a in attractionsInScene)
        {
            if (!PointsOfInterest.ContainsKey(a.InterestCategory))
                PointsOfInterest.Add(a.InterestCategory, new List<AttractionZone>());
            PointsOfInterest[a.InterestCategory].Add(a);
        }
    }

    void Update()
    {
        NavMesh.pathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
        NavMesh.avoidancePredictionTime = AvoidancePredictionTime;
    }

    public void RemoveAgent(Agent agent)
    {
        _agents[agent.AgentCategory].Remove(agent);

        if (OnAgentRemoved != null)
            OnAgentRemoved(agent);
    }

    public void SpawnAgentAtPosition(Vector3 position, Agent agentPrefab, AgentCategory category)
    {
        var newAgentGO = Instantiate(agentPrefab, position, Quaternion.identity, this.transform);
        var newAgent = newAgentGO.GetComponent<Agent>();
        newAgent.Init(category, this);
        AddAgent(newAgent);

        if (OnAgentSpawned != null)
            OnAgentSpawned(newAgent);
    }


    public Agent FindClosestNeighbourOfCategory(AgentCategory category, Agent agent)
    {
        if (!EnableSocialInteraction)
            return null;

        Agent closestNeighbour = null;

        var smallestDistanceSquared = float.PositiveInfinity;
        foreach (var a in GetPersons(category))
        {
            if (a == agent)
                continue;

            var diffVector = agent.transform.position - a.transform.position;
            var distanceSquared = Vector3.Dot(diffVector, diffVector);
            if (distanceSquared < smallestDistanceSquared)
            {
                closestNeighbour = a;
                smallestDistanceSquared = distanceSquared;
            }
        }

        return closestNeighbour;
    }


    private List<Agent> GetPersons(AgentCategory category)
    {
        if (_agents.ContainsKey(category))
            return _agents[category];

        return new List<Agent>();
    }


    private void AddAgent(Agent agent)
    {
        if (!_agents.ContainsKey(agent.AgentCategory))
            _agents.Add(agent.AgentCategory, new List<Agent>());
        _agents[agent.AgentCategory].Add(agent);
    }


    private Dictionary<AgentCategory, List<Agent>> _agents = new Dictionary<AgentCategory, List<Agent>>();
}
