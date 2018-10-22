using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
            var path = Application.dataPath + "/log.csv";
            EditorApplication.isPaused = true;
            WriteGenerateCSV(LogData, path);
            EditorApplication.isPaused = false;
            Debug.Log(path);
        }
    }


    public void HandleSpawnedAgent(Agent agent)
    {
        LogData.Add(new AgentLogData(agent));
    }

    private static List<AgentLogData> DuplicateAgentLogData(List<AgentLogData> original)
    {
        var duplicate = new List<AgentLogData>();
        foreach (var agentData in original)
        {
            duplicate.Add(agentData.Duplicate());
        }
        return duplicate;
    }

    private void WriteGenerateCSV(List<AgentLogData> data, string path)
    {
        var dataCopy = DuplicateAgentLogData(data);
        // if (dataCopy.Count < 1)
        // yield break;


        var interestCategories = new List<InterestCategory>();
        var catGUIDs = AssetDatabase.FindAssets("t:InterestCategory");
        foreach (var c in catGUIDs)
        {
            var catPath = AssetDatabase.GUIDToAssetPath(c);
            var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(InterestCategory)) as InterestCategory;
            interestCategories.Add(category);
        }


        var csv = GenerateHeader(dataCopy[0]);
        var i = 0f;
        foreach (var d in dataCopy)
        {
            i++;
            var progress = i / dataCopy.Count;
            foreach (var slice in d.LogDataSlices)
            {
                csv += d.Agent.id + "\t"
                + slice.SimulationTimeInSeconds + "\t"
                + slice.Position.x + "\t"
                + slice.Position.y + "\t"
                + slice.Position.z + "\t"
                + d.Agent.AgentCategory.name + "\t";

                foreach (var category in interestCategories)
                {
                    if (slice.CurrentInterests.ContainsKey(category))
                    {
                        csv += slice.CurrentInterests[category] + "\t";
                        continue;
                    }

                    if (slice.CurrentSocialInterests.ContainsKey(category))
                    {
                        csv += slice.CurrentSocialInterests[category] + "\t";
                        continue;
                    }
                    csv += "0.0f" + "\t";
                }
                csv += "\n";
            }
        }
        System.IO.File.WriteAllText(path, csv);
    }

    private static string GenerateHeader(AgentLogData data)
    {
        var baseHeader = String.Format("agentID\tsimulationTimeInSeconds\tpositionX\tpositionY\tpositionZ\tAgentCategory\t");

        var interestsHeader = "";
        foreach (var catGUID in AssetDatabase.FindAssets("t:InterestCategory"))
        {
            var catPath = AssetDatabase.GUIDToAssetPath(catGUID);
            var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(InterestCategory)) as InterestCategory;
            interestsHeader += "Interest." + category.name + "\t";
        }
        return baseHeader + interestsHeader + "\n";
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
