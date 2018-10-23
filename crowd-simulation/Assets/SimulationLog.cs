using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class SimulationLog : MonoBehaviour
{
    [SerializeField]
    private float _logRate = 1f;

    public List<AgentLogData> LoggedAgents = new List<AgentLogData>();

    void Start()
    {
        _simulation.OnAgentSpawned += HandleSpawnedAgent;
        StartCoroutine(WriteTimeIntoFile());
    }

    private IEnumerator WriteTimeIntoFile()
    {
        using (StreamWriter sw = new StreamWriter("log.csv"))
        {
            sw.Write(GenerateHeader(_interestCategoriesInProject));

            while (true)
            {
                _timeSinceLastLog += Time.deltaTime;
                if (_timeSinceLastLog > _logRate)
                {
                    _timeSinceLastLog -= _logRate;
                    foreach (var agentLogData in LoggedAgents)
                    {
                        var csvLine = agentLogData.LogSlice(_interestCategoriesInProject);
                        sw.Write(csvLine);
                    }
                }
                yield return null;
            }
        }
    }

    void OnDrawGizmos()
    {

        foreach (var agent in LoggedAgents)
        {
            var slices = agent.LogDataSlices;
            for (int i = 0; i < slices.Count - 1; i++)
            {
                var lockedInterest = slices[i + 1].LockedInterest;
                Gizmos.color = lockedInterest != null ? lockedInterest.Color : agent.Agent.AgentCategory.Color;
                Gizmos.DrawLine(slices[i].Position, slices[i + 1].Position);
            }
        }
    }


    public void HandleSpawnedAgent(Agent agent)
    {
        LoggedAgents.Add(new AgentLogData(agent));
    }

    private static string GenerateHeader(List<InterestCategory> categories)
    {
        var baseHeader = String.Format("agentID\tsimulationTimeInSeconds\tpositionX\tpositionY\tpositionZ\tAgentCategory\tLockedInterest\t");

        var interestsHeader = "";
        foreach (var category in categories)
        {
            interestsHeader += "Interest." + category.name + "\t";
        }
        return baseHeader + interestsHeader + "\n";
    }

    private float _timeSinceLastLog;

    private List<InterestCategory> _interestCategoriesInProjectCache;
    private List<InterestCategory> _interestCategoriesInProject
    {
        get
        {
            if (_interestCategoriesInProjectCache == null)
            {
                _interestCategoriesInProjectCache = new List<InterestCategory>();
                var catGUIDs = AssetDatabase.FindAssets("t:InterestCategory");
                foreach (var c in catGUIDs)
                {
                    var catPath = AssetDatabase.GUIDToAssetPath(c);
                    var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(InterestCategory)) as InterestCategory;
                    _interestCategoriesInProjectCache.Add(category);
                }
            }
            return _interestCategoriesInProjectCache;
        }
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
