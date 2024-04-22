using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LevelData.LevelFragment.Socket))]
public class SocketDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		// Draw the label for the entire property
		EditorGUI.LabelField(position, label);

		// Adjust the position for the checkboxes
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		// Split the position into three equal parts for the checkboxes
		Rect leftPos = new(position.x, position.y, position.width / 3f, position.height);
		Rect centerPos = new(position.x + position.width / 3f, position.y, position.width / 3f, position.height);
		Rect rightPos = new(position.x + (2 * position.width / 3f), position.y, position.width / 3f, position.height);

		// Find the serialized properties for the three bool fields
		SerializedProperty leftProp = property.FindPropertyRelative("left");
		SerializedProperty centerProp = property.FindPropertyRelative("center");
		SerializedProperty rightProp = property.FindPropertyRelative("right");

		// Draw the checkboxes with labels
		EditorGUI.PropertyField(leftPos, leftProp, GUIContent.none);
		EditorGUI.PropertyField(centerPos, centerProp, GUIContent.none);
		EditorGUI.PropertyField(rightPos, rightProp, GUIContent.none);

		EditorGUI.EndProperty();
	}
}
