using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string>
{
    public override bool ShouldRenderReadOnly()
    {
        return false;
    }
}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color>
{
    public override bool ShouldRenderReadOnly()
    {
        return false;
    }
}