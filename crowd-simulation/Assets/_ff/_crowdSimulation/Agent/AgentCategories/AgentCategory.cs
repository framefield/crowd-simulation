
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : ScriptableObject
{
    public Color AgentColor = Color.white;
    public List<AttractionInterest> Interests;

    //todo: use dictionary with custom inspector 
    public float GetAttractionInterest(AttractionCategory attractionCategory)
    {
        foreach (var interest in Interests)
            if (interest.AttractionCategory == attractionCategory)
                return interest.InterestValue;
        return 0f;
    }

    [Serializable]
    public class AttractionInterest
    {
        public AttractionCategory AttractionCategory;
        public float InterestValue;

        public AttractionInterest(AttractionCategory attraction)
        {
            AttractionCategory = attraction;
        }
    }

}

