using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEditor;
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
    private Dictionary<AgentCategory, List<EntryZone>> _entryZoneLookUp;

    [SerializeField]
    private AgentsPerSecond _agentsScheduledForSpawning;

    [SerializeField]
    public List<AgentCategory> _agentCategories;

    [Space(15f)]

    [Header("INTERNAL PREFAB REFERENCE - DO NOT TOUCH")]
    [SerializeField]
    Agent _agentPrefab;

    void Start()
    {
        _entryZoneLookUp = InitEntryZoneLookUp(_entryZones);
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

        foreach (var category in _agentCategories)
        {
            _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

            _agentsScheduledForSpawning[category] += _newAgentsPerSecondPerCategory[category] * Time.deltaTime;

            var agentsToSpawnRightNow = Mathf.FloorToInt(_agentsScheduledForSpawning[category]);
            for (int i = 0; i < agentsToSpawnRightNow && !HasReachedMaxAgents(category); i++)
            {
                var numberOfZonesForCategory = _entryZoneLookUp[category].Count;
                var randomIndex = Random.Range(0, numberOfZonesForCategory);
                var randomEntryZone = _entryZoneLookUp[category][randomIndex];
                _simulation.SpawnAgentAtPosition(randomEntryZone.transform.position, _agentPrefab, randomEntryZone.GetAgentCategory());
                _agentsScheduledForSpawning[category]--;
            }
        }
    }

    private bool HasReachedMaxAgents(AgentCategory category)
    {
        return _simulation.GetNumberOfAgentsInSimulation(category) >= _maxNumberOfAgentsPerCategory[category];
    }

    void OnValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)

            Debug.Log("OnValidate");

        if (HaveEntryZonesChanged() && !EditorApplication.isPlaying)
            DeriveDataFromEntryZones();

        _globalMaxAgentNumber = CalculateTotalMaxAgents();
        _newAgentsPerSecondPerCategory = DeriveAgentsPerMinute(_globalMaxAgentNumber,
                                                                _globalNewAgentsPerSecond,
                                                                _maxNumberOfAgentsPerCategory);

        _SecondsUntilAgentLimitReached = _globalMaxAgentNumber / _globalNewAgentsPerSecond;
    }

    void DeriveDataFromEntryZones()
    {
        Debug.Log("DeriveDataFromEntryZones");
        _agentCategories = new List<AgentCategory>();
        _newAgentsPerSecondPerCategory = new AgentsPerSecond();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
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

    private AgentCategory PickRandomCategory()
    {
        var iRandomCategory = Random.Range(0, _agentCategories.Count);
        return _agentCategories[iRandomCategory];
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
