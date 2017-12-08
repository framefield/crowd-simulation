using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : Singleton<Simulation>
{
    [HideInInspector]
    public Dictionary<AttractionCategory, List<AttractionZone>> PointsOfInterest = new Dictionary<AttractionCategory, List<AttractionZone>>();


    [Header("MANUAL AGENT SPAWNING")]

    [SerializeField]
    bool SpawnAgentOnMouseDown;

    [SerializeField]
    AgentCategory CategoryToSpawnOnMouseDown;

    [Header("NAVMESH PARAMETERS")]

    [SerializeField]
    int PathfindingIterationsPerFrame;

    [SerializeField]
    [Range(0.5f, 20)]
    float AvoidancePredictionTime;

    [Header("SIMULATION PARAMETERS")]

    [SerializeField]
    bool EnableSocialInteraction;

    [SerializeField]
    int MaxAgentCount;

    [Header("INTERNAL - DO NOT TOUCH")]

    [SerializeField]
    GameObject AgentPrefab;

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

        if (Input.GetMouseButton(0) && SpawnAgentOnMouseDown)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                SpawnAgentAtPosition(hit.point, AgentPrefab, CategoryToSpawnOnMouseDown);
            }
        }
        WriteStatusToPanel();
    }


    public bool HasReachedMaxAgentCount()
    {
        int agentCount = 0;
        foreach (var agentList in _agents.Values)
            agentCount += agentList.Count;
        return agentCount < MaxAgentCount;
    }


    public void RemoveAgent(Agent agent)
    {
        _agents[agent.AgentCategory].Remove(agent);
    }


    private void WriteStatusToPanel()
    {
        var output = "";
        foreach (var key in _agents.Keys)
            output += key.name + " : " + _agents[key].Count + "         ";

        // output += " FPS: ";
        // output += (1f / Time.smoothDeltaTime).ToString("n0"); ;

        Log.text = output;
    }


    public void SpawnAgentAtPosition(Vector3 position, GameObject agentPrefab, AgentCategory category)
    {
        var newAgentGO = Instantiate(agentPrefab, position, Quaternion.identity, this.transform);
        var newAgent = newAgentGO.GetComponent<Agent>();
        newAgent.AgentCategory = category;
        AddAgentToPotentialInterlocutors(newAgent);
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


    private void AddAgentToPotentialInterlocutors(Agent agent)
    {
        if (!_agents.ContainsKey(agent.AgentCategory))
            _agents.Add(agent.AgentCategory, new List<Agent>());

        _agents[agent.AgentCategory].Add(agent);
    }


    private Dictionary<AgentCategory, List<Agent>> _agents = new Dictionary<AgentCategory, List<Agent>>();
}
