using System.Collections;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace AkaitoAi.Events
{
    public class CustomEvents : MonoBehaviour
    {
        [Header("Use Your Desired Events")]
        public bool DisableOrNot = false;
        public float TimeToDisable = 5f;
        public float DelayEventTime = 5f;
        public string TriggerTagToDetect = "Player";

        public UnityEvent AwakeEvent;
        public UnityEvent OnStartEvent;
        public UnityEvent OnEnableEvent;
        public UnityEvent OnDisableEvent;
        public UnityEvent OnDestroyEvent;
        public UnityEvent OnTriggerEnterEvent;
        public UnityEvent EventAfterTime;
        public UnityEvent OntriggerExitEvent;

        public void Awake() => AwakeEvent?.Invoke();

        public void OnEnable() => OnEnableEvent?.Invoke();

        void Start()
        {
            OnStartEvent?.Invoke();
            StartCoroutine(PerformActionAfterTime());

            if (DisableOrNot)
                StartCoroutine(DisableThisObject());
        }

        IEnumerator DisableThisObject()
        {
            yield return new WaitForSeconds(TimeToDisable);
            gameObject.SetActive(false);
        }

        IEnumerator PerformActionAfterTime()
        {
            yield return new WaitForSeconds(DelayEventTime);
            EventAfterTime?.Invoke();
        }

        private void OnDisable() => OnDisableEvent?.Invoke();

        private void OnDestroy() => OnDestroyEvent?.Invoke();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TriggerTagToDetect))
                OnTriggerEnterEvent?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(TriggerTagToDetect))
                OntriggerExitEvent?.Invoke();
        }
    }
}
