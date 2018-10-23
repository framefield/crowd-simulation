﻿using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class EntryZoneManager : MonoBehaviour
{
    [SerializeField]
    List<EntryZone> _entryZones;

    [Space(15f)]

    [Header("NUMBER OF AGENTS")]
    [SerializeField]
    private MaxNumberOfAgents _maxNumberOfAgentsPerCategory;

    [SerializeField]
    [ReadOnly]
    private int _globalMaxAgentNumber;

    [SerializeField]
    private AgentCategoryDictionary _numberOfActiveAgents = new AgentCategoryDictionary();

    [SerializeField]
    private AgentCategoryDictionary _numberOfAgentsThatLeft = new AgentCategoryDictionary();

    [Header("SPAWNING")]
    [SerializeField]
    private float _globalNewAgentsPerSecond = 1f;

    [SerializeField]
    private AgentCategoryDictionary _newAgentsPerSecond;

    private AgentCategoryDictionary _numberOfAgentsScheduledForSpawning = new AgentCategoryDictionary();

    [SerializeField]
    [ReadOnly]
    private string _estimatedTimeUntilAgentLimitReached;

    [Space(15f)]

    [Header("INTERNAL PREFAB REFERENCE - DO NOT TOUCH")]
    [SerializeField]
    Agent _agentPrefab;

    [HideInInspector]
    [SerializeField]
    private Dictionary<AgentCategory, List<EntryZone>> _entryZoneLookUp;

    [HideInInspector]
    [SerializeField]
    public List<AgentCategory> _agentCategories;

    void Start()
    {
        _entryZoneLookUp = InitEntryZoneLookUp(_entryZones);
        _simulation.OnAgentRemoved += (agent) =>
        {
            _numberOfAgentsThatLeft[agent.AgentCategory]++;
            _numberOfActiveAgents[agent.AgentCategory]--;
        };

        _simulation.OnAgentSpawned += (agent) => _numberOfActiveAgents[agent.AgentCategory]++;

        foreach (var category in _agentCategories)
        {
            if (!_numberOfAgentsScheduledForSpawning.ContainsKey(category))
            {
                _numberOfAgentsScheduledForSpawning.Add(category, 0f);
                _numberOfActiveAgents.Add(category, 0f);
                _numberOfAgentsThatLeft.Add(category, 0f);
            }
        }
    }

    void Update()
    {
        foreach (var category in _agentCategories)
        {
            _newAgentsPerSecond = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

            _numberOfAgentsScheduledForSpawning[category] += _newAgentsPerSecond[category] * Time.deltaTime;

            var agentsToSpawnRightNow = Mathf.FloorToInt(_numberOfAgentsScheduledForSpawning[category]);
            for (int i = 0; i < agentsToSpawnRightNow && !HasReachedMaxAgents(category); i++)
            {
                SpawnAgent(category);
            }
        }
    }

    void OnValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)

            Debug.Log("OnValidate");

        if (HaveEntryZonesChanged() && !EditorApplication.isPlaying)
            DeriveDataFromEntryZones();

        _globalMaxAgentNumber = CalculateTotalMaxAgents();
        _newAgentsPerSecond = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

        _estimatedTimeUntilAgentLimitReached = (_globalMaxAgentNumber / _globalNewAgentsPerSecond).ToString();
    }

    private void SpawnAgent(AgentCategory category)
    {
        var randomEntryZone = PickRandomEntryZone(category);
        _simulation.SpawnAgentAtPosition(randomEntryZone.transform.position, _agentPrefab, randomEntryZone.GetAgentCategory());
        _numberOfAgentsScheduledForSpawning[category]--;
    }

    private EntryZone PickRandomEntryZone(AgentCategory category)
    {
        var numberOfZonesForCategory = _entryZoneLookUp[category].Count;
        var randomIndex = Random.Range(0, numberOfZonesForCategory);
        return _entryZoneLookUp[category][randomIndex];
    }

    private bool HasReachedMaxAgents(AgentCategory category)
    {
        return _numberOfActiveAgents[category] >= _maxNumberOfAgentsPerCategory[category];
    }

    private void DeriveDataFromEntryZones()
    {
        _agentCategories = new List<AgentCategory>();
        _newAgentsPerSecond = new AgentCategoryDictionary();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
        foreach (var entryZone in _entryZones)
        {
            var category = entryZone.GetAgentCategory();
            if (!_newAgentsPerSecond.ContainsKey(category))
            {
                _agentCategories.Add(category);
                _newAgentsPerSecond.Add(category, 0f);
                _maxNumberOfAgentsPerCategory.Add(category, 100);
            }
        }
    }

    private static Dictionary<AgentCategory, List<EntryZone>> InitEntryZoneLookUp(List<EntryZone> entryZones)
    {
        var entryZoneLookUp = new Dictionary<AgentCategory, List<EntryZone>>();
        foreach (var entryZone in entryZones)
        {
            var category = entryZone.GetAgentCategory();
            if (!entryZoneLookUp.ContainsKey(category))
                entryZoneLookUp.Add(category, new List<EntryZone>());
            entryZoneLookUp[category].Add(entryZone);
        }
        return entryZoneLookUp;
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

    private static AgentCategoryDictionary DeriveAgentsPerMinute(int globalMaxAgentNumber,
                                                           float globalNewAgentsPerMinute,
                                                           MaxNumberOfAgents maxAgentsPerCategory)
    {
        var newValues = new AgentCategoryDictionary();
        foreach (var kvp in maxAgentsPerCategory)
        {
            var newValue = 1f * maxAgentsPerCategory[kvp.Key] / globalMaxAgentNumber * globalNewAgentsPerMinute;
            newValues[kvp.Key] = newValue;
        }

        return newValues;
    }

    private bool HaveEntryZonesChanged()
    {
        var categoriesInScene = new List<AgentCategory>();
        foreach (var entryZone in _entryZones)
        {
            if (entryZone == null)
            {
                Debug.Log("HaveEntryZonesChanged? undefined");
                return false; //hack
            }
            var category = entryZone.GetAgentCategory();
            if (!categoriesInScene.Contains(category))
                categoriesInScene.Add(category);

            if (!_agentCategories.Contains(category))
            {
                Debug.Log("HaveEntryZonesChanged? category not found");
                return true;
            }
        }
        if (_agentCategories.Count != categoriesInScene.Count)
        {
            Debug.Log("HaveEntryZonesChanged? count not equal");
            return true;
        }
        return false;
    }

    private Simulation _simulationCache;
    private Simulation _simulation
    {
        get
        {
            if (!_simulationCache)
                _simulationCache = GetComponent<Simulation>();
            return _simulationCache;
        }
    }
}
