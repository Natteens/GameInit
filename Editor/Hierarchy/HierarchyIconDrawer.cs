using System;
using System.Collections.Generic;
using System.Reflection;
using GameInit.Hierarchy;
using UnityEditor;
using UnityEngine;

namespace GameInit.Editor. Hierarchy
{
    [InitializeOnLoad]
    public static class HierarchyIconDrawer {
        static readonly Texture2D requiredIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
    
        static readonly Dictionary<Type, FieldInfo[]> cachedFieldInfo = new();

        static HierarchyIconDrawer() {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            if (EditorUtility.EntityIdToObject(instanceID) is not GameObject gameObject) return;

            foreach (var component in gameObject.GetComponents<Component>()) {
                if (component == null) continue;
            
                var fields = GetCachedFieldsWithRequiredAttribute(component.GetType());
                if (fields == null || fields.Length == 0) continue;

                foreach (var field in fields) {
                    if (IsFieldUnassigned(field.GetValue(component), field)) {
                        var iconRect = new Rect(selectionRect. xMax - 20, selectionRect.y - 8f, 32, 32);
                        GUI.Label(iconRect, new GUIContent(requiredIcon, "One or more required fields are missing or empty."));
                        return;
                    }
                }
            }
        }

        static FieldInfo[] GetCachedFieldsWithRequiredAttribute(Type componentType) {
            if (!cachedFieldInfo.TryGetValue(componentType, out FieldInfo[] fields)) {
                fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                List<FieldInfo> requiredFields = new();

                foreach (FieldInfo field in fields) {
                    bool isSerialized = field.IsPublic || field. IsDefined(typeof(SerializeField), false);
                    bool isRequired = field.IsDefined(typeof(RequiredFieldAttribute), false);

                    if (isSerialized && isRequired) {
                        requiredFields.Add(field);
                    }
                }
            
                fields = requiredFields.ToArray();
                cachedFieldInfo[componentType] = fields;
            }
            return fields;
        }

        static bool IsFieldUnassigned(object fieldValue, FieldInfo fieldInfo) {
            if (fieldValue == null) return true;

            if (fieldValue is UnityEngine.Object unityObject) {
                return unityObject == null || unityObject. Equals(null);
            }

            if (fieldValue is string stringValue) {
                return string.IsNullOrEmpty(stringValue);
            }

            if (fieldValue is System.Collections. ICollection collection) {
                if (collection.Count == 0) return true;
                
                foreach (var item in collection) {
                    if (item == null || (item is UnityEngine.Object unityItem && (unityItem == null || unityItem. Equals(null)))) {
                        return true;
                    }
                }
                return false;
            }

            if (fieldInfo.FieldType.IsValueType) {
                var defaultValue = Activator.CreateInstance(fieldInfo.FieldType);
                return fieldValue.Equals(defaultValue);
            }

            return false;
        }
    }
}