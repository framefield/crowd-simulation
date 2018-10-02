using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryZone : MonoBehaviour
{
    [Header("PARAMETERS")]

    [SerializeField]
    AgentCategory _agentCategory;

    [SerializeField] float AgentsSpawnedPerSecond;

    [Header("INTERNAL - DO NOT TOUCH")]

    [SerializeField]
    GameObject AgentPrefab;

    [SerializeField] TMPro.TextMeshPro Label;

    public AgentCategory GetAgentCategory()
    {
        return _agentCategory;
    }

    void Update()
    {
        _agentsToSpawn += Time.deltaTime * AgentsSpawnedPerSecond;

        var n = Mathf.Ceil(_agentsToSpawn);
        for (int i = 0; i < n; i++)
            Simulation.Instance.SpawnAgentAtPosition(transform.position, AgentPrefab, _agentCategory);

        _agentsToSpawn -= n;
    }

    void OnDrawGizmos()
    {
        DrawGizmoCircle(RADIUS, _agentCategory.Color);

        var category = _agentCategory.name;
        Label.text = category;
        var gameObjectName = category + "- EntryZone";
        if (gameObject.name != gameObjectName)
            gameObject.name = gameObjectName;

        Label.color = _agentCategory.Color;
    }

    private void DrawGizmoCircle(float radius, Color color, bool dotted = false, int resolution = 64)
    {
        Gizmos.color = color;
        var circleVertices = GenerateCircleVertices(radius, resolution);
        DrawLine(circleVertices, dotted);
    }

    private Vector3[] GenerateCircleVertices(float radius, int resolution)
    {
        List<Vector3> allPositions = new List<Vector3>();
        var pos = new Vector3(radius, 0, 0);

        for (int i = 0; i < resolution; i++)
        {
            var circlePosition = Quaternion.Euler(0, i * 360 / resolution, 0) * pos;
            allPositions.Add(transform.position + circlePosition);
        }

        return allPositions.ToArray();
    }

    private void DrawLine(Vector3[] positions, bool dotted)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            var frequency = 1f;
            if (dotted)
            {
                var x = (1 + Mathf.Sin(i * frequency)) * 0.5f;
                Gizmos.color = Color.Lerp(Color.white, Color.grey, x);
            }
            Gizmos.DrawLine(positions[i], positions[(i + 1) % positions.Length]);
        }
    }

    private float _agentsToSpawn = 0f;
    private const float RADIUS = 1f;
}
