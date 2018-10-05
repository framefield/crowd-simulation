using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentLogData
{
    public Agent _agent;
    public AgentCategory _agentCategory;
    public List<InterestCategory> InterestCategories = new List<InterestCategory>();
    public List<AgentCategory> SocialInterestCategories = new List<AgentCategory>();
    public List<LogDataSlice> LogDataSlices = new List<LogDataSlice>();

    public AgentLogData(Agent agent, Action logSliceEvent)
    {
        _agent = agent;
        _agentCategory = agent.AgentCategory;
        foreach (var kvp in agent.CurrentInterests)
        {
            InterestCategories.Add(kvp.Key);
        }
        SocialInterestCategories = new List<AgentCategory>();
        foreach (var kvp in agent.CurrentSocialInterests)
        {
            SocialInterestCategories.Add(kvp.Key as AgentCategory);
        }
        logSliceEvent += LogSlice;
    }

    private void LogSlice()
    {
        if (_agent == null)
            return;
        LogDataSlices.Add(new LogDataSlice(_agent));
    }
}
