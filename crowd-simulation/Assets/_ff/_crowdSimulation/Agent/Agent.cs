using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] AgentCategory AgentCategory;

    void Start()
    {
        _currentInterests = new Interests();
        _currentInterests.CopyFrom(AgentCategory.AgentInterests);

        _agentWalking = GetComponent<AgentWalking>();

        RenderAttractedness();
    }

    void Update()
    {
        var choosenPointOfInterest = ChoosePointOfInterest();
        var foundAttraction = choosenPointOfInterest != null;
        var wasLockedToAttraction = _lockedPointOfInterest != null;

        if (!foundAttraction)
        {
            if (wasLockedToAttraction)
            {
                _lockedPointOfInterest = null;
                _agentWalking.SetNewRandomDestination();
                return;
            }
            else
            {
                if (_agentWalking.CheckIfReachedRandomDestination())
                {
                    _agentWalking.SetNewRandomDestination();
                }
                return;
            }
        }

        if (choosenPointOfInterest == _lockedPointOfInterest)
        {
            MakeInterestSimulationStep();
            RenderAttractedness();
            return;
        }

        _agentWalking.SetDestination(choosenPointOfInterest.transform.position);
        _lockedPointOfInterest = choosenPointOfInterest;
    }

    private void MakeInterestSimulationStep()
    {
        var key = _lockedPointOfInterest.AttractionCategory;
        var value = _currentInterests[key];
        value = Mathf.Max(value + AgentCategory.Stamina, 0f);
        _currentInterests[key] = value;
    }


    private PointOfInterest ChoosePointOfInterest()
    {
        PointOfInterest choosenPointOfInterest = null;
        var maxFoundInterestedness = 0f;

        foreach (KeyValuePair<InterestCategory, float> interest in _currentInterests)
        {
            var attraction = GetMostVisibilePointOfInterest(interest.Key);

            float generalAttraction;

            var foundAttraction = attraction != null;

            generalAttraction = foundAttraction
               ? attraction.GetVisibilityAtGlobalPosition(this.transform.position)
               : 0f;

            var personalAttraction = generalAttraction * GetCurrentInterest(interest.Key);
            if (personalAttraction > maxFoundInterestedness)
            {
                choosenPointOfInterest = attraction;
                maxFoundInterestedness = personalAttraction;
            }
        }
        return choosenPointOfInterest;
    }

    public PointOfInterest GetMostVisibilePointOfInterest(InterestCategory interestCategory)
    {
        PointOfInterest mostVisiblePointOfInterest = null;
        var maxFoundVisibility = 0f;
        foreach (var poi in Simulation.Instance.PointsOfInterest[interestCategory])
        {
            var poiVisibility = poi.GetVisibilityAtGlobalPosition(this.transform.position);
            if (poiVisibility > maxFoundVisibility)
            {
                mostVisiblePointOfInterest = poi;
                maxFoundVisibility = poiVisibility;
            }
        }
        return mostVisiblePointOfInterest;
    }

    public float GetCurrentInterest(InterestCategory interestCategory)
    {
        if (_currentInterests.ContainsKey(interestCategory))
            return _currentInterests[interestCategory];
        else
            return 0f;
    }

    float _attractednessOnStart = -1f;
    private void RenderAttractedness()
    {
        var culminatedAttractedness = 0f;
        foreach (KeyValuePair<InterestCategory, float> pair in _currentInterests)
            culminatedAttractedness += pair.Value;

        if (_attractednessOnStart == -1f)
        {
            _attractednessOnStart = culminatedAttractedness;
        }

        var brightness = culminatedAttractedness / _attractednessOnStart;
        var color = AgentCategory.AgentColor * new Color(brightness, brightness, brightness, 1);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }


    private AgentWalking _agentWalking;
    private PointOfInterest _lockedPointOfInterest;
    public Interests _currentInterests;

}
