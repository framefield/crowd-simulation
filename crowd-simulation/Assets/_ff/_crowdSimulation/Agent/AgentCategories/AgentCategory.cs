
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : AttractionCategory
{
    [Header("PARAMTERS")]
    public float MaxTimeOnAgora = float.PositiveInfinity;
    public Interests Interests;
    public Interests SocialInterests;
    public float MotivationDepletionIfBored;
}

