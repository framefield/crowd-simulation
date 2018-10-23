using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class SimulationLog : MonoBehaviour
{
    [SerializeField]
    float _logRate = 1f;

    [SerializeField]
    RenderingStyle _renderingStyle;

    [SerializeField]
    GizmoFilter _gizmoFilter;

    [SerializeField]
    [Range(0f, 1f)]
    float _gizmoAlpha = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float _gizmoSaturation = 1f;

    public enum GizmoFilter
    {
        Nothing,
        ForAllAgents,
        SelectedAgent,
    }

    public enum RenderingStyle
    {
        ByAgentCategory,
        ByCurrentInterest,
    }

    public Dictionary<Agent, AgentLogData> LoggedAgents = new Dictionary<Agent, AgentLogData>();

    void Start()
    {
        _simulation.OnAgentSpawned += HandleSpawnedAgent;
        StartCoroutine(WriteTimeIntoFile());
    }

    private IEnumerator WriteTimeIntoFile()
    {
        using (StreamWriter sw = new StreamWriter("log.csv"))
        {
            sw.Write(GenerateHeader(_interestCategoriesInProject));

            while (true)
            {
                _timeSinceLastLog += Time.deltaTime;
                if (_timeSinceLastLog > _logRate)
                {
                    _timeSinceLastLog -= _logRate;
                    foreach (var agentLogData in LoggedAgents.Values)
                    {
                        var csvLine = agentLogData.LogSlice(_interestCategoriesInProject);
                        sw.Write(csvLine);
                    }
                }
                yield return null;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (_gizmoFilter == GizmoFilter.Nothing)
            return;



        var colorsByAgentCategory = GetGizmoColorsByAgentCategory(_gizmoAlpha, _gizmoSaturation, _agentCategoriesInProject);
        var colorsByInterest = GetGizmoColorsByInterest(_gizmoAlpha, _gizmoSaturation, _interestCategoriesInProject);

        var isAgentSelected = Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Agent>() != null;
        if (_gizmoFilter == GizmoFilter.SelectedAgent && !isAgentSelected)
            return;

        Agent selectedAgent = isAgentSelected ? Selection.activeGameObject.GetComponent<Agent>() : null;

        foreach (var kvp in LoggedAgents)
        {
            if (_gizmoFilter == GizmoFilter.SelectedAgent && kvp.Key != selectedAgent)
                continue;

            var agentLogData = kvp.Value;

            if (_renderingStyle == RenderingStyle.ByAgentCategory)
                Gizmos.color = colorsByAgentCategory[agentLogData.Agent.AgentCategory];

            var slices = agentLogData.LogDataSlices;
            for (int i = 0; i < slices.Count - 1; i++)
            {
                var lockedInterest = slices[i + 1].LockedInterest;

                if (_renderingStyle == RenderingStyle.ByCurrentInterest)
                    Gizmos.color = lockedInterest != null ? colorsByInterest[lockedInterest] : Color.gray;



                Gizmos.DrawLine(slices[i].Position, slices[i + 1].Position);
            }
        }
    }

    private static Dictionary<AgentCategory, Color> GetGizmoColorsByAgentCategory(float gizmoAlpha, float gizmoSaturation, List<AgentCategory> agentCategories)
    {
        var colors = new Dictionary<AgentCategory, Color>();
        foreach (var category in agentCategories)
        {
            var color = category.Color;
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            s *= gizmoSaturation;
            color = Color.HSVToRGB(h, s, v);
            color.a *= gizmoAlpha;
            colors.Add(category, color);
        }
        return colors;
    }

    private static Dictionary<InterestCategory, Color> GetGizmoColorsByInterest(float gizmoAlpha, float gizmoSaturation, List<InterestCategory> interestCategories)
    {
        var colors = new Dictionary<InterestCategory, Color>();
        foreach (var category in interestCategories)
        {
            var color = category.Color;
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            s *= gizmoSaturation;
            color = Color.HSVToRGB(h, s, v);
            color.a *= gizmoAlpha;
            colors.Add(category, color);
        }
        return colors;
    }




    private static Color GetColor(
        RenderingStyle renderingStyle,
        InterestCategory lockedInterest,
        Color agentCategoryColor,
        float gizmoAlpha,
        float gizmoSaturation)
    {
        Color color;
        switch (renderingStyle)
        {
            case RenderingStyle.ByAgentCategory:
                color = agentCategoryColor;
                break;
            case RenderingStyle.ByCurrentInterest:
            default:
                color = lockedInterest != null ? lockedInterest.Color : Color.grey;
                break;
        }

        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        s *= gizmoSaturation;
        color = Color.HSVToRGB(h, s, v);
        color.a *= gizmoAlpha;

        return color;
    }


    public void HandleSpawnedAgent(Agent agent)
    {
        LoggedAgents.Add(agent, new AgentLogData(agent));
    }

    private static string GenerateHeader(List<InterestCategory> categories)
    {
        var baseHeader = String.Format("agentID\tsimulationTimeInSeconds\tpositionX\tpositionY\tpositionZ\tAgentCategory\tLockedInterest\t");

        var interestsHeader = "";
        foreach (var category in categories)
        {
            interestsHeader += "Interest." + category.name + "\t";
        }
        return baseHeader + interestsHeader + "\n";
    }

    private float _timeSinceLastLog;

    private List<InterestCategory> _interestCategoriesInProjectCache;
    private List<InterestCategory> _interestCategoriesInProject
    {
        get
        {
            if (_interestCategoriesInProjectCache == null)
            {
                _interestCategoriesInProjectCache = new List<InterestCategory>();
                var catGUIDs = AssetDatabase.FindAssets("t:InterestCategory");
                foreach (var c in catGUIDs)
                {
                    var catPath = AssetDatabase.GUIDToAssetPath(c);
                    var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(InterestCategory)) as InterestCategory;
                    _interestCategoriesInProjectCache.Add(category);
                }
            }
            return _interestCategoriesInProjectCache;
        }
    }

    private List<AgentCategory> _agentCategoriesInProjectCache;
    private List<AgentCategory> _agentCategoriesInProject
    {
        get
        {
            if (_agentCategoriesInProjectCache == null)
            {
                _agentCategoriesInProjectCache = new List<AgentCategory>();
                var catGUIDs = AssetDatabase.FindAssets("t:AgentCategory");
                foreach (var c in catGUIDs)
                {
                    var catPath = AssetDatabase.GUIDToAssetPath(c);
                    var category = UnityEditor.AssetDatabase.LoadAssetAtPath(catPath, typeof(AgentCategory)) as AgentCategory;
                    _agentCategoriesInProjectCache.Add(category);
                }
            }
            return _agentCategoriesInProjectCache;
        }
    }

    private Simulation _simulationCache;
    private Simulation _simulation
    {
        get
        {
            if (_simulationCache == null)
                _simulationCache = GetComponent<Simulation>();
            return _simulationCache;
        }
    }
}
