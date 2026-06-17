#if UNITY_EDITOR
using GameInit.Hierarchy;
using UnityEditor;
using UnityEngine;

namespace GameInit.Editor.Hierarchy
{
    /// <summary>
    /// Gerencia a renderização customizada de objetos na janela Hierarchy do Unity Editor
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyCustomizer
    {
        static HierarchyCustomizer()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
        }

        /// <summary>
        /// Renderiza objetos customizados na hierarchy quando detectado componente apropriado
        /// </summary>
        private static void OnHierarchyItemGUI(EntityId instanceID, Rect rect)
        {
            var gameObject = EditorUtility.EntityIdToObject(instanceID) as GameObject;
            
            if (!IsValidGameObject(gameObject)) return;
            
            if (gameObject && gameObject.TryGetComponent<HierarchyHeader>(out var header))
            {
                HierarchyRenderer.RenderHeader(rect, gameObject.name, header);
            }
        }

        /// <summary>
        /// Valida se o GameObject é válido para processamento
        /// </summary>
        private static bool IsValidGameObject(GameObject gameObject)
        {
            return gameObject;
        }
    }
}
#endif