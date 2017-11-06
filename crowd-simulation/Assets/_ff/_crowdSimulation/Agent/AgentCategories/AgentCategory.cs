
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : ScriptableObject
{
    public Color AgentColor = Color.white;
    public List<Interest> Interests;
    public SerializableDictionary<AttractionCategory, float> InterestDict;

    //todo: use dictionary with custom inspector 
    public float GetInterest(AttractionCategory attractionCategory)
    {
        foreach (var interest in Interests)
            if (interest.AttractionCategory == attractionCategory)
                return interest.Attractiveness;
        return 0f;
    }
}

