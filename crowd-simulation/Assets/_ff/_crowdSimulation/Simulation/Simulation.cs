using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : Singleton<Simulation>
{
    public AttractionZone[] AllAttractionZones;

    [SerializeField] GameObject AgentPrefabRot;
    [SerializeField] GameObject AgentPrefabBlau;

    [SerializeField] int PathfindingIterationsPerFrame;
    [Range(0.5f, 20)]
    [SerializeField]
    float AvoidancePredictionTime;

    [SerializeField] float SpawnRadius;

    void Start()
    {
        AllAttractionZones = FindObjectsOfType<AttractionZone>();
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
                SpawnAgentAtPosition(hit.point, AgentPrefabRot);
            }
        }
        if (Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                SpawnAgentAtPosition(hit.point, AgentPrefabBlau);
            }
        }
    }

    private void SpawnAgent()
    {

        var spawnPosition = Random.insideUnitSphere;
        spawnPosition.Scale(new Vector3(1, 0, 1));
        spawnPosition *= SpawnRadius;
        SpawnAgentAtPosition(spawnPosition, AgentPrefabBlau);
    }

    private void SpawnAgentAtPosition(Vector3 position, GameObject agentPrefab)
    {
        Instantiate(agentPrefab, position, Quaternion.identity);
    }
}
