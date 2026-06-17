#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using GameInit.Hierarchy;

namespace GameInit.Editor.Hierarchy
{
    [InitializeOnLoad]
    public static class HierarchyIconDisplay
    {
        private static EditorWindow _hierarchyWindow;
        private static bool _isHierarchyFocused;
        
        static HierarchyIconDisplay()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
            EditorApplication.update += UpdateHierarchyFocus;
        }
        
        private static void UpdateHierarchyFocus()
        {
            if (_hierarchyWindow != null && _hierarchyWindow == null)
            {
                _hierarchyWindow = null;
            }
    
            if (_hierarchyWindow == null)
            {
                Type hierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
                if (hierarchyType != null)
                {
                    var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                    _hierarchyWindow = allWindows.FirstOrDefault(w => w.GetType() == hierarchyType);
                }
            }
            _isHierarchyFocused = _hierarchyWindow != null && EditorWindow.focusedWindow == _hierarchyWindow;
        }
        
        private static void OnHierarchyItemGUI(EntityId instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.EntityIdToObject(instanceID) as GameObject;
            if (gameObject == null || PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject) != null)
                return;
            
            // Não desenhar ícone para Headers
            if (gameObject.GetComponent<HierarchyHeader>() != null)
                return;
            
            Component targetComponent = GetPriorityComponent(gameObject);
            if (targetComponent == null)
                return;
            
            GUIContent iconContent = CreateIconContent(targetComponent);
            if (iconContent?.image == null)
                return;
            
            DrawIcon(instanceID, selectionRect, iconContent);
        }
        
        private static Component GetPriorityComponent(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            if (components == null || components.Length == 0)
                return null;
            
            Component tmpComponent = null;
            Component otherComponent = null;
            Component transformComponent = null;
            
            for (int i = 0; i < components.Length; i++)
            {
                Component comp = components[i];
                if (comp == null) continue;
                
                Type compType = comp.GetType();
                
                if (compType.Name.Contains("TMP") || compType.Name.Contains("TextMesh"))
                {
                    tmpComponent = comp;
                }
                else if (compType != typeof(Transform) && compType != typeof(RectTransform) && compType != typeof(CanvasRenderer))
                {
                    if (otherComponent == null)
                        otherComponent = comp;
                }
                else if (compType == typeof(Transform) || compType == typeof(RectTransform))
                {
                    transformComponent = comp;
                }
            }
            
            return tmpComponent ?? otherComponent ?? transformComponent;
        }
        
        private static GUIContent CreateIconContent(Component component)
        {
            GUIContent content = EditorGUIUtility.ObjectContent(component, component.GetType());
            content.text = null;
            content.tooltip = component.GetType().Name;
            return content;
        }
        
        private static void DrawIcon(EntityId instanceID, Rect selectionRect, GUIContent iconContent)
        {
            bool isSelected = Selection.entityIds.Contains(instanceID);
            bool isHovered = selectionRect.Contains(Event.current.mousePosition);
            
            Color backgroundColor = GetBackgroundColor(isSelected, isHovered);
            
            Rect iconRect = new Rect(selectionRect.x -2, selectionRect.y -.5f, 18f, selectionRect.height);
            
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(iconRect, backgroundColor);
                GUI.DrawTexture(new Rect(iconRect.x, iconRect.y, 16f, 16f), iconContent.image);
            }
        }
        
        private static Color GetBackgroundColor(bool isSelected, bool isHovered)
        {
            if (isSelected && _isHierarchyFocused)
                return EditorGUIUtility.isProSkin ? new Color(0.1725f, 0.3647f, 0.5294f) : new Color(0.2274f, 0.447f, 0.6902f);
            
            if (isSelected)
                return EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.68f, 0.68f, 0.68f);
            
            if (isHovered)
                return EditorGUIUtility.isProSkin ? new Color(0.2706f, 0.2706f, 0.2706f) : new Color(0.698f, 0.698f, 0.698f);
            
            return EditorGUIUtility.isProSkin ? new Color(0.2196f, 0.2196f, 0.2196f) : new Color(0.7843f, 0.7843f, 0.7843f);
        }
    }
}
#endif