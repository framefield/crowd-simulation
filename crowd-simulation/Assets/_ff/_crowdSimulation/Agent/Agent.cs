using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public AgentCategory AgentCategory;
    public Interests CurrentInterests;
    public Interests CurrentSocialInterests;

    public float PersistencyBonus;
    public float MaxTimeToFollowPerson;

    [SerializeField] float SocialInteractionRadius;
    [SerializeField] float SocialTransactionRadius;

    void Start()
    {
        CurrentInterests = new Interests();
        CurrentInterests.CopyFrom(AgentCategory.Interests);

        CurrentSocialInterests = new Interests();
        CurrentSocialInterests.CopyFrom(AgentCategory.SocialInterests);

        _agentWalking = GetComponent<AgentWalking>();
        RenderAttractedness();

        _currentState = State.RandomWalking;
        _creationTime = Time.time;
        _exit = transform.position;
        _allRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (_currentState == State.WalkingToPerson || _currentState == State.WalkingToPOI)
            ReduceValueOfLockedInterest();
        else
            _isBoredOfWaiting = false;

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

        var interestInPOI = favouritePOI != null ? GetCurrentInterest(favouritePOI.InterestCategory) : 0f;
        var interestInPerson = favouritePerson != null ? GetCurrentInterest(favouritePerson.AgentCategory) : 0f;

        var hasFoundPOI = favouritePOI != null && interestInPOI >= interestInPerson;
        var hasFoundPerson = favouritePerson != null && interestInPOI < interestInPerson;

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

            // if (_currentState == State.WalkingToPOI || _currentState == State.WalkingToPerson)            // lost target -> change to random walk
            if (_currentState == State.WalkingToPerson)            // lost target -> change to random walk
            {
                _lockedInterest = null;
                _currentState = State.RandomWalking;
                _agentWalking.SetNewRandomDestination();
                return;
            }
            if (_currentState == State.WalkingToPOI)            // lost target -> change to random walk
            {
                // need to make a difference between : lost interest and making detour

                var i = GetCurrentInterest(_lockedInterest);
                var hasTriedForTooLong = i <= 0;
                if (hasTriedForTooLong)
                {
                    CompleteTransaction();
                    _currentState = State.RandomWalking;
                    return;
                }

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
            if (_currentState == State.RandomWalking || _currentState == State.WalkingToPerson) // just found poi
            {
                InitWalkToPOI(favouritePOI);
                _currentState = State.WalkingToPOI;
                return;
            }
            if (_currentState == State.WalkingToPOI) // continue towards poi
            {
                if (favouritePOI.IsInsideTransactionRadius(transform.position))
                {
                    InitTransaction();
                    _currentState = State.DoingTransaction;
                    return;
                }

                if (favouritePOI.InterestCategory != _lockedInterest)
                {
                    InitWalkToPOI(favouritePOI);
                    return;
                }
            }
            return;
        }

        if (hasFoundPerson)
        {
            if (_currentState == State.RandomWalking || _currentState == State.WalkingToPOI)
            {
                InitWalkToPerson(favouritePerson);
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

    void OnDrawGizmos()
    {
        if (_lockedInterest != null)
        {
            var spheresize = 0.3f;
            var frequency = 3;
            if (_currentState == State.DoingTransaction || _currentState == State.DoingPersonTransaction)
                spheresize = spheresize * Mathf.Abs(Mathf.Sin(frequency * Time.time));

            Gizmos.color = _isBoredOfWaiting ? Color.red : _lockedInterest.Color;
            // if (_isBoredOfWaiting)
            // Gizmos.color = Color.Lerp(Gizmos.color, Color.red, Mathf.Abs(Mathf.Sin(frequency * Time.time)));


            // Gizmos.DrawSphere(transform.position + 3f * Vector3.up, spheresize);
            Gizmos.DrawWireSphere(transform.position + 3f * Vector3.up, spheresize);
        }
    }

    private void InitWalkToPOI(PointOfInterest poi)
    {
        _agentWalking.SetDestination(poi.transform.position);
        _lockedInterest = poi.InterestCategory;
        SetLockedInterestValue(GetCurrentInterest(_lockedInterest) + PersistencyBonus);
        _lockedInterestTime = Time.time;
    }

    private void InitWalkToPerson(Agent person)
    {
        _agentWalking.SetDestination(person.transform.position);
        _lockedInterest = person.AgentCategory as InterestCategory;
        SetLockedInterestValue(GetCurrentInterest(_lockedInterest) + PersistencyBonus);
        _lockedInterestTime = Time.time;
    }

    private Agent ChoosePersonInNeighbourhood()
    {
        Agent mostAttractiveInterlocutor = null;
        float highestAttractiveness = 0f;

        foreach (KeyValuePair<InterestCategory, float> socialInterest in CurrentSocialInterests)
        {
            var personCategory = socialInterest.Key as AgentCategory;
            var personAttractiveness = socialInterest.Value;

            var closestNeighbour = Simulation.Instance.FindClosestNeighbourOfCategory(personCategory, this);
            if (closestNeighbour == null)
                continue;

            var distanceToClosestNeighbour = Vector3.Distance(closestNeighbour.transform.position, transform.position);
            var neighbourInSocialInteractionRadius = distanceToClosestNeighbour < SocialInteractionRadius;

            if (neighbourInSocialInteractionRadius && personAttractiveness > highestAttractiveness)
            {
                mostAttractiveInterlocutor = closestNeighbour;
                highestAttractiveness = personAttractiveness;
            }
        }
        return mostAttractiveInterlocutor;
    }


    private void InitTransaction()
    {
        _transactionStartTime = Time.time;
    }

    private void CompleteTransaction()
    {
        //complete transaction
        SetLockedInterestValue(0f);
        _lockedInterest = null;
    }
    private bool HasSpentMaxTimeFollowingPerson()
    {
        var timeSpentFollowingPerson = Time.time - _lockedInterestTime;
        return timeSpentFollowingPerson > MaxTimeToFollowPerson;
    }

    private bool HasFinishedTransaction()
    {
        var timeSinceTransactionStarted = Time.time - _transactionStartTime;
        return timeSinceTransactionStarted > _lockedInterest.TransactionTime;
    }

    private bool HasReachedPerson(Agent person)
    {
        var distanceToInterlocutor = Vector3.Distance(person.transform.position, transform.position);
        return distanceToInterlocutor < SocialTransactionRadius;
    }

    private bool HasSatisfiedAllInterests()
    {
        var cullminatedInterests = 0f;

        foreach (var value in CurrentInterests.Values)
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
        CurrentInterests.Clear();
    }


    // only necessary if stamina is used
    public float MinDistanceTraveledRecentlyBeforeGiveUp;
    public float distanceTraveledRecently;
    private bool _isBoredOfWaiting;

    private void ReduceValueOfLockedInterest()
    {
        distanceTraveledRecently = GetSquaredDistanceTraveledRecently();

        if (_lockedInterest == null)
            return;

        var key = _lockedInterest;
        float currentInterest;
        if (CurrentInterests.ContainsKey(key))
            currentInterest = CurrentInterests[key];
        else
            currentInterest = CurrentSocialInterests[key];

        _isBoredOfWaiting = distanceTraveledRecently < MinDistanceTraveledRecentlyBeforeGiveUp;
        if (_isBoredOfWaiting)
            currentInterest = Mathf.Max(currentInterest + AgentCategory.Stamina, 0f);

        SetLockedInterestValue(currentInterest);
    }

    private const int N = 60;
    private Queue<float> _lastNDistancesTraveled;
    private Vector3 _lastPosition;

    private float GetSquaredDistanceTraveledRecently()
    {
        if (_lastNDistancesTraveled == null)
        {
            _lastPosition = transform.position;

            _lastNDistancesTraveled = new Queue<float>();
            for (int i = 0; i < N; i++)
                _lastNDistancesTraveled.Enqueue(MinDistanceTraveledRecentlyBeforeGiveUp);
            return float.PositiveInfinity;
        }

        var vectorTraveledSinceLastFrame = transform.position - _lastPosition;
        var distanceTraveledSinceLastFrameSquared = Vector3.Dot(vectorTraveledSinceLastFrame, vectorTraveledSinceLastFrame);


        _lastNDistancesTraveled.Enqueue(distanceTraveledSinceLastFrameSquared);
        _lastNDistancesTraveled.Dequeue();

        float distance = 0f;
        foreach (var d in _lastNDistancesTraveled)
            distance += d;

        _lastPosition = transform.position;
        return distance;
    }

    private void SetLockedInterestValue(float newInterestValue)
    {
        var key = _lockedInterest;
        if (CurrentInterests.ContainsKey(key))
            CurrentInterests[key] = newInterestValue;
        else
            CurrentSocialInterests[key] = newInterestValue;
    }


    private PointOfInterest ChoosePointOfInterest()
    {
        PointOfInterest choosenPOI = null;
        var maxFoundAttraction = 0f;

        foreach (KeyValuePair<InterestCategory, float> interest in CurrentInterests)
        {
            var mostVisiblePOI = GetMostVisiblePointOfInterest(interest.Key);
            var foundPOI = mostVisiblePOI != null;

            var attraction = foundPOI ? GetCurrentInterest(interest.Key) : 0f;
            if (attraction > maxFoundAttraction)
            {
                choosenPOI = mostVisiblePOI;
                maxFoundAttraction = attraction;
            }
        }
        return choosenPOI;
    }

    public PointOfInterest GetMostVisiblePointOfInterest(InterestCategory interestCategory)
    {
        PointOfInterest mostVisiblePointOfInterest = null;
        var maxFoundVisibility = 0f;
        if (!Simulation.Instance.PointsOfInterest.ContainsKey(interestCategory))
            return null;

        foreach (var poi in Simulation.Instance.PointsOfInterest[interestCategory])
        {
            var poiVisibility = poi.GetVisibilityAt(this.transform.position);
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
        if (CurrentInterests.ContainsKey(interestCategory))
            return CurrentInterests[interestCategory];
        if (CurrentSocialInterests.ContainsKey(interestCategory))
            return CurrentSocialInterests[interestCategory];
        else
            return 0f;
    }

    // float _attractednessOnStart = -1f;
    private void RenderAttractedness()
    {
        // var culminatedAttractedness = 0f;

        // foreach (KeyValuePair<InterestCategory, float> pair in _currentInterests)
        //     culminatedAttractedness += pair.Value;

        // if (_attractednessOnStart == -1f)
        // {
        //     _attractednessOnStart = culminatedAttractedness;
        // }
        // var brightness = culminatedAttractedness / _attractednessOnStart;

        foreach (var r in _allRenderers)
            r.material.SetColor("_Color", AgentCategory.Color);
    }

    private Renderer[] _allRenderers = new Renderer[0];
    private AgentWalking _agentWalking;

    private InterestCategory _lockedInterest;


    private float _lockedInterestTime;
    private float _transactionStartTime;
    private float _creationTime;

    private Vector3 _exit;

    private State _currentState;
    private enum State { RandomWalking, WalkingToPOI, WalkingToPerson, WalkingToExit, DoingTransaction, DoingPersonTransaction }
}
