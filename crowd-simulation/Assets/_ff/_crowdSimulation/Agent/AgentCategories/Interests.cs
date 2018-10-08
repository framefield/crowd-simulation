using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Interests : SerializableDictionary<InterestCategory, float>
{
    public Interests Duplicate()
    {
        var duplicate = new Interests();
        foreach (var kvp in this)
        {
            duplicate.Add(kvp.Key, kvp.Value);
        }
        return duplicate;
    }
}
