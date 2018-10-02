using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [Header("GENERAL PARAMETERS")]

    public AgentCategory AgentCategory;

    public Interests CurrentInterests;

    public Interests CurrentSocialInterests;


    [Header("DECISION MAKING PARAMETERS")]

    [SerializeField]
    float PersistencyBonus;

    [SerializeField] float MaxTimeToFollowPerson;

    [SerializeField] float MinDistanceTraveledRecentlyBeforeGiveUp;


    [Header("RADII FOR SOCIAL INTERACTION")]

    [SerializeField]
    float SocialInteractionRadius;

    [SerializeField] float SocialTransactionRadius;


    [Header("VISUALIZATION")]

    [SerializeField]
    bool DrawIndicatorAboveAgent;
    [SerializeField] bool DrawSocialInteractionRadii;

    [Range(0f, 1f)]
    [SerializeField]
    float SocialInteractionRadiiAlpha;

    public void Init(AgentCategory category, Simulation simulation)
    {
        AgentCategory = category;
        _simulation = simulation;
    }

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
        _distanceTraveledRecently = GetSquaredDistanceTraveledRecently();
        _isBored = _distanceTraveledRecently < MinDistanceTraveledRecentlyBeforeGiveUp;

        if (_currentState == State.WalkingToPOI && _isBored)
            ReduceValueOfLockedPOIIfNotMoving();
        else if (_currentState == State.WalkingToPerson)
            ReduceValueOfLockedPersonIfFollowingForTooLong();

        RenderAttractedness();

        if (_currentState == State.WalkingToExit)
        {
            if (_agentWalking.GetSquaredDistanceToCurrentDestination() < 4f)
                Kill();
            return;
        }


        if (HasSatisfiedAllInterests())
        {
            _agentWalking.SetDestination(_exit);
            _currentState = State.WalkingToExit;
            return;
        }

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
                transactionTargetIsStillInRange = favouritePerson != null && _lockedInterest == favouritePerson.AgentCategory as AttractionCategory;

            if (!transactionTargetIsStillInRange)
            {
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

            if (_currentState == State.WalkingToPerson)            // lost target -> change to random walk
            {
                _lockedInterest = null;
                _currentState = State.RandomWalking;
                _agentWalking.SetNewRandomDestination();
                return;
            }
            if (_currentState == State.WalkingToPOI)            // lost target -> change to random walk
            {
                // distinguish between : lost interest and making detour

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
                if (_agentWalking.HasReachedCurrentDestination() || _isBored)
                {
                    InitRecentDistanceCheck();
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

    private void Kill()
    {
        Debug.Log("kill");
        _simulation.RemoveAgent(this);
        GameObject.Destroy(gameObject);
    }


    void OnDrawGizmos()
    {
        if (DrawIndicatorAboveAgent && _lockedInterest != null || _isBored)
        {
            var spheresize = 0.3f;
            var frequency = 3;
            if (_currentState == State.DoingTransaction || _currentState == State.DoingPersonTransaction)
                spheresize = spheresize * Mathf.Abs(Mathf.Sin(frequency * Time.time));

            Gizmos.color = _isBored && _currentState != State.DoingTransaction ? Color.red : _lockedInterest.Color;
            Gizmos.DrawWireSphere(transform.position + 3f * Vector3.up, spheresize);
        }

        if (DrawSocialInteractionRadii)
        {
            Gizmos.color = AgentCategory.Color * new Color(1, 1, 1, SocialInteractionRadiiAlpha);
            GizmoHelper.DrawGizmoCircle(SocialTransactionRadius, transform.position);
            GizmoHelper.DrawGizmoCircle(SocialInteractionRadius, transform.position);
        }
    }


    private void InitWalkToPOI(AttractionZone poi)
    {
        _agentWalking.SetDestination(poi.GetRandomPositionInsideSatisfactionCircle());
        _lockedInterest = poi.InterestCategory;
        SetLockedInterestValue(GetCurrentInterest(_lockedInterest) + PersistencyBonus);
        _lockedInterestTime = Time.time;
    }


    private void InitWalkToPerson(Agent person)
    {
        _agentWalking.SetDestination(person.transform.position);
        _lockedInterest = person.AgentCategory as AttractionCategory;
        SetLockedInterestValue(GetCurrentInterest(_lockedInterest) + PersistencyBonus);
        _lockedInterestTime = Time.time;
    }


    private Agent ChoosePersonInNeighbourhood()
    {
        Agent mostAttractiveInterlocutor = null;
        float highestAttractiveness = 0f;

        foreach (KeyValuePair<AttractionCategory, float> socialInterest in CurrentSocialInterests)
        {
            var personCategory = socialInterest.Key as AgentCategory;
            var personAttractiveness = socialInterest.Value;

            var closestNeighbour = _simulation.FindClosestNeighbourOfCategory(personCategory, this);
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
        return (Time.time - _creationTime) > AgentCategory.MaxTimeOnAgora;
    }


    private void SetAllInterestsToZero()
    {
        CurrentInterests.Clear();
    }

    private bool IsBoredBecauseDidNotMoveMuchRecently()
    {
        return _distanceTraveledRecently < MinDistanceTraveledRecentlyBeforeGiveUp;
    }

    private void ReduceValueOfLockedPOIIfNotMoving()
    {
        if (_lockedInterest == null)
            return;
        var updatedInterest = CurrentInterests[_lockedInterest];
        updatedInterest = Mathf.Max(updatedInterest + AgentCategory.MotivationDepletionIfBored, 0f);
        CurrentInterests[_lockedInterest] = updatedInterest;
    }


    private void ReduceValueOfLockedPersonIfFollowingForTooLong()
    {
        _isBored = HasSpentMaxTimeFollowingPerson();
        if (!_isBored)
            return;

        var updatedInterest = CurrentSocialInterests[_lockedInterest];
        updatedInterest = Mathf.Max(updatedInterest + AgentCategory.MotivationDepletionIfBored, 0f);
        CurrentSocialInterests[_lockedInterest] = updatedInterest;
    }


    private const int N = 60;
    private Queue<float> _lastNDistancesTraveled;

    private void InitRecentDistanceCheck()
    {
        _lastNDistancesTraveled = new Queue<float>();
        for (int i = 0; i < N; i++)
            _lastNDistancesTraveled.Enqueue(MinDistanceTraveledRecentlyBeforeGiveUp);
    }

    private float GetSquaredDistanceTraveledRecently()
    {
        if (_lastNDistancesTraveled == null)
        {
            _lastPosition = transform.position;

            InitRecentDistanceCheck();
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


    private AttractionZone ChoosePointOfInterest()
    {
        AttractionZone choosenPOI = null;
        var maxFoundAttraction = 0f;

        foreach (KeyValuePair<AttractionCategory, float> interest in CurrentInterests)
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


    public AttractionZone GetMostVisiblePointOfInterest(AttractionCategory interestCategory)
    {
        AttractionZone mostVisiblePointOfInterest = null;
        var maxFoundVisibility = 0f;
        if (!_simulation.PointsOfInterest.ContainsKey(interestCategory))
            return null;

        foreach (var poi in _simulation.PointsOfInterest[interestCategory])
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


    public float GetCurrentInterest(AttractionCategory interestCategory)
    {
        if (CurrentInterests.ContainsKey(interestCategory))
            return CurrentInterests[interestCategory];
        if (CurrentSocialInterests.ContainsKey(interestCategory))
            return CurrentSocialInterests[interestCategory];
        else
            return 0f;
    }


    private void RenderAttractedness()
    {
        foreach (var r in _allRenderers)
            r.material.SetColor("_Color", AgentCategory.Color);
    }


    private Renderer[] _allRenderers = new Renderer[0];
    private AgentWalking _agentWalking;
    private AttractionCategory _lockedInterest;

    private bool _isBored;
    private Vector3 _lastPosition;
    private float _distanceTraveledRecently;

    private float _lockedInterestTime;
    private float _transactionStartTime;
    private float _creationTime;
    private Vector3 _exit;

    private State _currentState;
    private enum State { RandomWalking, WalkingToPOI, WalkingToPerson, WalkingToExit, DoingTransaction, DoingPersonTransaction }

    private Simulation _simulation;
}
