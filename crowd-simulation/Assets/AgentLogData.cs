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

    public string LogSlice(List<InterestCategory> interestCategories)
    {
        if (Agent == null)
            return "";

        var newSlice = new LogDataSlice(Agent);

        LogDataSlices.Add(newSlice);

        return newSlice.ToCSVString(interestCategories);
    }

    public AgentLogData Duplicate()
    {
        var newLogData = new AgentLogData(Agent);
        newLogData.LogDataSlices = new List<LogDataSlice>(LogDataSlices);
        return newLogData;
    }
}