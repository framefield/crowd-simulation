using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointOfInterest : MonoBehaviour
{
    public InterestCategory InterestCategory;

    public float InnerSatisfactionRadius;
    [SerializeField] float OuterVisibilityRadius;

    [SerializeField] TMPro.TextMeshPro Label;
    [SerializeField] float GizmoCirclesPerMeter;

    public float GetVisibilityAt(Vector3 position)
    {
        var squaredDistanceOnXZ = GetSquaredDistanceOnXZPlane(position);
        var normalizedDistance = squaredDistanceOnXZ / (OuterVisibilityRadius * OuterVisibilityRadius);
        var visibility = normalizedDistance > 1 ? 0f : 1 / normalizedDistance;
        return visibility;
    }

    public bool IsInsideTransactionRadius(Vector3 position)
    {
        var squaredDistanceOnXZ = GetSquaredDistanceOnXZPlane(position);
        var normalizedDistance = squaredDistanceOnXZ / (InnerSatisfactionRadius * InnerSatisfactionRadius);
        return normalizedDistance < 1;
    }

    private float GetSquaredDistanceOnXZPlane(Vector3 position)
    {
        var spectatorOnXZ = new Vector2(position.x, position.z);
        var thisPOIOnXZ = new Vector2(transform.position.x, transform.position.z);
        var diffVectorOnXZ = spectatorOnXZ - thisPOIOnXZ;
        var squaredDistance = Vector2.Dot(diffVectorOnXZ, diffVectorOnXZ);
        return squaredDistance;
    }

    void OnDrawGizmos()
    {
        var color = InterestCategory.Color;
        DrawGizmoCircle(OuterVisibilityRadius, color);
        DrawGizmoCircle(InnerSatisfactionRadius, color);


        var category = InterestCategory.name;
        Label.text = category;
        var gameObjectName = category + "- PointOfInterest";
        if (gameObject.name != gameObjectName)
            gameObject.name = gameObjectName;

        Label.color = InterestCategory.Color;
    }

    void OnDrawGizmosSelected()
    {
        var offset = 0.1f;
        DrawGizmoCircle(OuterVisibilityRadius, Color.grey, true, 256);
        DrawGizmoCircle(OuterVisibilityRadius + offset, Color.black, true, 256);
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
}
