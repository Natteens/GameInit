using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameInit.Editor.Tools
{
    public class MainToolbarTimescaleSlider
    {
        const float k_minTimeScale = 0f, k_maxTimeScale = 5f;

        [MainToolbarElement("Timescale/Slider", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement TimeSlider()
        {
            MainToolbarElementStyler.StyleElement<VisualElement>("Timescale/Slider", (element) =>
            {
                element.style.paddingLeft = 10f;
            });

            return new MainToolbarSlider(
                new MainToolbarContent("Time Scale", "Time Scale"),//Content(Text, Tooltip)
                Time.timeScale,//Value
                k_minTimeScale,//Min Value
                k_maxTimeScale,//Max Value
                value => Time.timeScale = value//Action
            );
        }
    }
}