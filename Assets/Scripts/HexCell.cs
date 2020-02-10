using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public class HexCell : MonoBehaviour
{
    /*FIELDS*/
    public HexCoordinates coords;
    public Color color;
    [SerializeField] private HexCell[] neighbors;
    private int elevation;

    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;
        }
    }

    
    
    /*HELPER FUNCTIONS*/
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int) direction] = cell;
        cell.neighbors[(int) direction.Opposite()] = this;
    }

}
