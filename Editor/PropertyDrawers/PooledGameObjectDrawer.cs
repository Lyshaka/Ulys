using Ulys.Runtime.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ulys.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(PooledGameObject))]
    public class PooledGameObjectDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prefab = property.FindPropertyRelative("prefab");
            SerializedProperty parent = property.FindPropertyRelative("parent");
            SerializedProperty initialSize = property.FindPropertyRelative("initialSize");
            SerializedProperty prewarm = property.FindPropertyRelative("prewarm");

            float lineHeight = EditorGUIUtility.singleLineHeight;

            // Draw the normal label and prefab field.
            Rect labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                lineHeight
            );

            EditorGUI.LabelField(labelRect, label);

            Rect prefabRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                lineHeight
            );

            EditorGUI.PropertyField(prefabRect, prefab, GUIContent.none);

            // Draw the foldout arrow slightly outside the normal property area.
            Rect foldoutRect = new Rect(
                position.x - 2f,
                position.y,
                lineHeight,
                lineHeight
            );

            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                GUIContent.none
            );

            Rect rect = new(position.x, position.y, position.width, lineHeight);

            // Make the entire first row clickable.
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                property.isExpanded = !property.isExpanded;
            }

            // Draw expanded settings.
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                rect.y += lineHeight + Spacing;
                EditorGUI.PropertyField(rect, parent);

                rect.y += lineHeight + Spacing;
                EditorGUI.PropertyField(rect, initialSize);

                rect.y += lineHeight + Spacing;
                EditorGUI.PropertyField(rect, prewarm);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            int fieldCount = 3;

            return EditorGUIUtility.singleLineHeight + Spacing + fieldCount * (EditorGUIUtility.singleLineHeight + Spacing) - Spacing;
        }
    }
}