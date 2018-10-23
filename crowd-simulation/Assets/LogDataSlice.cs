using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogDataSlice
{
    public float SimulationTimeInSeconds;
    public Vector3 Position;
    public Interests CurrentInterests;
    public Interests CurrentSocialInterests;
    public InterestCategory LockedInterest;
    private Agent _agent;
    public LogDataSlice(Agent agent)
    {
        SimulationTimeInSeconds = Time.time;
        Position = agent.transform.position;
        CurrentInterests = agent.CurrentInterests.Duplicate();
        CurrentSocialInterests = agent.CurrentSocialInterests.Duplicate();
        LockedInterest = agent.LockedInterest;

        _agent = agent;
    }

    public string ToCSVString(List<InterestCategory> interestCategories)
    {
        var csvLine = "";
        csvLine += _agent.id + "\t"
        + SimulationTimeInSeconds + "\t"
        + Position.x + "\t"
        + Position.y + "\t"
        + Position.z + "\t"
        + _agent.AgentCategory.name + "\t";

        var lockedInterest = LockedInterest != null ? LockedInterest.name : "-";
        csvLine += lockedInterest + "\t";

        foreach (var category in interestCategories)
        {
            if (CurrentInterests.ContainsKey(category))
            {
                csvLine += CurrentInterests[category] + "\t";
                continue;
            }

            if (CurrentSocialInterests.ContainsKey(category))
            {
                csvLine += CurrentSocialInterests[category] + "\t";
                continue;
            }
            csvLine += "0.0" + "\t";
        }
        csvLine += "\n";
        return csvLine;
    }
}