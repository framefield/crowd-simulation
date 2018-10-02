using System.Collections;
using System.Collections.Generic;
using ff.utils;
using UnityEngine;

public class EntryZoneManager : MonoBehaviour
{
    [Header("PARAMETERS")]
    [SerializeField]
    private AgentsPerMinute _agentsPerMinutePerCategory;

    [SerializeField]
    private int _globalMaxAgentNumber;

    [Header("READ ONLY")]
    [SerializeField]
    private MaxNumberOfAgents _maxNumberOfAgentsPerCategory;

    [TextArea]
    private string _configurationSummary;

    void Start()
    {

    }

    void OnValidate()
    {
        Debug.Log("OnValidate");
        var newValues = new MaxNumberOfAgents();
        var totalAgentsPerMinute = CalculateTotalAgentsPerMinute();
        foreach (var kvp in _maxNumberOfAgentsPerCategory)
        {
            var newValue = _agentsPerMinutePerCategory[kvp.Key] / totalAgentsPerMinute * _globalMaxAgentNumber;
            Debug.LogFormat(">> {0}", newValue);
            newValues[kvp.Key] = Mathf.RoundToInt(newValue);

        }
        _maxNumberOfAgentsPerCategory = newValues;
    }

    [ContextMenu("InitData")]
    void InitData()
    {
        _entryZonesInScene = Object.FindObjectsOfType<EntryZone>();
        _agentsPerMinutePerCategory = new AgentsPerMinute();
        _maxNumberOfAgentsPerCategory = new MaxNumberOfAgents();
        foreach (var entryZone in _entryZonesInScene)
        {
            var category = entryZone.GetAgentCategory();
            if (!_agentsPerMinutePerCategory.ContainsKey(category))
            {
                _agentsPerMinutePerCategory.Add(category, 0f);
                _maxNumberOfAgentsPerCategory.Add(category, 0);
            }
        }
    }

    private float CalculateTotalAgentsPerMinute()
    {
        var totalAgentsPerMinute = 0f;
        foreach (var kvp in _agentsPerMinutePerCategory)
        {
            totalAgentsPerMinute += kvp.Value;
        }
        return totalAgentsPerMinute;
    }

    private EntryZone[] _entryZonesInScene;
}
