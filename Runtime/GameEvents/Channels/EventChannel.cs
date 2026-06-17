using System;
using UnityEngine;

namespace GameInit.GameEvents.Channels
{
    public abstract class EventChannel<T> : ScriptableObject
    {
        private event Action<T> onEventRaised;

        public void RaiseEvent(T value) => onEventRaised?.Invoke(value);

        public void Subscribe(Action<T> callback) => onEventRaised += callback;

        public void Unsubscribe(Action<T> callback) => onEventRaised -= callback;

        public void ResetValue() => onEventRaised = null;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                ResetValue();
            }
        }
    }
}
