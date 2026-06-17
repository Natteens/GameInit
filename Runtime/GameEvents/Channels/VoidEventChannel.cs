using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Scriptable Objects/GameInit/Events/Void Event")]
    public class VoidEventChannel : EventChannel<VoidEvent> { }
}