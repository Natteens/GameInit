using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    [CreateAssetMenu(fileName = "FloatEventChannel", menuName = "Scriptable Objects/GameInit/Events/Float Event")]
    public class FloatEventChannel : EventChannel<float> { }
}