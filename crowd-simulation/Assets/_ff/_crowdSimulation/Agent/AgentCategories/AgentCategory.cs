
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : ScriptableObject
{
    public Color AgentColor = Color.white;
    public Interests AgentInterests;
    public float Stamina;
}

