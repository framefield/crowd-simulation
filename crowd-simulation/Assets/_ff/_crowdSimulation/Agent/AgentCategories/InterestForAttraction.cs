
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct Interest
{
    public AttractionCategory AttractionCategory;
    public float Attractiveness;

    public Interest(AttractionCategory attraction)
    {
        AttractionCategory = attraction;
        Attractiveness = 0f;
    }

    public Interest(AttractionCategory attraction, float interestValue)
    {
        AttractionCategory = attraction;
        Attractiveness = interestValue;
    }

    static public List<Interest> Duplicate(List<Interest> source)
    {
        var newList = new List<Interest>();

        foreach (var interest in source)
        {
            var interestCopy = new Interest(interest.AttractionCategory, interest.Attractiveness);
            newList.Add(interestCopy);
        }
        return newList;
    }
}

