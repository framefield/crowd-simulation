
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AgentCategory", menuName = "CrowdSimulation/AgentCategory", order = 1)]
public class AgentCategory : ScriptableObject
{
    public Color AgentColor = Color.white;
    public List<AttractionInterest> Interests;


    [Serializable]
    public class AttractionInterest
    {
        public AttractionZone.AttractionCategory Attraction;
        public float Interest;

        public AttractionInterest(AttractionZone.AttractionCategory attraction)
        {
            Attraction = attraction;
        }
    }

    void OnEnable()
    {
        var categories = Enum.GetValues(typeof(AttractionZone.AttractionCategory));
        Interests = new List<AttractionInterest>();
        foreach (var categorie in categories)
        {
            var categorieCasted = (AttractionZone.AttractionCategory)categorie;
            Interests.Add(new AttractionInterest(categorieCasted));
        }
    }


}

