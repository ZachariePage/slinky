#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = false;

        // Handle Range attribute for sliders
        RangeAttribute range = (RangeAttribute)fieldInfo.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault();
        if (range != null && property.propertyType == SerializedPropertyType.Integer) {
            EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max, label);
        }
        else if (range != null && property.propertyType == SerializedPropertyType.Float) {
            EditorGUI.Slider(position, property, range.min, range.max, label);
        }
        else {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }
}
#endif