using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : Singleton<Simulation>
{
    public Dictionary<InterestCategory, List<PointOfInterest>> PointsOfInterest = new Dictionary<InterestCategory, List<PointOfInterest>>();

    [SerializeField] GameObject AgentPrefab;
    [SerializeField] AgentCategory CategoryToSpawnOnMouseDown;

    [SerializeField] int PathfindingIterationsPerFrame;

    [SerializeField] [Range(0.5f, 20)] float AvoidancePredictionTime;

    [SerializeField] TMPro.TMP_Text Log;

    [SerializeField] bool EnableSocialInteraction = false;


    void Start()
    {
        var attractionsInScene = FindObjectsOfType<PointOfInterest>();
        foreach (var a in attractionsInScene)
        {
            if (!PointsOfInterest.ContainsKey(a.InterestCategory))
                PointsOfInterest.Add(a.InterestCategory, new List<PointOfInterest>());
            PointsOfInterest[a.InterestCategory].Add(a);
        }
    }


    void Update()
    {
        NavMesh.pathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
        NavMesh.avoidancePredictionTime = AvoidancePredictionTime;

        if (Input.GetMouseButton(0))
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

    private void WriteStatusToPanel()
    {
        var output = "";
        foreach (var key in _agents.Keys)
            output += " | " + key.name + " : " + _agents[key].Count;
        output += " |";

        output += " FPS: ";
        output += (1f / Time.smoothDeltaTime).ToString("n0"); ;

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
