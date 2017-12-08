using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AttractionZone : MonoBehaviour
{
    [Header("PARAMETERS")]

    public AttractionCategory InterestCategory;

    [SerializeField]
    float InnerSatisfactionRadius;

    [SerializeField]
    float OuterVisibilityRadius;


    [Header("INTERNAL - DO NOT TOUCH")]

    [SerializeField]
    TMPro.TextMeshPro Label;

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
        Gizmos.color = InterestCategory.Color;
        GizmoHelper.DrawGizmoCircle(OuterVisibilityRadius, transform.position);
        GizmoHelper.DrawGizmoCircle(InnerSatisfactionRadius, transform.position);


        var category = InterestCategory.name;
        Label.text = category;
        var gameObjectName = category + "- AttractionZone";
        if (gameObject.name != gameObjectName)
            gameObject.name = gameObjectName;

        Label.color = InterestCategory.Color;
    }

    void OnDrawGizmosSelected()
    {
        var offset = 0.1f;
        GizmoHelper.DrawGizmoCircle(OuterVisibilityRadius, transform.position, true, 256);
        GizmoHelper.DrawGizmoCircle(OuterVisibilityRadius + offset, transform.position, true, 256);
    }
}
