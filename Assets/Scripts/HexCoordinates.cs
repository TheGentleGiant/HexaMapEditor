using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField] private int x, z;
    public int X => x;
    public int Z => z;

    public int Y
    {
        get { return -X - Z;}
        
    }
    public HexCoordinates(int x, int z) { this.x = x; this.z = z; }
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2 , z);
    }

    public override string ToString()
    {
        return "(" + X + "," + Y + "," + Z + ")";
    }
    
    public string ToStringOnSeparateLines()
    {
        return  X + "\n" + Y + "\n" + Z ;
    }


    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float deltaX = Mathf.Abs(x - iX);
            float deltaY = Mathf.Abs(y - iY);
            float deltaZ = Mathf.Abs(-x - y - iZ);
            if (deltaX > deltaY && deltaX > deltaZ)
            {
                iX = -iY - iZ;
            }
            else if (deltaZ > deltaY)
            {
                iZ = - iX - iY;
            }
        }
        return new HexCoordinates(iX, iZ);
    }
}
