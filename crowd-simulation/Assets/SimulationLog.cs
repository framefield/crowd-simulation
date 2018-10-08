using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class SimulationLog : MonoBehaviour
{
    [SerializeField]
    private float _logRate = 1f;

    public List<AgentLogData> LogData = new List<AgentLogData>();

    void Start()
    {
        _simulation.OnAgentSpawned += HandleSpawnedAgent;
    }

    private float _timeSinceLastLog;
    void Update()
    {
        _timeSinceLastLog += Time.deltaTime;
        if (_timeSinceLastLog > _logRate)
        {
            _timeSinceLastLog -= _logRate;
            foreach (var agentLogData in LogData)
            {
                agentLogData.LogSlice();
            }

        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            var csv = AgentLogData.GenerateCSV(LogData);
            var path = Application.streamingAssetsPath;
            Debug.Log(path);
            System.IO.File.WriteAllText(path, csv);
        }
    }

    public void HandleSpawnedAgent(Agent agent)
    {
        LogData.Add(new AgentLogData(agent));
    }

    private Simulation _simulationCache;
    private Simulation _simulation
    {
        get
        {
            if (_simulationCache == null)
                _simulationCache = GetComponent<Simulation>();
            return _simulationCache;
        }
    }
}
