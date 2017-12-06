using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryZone : MonoBehaviour
{
    [SerializeField]
    GameObject AgentPrefab;

    [SerializeField] TMPro.TextMeshPro Label;

    [SerializeField]
    AgentCategory AgentCategory;
    public float AgentsPerSecond;

    void Update()
    {
        _agentsToSpawn += Time.deltaTime * AgentsPerSecond;

        var n = Mathf.Ceil(_agentsToSpawn);
        for (int i = 0; i < n; i++)
            Simulation.Instance.SpawnAgentAtPosition(transform.position, AgentPrefab, AgentCategory);

        _agentsToSpawn -= n;
    }

    void OnDrawGizmos()
    {
        DrawGizmoCircle(RADIUS, AgentCategory.Color);

        var category = AgentCategory.ToString().Split('(')[0];
        Label.text = category;
        var gameObjectName = category + "- EntryZone";
        if (gameObject.name != gameObjectName)
            gameObject.name = gameObjectName;

        Label.color = AgentCategory.Color;
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
    private const float RADIUS = 3f;
}
