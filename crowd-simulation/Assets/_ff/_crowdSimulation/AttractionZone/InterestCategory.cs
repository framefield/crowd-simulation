
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new InterestCategory", menuName = "CrowdSimulation/InterestCategory", order = 2)]
public class InterestCategory : ScriptableObject
{
    public Color Color = Color.white;
    public float TransactionTime = 1f;
}

