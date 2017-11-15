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
    [Range(0.5f, 20)]
    [SerializeField]
    float AvoidancePredictionTime;

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

    private void SpawnAgentAtPosition(Vector3 position, GameObject agentPrefab)
    {
        Instantiate(agentPrefab, position, Quaternion.identity);
    }
}
