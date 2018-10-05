using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Interests : SerializableDictionary<InterestCategory, float>
{
    public override bool ShouldRenderReadOnly()
    {
        return false;
    }
}
