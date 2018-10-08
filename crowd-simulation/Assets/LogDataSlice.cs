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
    public LogDataSlice(Agent agent)
    {
        SimulationTimeInSeconds = Time.time;
        Position = agent.transform.position;
        CurrentInterests = agent.CurrentInterests.Duplicate();
        CurrentSocialInterests = agent.CurrentSocialInterests.Duplicate();
    }
}