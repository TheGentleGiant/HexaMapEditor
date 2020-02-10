using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HexGrid : MonoBehaviour
{
    /*GRID SIZE*/
    public int width = 6;
    public int height = 6;

    /*CELL*/
    public HexCell cellPrefab = null;
    private HexCell[] _cells;

    /*UI*/
    public Text cellTextPrefab = null;
    private Canvas gridCanvas;
    
    /*HEX MESH*/
    private HexMesh hexMesh;
    
    /*GRID COLORING*/
    public Color defaultColor = Color.white;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
        
        _cells = new HexCell[height * width];
        for (int z = 0, i =0 ; z < height ; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void Start()
    {
        hexMesh.Triangulate(_cells);
    }

    public HexCell GetCell (Vector3 hitPoint)
    {
        hitPoint = transform.InverseTransformPoint(hitPoint);
        HexCoordinates coords = HexCoordinates.FromPosition(hitPoint);
        int index = coords.X + coords.Z * width + coords.Z / 2;
        return _cells[index];
        //HexCell cell = _cells[index];
        //cell.color = color;
        //hexMesh.Triangulate(_cells);
        //Debug.Log("clicked on cell at: " + coords);
    }

    public void Refresh()
    {
        hexMesh.Triangulate(_cells);
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = _cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coords = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, _cells[i -width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i -width -1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, _cells[i -width]);
                if (x < width -1)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - width + 1]);
                }
            }
        }
        Text label = Instantiate<Text>(cellTextPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = x + "\n" + z;
        label.text = cell.coords.ToStringOnSeparateLines();
    }
}


