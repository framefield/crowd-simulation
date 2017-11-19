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
        Border = new List<int>();

        Vertices.Add(new Vector3(INITIALSIZE, 0, INITIALSIZE));
        Vertices.Add(new Vector3(-INITIALSIZE, 0, INITIALSIZE));
        Vertices.Add(new Vector3(-INITIALSIZE, 0, -INITIALSIZE));

        int[] t0 = { 0, 2, 1 };
        Triangles.Add(t0);
        Border.AddRange(t0);
    }

    void OnDrawGizmos()
    {
        DrawMesh();
        // foreach (var t in Triangles)
        //     DrawTriangle(t);
        DrawPolygon();
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

        Debug.Log(Triangles.Remove(triangleToSplit));
        int[] newTriangle0 = { v0, iNewVertex, iOppositeVertex };
        int[] newTriangle1 = { v1, iOppositeVertex, iNewVertex };

        Triangles.Add(newTriangle0);
        Triangles.Add(newTriangle1);
    }

    void InsertVertexIntoBorder(int iNewVertex, int iNeighbour0, int iNeighbour1)
    {
        int positionToInsertAt = -1;
        for (int i = 0; i < Border.Count; i++)
        {
            var iBorderVertex0 = Border[i];
            var iBorderVertex1 = Border[(i + 1) % Border.Count];

            if ((iBorderVertex0 == iNeighbour0 && iBorderVertex1 == iNeighbour1) || (iBorderVertex0 == iNeighbour1 && iBorderVertex1 == iNeighbour0))
            {
                positionToInsertAt = ((i + 1) % Border.Count);
            }
        }
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
            Gizmos.DrawLine(Vertices[Border[i]], Vertices[Border[(i + 1) % Border.Count]]);
    }

    public void DrawTriangle(int[] triangle)
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < 3; i++)
            Gizmos.DrawLine(Vertices[triangle[i]], Vertices[triangle[(i + 1) % 3]]);
    }

    public void DrawMesh()
    {
        var m = new Mesh();
        m.SetVertices(Vertices);

        var tris = new List<int>();
        foreach (var t in Triangles)
            tris.AddRange(t);
        m.SetTriangles(tris, 0);

        var normals = new List<Vector3>();
        foreach (var v in Vertices)
            normals.Add(Vector3.up);
        m.SetNormals(normals);

        Gizmos.DrawMesh(m);
    }

    public List<Vector3> Vertices;
    public List<int[]> Triangles;
    public List<int> Border;
    const float INITIALSIZE = 10f;
}

