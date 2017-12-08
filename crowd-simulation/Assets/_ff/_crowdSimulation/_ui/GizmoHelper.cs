using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper
{

    static public void DrawGizmoCircle(float radius, Vector3 center, bool dotted = false, int resolution = 32)
    {
        var circleVertices = GenerateCircleVertices(center, radius, resolution);
        DrawLine(circleVertices, dotted);
    }

    static private Vector3[] GenerateCircleVertices(Vector3 center, float radius, int resolution)
    {
        List<Vector3> allPositions = new List<Vector3>();
        var pos = new Vector3(radius, 0, 0);

        for (int i = 0; i < resolution; i++)
        {
            var circlePosition = Quaternion.Euler(0, i * 360 / resolution, 0) * pos;
            allPositions.Add(center + circlePosition);
        }

        return allPositions.ToArray();
    }


    static private void DrawLine(Vector3[] positions, bool dotted)
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

}
