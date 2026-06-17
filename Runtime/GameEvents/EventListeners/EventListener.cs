using GameInit.GameEvents.Channels;
using UnityEngine;
using UnityEngine.Events;

namespace GameInit.GameEvents.EventListeners {
    public abstract class EventListener<T> : MonoBehaviour {
        [Header("Event Configuration")]
        [SerializeField] protected EventChannel<T> eventChannel;

        [Header("Response")]
        [SerializeField] protected UnityEvent<T> onEventRaised;

        protected virtual void OnEnable() {
            if (eventChannel != null) {
                eventChannel.Subscribe(OnEventRaised);
            }
        }

        protected virtual void OnDisable() {
            if (eventChannel != null) {
                eventChannel.Unsubscribe(OnEventRaised);
            }
        }

        public virtual void OnEventRaised(T value) {
            onEventRaised?.Invoke(value);
        }
    }
}
