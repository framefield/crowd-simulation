using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public AgentCategory AgentCategory;
    private Vector3 _exit;

    [SerializeField] float SocialInteractionRadius;
    [SerializeField] float SocialTransactionRadius;

    void Start()
    {
        _currentInterests = new Interests();
        _currentInterests.CopyFrom(AgentCategory.Interests);

        _currentSocialInterests = new Interests();
        _currentSocialInterests.CopyFrom(AgentCategory.SocialInterests);

        _agentWalking = GetComponent<AgentWalking>();
        RenderAttractedness();

        _currentState = State.RandomWalking;
        _creationTime = Time.time;
        _exit = transform.position;
        _allRenderers = GetComponentsInChildren<Renderer>();
        _camera = Camera.main;
    }

    private void CompleteTransaction()
    {
        //complete transaction
        SetInterestOfCurrentPOISCategory(0f);
        _lockedInterest = null;
    }

    private bool HasFinishedTransaction()
    {
        var timeSinceTransactionStarted = Time.time - _transactionStartTime;
        return timeSinceTransactionStarted > _lockedInterest.TransactionTime;
    }

    private Camera _camera;
    void Update()
    {
        RenderAttractedness();

        if (_currentState == State.WalkingToExit)
            return;

        if (_currentState == State.DoingTransaction)
        {
            if (!HasFinishedTransaction())
                return;

            CompleteTransaction();
            _currentState = State.RandomWalking;
        }

        if (HasSatisfiedAllInterests())
        {
            _agentWalking.SetDestination(_exit);
            _currentState = State.WalkingToExit;
            return;
        }

        // search for possible targets

        var choosenPointOfInterest = ChoosePointOfInterest();
        var choosenInterlocutor = SearchForAttractivePersonInNeighbourhood();

        var foundPOI = choosenPointOfInterest != null;
        var foundInterlocutor = choosenInterlocutor != null;

        if (!foundPOI)
        {
            if (HasSpentMaxTimeOnMarket())
                SetAllInterestsToZero();

            if (_currentState == State.WalkingToPOI)
            {
                _lockedInterest = null;
                _currentState = State.RandomWalking;
                _agentWalking.SetNewRandomDestination();
                return;
            }
            else
            {
                if (foundInterlocutor)
                {
                    _currentState = State.WalkingToInterlocutor;
                    _agentWalking.SetDestination(choosenInterlocutor.transform.position);
                    _lockedInterest = choosenInterlocutor.AgentCategory as InterestCategory;

                    var distanceToInterlocutor = Vector3.Distance(choosenInterlocutor.transform.position, transform.position);
                    if (distanceToInterlocutor < SocialTransactionRadius)
                        _currentState = State.DoingTransaction;

                    return;
                }
                _lockedInterest = null;
                _currentState = State.RandomWalking;

                if (_agentWalking.CheckIfReachedRandomDestination())
                {
                    _agentWalking.SetNewRandomDestination();
                    return;
                }
                return;
            }
        }

        if (choosenPointOfInterest.InterestCategory == _lockedInterest)
        {
            if (ReachedCurrentPOI(choosenPointOfInterest))
            {
                _currentState = State.DoingTransaction;
                _transactionStartTime = Time.time;
                return;
            }

            MakeInterestSimulationStep();
            return;
        }

        _agentWalking.SetDestination(choosenPointOfInterest.transform.position);
        _currentState = State.WalkingToPOI;
        _lockedInterest = choosenPointOfInterest.InterestCategory;
    }

    void OnDrawGizmos()
    {
        if (_currentState == State.DoingTransaction)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position + 3f * Vector3.up, 0.2f);
        }
    }

    private Agent SearchForAttractivePersonInNeighbourhood()
    {
        Agent mostAttractiveInterlocutor = null;
        float highestAttractiveness = 0f;

        foreach (KeyValuePair<InterestCategory, float> socialInterest in _currentSocialInterests)
        {
            var interlocutorsCategory = socialInterest.Key as AgentCategory;
            var interlocutorsAttractiveness = socialInterest.Value;

            var closestNeighbour = Simulation.Instance.FindClosestNeighbourOfCategory(interlocutorsCategory, this);
            var neighbourInSocialInteractionRadius = Vector3.Distance(closestNeighbour.transform.position, transform.position) < SocialInteractionRadius;

            if (neighbourInSocialInteractionRadius && interlocutorsAttractiveness > highestAttractiveness)
            {
                mostAttractiveInterlocutor = closestNeighbour;
                highestAttractiveness = interlocutorsAttractiveness;
            }
        }
        return mostAttractiveInterlocutor;
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

    private bool HasSpentMaxTimeOnMarket()
    {
        return (Time.time - _creationTime) > AgentCategory.MaxTimeOnMarket;
    }

    private void SetAllInterestsToZero()
    {
        _currentInterests.Clear();
    }

    private void MakeInterestSimulationStep()
    {
        var key = _lockedInterest;
        var currentInterest = _currentInterests[key];
        currentInterest = Mathf.Max(currentInterest + AgentCategory.Stamina, 0f);
        SetInterestOfCurrentPOISCategory(currentInterest);
    }

    private void SetInterestOfCurrentPOISCategory(float newInterestValue)
    {
        var key = _lockedInterest;
        if (_currentInterests.ContainsKey(key))
            _currentInterests[key] = newInterestValue;
        else
            _currentSocialInterests[key] = newInterestValue;
    }

    private bool ReachedCurrentPOI(PointOfInterest poi)
    {
        var distance = Vector3.Distance(transform.position, poi.transform.position);
        return distance < poi.InnerSatisfactionRadius;
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
        if (_lockedInterest != null)
            colorByCategory = _lockedInterest.Color;
        else
        {
            colorByCategory = AgentCategory.Color;
        }

        var culminatedAttractedness = 0f;
        foreach (KeyValuePair<InterestCategory, float> pair in _currentInterests)
            culminatedAttractedness += pair.Value;

        if (_attractednessOnStart == -1f)
        {
            _attractednessOnStart = culminatedAttractedness;
        }

        var brightness = culminatedAttractedness / _attractednessOnStart;

        foreach (var r in _allRenderers)
            r.material.SetColor("_Color", colorByCategory);
    }

    private Renderer[] _allRenderers = new Renderer[0];
    private AgentWalking _agentWalking;
    private InterestCategory _lockedInterest;

    public Interests _currentInterests;
    public Interests _currentSocialInterests;

    private float _transactionStartTime;
    private float _creationTime;
    private State _currentState;
    private enum State { RandomWalking, WalkingToPOI, WalkingToInterlocutor, WalkingToExit, DoingTransaction }
}
