using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : Singleton<Simulation>
{
    public Dictionary<InterestCategory, List<PointOfInterest>> PointsOfInterest = new Dictionary<InterestCategory, List<PointOfInterest>>();

    [SerializeField] GameObject AgentPrefab;

    [SerializeField] int PathfindingIterationsPerFrame;

    [SerializeField] [Range(0.5f, 20)] float AvoidancePredictionTime;

    [SerializeField] float SpawnRadius;


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
                SpawnAgentAtPosition(hit.point, AgentPrefab);
            }
        }
    }


    public void SpawnAgentAtPosition(Vector3 position, GameObject agentPrefab)
    {
        Debug.Log("spawning");
        var newAgentGO = Instantiate(agentPrefab, position, Quaternion.identity, this.transform);
        var newAgent = newAgentGO.GetComponent<Agent>();
        AddAgentToPotentialInterlocutors(newAgent);
    }


    public Agent FindClosestNeighbourOfCategory(AgentCategory category, Agent agent)
    {
        Agent closestNeighbour = null;
        var smallestDistance = float.PositiveInfinity;

        foreach (var a in _potentialInterlocutors[category])
        {
            if (a == agent)
                continue;

            var distance = Vector3.Distance(a.transform.position, agent.transform.position);
            if (distance < smallestDistance)
            {
                closestNeighbour = a;
                smallestDistance = distance;
            }
        }

        return closestNeighbour;
    }


    private void AddAgentToPotentialInterlocutors(Agent agent)
    {
        if (!_potentialInterlocutors.ContainsKey(agent.AgentCategory))
            _potentialInterlocutors.Add(agent.AgentCategory, new List<Agent>());

        _potentialInterlocutors[agent.AgentCategory].Add(agent);
    }

    private Dictionary<AgentCategory, List<Agent>> _potentialInterlocutors = new Dictionary<AgentCategory, List<Agent>>();

}
