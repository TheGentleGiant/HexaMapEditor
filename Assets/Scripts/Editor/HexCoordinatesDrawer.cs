
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HexCoordinates coords = new HexCoordinates( property.FindPropertyRelative("x").intValue, property.FindPropertyRelative("z").intValue);
        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coords.ToString());
        
        
    }
}
