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
        SetLockedInterestValue(0f);
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
        MakeInterestSimulationStep();

        RenderAttractedness();

        if (_currentState == State.WalkingToExit)
            return;


        if (HasSatisfiedAllInterests())
        {
            _agentWalking.SetDestination(_exit);
            _currentState = State.WalkingToExit;
            return;
        }

        // search for possible targets > possible state are: randomWalking, walkingToPOI, walkingToPerson

        var favouritePOI = ChoosePointOfInterest();
        var favouritePerson = ChoosePersonInNeighbourhood();

        var hasFoundPOI = favouritePOI != null;
        var hasFoundPerson = favouritePerson != null;

        if (_currentState == State.DoingTransaction || _currentState == State.DoingPersonTransaction)
        {
            bool transactionTargetIsStillInRange = false;
            if (_currentState == State.DoingTransaction)
                transactionTargetIsStillInRange = favouritePOI != null && _lockedInterest == favouritePOI.InterestCategory;
            if (_currentState == State.DoingPersonTransaction)
                transactionTargetIsStillInRange = favouritePerson != null && _lockedInterest == favouritePerson.AgentCategory as InterestCategory;

            if (!transactionTargetIsStillInRange)
            {
                // cancel transaction
                Debug.Log("cancel transaction with lockedintererst : " + _lockedInterest);
                _lockedInterest = null;
                _currentState = State.RandomWalking;
                return;
            }

            if (_currentState == State.DoingPersonTransaction)
                _agentWalking.SetDestination(favouritePerson.transform.position);


            if (!HasFinishedTransaction())
                return;

            CompleteTransaction();
            _currentState = State.RandomWalking;
            return;
        }

        if (!hasFoundPOI && !hasFoundPerson)
        {
            if (HasSpentMaxTimeOnMarket())
                SetAllInterestsToZero();

            if (_currentState == State.WalkingToPOI || _currentState == State.WalkingToPerson)            // lost target -> change to random walk
            {
                _lockedInterest = null;
                _currentState = State.RandomWalking;
                _agentWalking.SetNewRandomDestination();
                return;
            }
            if (_currentState == State.RandomWalking)
            {
                if (_agentWalking.CheckIfReachedRandomDestination())
                {
                    _agentWalking.SetNewRandomDestination();
                    return;
                }
            }
        }

        if (hasFoundPOI)
        {
            if (_currentState == State.RandomWalking) // just found poi
            {
                _agentWalking.SetDestination(favouritePOI.transform.position);
                _lockedInterest = favouritePOI.InterestCategory;
                _lockedInterestTime = Time.time;
                _currentState = State.WalkingToPOI;
                return;
            }
            if (_currentState == State.WalkingToPOI) // continue towards poi
            {
                if (ReachedPOI(favouritePOI))
                {
                    InitTransaction();
                    _currentState = State.DoingTransaction;
                    return;
                }

                var hasTriedForTooLong = Time.time - _lockedInterestTime > AgentCategory.MaxTimeTryingToReachInteraction;
                if (hasTriedForTooLong)
                {
                    CompleteTransaction();
                    _currentState = State.RandomWalking;
                    return;
                }
            }
        }

        if (hasFoundPerson)
        {
            if (_currentState == State.RandomWalking)
            {
                _agentWalking.SetDestination(favouritePerson.transform.position);
                _lockedInterest = favouritePerson.AgentCategory as InterestCategory;
                _lockedInterestTime = Time.time;
                _currentState = State.WalkingToPerson;
                return;
            }

            if (_currentState == State.WalkingToPerson)
            {
                _agentWalking.SetDestination(favouritePerson.transform.position);
                if (HasReachedPerson(favouritePerson))
                {
                    InitTransaction();
                    _currentState = State.DoingPersonTransaction;
                }
                return;
            }
        }
    }

    private bool HasReachedPerson(Agent person)
    {
        var distanceToInterlocutor = Vector3.Distance(person.transform.position, transform.position);
        return distanceToInterlocutor < SocialTransactionRadius;
    }

    private void InitTransaction()
    {
        _transactionStartTime = Time.time;
    }

    void OnDrawGizmos()
    {
        if (_lockedInterest != null)
        {
            Gizmos.color = _lockedInterest.Color;
            Gizmos.DrawSphere(transform.position + 3f * Vector3.up, 0.3f);
        }
        if (_currentState == State.DoingTransaction || _currentState == State.DoingPersonTransaction)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position + 4f * Vector3.up, 0.2f);
        }
    }

    private Agent ChoosePersonInNeighbourhood()
    {
        Agent mostAttractiveInterlocutor = null;
        float highestAttractiveness = 0f;

        foreach (KeyValuePair<InterestCategory, float> socialInterest in _currentSocialInterests)
        {
            var interlocutorsCategory = socialInterest.Key as AgentCategory;
            var interlocutorsAttractiveness = socialInterest.Value;

            var closestNeighbour = Simulation.Instance.FindClosestNeighbourOfCategory(interlocutorsCategory, this);
            if (closestNeighbour == null)
                continue;

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
        if (_lockedInterest == null)
            return;

        var key = _lockedInterest;
        float currentInterest;
        if (_currentInterests.ContainsKey(key))
            currentInterest = _currentInterests[key];
        else
            currentInterest = _currentSocialInterests[key];

        currentInterest = Mathf.Max(currentInterest + AgentCategory.Stamina, 0f);
        SetLockedInterestValue(currentInterest);
    }

    private void SetLockedInterestValue(float newInterestValue)
    {
        var key = _lockedInterest;
        if (_currentInterests.ContainsKey(key))
            _currentInterests[key] = newInterestValue;
        else
            _currentSocialInterests[key] = newInterestValue;
    }

    private bool ReachedPOI(PointOfInterest poi)
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
        // if (_lockedInterest != null)
        //     colorByCategory = _lockedInterest.Color;
        // else
        // {
        colorByCategory = AgentCategory.Color;
        // }

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

    public Interests _currentInterests;
    public Interests _currentSocialInterests;

    private InterestCategory _lockedInterest;
    private float _lockedInterestTime;

    private float _transactionStartTime;

    private float _creationTime;
    private State _currentState;
    private enum State { RandomWalking, WalkingToPOI, WalkingToPerson, WalkingToExit, DoingTransaction, DoingPersonTransaction }
}
