using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<AttractionCategory, List<AttractionZone>> PointsOfInterest = new Dictionary<AttractionCategory, List<AttractionZone>>();

    [Header("NAVMESH PARAMETERS")]

    [SerializeField]
    int PathfindingIterationsPerFrame;

    [SerializeField]
    [Range(0.5f, 20)]
    float AvoidancePredictionTime;

    [Header("SIMULATION PARAMETERS")]

    [SerializeField]
    bool EnableSocialInteraction;

    [Header("STATUS")]

    [SerializeField]
    private MaxNumberOfAgents _numberOfActiveAgents = new MaxNumberOfAgents();

    [Header("INTERNAL - DO NOT TOUCH")]

    [SerializeField]
    TMPro.TMP_Text Log;

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
        // WriteStatusToPanel();
    }

    public int GetNumberOfAgentsInSimulation(AgentCategory category)
    {
        if (!_agents.ContainsKey(category))
            return 0;
        return _agents[category].Count;
    }

    public void RemoveAgent(Agent agent)
    {
        _agents[agent.AgentCategory].Remove(agent);
        _numberOfActiveAgents[agent.AgentCategory]--;
    }

    private void WriteStatusToPanel()
    {
        var output = "";
        foreach (var key in _agents.Keys)
            output += key.name + " : " + _agents[key].Count + "         ";

        Log.text = output;
    }


    public void SpawnAgentAtPosition(Vector3 position, Agent agentPrefab, AgentCategory category)
    {
        var newAgentGO = Instantiate(agentPrefab, position, Quaternion.identity, this.transform);
        var newAgent = newAgentGO.GetComponent<Agent>();
        newAgent.Init(category, this);
        AddAgent(newAgent);
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


        if (!_numberOfActiveAgents.ContainsKey(agent.AgentCategory))
            _numberOfActiveAgents.Add(agent.AgentCategory, 0);
        _numberOfActiveAgents[agent.AgentCategory]++;
    }


    private Dictionary<AgentCategory, List<Agent>> _agents = new Dictionary<AgentCategory, List<Agent>>();
}
