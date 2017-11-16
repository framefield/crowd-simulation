using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RandomWalkArea : MonoBehaviour
{
    void OnEnable()
    {

        Vertices = new List<Vector3>();
        Triangles = new List<int[]>();

        Vertices.Add(new Vector3(INITIALSIZE, 0, INITIALSIZE));
        Vertices.Add(new Vector3(-INITIALSIZE, 0, INITIALSIZE));
        Vertices.Add(new Vector3(-INITIALSIZE, 0, -INITIALSIZE));

        int[] t0 = { 0, 1, 2 };
        Triangles.Add(t0);
        Border.Add(0);
        Border.Add(1);
        Border.Add(2);
    }


    void OnDrawGizmos()
    {
        foreach (var t in Triangles)
            DrawTriangle(t);
        // DrawPolygon();

        // Debug.Log("TRIANGLES");
        // foreach (var t in Triangles)
        //     Debug.Log(t[0] + ", " + t[1] + ", " + t[2]);
    }

    public void InsertVertexBetweenVertices(int v0, int v1)
    {
        var triangleToSplit = FindTriangleThatContains(v0, v1);
        int iOppositeVertex = -1;
        foreach (var v in triangleToSplit)
            if (v != v0 && v != v1)
                iOppositeVertex = v;

        var newPointBetweenV0AndV1 = 0.5f * (Vertices[v0] + Vertices[v1]);
        int iNewVertex = Vertices.Count;
        InsertVertexIntoBorder(iNewVertex, v0, v1);
        Vertices.Add(newPointBetweenV0AndV1);




        int[] newTriangle0 = { v0, iNewVertex, iOppositeVertex };
        int[] newTriangle1 = { v1, iNewVertex, iOppositeVertex };

        Triangles.Add(newTriangle0);
        Triangles.Add(newTriangle1);

    }

    void InsertVertexIntoBorder(int iNewVertex, int leftNeighbour, int rightNeighbour)
    {
        int positionToInsertAt = -1;
        for (int i = 0; i < Border.Count; i++)
        {
            if (i == rightNeighbour && (i + 1 % Border.Count) == leftNeighbour)
            {
                positionToInsertAt = (i + 1 % Border.Count);
            }
        }
        Debug.Log(positionToInsertAt);
        Border.Insert(positionToInsertAt, iNewVertex);
    }

    int[] FindTriangleThatContains(int v0, int v1)
    {
        foreach (var triangle in Triangles)
        {
            var foundV0 = false;
            var foundV1 = false;
            foreach (var vertex in triangle)
            {
                if (vertex == v0)
                    foundV0 = true;
                if (vertex == v1)
                    foundV1 = true;
            }
            if (foundV0 && foundV1)
            {
                return triangle;
            }
        }
        return null;
    }

    public void DrawPolygon()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < Border.Count; i++)
        {
            Gizmos.DrawLine(Vertices[Border[i]], Vertices[Border[(i + 1) % Border.Count]]);
        }
    }

    public void DrawTriangle(int[] triangle)
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawLine(Vertices[triangle[i]], Vertices[triangle[(i + 1) % 3]]);
        }
    }

    public List<Vector3> Vertices;
    public List<int[]> Triangles;
    public List<int> Border;
    const float INITIALSIZE = 10f;
}

