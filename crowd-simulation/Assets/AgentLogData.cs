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
}