using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AttractionZone : MonoBehaviour
{
    public AttractionCategory Category;
    [SerializeField] float Radius;
    [SerializeField] float IconHeight;
    [SerializeField] float AttractionStrength;
    [SerializeField] AnimationCurve AttractionDistribution;
    [SerializeField] TMPro.TextMeshPro Label;
    [SerializeField] int NumberOfCircles;

    public float GetGeneralAttractivenessAtGlobalPosition(Vector3 spectator)
    {
        var distance = Vector3.Distance(spectator, this.transform.position);
        var distanceNormalized = distance / Radius;
        var attractiveness = AttractionDistribution.Evaluate(distanceNormalized);
        return attractiveness;
    }


    void OnDrawGizmos()
    {

        for (int i = 0; i < NumberOfCircles; i++)
        {
            Gizmos.color = Category.Color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, AttractionDistribution.Evaluate((i + 1f) / NumberOfCircles));

            var radiusFraction = Radius * (i + 1) / NumberOfCircles;
            var positions = GetCirclePositions(radiusFraction);

            ConnectPositions(positions);
        }

        Label.text = Category.ToString().Split('(')[0];
        Label.color = Category.Color;
    }

    private Vector3[] GetCirclePositions(float radius)
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

    private void ConnectPositions(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            Gizmos.DrawLine(positions[i], positions[(i + 1) % positions.Length]);
    }




}
