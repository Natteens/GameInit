using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameInit.Editor.Tools
{
    public static class MainToolbarElementStyler
    {
        public static void StyleElement<T>(string elementName, System.Action<T> styleAction) where T : VisualElement
        {
            EditorApplication.delayCall += () =>
            {
                ApplyStyle(elementName, (element) =>
                {
                    T targetElement = element as T ?? element.Query<T>().First();
                    if (targetElement == null) return;
                    styleAction(targetElement);
                });
            };
        }

        static void ApplyStyle(string elementName, System.Action<VisualElement> styleCallback)
        {
            var element = FindElementByName(elementName);
            if (element == null) return;
            styleCallback(element);
        }

        static VisualElement FindElementByName(string name)
        {
            var windows= Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                var root= window.rootVisualElement;
                if (root == null) continue;

                VisualElement element;
                if ((element = root.Q<VisualElement>(name)) != null) return element;
                if ((element = root.Query<VisualElement>().Where(e => e.tooltip == name).First()) != null) return element;
            }
            return null;
        }
    }
}