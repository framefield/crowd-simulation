using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryZone : MonoBehaviour
{

    [SerializeField]
    GameObject AgentPrefab;
    public float AgentsPerSecond;


    void Update()
    {
        _agentsToSpawn += Time.deltaTime * AgentsPerSecond;

        var n = Mathf.Ceil(_agentsToSpawn);
        for (int i = 0; i < n; i++)
            Simulation.Instance.SpawnAgentAtPosition(transform.position, AgentPrefab);

        _agentsToSpawn -= n;
    }

    private float _agentsToSpawn = 0f;
}
