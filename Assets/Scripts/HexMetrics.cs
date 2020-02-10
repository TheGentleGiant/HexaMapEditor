using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMetrics
{
    /*HEX BASE CONSTRUCTION*/
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    private static Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius), 

    };
    
    public static Vector3 GetFirstCorner(HexDirection direction) => corners[(int) direction];
    public static Vector3 GetSecondCorner(HexDirection direction) => corners[(int) direction + 1];
    
    /*COLOR BLENDING*/
    public const float solidFactor = 0.75f;
    public const float blendFactor = 1f - solidFactor;

    public static Vector3 GetFirstSolidCorner(HexDirection direction) => corners[(int) direction] * solidFactor;
    public static Vector3 GetSecondSolidCorner(HexDirection direction) => corners[(int) direction + 1] * solidFactor;

    public static Vector3 GetBridge(HexDirection direction) =>
        (corners[(int) direction] + corners[(int) direction + 1]) * blendFactor;

    
    /*ELEVATION*/
    public const float elevationStep = 5f;
}

