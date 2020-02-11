using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
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
#region TriangulateConnections
    void TriangulateConnections(HexDirection direction, HexCell cell, Vector3 vertex1, Vector3 vertex2)
    {
        
        HexCell currentNeighbor = cell.GetNeighbor(direction);
        
        if (!currentNeighbor)
            return;
        
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 vertex3 = vertex1 + bridge;
        Vector3 vertex4 = vertex2 + bridge;
        /*CREATING A SLOPE*/
        vertex3.y = vertex4.y = currentNeighbor.Elevation * HexMetrics.elevationStep;
        /*TERRACING ONLY ONE HEIGHT DIFFERENCE CONNECTIONS*/
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(vertex1, vertex2, cell, vertex3, vertex4, currentNeighbor);
        }
        else
        {
            AddQuad(vertex1, vertex2, vertex3, vertex4);
            AddQuadColor(cell.color, currentNeighbor.color);
        }
        
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor)
        {
            /*SLOPE CONNECTIONS*/
            Vector3 vertex5 = vertex2 + HexMetrics.GetBridge(direction.Next());
            vertex5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;
            //AddTriangle(vertex2, vertex4, vertex5);
            //AddTriangleColor(cell.color, currentNeighbor.color, nextNeighbor.color);
            if (cell.Elevation <= currentNeighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(vertex2, cell, vertex4, currentNeighbor, vertex5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(vertex5, nextNeighbor, vertex2, cell, vertex4, currentNeighbor);
                }
            }
            else if (currentNeighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(vertex4, currentNeighbor, vertex5, nextNeighbor, vertex2, cell);
            }
            else
            {
                TriangulateCorner(vertex5, nextNeighbor, vertex2, cell, vertex4, currentNeighbor);
            }
        }

    }

    void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, Vector3 endLeft,
        Vector3 endRight, HexCell endCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);
        
        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.color, c2);
        for (int i = 2; i < HexMetrics.terracesSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;

            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c1 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }
        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.color);
    }

    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right,
        HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }

            if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell,  bottom, bottomCell);
                return;
            }
            TriangulateCornerTerracesCliffL(bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }

        if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerTerracesCliffR(bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }
        AddTriangle(bottom, left, right);
        AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
    }

    void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);
        
        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for (int i = 0; i < HexMetrics.terracesSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c1 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }
        AddQuad(v3, v4,left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);
    }

    void TriangulateCornerTerracesCliffL(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right,
        HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);
        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }
    void TriangulateCornerTerracesCliffR(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right,
        HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        Vector3 boundary = Vector3.Lerp(begin, left, b);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);
        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }
    
    void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary,
        Color boundaryColor)
    {
        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        
        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);
        for (int i = 0; i < HexMetrics.terracesSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }
        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }
#endregion
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
