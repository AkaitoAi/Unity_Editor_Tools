
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AkaitoAi
{
    public enum UpdateType { FrameBased, TimeBased, CoroutineBased }

    [DefaultExecutionOrder(-100)]
    public class AdvanceUpdateManager : MonoBehaviour
    {
        private class SubscriberGroup
        {
            public UpdateType Type;
            public float TimeInterval;
            public int FrameInterval;
            public List<Action> Subscribers = new();
            public float Timer;
            public int LastFrame;
            public Coroutine Coroutine;
        }

        private readonly Dictionary<string, SubscriberGroup> groups = new();

        private void Update()
        {
            foreach (var group in groups.Values)
            {
                switch (group.Type)
                {
                    case UpdateType.FrameBased:
                        if ((Time.frameCount - group.LastFrame) >= group.FrameInterval)
                        {
                            group.LastFrame = Time.frameCount;
                            for (int i = 0; i < group.Subscribers.Count; i++)
                                group.Subscribers[i]?.Invoke();
                        }
                        break;

                    case UpdateType.TimeBased:
                        group.Timer += Time.deltaTime;
                        if (group.Timer >= group.TimeInterval)
                        {
                            group.Timer = 0f;
                            for (int i = 0; i < group.Subscribers.Count; i++)
                                group.Subscribers[i]?.Invoke();
                        }
                        break;
                }
            }
        }

        public void Register(Action callback, UpdateType type, float timeInterval = 0.1f, int frameInterval = 5)
        {
            if (callback == null) return;

            var key = GetGroupKey(type, timeInterval, frameInterval);

            if (!groups.TryGetValue(key, out var group))
            {
                group = new SubscriberGroup
                {
                    Type = type,
                    TimeInterval = timeInterval,
                    FrameInterval = frameInterval,
                    LastFrame = Time.frameCount
                };

                groups[key] = group;

                if (type == UpdateType.CoroutineBased)
                {
                    group.Coroutine = StartCoroutine(CoroutineGroupLoop(group));
                }
            }

            if (!group.Subscribers.Contains(callback))
                group.Subscribers.Add(callback);
        }

        public void Unregister(Action callback)
        {
            foreach (var key in groups.Keys.ToList())
            {
                var group = groups[key];
                if (group.Subscribers.Remove(callback))
                {
                    if (group.Subscribers.Count == 0)
                    {
                        if (group.Type == UpdateType.CoroutineBased && group.Coroutine != null)
                            StopCoroutine(group.Coroutine);

                        groups.Remove(key);
                    }
                    break;
                }
            }
        }

        private IEnumerator CoroutineGroupLoop(SubscriberGroup group)
        {
            while (true)
            {
                for (int i = 0; i < group.Subscribers.Count; i++)
                    group.Subscribers[i]?.Invoke();

                yield return new WaitForSeconds(group.TimeInterval);
            }
        }

        private string GetGroupKey(UpdateType type, float timeInterval, int frameInterval)
        {
            return $"{type}_{timeInterval}_{frameInterval}";
        }

        public void ListGroups()
        {
            if (groups.Count == 0)
            {
                Debug.Log("AdvanceUpdateManager: No active subscriber groups.");
                return;
            }

            foreach (var kvp in groups)
            {
                Debug.Log($"Group: {kvp.Key} | Subscribers: {kvp.Value.Subscribers.Count}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("🔁 Test Dummy Registration")]
        private void TestRegisterDummy()
        {
            Debug.Log("AdvanceUpdateManager: Registering dummy callback for testing.");
            Register(DummyCallback, UpdateType.TimeBased, timeInterval: 1f);
        }

        private void DummyCallback()
        {
            Debug.Log("AdvanceUpdateManager: Dummy callback invoked.");
        }

        [ContextMenu("📋 List Active Groups")]
        private void EditorListGroups()
        {
            ListGroups();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUI.Box(new Rect(10, 10, 300, 25 + groups.Count * 20), "AdvanceUpdateManager - Active Groups");

            int y = 35;
            foreach (var kv in groups)
            {
                GUI.Label(new Rect(20, y, 280, 20), $"- {kv.Key} → {kv.Value.Subscribers.Count} subs");
                y += 20;
            }
        }
#endif
    }

    //[SerializeField] private UpdateManager updateManager;

    //    private void OnEnable()
    //    {
    //        updateManager.Register(RegenerateHealth, UpdateType.CoroutineBased, timeInterval: 2f); // every 2 seconds
    //    }

    //    private void OnDisable()
    //    {
    //        updateManager.Unregister(RegenerateHealth);
    //    }

    }
