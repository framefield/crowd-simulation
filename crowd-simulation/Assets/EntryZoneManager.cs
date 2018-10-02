using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;

public class EntryZoneManager : MonoBehaviour
{
    [Header("NUMBER OF AGENTS")]
    [SerializeField]
    private MaxNumberOfAgents _maxNumberOfAgentsPerCategory;
    [SerializeField]
    [ReadOnly]
    private int _globalMaxAgentNumber;

    [Header("SPAWNING SPEED")]
    [SerializeField]
    private float _globalNewAgentsPerSecond;

    [SerializeField]
    private AgentsPerSecond _newAgentsPerSecondPerCategory;

    [SerializeField]
    [ReadOnly]
    private float _MinutesUntilAgentLimitReached;

    [SerializeField]
    private Dictionary<AgentCategory, List<EntryZone>> _entryZonesOverCategory;

    [SerializeField]
    private AgentsPerSecond _agentsScheduledForSpawning;

    [SerializeField]
    private Dictionary<AgentCategory, int> _numberOfAgentsSpawned;

    void Start()
    {
        _entryZonesOverCategory = InitEntryZonesOverCategory();
        _numberOfAgentsSpawned = new Dictionary<AgentCategory, int>();
        _agentsScheduledForSpawning = new AgentsPerSecond();

        foreach (var entryZone in _entryZonesOverCategory)
        {
            var category = entryZone.Key;
            if (!_agentsScheduledForSpawning.ContainsKey(category))
            {
                _agentsScheduledForSpawning.Add(category, 0f);
                _numberOfAgentsSpawned.Add(category, 0);
            }
        }
    }

    void Update()
    {
        foreach (var kvp in _entryZonesOverCategory)
        {
            var category = kvp.Key;
            _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);
            _agentsScheduledForSpawning[category] += _newAgentsPerSecondPerCategory[category] * Time.deltaTime;
            Debug.Log(category);
            if (_numberOfAgentsSpawned[category] >= _maxNumberOfAgentsPerCategory[category])
                continue;

            var agentsToSpawnRightNow = Mathf.FloorToInt(_agentsScheduledForSpawning[category]);
            for (int i = 0; i < agentsToSpawnRightNow; i++)
            {
                var numberOfZonesForCategory = _entryZonesOverCategory[category].Count;
                var randomIndex = Random.Range(0, numberOfZonesForCategory);
                var randomEntryZone = _entryZonesOverCategory[category][randomIndex];
                randomEntryZone.SpawnAgent();
                _numberOfAgentsSpawned[category]++;
                _agentsScheduledForSpawning[category]--;
            }
        }
    }

    void OnValidate()
    {
        Debug.Log("OnValidate");

        _globalMaxAgentNumber = CalculateTotalMaxAgents();
        _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

        _MinutesUntilAgentLimitReached = _globalMaxAgentNumber / _globalNewAgentsPerSecond;
    }

    [ContextMenu("InitData")]
    void InitData()
    {
        Debug.Log("InitDate");
        _newAgentsPerSecondPerCategory = new AgentsPerSecond();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
        var entryZonesInScene = Object.FindObjectsOfType<EntryZone>();
        foreach (var entryZone in entryZonesInScene)
        {
            var category = entryZone.GetAgentCategory();
            if (!_newAgentsPerSecondPerCategory.ContainsKey(category))
            {
                _newAgentsPerSecondPerCategory.Add(category, 0f);
                _maxNumberOfAgentsPerCategory.Add(category, 0);
            }
        }
    }

    private static Dictionary<AgentCategory, List<EntryZone>> InitEntryZonesOverCategory()
    {
        var entryZonesOverCategory = new Dictionary<AgentCategory, List<EntryZone>>();
        var entryZonesInScene = Object.FindObjectsOfType<EntryZone>();
        foreach (var entryZone in entryZonesInScene)
        {
            var category = entryZone.GetAgentCategory();
            if (!entryZonesOverCategory.ContainsKey(category))
                entryZonesOverCategory.Add(category, new List<EntryZone>());
            entryZonesOverCategory[category].Add(entryZone);
        }
        return entryZonesOverCategory;
    }

    private int CalculateTotalMaxAgents()
    {
        var totalAgents = 0;
        foreach (var kvp in _maxNumberOfAgentsPerCategory)
        {
            totalAgents += kvp.Value;
        }
        return totalAgents;
    }

    private static AgentsPerSecond DeriveAgentsPerMinute(int globalMaxAgentNumber,
                                                           float globalNewAgentsPerMinute,
                                                           MaxNumberOfAgents maxAgentsPerCategory)
    {
        var newValues = new AgentsPerSecond();
        foreach (var kvp in maxAgentsPerCategory)
        {
            var newValue = 1f * maxAgentsPerCategory[kvp.Key] / globalMaxAgentNumber * globalNewAgentsPerMinute;
            newValues[kvp.Key] = newValue;
        }

        return newValues;
    }
}
