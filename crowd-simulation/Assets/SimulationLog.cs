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
    public event Action LogDataSliceEvent;

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
                Debug.Log(agentLogData._agent.AgentCategory);
                LogDataSliceEvent();
            }
        }


    }

    public void HandleSpawnedAgent(Agent agent)
    {
        LogData.Add(new AgentLogData(agent, LogDataSliceEvent));
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
