using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MaxNumberOfAgents : SerializableDictionary<AgentCategory, int>
{
    public override bool ShouldRenderReadOnly()
    {

        return false;
    }
}
