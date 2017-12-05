using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public AgentCategory AgentCategory;
    // [SerializeField] TMPro.TextMeshPro Label;
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

    private Camera _camera;
    void Update()
    {
        SearchForAttractivePersonInNeighbourhood();

        // Label.transform.LookAt(transform.position + transform.position - _camera.transform.position);
        // Label.text = _currentState.ToString();

        if (_currentState == State.WalkingToExit)
            return;

        RenderAttractedness();

        if (_currentState == State.DoingTransaction)
        {
            var timeSinceTransactionStarted = Time.time - _transactionStartTime;
            if (timeSinceTransactionStarted < _lockedPointOfInterest.InterestCategory.TransactionTime)
                return;

            //complete transaction
            SetInterestOfCurrentPOISCategory(0f);
            _lockedPointOfInterest = null;
            _currentState = State.RandomWalking;
        }


        if (HasSatisfiedAllInterests())
        {
            _currentState = State.WalkingToExit;
            _agentWalking.SetDestination(_exit);
            return;
        }

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
                _lockedPointOfInterest = null;
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

                    var distanceToInterlocutor = Vector3.Distance(choosenInterlocutor.transform.position, transform.position);
                    if (distanceToInterlocutor < SocialTransactionRadius)
                        _currentState = State.DoingSocialTransaction;

                    return;
                }

                if (_agentWalking.CheckIfReachedRandomDestination())
                {
                    _agentWalking.SetNewRandomDestination();
                    _currentState = State.RandomWalking;
                    return;
                }
                return;
            }
        }

        if (choosenPointOfInterest == _lockedPointOfInterest)
        {
            if (ReachedCurrentPOI())
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
        _lockedPointOfInterest = choosenPointOfInterest;
    }

    void OnDrawGizmos()
    {
        if (_currentState == State.WalkingToInterlocutor)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position + 1.4f * Vector3.up, 0.3f);
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
        var key = _lockedPointOfInterest.InterestCategory;
        var currentInterest = _currentInterests[key];
        currentInterest = Mathf.Max(currentInterest + AgentCategory.Stamina, 0f);
        SetInterestOfCurrentPOISCategory(currentInterest);
    }

    private void SetInterestOfCurrentPOISCategory(float newInterestValue)
    {
        var key = _lockedPointOfInterest.InterestCategory;
        _currentInterests[key] = newInterestValue;
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
        // var color = colorByCategory * new Color(brightness, brightness, brightness, 1);

        foreach (var r in _allRenderers)
            r.material.SetColor("_Color", colorByCategory);
    }

    private Renderer[] _allRenderers = new Renderer[0];
    private AgentWalking _agentWalking;

    private PointOfInterest _lockedPointOfInterest;


    public Interests _currentInterests;
    public Interests _currentSocialInterests;
    private float _transactionStartTime;
    private float _creationTime;
    private State _currentState;
    private enum State { RandomWalking, WalkingToPOI, WalkingToInterlocutor, WalkingToExit, DoingTransaction, DoingSocialTransaction }
}
