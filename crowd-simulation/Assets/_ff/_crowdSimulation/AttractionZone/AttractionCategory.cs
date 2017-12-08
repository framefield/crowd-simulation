
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new AttractionCategory", menuName = "CrowdSimulation/AttractionCategory", order = 2)]
public class AttractionCategory : ScriptableObject
{
    public Color Color = Color.white;
    public float TransactionTime = 1f;
}

