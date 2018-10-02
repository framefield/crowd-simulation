using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class EntryZoneManager : MonoBehaviour
{

    [SerializeField]
    bool _spawnAgentOnMouseDown;

    [SerializeField]
    List<EntryZone> _entryZones;

    [Space(15f)]

    [Header("NUMBER OF AGENTS")]
    [SerializeField]
    private MaxNumberOfAgents _maxNumberOfAgentsPerCategory;

    [SerializeField]
    [ReadOnly]
    private int _globalMaxAgentNumber;

    [Header("SPAWNING SPEED")]
    [SerializeField]
    private float _globalNewAgentsPerSecond = 1f;

    [SerializeField]
    private AgentsPerSecond _newAgentsPerSecondPerCategory;

    [Space(15f)]

    [SerializeField]
    [ReadOnly]
    private float _SecondsUntilAgentLimitReached;

    [SerializeField]
    private Dictionary<AgentCategory, List<EntryZone>> _entryZonesOverCategory;

    [SerializeField]
    private AgentsPerSecond _agentsScheduledForSpawning;

    [Space(15f)]

    [Header("INTERNAL PREFAB REFERENCE - DO NOT TOUCH")]
    [SerializeField]
    Agent _agentPrefab;

    void Start()
    {
        _entryZonesOverCategory = InitEntryZonesOverCategory(_entryZones);
        // _numberOfAgentsSpawned = new Dictionary<AgentCategory, int>();
        _agentsScheduledForSpawning = new AgentsPerSecond();

        foreach (var category in _agentCategories)
        {
            if (!_agentsScheduledForSpawning.ContainsKey(category))
            {
                _agentsScheduledForSpawning.Add(category, 0f);
                // _numberOfAgentsSpawned.Add(category, 0);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && _spawnAgentOnMouseDown)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                _simulation.SpawnAgentAtPosition(hit.point, _agentPrefab, PickRandomCategory());
            }
        }
        foreach (var kvp in _entryZonesOverCategory)
        {
            var category = kvp.Key;
            _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);
            _agentsScheduledForSpawning[category] += _newAgentsPerSecondPerCategory[category] * Time.deltaTime;
            Debug.Log(category);
            if (_simulation.GetNumberOfAgentsInSimulation(category) >= _maxNumberOfAgentsPerCategory[category])
                continue;

            var agentsToSpawnRightNow = Mathf.FloorToInt(_agentsScheduledForSpawning[category]);
            for (int i = 0; i < agentsToSpawnRightNow; i++)
            {
                var numberOfZonesForCategory = _entryZonesOverCategory[category].Count;
                var randomIndex = Random.Range(0, numberOfZonesForCategory);
                var randomEntryZone = _entryZonesOverCategory[category][randomIndex];
                _simulation.SpawnAgentAtPosition(transform.position, _agentPrefab, randomEntryZone.GetAgentCategory());
                _agentsScheduledForSpawning[category]--;
            }
        }
    }

    private bool HaveEntryZonesChanged()
    {
        var categoriesInScene = new List<AgentCategory>();
        foreach (var entryZone in _entryZones)
        {
            var category = entryZone.GetAgentCategory();
            if (!categoriesInScene.Contains(category))
                categoriesInScene.Add(category);

            if (!_agentCategories.Contains(category))
                return true;
        }
        if (_agentCategories.Count != categoriesInScene.Count)
            return true;
        return false;
    }

    void OnValidate()
    {
        Debug.Log("OnValidate");

        if (HaveEntryZonesChanged())
            InitData();

        _globalMaxAgentNumber = CalculateTotalMaxAgents();
        _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

        _SecondsUntilAgentLimitReached = _globalMaxAgentNumber / _globalNewAgentsPerSecond;
    }

    [ContextMenu("InitData")]
    void InitData()
    {
        Debug.Log("InitDate");
        _agentCategories = new List<AgentCategory>();
        _newAgentsPerSecondPerCategory = new AgentsPerSecond();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
        // var entryZonesInScene =// Object.FindObjectsOfType<EntryZone>();
        foreach (var entryZone in _entryZones)
        {
            var category = entryZone.GetAgentCategory();
            if (!_newAgentsPerSecondPerCategory.ContainsKey(category))
            {
                _agentCategories.Add(category);
                _newAgentsPerSecondPerCategory.Add(category, 0f);
                _maxNumberOfAgentsPerCategory.Add(category, 100);
            }
        }
    }

    private static Dictionary<AgentCategory, List<EntryZone>> InitEntryZonesOverCategory(List<EntryZone> _entryZones)
    {
        var entryZonesOverCategory = new Dictionary<AgentCategory, List<EntryZone>>();
        // var entryZonesInScene = Object.FindObjectsOfType<EntryZone>();
        foreach (var entryZone in _entryZones)
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

    private AgentCategory PickRandomCategory()
    {
        var iRandomCategory = Random.Range(0, _agentCategories.Count);
        return _agentCategories[iRandomCategory];
    }

    private List<AgentCategory> _agentCategories;

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
