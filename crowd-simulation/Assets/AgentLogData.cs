using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AgentLogData
{
    public Agent Agent;
    public List<LogDataSlice> LogDataSlices = new List<LogDataSlice>();

    public AgentLogData(Agent agent)
    {
        Agent = agent;
    }

    public void LogSlice()
    {
        if (Agent == null)
            return;
        LogDataSlices.Add(new LogDataSlice(Agent));
    }

    public static string GenerateCSV(List<AgentLogData> data)
    {
        if (data.Count < 1)
            return "";

        var categories = AssetDatabase.FindAssets("t:InterestCategory");

        var csv = GenerateHeader(data[0]);
        foreach (var d in data)
        {
            foreach (var slice in d.LogDataSlices)
            {
                csv += d.Agent.id + "\t"
                + slice.SimulationTimeInSeconds + "\t"
                + slice.Position.x + "\t"
                + slice.Position.y + "\t"
                + slice.Position.z + "\t"
                + d.Agent.AgentCategory.name + "\t";

                foreach (var catGUID in AssetDatabase.FindAssets("t:InterestCategory"))
                {
                    var catPath = AssetDatabase.GUIDToAssetPath(catGUID);
                    var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(InterestCategory)) as InterestCategory;

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
                    csv += "0" + "\t";

                }
                csv += "\n";
            }
        }
        return csv;
    }


    public static string GenerateHeader(AgentLogData data)
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
}