using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityScript.Core;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    /*MESH*/
    private Mesh hexMesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshCollider meshCollider;
    
    /*COLOR*/
    private List<Color> meshColors;
    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "HEX Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        meshColors = new List<Color>();
    }

    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        meshColors.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = meshColors.ToArray(); 
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    private void Triangulate(HexCell cell)
    {
        for (HexDirection dir = HexDirection.NE; dir <=HexDirection.NW; dir++)
        {
            Triangulate(dir, cell);
        }
    }

    private void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 vertex1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 vertex2 = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangle(center, vertex1, vertex2);
        AddTriangleColor(cell.color);
        /*ADDING A BRIDGE IN BETWEEN HEXAGONS*/
        if (direction <= HexDirection.SE)
        {
            TriangulateConnections(direction, cell, vertex1, vertex2);
        }
        /*
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 vertex3 = vertex1 + bridge;
        Vector3 vertex4 = vertex2 + bridge;
        AddQuad(vertex1, vertex2, vertex3, vertex4);

        HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        HexCell currentNeighbor = cell.GetNeighbor(direction) ?? cell;
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
        Color bridgeColor = (cell.color + currentNeighbor.color) * 0.5f;
        AddQuadColor(cell.color, bridgeColor);*/
        /*CLOSING THE GAPS FROM THE BRIDGING*/
        /*
        AddTriangle(vertex1, center + HexMetrics.GetFirstCorner(direction), vertex3);
        AddTriangleColor(cell.color, (cell.color + prevNeighbor.color + currentNeighbor.color) / 3f, bridgeColor);
        AddTriangle(vertex2, vertex4, center + HexMetrics.GetSecondCorner(direction));
        AddTriangleColor(cell.color, bridgeColor, (cell.color + currentNeighbor.color + nextNeighbor.color) / 3f);
        */
    }

    void TriangulateConnections(HexDirection direction, HexCell cell, Vector3 vertex1, Vector3 vertex2)
    {
        
        HexCell currentNeighbor = cell.GetNeighbor(direction);
        
        if (!currentNeighbor)
            return;
        
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 vertex3 = vertex1 + bridge;
        Vector3 vertex4 = vertex2 + bridge;
        AddQuad(vertex1, vertex2, vertex3, vertex4);
        AddQuadColor(cell.color, currentNeighbor.color);
        
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor)
        {
            AddTriangle(vertex2, vertex4, vertex2+ HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, currentNeighbor.color, nextNeighbor.color);
        }

    }
    
    
    
#region DrawingQuands'n'Tris
    /*CREATE TRIANGLES FOR EACH HEXAGON*/
    void AddTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(vertex1);
        vertices.Add(vertex2);
        vertices.Add(vertex3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex +1);
        triangles.Add(vertexIndex +2);
    }

    void AddTriangleColor(Color color)
     {
         meshColors.Add(color);
         meshColors.Add(color);
         meshColors.Add(color);
     }
    
    void AddTriangleColor(Color color1, Color color2, Color color3)
    {
        meshColors.Add(color1);
        meshColors.Add(color2);
        meshColors.Add(color3);
    }
    
    /*CREATE QUADS TO CLOSE GAP IN BETWEEN EACH HEX*/

    private void AddQuad(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4)
    {
        int vertexindex = vertices.Count;
        vertices.Add(vertex1);
        vertices.Add(vertex2);
        vertices.Add(vertex3);
        vertices.Add(vertex4);
        triangles.Add(vertexindex);
        triangles.Add(vertexindex + 2);
        triangles.Add(vertexindex + 1);
        triangles.Add(vertexindex + 1);
        triangles.Add(vertexindex + 2);
        triangles.Add(vertexindex + 3);
    }

    private void AddQuadColor(Color color1, Color color2, Color color3, Color color4)
    {
        meshColors.Add(color1);
        meshColors.Add(color2);
        meshColors.Add(color3);
        meshColors.Add(color4);
    }

    void AddQuadColor(Color color1, Color color2)
    {
        meshColors.Add(color1);
        meshColors.Add(color1);
        meshColors.Add(color2);
        meshColors.Add(color2);

    }
    #endregion
}
