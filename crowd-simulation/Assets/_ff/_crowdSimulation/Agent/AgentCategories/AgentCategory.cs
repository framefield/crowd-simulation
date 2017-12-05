
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : InterestCategory
{
    public float MaxTimeOnMarket = float.PositiveInfinity;
    public Interests Interests;
    public Interests SocialInterests;
    public float Stamina;
    public float MaxTimeTryingToReachInteraction;
}

