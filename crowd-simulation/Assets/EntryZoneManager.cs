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

    [TextArea]
    private string _configurationSummary;

    void Start()
    {

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
        _entryZonesInScene = Object.FindObjectsOfType<EntryZone>();
        _newAgentsPerSecondPerCategory = new AgentsPerSecond();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
        foreach (var entryZone in _entryZonesInScene)
        {
            var category = entryZone.GetAgentCategory();
            if (!_newAgentsPerSecondPerCategory.ContainsKey(category))
            {
                _newAgentsPerSecondPerCategory.Add(category, 0f);
                _maxNumberOfAgentsPerCategory.Add(category, 0);
            }
        }
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
            newValues[kvp.Key] = Mathf.RoundToInt(newValue);
        }

        return newValues;
    }

    private EntryZone[] _entryZonesInScene;
}
