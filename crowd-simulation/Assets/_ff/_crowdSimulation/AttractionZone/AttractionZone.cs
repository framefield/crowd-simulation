using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AttractionZone : MonoBehaviour
{
    [SerializeField] AttractionCategory Category;
    [SerializeField] float Radius;
    [SerializeField] float AttractionStrength;
    [SerializeField] AnimationCurve AttractionDistribution;

    public int NumberOfCircles;

    public enum AttractionCategory
    {
        Food,
        Wine,
        Textiles,
        Pottery,
        Philosophy
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {

        for (int i = 0; i < NumberOfCircles; i++)
        {
            var radiusFraction = Radius * (i + 1) / NumberOfCircles;
            // var thickness = AttractionDistribution.Evaluate((i + 1) / nCircles);
            // AttractionDistribution.
            Gizmos.color = new Color(1, 1, 1, AttractionDistribution.Evaluate((i + 1f) / NumberOfCircles));
            var positions = GetCirclePositions(radiusFraction);
            ConnectPositions(positions);
        }

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

    public float GetAttractivenessAtGlobalPosition(Vector3 spectator)
    {
        var distance = Vector3.Distance(spectator, this.transform.position);
        var distanceNormalized = distance / Radius;
        var attractiveness = AttractionDistribution.Evaluate(distanceNormalized);
        return attractiveness;
    }


}
