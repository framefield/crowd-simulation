using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointOfInterest : MonoBehaviour
{
    public InterestCategory InterestCategory;

    public float InnerSatisfactionRadius;
    [SerializeField] float Radius;
    [SerializeField] AnimationCurve AttractionDistribution;
    [SerializeField] TMPro.TextMeshPro Label;
    [SerializeField] float GizmoCirclesPerMeter;

    public float GetVisibilityAtGlobalPosition(Vector3 spectator)
    {
        var distance = Vector3.Distance(spectator, this.transform.position);
        var distanceNormalized = distance / Radius;
        var attractiveness = AttractionDistribution.Evaluate(distanceNormalized);
        return attractiveness;
    }

    void OnDrawGizmos()
    {
        var numberOfCircles = GizmoCirclesPerMeter * Radius;

        for (int i = 0; i < numberOfCircles; i++)
        {
            var alphaFromAttraction = AttractionDistribution.Evaluate((numberOfCircles - i) / numberOfCircles);
            var color = InterestCategory.Color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, alphaFromAttraction);
            var radiusFraction = Radius * (numberOfCircles - i) / numberOfCircles;
            DrawGizmoCircle(radiusFraction, color);
        }

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
        DrawGizmoCircle(Radius, Color.grey, true, 256);
        DrawGizmoCircle(Radius + offset, Color.black, true, 256);
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
