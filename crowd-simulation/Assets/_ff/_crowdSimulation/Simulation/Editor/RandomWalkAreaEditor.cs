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
        var border = randomWalkArea.Border;

        Handles.color = Color.white;
        var snapMode = DetermineSnapMode();

        for (var i = 0; i < border.Count; i++)
        {
            // Draw handles
            EditorGUI.BeginChangeCheck();

            var iPoint = border[i];
            var iNextPoint = border[(i + 1) % border.Count];
            var point = vertices[iPoint];
            var nextPoint = vertices[iNextPoint];

            point = Handles.DoPositionHandle(point, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(randomWalkArea, "Change randomWalkArea");
                EditorUtility.SetDirty(randomWalkArea);
                var snapPoint = ApplySnap(point, snapMode);
                vertices[iPoint] = snapPoint;
            }

            Handles.BeginGUI();
            var midPoint = (point + nextPoint) / 2;
            var viewPos = Camera.current.WorldToScreenPoint(midPoint);

            if (GUI.Button(new Rect(viewPos.x - 8, Camera.current.pixelHeight - viewPos.y - 8, 16, 16), "+"))
            {
                Debug.Log(iPoint + " , " + iNextPoint);
                randomWalkArea.InsertVertexBetweenVertices(iPoint, iNextPoint);

            }

            Handles.EndGUI();
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