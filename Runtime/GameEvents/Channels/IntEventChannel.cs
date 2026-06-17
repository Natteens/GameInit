using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "Scriptable Objects/GameInit/Events/Int Event")]
    public class IntEventChannel : EventChannel<int> { }
}