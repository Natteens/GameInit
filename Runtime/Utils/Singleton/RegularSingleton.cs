using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameInit.Utils {
    /// <summary>
    /// Persistent Regulator singleton, will destroy any other older components of the same type it finds on awake
    /// </summary>
    public sealed class RegulatorSingleton<T> : MonoBehaviour where T : Component {
        private static T instance;

        static RegulatorSingleton() {
            SingletonResetRegistry.Register(ResetStatics);
        }

        static void ResetStatics() => instance = null;

        public static bool HasInstance => instance != null;

        public float InitializationTime { get; private set; }

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        private void Awake() {
            InitializeSingleton();
        }

        private void InitializeSingleton() {
            if (!Application.isPlaying) return;
            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

#if UNITY_6000_5_OR_NEWER
            T[] oldInstances = FindObjectsByType<T>();
#else
            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
#endif
            foreach (T old in oldInstances) {
                if (old.GetComponent<RegulatorSingleton<T>>().InitializationTime < InitializationTime) {
                    Destroy(old.gameObject);
                }
            }

            if (instance == null) {
                instance = this as T;
            }
        }
    }

    internal static class SingletonResetRegistry {
        static readonly HashSet<Action> Resetters = new();

        internal static void Register(Action resetter) {
            Resetters.Add(resetter);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() {
            foreach (Action resetter in Resetters) {
                resetter();
            }
        }
    }
}
