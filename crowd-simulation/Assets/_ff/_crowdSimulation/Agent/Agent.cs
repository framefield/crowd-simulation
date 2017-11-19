using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField] AgentCategory AgentCategory;
    [SerializeField] Vector3 Exit;


    void Start()
    {
        _currentInterests = new Interests();
        _currentInterests.CopyFrom(AgentCategory.AgentInterests);

        _agentWalking = GetComponent<AgentWalking>();

        RenderAttractedness();
    }

    private bool HasSatisfiedAllInterests()
    {
        var cullminatedInterests = 0f;

        foreach (var value in _currentInterests.Values)
        {
            cullminatedInterests += value;
        }
        return cullminatedInterests == 0f;
    }

    void Update()
    {
        if (HasSatisfiedAllInterests())
        {
            _agentWalking.SetDestination(Exit);
            return;
        }

        var choosenPointOfInterest = ChoosePointOfInterest();
        var foundAttraction = choosenPointOfInterest != null;
        var isLockedToAttraction = _lockedPointOfInterest != null;

        if (!foundAttraction)
        {
            if (isLockedToAttraction)
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
        var key = _lockedPointOfInterest.InterestCategory;
        var currentInterest = _currentInterests[key];

        if (ReachedCurrentPOI())
            currentInterest = 0;
        else
            currentInterest = Mathf.Max(currentInterest + AgentCategory.Stamina, 0f);

        _currentInterests[key] = currentInterest;
    }

    private bool ReachedCurrentPOI()
    {
        var distance = Vector3.Distance(transform.position, _lockedPointOfInterest.transform.position);
        return distance < _lockedPointOfInterest.InnerSatisfactionRadius;
    }

    private PointOfInterest ChoosePointOfInterest()
    {
        PointOfInterest choosenPOI = null;
        var maxFoundAttraction = 0f;

        foreach (KeyValuePair<InterestCategory, float> interest in _currentInterests)
        {
            var mostVisiblePOI = GetMostVisibilePointOfInterest(interest.Key);

            float visibility;

            var foundPOI = mostVisiblePOI != null;

            visibility = foundPOI
               ? mostVisiblePOI.GetVisibilityAtGlobalPosition(this.transform.position)
               : 0f;

            var attraction = visibility * GetCurrentInterest(interest.Key);
            if (attraction > maxFoundAttraction)
            {
                choosenPOI = mostVisiblePOI;
                maxFoundAttraction = attraction;
            }
        }
        return choosenPOI;
    }

    public PointOfInterest GetMostVisibilePointOfInterest(InterestCategory interestCategory)
    {
        PointOfInterest mostVisiblePointOfInterest = null;
        var maxFoundVisibility = 0f;
        if (!Simulation.Instance.PointsOfInterest.ContainsKey(interestCategory))
            return null;

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

    public InterestCategory GetHighestInterest()
    {
        var highestInterestValue = 0f;
        InterestCategory highestInterestCategory = null;
        foreach (KeyValuePair<InterestCategory, float> pair in _currentInterests)
        {
            if (pair.Value > highestInterestValue)
            {
                highestInterestCategory = pair.Key;
                highestInterestValue = pair.Value;
            }
        }
        return highestInterestCategory;
    }


    float _attractednessOnStart = -1f;
    private void RenderAttractedness()
    {
        Color colorByCategory;
        if (_lockedPointOfInterest != null)
            colorByCategory = _lockedPointOfInterest.InterestCategory.Color;
        else
        {
            colorByCategory = GetHighestInterest().Color * 0.5f;
        }

        var culminatedAttractedness = 0f;
        foreach (KeyValuePair<InterestCategory, float> pair in _currentInterests)
            culminatedAttractedness += pair.Value;

        if (_attractednessOnStart == -1f)
        {
            _attractednessOnStart = culminatedAttractedness;
        }

        var brightness = culminatedAttractedness / _attractednessOnStart;
        var color = colorByCategory * new Color(brightness, brightness, brightness, 1);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }


    private AgentWalking _agentWalking;
    private PointOfInterest _lockedPointOfInterest;
    public Interests _currentInterests;

}
