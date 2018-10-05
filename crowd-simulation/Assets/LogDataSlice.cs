using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogDataSlice
{
    public float SimulationTimeInSeconds;
    public Vector3 Location;
    public List<float> CurrentInterestValues = new List<float>();
    public List<float> CurrentSocialInterestValues = new List<float>();
    public LogDataSlice(Agent agent)
    {
        SimulationTimeInSeconds = Time.time;
        Location = agent.transform.position;

        foreach (var kvp in agent.CurrentInterests)
        {
            CurrentInterestValues.Add(kvp.Value);
        }

        foreach (var kvp in agent.CurrentSocialInterests)
        {
            CurrentSocialInterestValues.Add(kvp.Value);
        }
    }
}