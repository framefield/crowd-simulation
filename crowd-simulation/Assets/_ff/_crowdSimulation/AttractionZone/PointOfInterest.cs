using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointOfInterest : MonoBehaviour
{
    public InterestCategory AttractionCategory;

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
            var alphaFromAttraction = AttractionDistribution.Evaluate((i + 1f) / numberOfCircles);
            Gizmos.color = AttractionCategory.Color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, alphaFromAttraction);
            var radiusFraction = Radius * (i + 1) / numberOfCircles;
            var circleVertices = GenerateCircleVertices(radiusFraction);
            DrawLine(circleVertices);
        }

        Label.text = AttractionCategory.ToString().Split('(')[0];
        Label.color = AttractionCategory.Color;
    }

    private Vector3[] GenerateCircleVertices(float radius)
    {
        var resolution = 32;
        List<Vector3> allPositions = new List<Vector3>();
        var pos = new Vector3(radius, 0, 0);

        for (int i = 0; i < resolution; i++)
        {
            var circlePosition = Quaternion.Euler(0, i * 360 / resolution, 0) * pos;
            allPositions.Add(transform.position + circlePosition);
        }

        return allPositions.ToArray();
    }

    private void DrawLine(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            Gizmos.DrawLine(positions[i], positions[(i + 1) % positions.Length]);
    }
}
