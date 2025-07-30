using UnityEditor;
using UnityEngine;

namespace HuGox.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float LineHeight = 20f;
        private const float Padding = 17f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, LineHeight),
                property.isExpanded,
                label
            );

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            // Calculate height and draw box background
            float totalHeight = GetPropertyHeight(property, label);
            Rect boxRect = new Rect(position.x, position.y + LineHeight - 5, position.width, totalHeight - LineHeight);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            // Content position
            Rect contentPosition = new Rect(position.x + 10, position.y + LineHeight, position.width - 20, LineHeight);

            // Headers
            EditorGUI.LabelField(contentPosition, "Keys", EditorStyles.boldLabel);
            EditorGUI.LabelField(
                new Rect(
                    contentPosition.x + contentPosition.width / 2,
                    contentPosition.y,
                    contentPosition.width / 2,
                    LineHeight
                ),
                "Values",
                EditorStyles.boldLabel
            );

            contentPosition.y += LineHeight;

            // Draw key-value pairs
            SerializedProperty keysProperty = property.FindPropertyRelative("_mKeys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("_mValues");

            if (keysProperty == null || valuesProperty == null)
            {
                EditorGUI.LabelField(position, "Error: Could not find serialized keys/values.");
                return;
            }

            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(i);
                SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(i);

                Rect keyRect = new Rect(
                    contentPosition.x,
                    contentPosition.y,
                    contentPosition.width / 2 - 20,
                    LineHeight
                );
                Rect valueRect = new Rect(
                    contentPosition.x + contentPosition.width / 2 - 20,
                    contentPosition.y,
                    contentPosition.width / 2 - 20,
                    LineHeight
                );
                Rect removeButtonRect = new Rect(
                    contentPosition.x + contentPosition.width - 20,
                    contentPosition.y,
                    20,
                    LineHeight
                );

                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

                if (GUI.Button(removeButtonRect, "-"))
                {
                    keysProperty.DeleteArrayElementAtIndex(i);
                    valuesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }

                contentPosition.y += LineHeight;
            }

            // Add button
            Rect addButtonRect = new Rect(contentPosition.x, contentPosition.y + 5, contentPosition.width, LineHeight);
            if (GUI.Button(addButtonRect, "Add"))
            {
                keysProperty.arraySize++;
                valuesProperty.arraySize++;
                keysProperty.GetArrayElementAtIndex(keysProperty.arraySize - 1).stringValue =
                    $"Key_{keysProperty.arraySize}";
                valuesProperty.GetArrayElementAtIndex(valuesProperty.arraySize - 1).objectReferenceValue = null;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LineHeight;

            SerializedProperty keysProperty = property.FindPropertyRelative("_mKeys");
            int count = keysProperty?.arraySize ?? 0;

            // Calculate total height: headers + key-value pairs + add button + padding
            return LineHeight * (2 + count) + Padding * 2;
        }
    }
}