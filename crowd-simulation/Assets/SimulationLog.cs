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
    float _logsPerSecond = 1f;

    [Header("Visualization")]

    [SerializeField]
    RenderingStyle _renderingStyle;

    [SerializeField]
    GizmoFilter _gizmoFilter;

    [SerializeField]
    [Range(0f, 1f)]
    float _gizmoAlpha = 1f;

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

        using (StreamWriter sw = new StreamWriter(_csvPath))
        {
            sw.Write(GenerateHeader(_interestCategoriesInProject));

            while (true)
            {
                _timeSinceLastLog += Time.deltaTime;
                if (_timeSinceLastLog > _logsPerSecond)
                {
                    _timeSinceLastLog -= _logsPerSecond;
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

        if (_gizmoFilter == GizmoFilter.SelectedAgent && _agentSelectedInEditor == null)
            return;

        var colorsByAgentCategory = GetGizmoColorsByCategory(_gizmoAlpha, _agentCategoriesInProject);
        var colorsByInterest = GetGizmoColorsByCategory(_gizmoAlpha, _interestCategoriesInProject);

        foreach (var kvp in LoggedAgents)
        {
            var isThisAgentSelected = kvp.Key == _agentSelectedInEditor;
            if (_gizmoFilter == GizmoFilter.SelectedAgent && !isThisAgentSelected)
                continue;

            var agentLogData = kvp.Value;

            if (_renderingStyle == RenderingStyle.ByAgentCategory)
                Gizmos.color = colorsByAgentCategory[agentLogData.Agent.AgentCategory];

            var slices = agentLogData.LogDataSlices;
            for (int i = 1; i < slices.Count; i++)
            {
                var lockedInterest = slices[i].LockedInterest;

                if (_renderingStyle == RenderingStyle.ByCurrentInterest)
                {
                    Gizmos.color = lockedInterest != null
                    ? colorsByInterest[lockedInterest] : _defaultGizmoColor;
                }

                Gizmos.DrawLine(slices[i - 1].Position, slices[i].Position);
            }
        }
    }

    private void HandleSpawnedAgent(Agent agent)
    {
        LoggedAgents.Add(agent, new AgentLogData(agent));
    }

    private static Dictionary<AgentCategory, Color> GetGizmoColorsByCategory(
        float gizmoAlpha,
        List<AgentCategory> agentCategories)
    {
        var colors = new Dictionary<AgentCategory, Color>();
        foreach (var category in agentCategories)
        {
            colors.Add(category, GetColorForCategory(category, gizmoAlpha));
        }
        return colors;
    }

    private static Dictionary<InterestCategory, Color> GetGizmoColorsByCategory(
        float gizmoAlpha,
        List<InterestCategory> interestCategories)
    {
        var colors = new Dictionary<InterestCategory, Color>();
        foreach (var category in interestCategories)
        {
            colors.Add(category, GetColorForCategory(category, gizmoAlpha));
        }
        return colors;
    }

    private static Color GetColorForCategory(InterestCategory category, float alpha)
    {
        var color = category.Color;
        return new Color(color.r, color.g, color.b, color.a * alpha);
    }

    private static string GenerateHeader(List<InterestCategory> categories)
    {
        var header = "";
        header += "AgentID\t";
        header += "SimulationTimeInSeconds\t";
        header += "PosX\t";
        header += "PosY\t";
        header += "PosZ\t";
        header += "AgentCategory\t";
        header += "LockedInterest\t";

        foreach (var category in categories)
        {
            header += "Interest." + category.name + "\t";
        }
        return header + "\n";
    }

    private Agent _agentSelectedInEditor
    {
        get
        {
            if (Selection.activeGameObject == null)
                return null;
            return Selection.activeGameObject.GetComponent<Agent>();
        }
    }

    private static string _csvPath
    {
        get
        {
            var subfolderPath = "logs/";
            var filename = DateTime.Now.ToString();
            return subfolderPath + filename + ".csv";
        }
    }

    private Color _defaultGizmoColor { get { return new Color(0.5f, 0.5f, 0.5f, _gizmoAlpha); } }

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
