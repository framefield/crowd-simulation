using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : Singleton<Simulation>
{
    public Dictionary<AttractionCategory, List<Attraction>> Attractions = new Dictionary<AttractionCategory, List<Attraction>>();

    [SerializeField] GameObject AgentPrefab;

    [SerializeField] int PathfindingIterationsPerFrame;
    [Range(0.5f, 20)]
    [SerializeField]
    float AvoidancePredictionTime;

    [SerializeField] float SpawnRadius;

    void Start()
    {
        var attractionsInScene = FindObjectsOfType<Attraction>();
        foreach (var a in attractionsInScene)
        {
            if (!Attractions.ContainsKey(a.AttractionCategory))
                Attractions.Add(a.AttractionCategory, new List<Attraction>());
            Attractions[a.AttractionCategory].Add(a);
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

    private void SpawnAgentAtPosition(Vector3 position, GameObject agentPrefab)
    {
        Instantiate(agentPrefab, position, Quaternion.identity);
    }
}
