using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    [CreateAssetMenu(fileName = "BoolEventChannel", menuName = "Scriptable Objects/GameInit/Events/Bool Event")]
    public class BoolEventChannel : EventChannel<bool> { }
}