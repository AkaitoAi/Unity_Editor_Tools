using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class CollisionDetector : MonoBehaviour
{
    #region Blueprint
    [System.Serializable]
    public struct TargetTransform
    {
        [Tooltip("Target GameObject that will trigger these events.")]
        public GameObject Target;
        public CustomEvents[] Events;
    }
    [System.Serializable]
    public struct TargetTag
    {
        [Tooltip("Tag that will trigger these events.")]
        public string Target;
        public CustomEvents[] Events;
    }
    [System.Serializable]
    public struct TargetLayer
    {
        [Tooltip("Layer that will trigger these events.")]
        public LayerMask Target;
        public CustomEvents[] Events;
    }
    [System.Serializable]
    public struct CustomEvents
    {
        [Header("Detection Settings")]
        public DetectionType DetectionType;

        public UnityEvent OnEnterUnityEvent;
        public UnityEvent OnStayUnityEvent;
        public UnityEvent OnExitUnityEvent;
    }

    public enum DetectionType { Trigger, Collision }
    public enum UseType { Once, Multiple }
    #endregion

    #region Inspector
    [Header("Layer Mask")]
    [Tooltip("Specify one or more layers that are allowed to interact. Defaults to all layers.")]
    [SerializeField] LayerMask _interactionLayers;

    [SerializeField] bool _useOwnEvents = false;

    [Header("Speed Settings (Optional)")]
    [SerializeField] bool _useSpeedImpact = false;
    [SerializeField] float _speedImpact = 5f;

    [Header("Targeted")]
    [SerializeField] TargetTransform[] _targetTransform;
    [SerializeField] TargetLayer[] _targetLayer;
    [SerializeField] TargetTag[] _targetTag;
    [SerializeField] CustomEvents[] _events;

    List<string> layerNames;
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private HashSet<GameObject> stayTriggeredObjects = new HashSet<GameObject>();
    #endregion

    #region Unity
    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        if (_useOwnEvents) { HandleTriggerEnter(other); }
        if (!other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleTriggerEnter(other, gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        if (_useOwnEvents) { HandleTriggerStay(other); }
        if (!other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleTriggerStay(other, gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        if (_useOwnEvents) { HandleTriggerExit(other); }
        if (!other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleTriggerExit(other, gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject.layer)) return;

        if (_useOwnEvents) { HandleCollisionEnter(collision); }
        if (!collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleCollisionEnter(collision, gameObject);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject.layer)) return;

        if (_useOwnEvents) { HandleCollisionStay(collision); }
        if (!collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleCollisionStay(collision, gameObject);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject.layer)) return;

        if (_useOwnEvents) { HandleCollisionExit(collision); }
        if (!collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) return;
        collisionEventHandler.HandleCollisionExit(collision, gameObject);
    }

    #endregion

    #region Private
    private void HandleTriggerEnter(Collider other)
    {
        GameObject triggeredObg = other.gameObject;
        if (!activeObjects.Contains(triggeredObg))
        {
            activeObjects.Add(triggeredObg);

            if (_targetTransform.Length > 0)
            {
                TargetTransform? foundTransform = GetTargetEvent(triggeredObg);
                if (foundTransform == null) return;
                for (int i = 0; i < foundTransform?.Events.Length; i++)
                {
                    if (foundTransform?.Events[i].DetectionType == DetectionType.Trigger)
                        foundTransform?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }

            if (_targetTag.Length > 0)
            {
                TargetTag? foundTag = GetTagEvent(triggeredObg.tag);
                if (foundTag == null) return;
                for (int i = 0; i < foundTag?.Events.Length; i++)
                {
                    if (foundTag?.Events[i].DetectionType == DetectionType.Trigger)
                        foundTag?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }
            if (_targetLayer.Length > 0)
            {
                TargetLayer? foundLayer = GetLayerEvent(triggeredObg.layer);
                if (foundLayer == null) return;
                for (int i = 0; i < foundLayer?.Events.Length; i++)
                {
                    if (foundLayer?.Events[i].DetectionType == DetectionType.Trigger)
                        foundLayer?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }
        }
    }
    private void HandleTriggerStay(Collider other)
    {
        GameObject triggeredObg = other.gameObject;

        if (activeObjects.Contains(triggeredObg) && !stayTriggeredObjects.Contains(triggeredObg))
        {
            stayTriggeredObjects.Add(triggeredObg);

            if (_targetTransform.Length > 0)
            {
                TargetTransform? foundTransform = GetTargetEvent(triggeredObg);
                if (foundTransform == null) return;
                for (int i = 0; i < foundTransform?.Events.Length; i++)
                {
                    if (foundTransform?.Events[i].DetectionType == DetectionType.Trigger)
                        foundTransform?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }

            if (_targetTag.Length > 0)
            {
                TargetTag? foundTag = GetTagEvent(triggeredObg.tag);
                if (foundTag == null) return;
                for (int i = 0; i < foundTag?.Events.Length; i++)
                {
                    if (foundTag?.Events[i].DetectionType == DetectionType.Trigger)
                        foundTag?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }
            if (_targetLayer.Length > 0)
            {
                TargetLayer? foundLayer = GetLayerEvent(triggeredObg.layer);
                if (foundLayer == null) return;
                for (int i = 0; i < foundLayer?.Events.Length; i++)
                {
                    if (foundLayer?.Events[i].DetectionType == DetectionType.Trigger)
                        foundLayer?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }
        }
    }
    private void HandleTriggerExit(Collider other)
    {
        GameObject triggeredObg = other.gameObject;

        if (_targetTransform.Length > 0)
        {
            TargetTransform? foundTransform = GetTargetEvent(triggeredObg);
            if (foundTransform == null) return;
            for (int i = 0; i < foundTransform?.Events.Length; i++)
            {
                if (foundTransform?.Events[i].DetectionType == DetectionType.Trigger)
                    foundTransform?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }

        if (_targetTag.Length > 0)
        {
            TargetTag? foundTag = GetTagEvent(triggeredObg.tag);
            if (foundTag == null) return;
            for (int i = 0; i < foundTag?.Events.Length; i++)
            {
                if (foundTag?.Events[i].DetectionType == DetectionType.Trigger)
                    foundTag?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }
        if (_targetLayer.Length > 0)
        {
            TargetLayer? foundLayer = GetLayerEvent(triggeredObg.layer);
            if (foundLayer == null) return;
            for (int i = 0; i < foundLayer?.Events.Length; i++)
            {
                if (foundLayer?.Events[i].DetectionType == DetectionType.Trigger)
                    foundLayer?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }

        activeObjects.Remove(triggeredObg);
        stayTriggeredObjects.Remove(triggeredObg);
    }
    private void HandleCollisionEnter(Collision collision)
    {
        GameObject CollidedObg = collision.gameObject;
        if (_useSpeedImpact)
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed < _speedImpact)
                return;
        }
        if (!activeObjects.Contains(CollidedObg))
        {
            activeObjects.Add(CollidedObg);

            if (_targetTransform.Length > 0)
            {
                TargetTransform? foundTransform = GetTargetEvent(CollidedObg);
                if (foundTransform == null) return;
                for (int i = 0; i < foundTransform?.Events.Length; i++)
                {
                    if (foundTransform?.Events[i].DetectionType == DetectionType.Collision)
                        foundTransform?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }

            if (_targetTag.Length > 0)
            {
                TargetTag? foundTag = GetTagEvent(CollidedObg.tag);
                if (foundTag == null) return;
                for (int i = 0; i < foundTag?.Events.Length; i++)
                {
                    if (foundTag?.Events[i].DetectionType == DetectionType.Collision)
                        foundTag?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }
            if (_targetLayer.Length > 0)
            {
                TargetLayer? foundLayer = GetLayerEvent(CollidedObg.layer);
                if (foundLayer == null) return;
                for (int i = 0; i < foundLayer?.Events.Length; i++)
                {
                    if (foundLayer?.Events[i].DetectionType == DetectionType.Collision)
                        foundLayer?.Events[i].OnEnterUnityEvent?.Invoke();
                }
            }
        }
    }
    private void HandleCollisionStay(Collision collision)
    {
        GameObject CollidedObg = collision.gameObject;

        if (activeObjects.Contains(CollidedObg) && !stayTriggeredObjects.Contains(CollidedObg))
        {
            stayTriggeredObjects.Add(CollidedObg);

            if (_targetTransform.Length > 0)
            {
                TargetTransform? foundTransform = GetTargetEvent(CollidedObg);
                if (foundTransform == null) return;
                for (int i = 0; i < foundTransform?.Events.Length; i++)
                {
                    if (foundTransform?.Events[i].DetectionType == DetectionType.Collision)
                        foundTransform?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }

            if (_targetTag.Length > 0)
            {
                TargetTag? foundTag = GetTagEvent(CollidedObg.tag);
                if (foundTag == null) return;
                for (int i = 0; i < foundTag?.Events.Length; i++)
                {
                    if (foundTag?.Events[i].DetectionType == DetectionType.Collision)
                        foundTag?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }
            if (_targetLayer.Length > 0)
            {
                TargetLayer? foundLayer = GetLayerEvent(CollidedObg.layer);
                if (foundLayer == null) return;
                for (int i = 0; i < foundLayer?.Events.Length; i++)
                {
                    if (foundLayer?.Events[i].DetectionType == DetectionType.Collision)
                        foundLayer?.Events[i].OnStayUnityEvent?.Invoke();
                }
            }
        }
    }
    private void HandleCollisionExit(Collision collision)
    {
        GameObject CollidedObg = collision.gameObject;

        if (_targetTransform.Length > 0)
        {
            TargetTransform? foundTransform = GetTargetEvent(CollidedObg);
            if (foundTransform == null) return;
            for (int i = 0; i < foundTransform?.Events.Length; i++)
            {
                if (foundTransform?.Events[i].DetectionType == DetectionType.Collision)
                    foundTransform?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }

        if (_targetTag.Length > 0)
        {
            TargetTag? foundTag = GetTagEvent(CollidedObg.tag);
            if (foundTag == null) return;
            for (int i = 0; i < foundTag?.Events.Length; i++)
            {
                if (foundTag?.Events[i].DetectionType == DetectionType.Collision)
                    foundTag?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }
        if (_targetLayer.Length > 0)
        {
            TargetLayer? foundLayer = GetLayerEvent(CollidedObg.layer);
            if (foundLayer == null) return;
            for (int i = 0; i < foundLayer?.Events.Length; i++)
            {
                if (foundLayer?.Events[i].DetectionType == DetectionType.Collision)
                    foundLayer?.Events[i].OnStayUnityEvent?.Invoke();
            }
        }

        activeObjects.Remove(CollidedObg);
        stayTriggeredObjects.Remove(CollidedObg);

    }
    private bool IsInLayerMask(LayerMask layer)
    {
        return ((_interactionLayers.value & (1 << layer.value)) != 0);
    }
    private TargetTransform? GetTargetEvent(GameObject obj)
    {
        if (_targetTransform.Length == 0)
            return null;

        for (int i = _targetTransform.Length - 1; i >= 0; i--)
        {
            if (_targetTransform[i].Target == obj)
            {
                return _targetTransform[i];
            }
        }
        return null;
    }
    private TargetLayer? GetLayerEvent(LayerMask layer)
    {
        if (_targetLayer.Length == 0)
            return null;

        for (int i = _targetLayer.Length - 1; i >= 0; i--)
        {
            if ((_targetLayer[i].Target.value & (1 << layer.value)) != 0)
            {
                return _targetLayer[i];
            }
        }

        return null;
    }
    private TargetTag? GetTagEvent(string tag)
    {
        if (_targetTag.Length == 0)
            return null;

        for (int i = _targetTag.Length - 1; i >= 0; i--)
        {
            if (_targetTag[i].Target == tag)
            {
                return _targetTag[i];
            }
        }
        return null;
    }
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(CollisionDetector))]
public class CollisionDetectorEditor : Editor
{
    private SerializedProperty _interactionLayers;

    private SerializedProperty _useOwnEvents;

    private SerializedProperty _useSpeedImpact;
    private SerializedProperty _speedImpact;


    private SerializedProperty _targetTransform;
    private SerializedProperty _targetTag;
    private SerializedProperty _targetLayer;
    private SerializedProperty _events;

    private bool showTargetTransform = false;
    private bool showTargetTag = false;
    private bool showTargetLayer = false;
    private bool showEvents = false;

    private int currentOpenIndex = -1;
    private bool allowMultipleSelection = false;

    private void OnEnable()
    {
        _interactionLayers = serializedObject.FindProperty("_interactionLayers");

        _useOwnEvents = serializedObject.FindProperty("_useOwnEvents");


        _useSpeedImpact = serializedObject.FindProperty("_useSpeedImpact");
        _speedImpact = serializedObject.FindProperty("_speedImpact");


        _targetTransform = serializedObject.FindProperty("_targetTransform");
        _targetTag = serializedObject.FindProperty("_targetTag");
        _targetLayer = serializedObject.FindProperty("_targetLayer");
        _events = serializedObject.FindProperty("_events");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        allowMultipleSelection = EditorGUILayout.Toggle("Allow Multiple Selection", allowMultipleSelection);
        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_interactionLayers"), new GUIContent("Interaction Layers"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_useOwnEvents"), new GUIContent("Use Own Events"));
        SerializedProperty useDelay = serializedObject.FindProperty("_useOwnEvents");
        if (useDelay.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_useSpeedImpact"), new GUIContent("Use Speed Impact"));
            SerializedProperty useSpeedImpact = serializedObject.FindProperty("_useSpeedImpact");

            if (useSpeedImpact.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_speedImpact"), new GUIContent("Speed Impact"));
            }

            DrawEventToggle(0, ref showTargetTransform, "Target Transform", _targetTransform);
            DrawEventToggle(1, ref showTargetTag, "Tag Transform", _targetTag);
            DrawEventToggle(2, ref showTargetLayer, "Layer Transform", _targetLayer);
            DrawEventToggle(3, ref showEvents, "Open Events", _events);
        }

        EditorGUILayout.Space(10);
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawEventToggle(int index, ref bool toggle, string label, SerializedProperty property)
    {
        bool isArray = property.isArray; // Check if the property is an array
        bool hasEvents = isArray && property.arraySize > 0;

        if (toggle)
        {
            GUI.backgroundColor = hasEvents ? Color.yellow : Color.red;
        }
        else
        {
            GUI.backgroundColor = hasEvents ? Color.green : Color.red;

        }

        if (GUILayout.Button(toggle ? "Hide " + label : "Show " + label, GUILayout.Height(25)))
        {
            if (allowMultipleSelection)
            {
                toggle = !toggle;
            }
            else
            {
                if (currentOpenIndex == index)
                {
                    currentOpenIndex = -1;
                    toggle = false;
                }
                else
                {
                    currentOpenIndex = index;
                    ResetToggles();
                    toggle = true;
                }
            }
        }
        GUI.backgroundColor = Color.white;

        if (toggle)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }
    }

    private void ResetToggles()
    {
        showTargetTransform = false;
        showTargetTag = false;
        showTargetLayer = false;
        showEvents = false;
    }
}
#endif
