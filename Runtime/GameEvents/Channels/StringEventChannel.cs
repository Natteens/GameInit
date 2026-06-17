using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    [CreateAssetMenu(fileName = "StringEventChannel", menuName = "Scriptable Objects/GameInit/Events/String Event")]
    public class StringEventChannel : EventChannel<string> { }
}