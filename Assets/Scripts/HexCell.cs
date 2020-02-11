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
    /*UI*/
    public RectTransform uiLabelTransform;
    
    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;
            
            /*UI UPDATE*/
            Vector3 uiPosition = uiLabelTransform.localPosition;
            uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiLabelTransform.localPosition = uiPosition;
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

    public HexEdgeType GetEdgeType(HexDirection direction) => HexMetrics.GetEdgeType(elevation, neighbors[(int) direction].elevation);
    public HexEdgeType GetEdgeType(HexCell otherCell) => HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    

}
