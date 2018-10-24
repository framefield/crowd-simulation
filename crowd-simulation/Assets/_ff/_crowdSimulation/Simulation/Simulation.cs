using System;
using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : MonoBehaviour
{
    [Header("VISUALIZATION")]

    [SerializeField]
    bool _drawAttractionZoneRadii = true;

    [SerializeField]
    bool _renderAgents = true;

    [SerializeField]
    bool _drawAgentSocialInteractionRadii = false;

    [SerializeField]
    bool _drawAgentIndicators = true;

    [SerializeField]
    [Range(0f, 1f)]
    float _agentSocialInteractionRadiiAlpha = 0.1f;

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

    [HideInInspector]
    public Dictionary<InterestCategory, List<AttractionZone>> AttractionZones = new Dictionary<InterestCategory, List<AttractionZone>>();

    public event Action<Agent> OnAgentSpawned;
    public event Action<Agent> OnAgentRemoved;

    void Start()
    {
        var attractionsInScene = FindObjectsOfType<AttractionZone>();
        foreach (var a in attractionsInScene)
        {
            if (!AttractionZones.ContainsKey(a.InterestCategory))
                AttractionZones.Add(a.InterestCategory, new List<AttractionZone>());
            AttractionZones[a.InterestCategory].Add(a);
        }
    }

    void Update()
    {
        NavMesh.pathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
        NavMesh.avoidancePredictionTime = AvoidancePredictionTime;

        foreach (var agentsByCategory in _agents)
        {
            foreach (var agent in agentsByCategory.Value)
            {
                agent.DrawIndicatorAboveAgent = _drawAgentIndicators;
                agent.DrawSocialInteractionRadii = _drawAgentSocialInteractionRadii;
                agent.GetComponent<Renderer>().enabled = _renderAgents;
                agent.SocialInteractionRadiiAlpha = _agentSocialInteractionRadiiAlpha;
            }
        }

        foreach (var attractionZoneList in AttractionZones.Values)
        {
            foreach (var attractionZone in attractionZoneList)
            {
                attractionZone.DrawGizmos = _drawAttractionZoneRadii;
            }
        }
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
