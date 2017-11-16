using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomWalkArea))]
public class RandomWalkAreaEditor : Editor
{

    void OnSceneGUI()
    {
        var randomWalkArea = target as RandomWalkArea;
        var vertices = randomWalkArea.Vertices;

        Handles.color = Color.white;
        var snapMode = DetermineSnapMode();

        var iLastPoint = -1;
        for (var i = 0; i < vertices.Count; i++)
        {
            // Draw handles
            EditorGUI.BeginChangeCheck();
            var point = vertices[i];
            point = Handles.DoPositionHandle(point, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(randomWalkArea, "Change randomWalkArea");
                EditorUtility.SetDirty(randomWalkArea);
                var snapPoint = ApplySnap(point, snapMode);
                vertices[i] = snapPoint;
            }

            if (i > 0)
            {
                // Handles.DrawLine(point, vertices[iLastPoint]);

                Handles.BeginGUI();
                var midPoint = (point + vertices[iLastPoint]) / 2;
                var viewPos = Camera.current.WorldToScreenPoint(midPoint);
                if (GUI.Button(new Rect(viewPos.x - 8, Camera.current.pixelHeight - viewPos.y - 8, 16, 16), "+"))
                {
                    randomWalkArea.InsertVertexBetweenVertices(i, iLastPoint);
                    // vertices.Insert(i, midPoint);
                    // InsertPoint(line, points, i, midPoint);
                }
                Handles.EndGUI();
            }
            iLastPoint = i;
        }
    }


    private static Vector3 ApplySnap(Vector3 point, SnapMode snapMode)
    {
        if (snapMode == SnapMode.Unit)
        {
            point.x = Mathf.RoundToInt(point.x * SNAPSPERUNITYUNIT) * 1f / SNAPSPERUNITYUNIT;
            point.y = 0;
            point.z = Mathf.RoundToInt(point.z * SNAPSPERUNITYUNIT) * 1f / SNAPSPERUNITYUNIT;
        }
        return point;
    }

    private static SnapMode DetermineSnapMode()
    {
        if (Event.current.modifiers == EventModifiers.Shift)
        {
            return SnapMode.NoSnap;
        }
        return SnapMode.Unit;
    }

    enum SnapMode
    {
        NoSnap,
        Unit
    }

    private const int SNAPSPERUNITYUNIT = 1;

}