using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CollisionEventHandler : MonoBehaviour
{
    #region Blueprint
    [System.Serializable]
    public struct TargetTransform
    {
        [Tooltip("Target GameObject that will trigger these events.")]
        public GameObject Target;
        public Event Events;
    }
    [System.Serializable]
    public struct TargetTag
    {
        [Tooltip("Tag that will trigger these events.")]
        public string Target;
        public Event Events;
    }
    [System.Serializable]
    public struct TargetLayer
    {
        [Tooltip("Layer that will trigger these events.")]
        public LayerMask Target;
        public Event Events;
    }
    [System.Serializable]
    public struct Event
    {
        public UnityEvent OnEnterUnityEvent;
        public UnityEvent OnStayUnityEvent;
        public UnityEvent OnExitUnityEvent;
    }

    public enum DetectionType { Trigger, Collision }
    public enum UseType { Once, Multiple }
    #endregion

    #region Inspector
    [Header("Detection Settings")]
    [Tooltip("Choose whether to use trigger or collision events.")]
    [SerializeField] DetectionType _detectionType = DetectionType.Trigger;

    [Header("General Settings")]
    [Tooltip("Use 'Once' to fire events a single time; 'Multiple' allows repeated invocations.")]
    [SerializeField] UseType _currentUseType = UseType.Once;
    [Tooltip("Delay (in seconds) before destroying the object if using 'Once' mode.")]
    [SerializeField] float _delay = 5f;

    [Header("Speed Settings (Optional)")]
    [Tooltip("Enable to require a minimum speed before triggering events.")]
    [SerializeField] bool _useSpeedThreshold = false;
    [SerializeField] float _speedThreshold = 5f;
    [Tooltip("If using speed, you can scale the effect if desired.")]
    [SerializeField] float _speedMultiplier = 1f;

    [Header("Targeted")]
    [SerializeField] TargetTransform[] _targetTransform;
    [SerializeField] TargetLayer[] _targetLayer;
    [SerializeField] TargetTag[] _targetTag;
    [SerializeField] Event _events;

    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private HashSet<GameObject> stayTriggeredObjects = new HashSet<GameObject>();
    #endregion

    #region Public
    public void HandleTriggerEnter(Collider other, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Trigger)
            return;

        GameObject obj = other.gameObject;
        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);

            TargetTransform? foundTransform = GetTargetEvent(sender);
            TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
            TargetLayer? foundLayer = GetLayerEvent(sender);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            foundLayer?.Events.OnEnterUnityEvent?.Invoke();
            _events.OnEnterUnityEvent?.Invoke();
        }
    }
    public void HandleTriggerStay(Collider other, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Trigger)
            return;

        GameObject obj = other.gameObject;

        if (activeObjects.Contains(obj) && !stayTriggeredObjects.Contains(obj))
        {
            stayTriggeredObjects.Add(obj);

            TargetTransform? foundTransform = GetTargetEvent(sender);
            TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
            TargetLayer? foundLayer = GetLayerEvent(sender);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            foundLayer?.Events.OnEnterUnityEvent?.Invoke();
            _events.OnStayUnityEvent?.Invoke();
        }
    }
    public void HandleTriggerExit(Collider other, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Trigger)
            return;

        GameObject obj = other.gameObject;

        TargetTransform? foundTransform = GetTargetEvent(sender);
        TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
        TargetLayer? foundLayer = GetLayerEvent(sender);

        foundTransform?.Events.OnEnterUnityEvent?.Invoke();
        foundTag?.Events.OnEnterUnityEvent?.Invoke();
        foundLayer?.Events.OnEnterUnityEvent?.Invoke();
        _events.OnExitUnityEvent?.Invoke();

        activeObjects.Remove(obj);
        stayTriggeredObjects.Remove(obj);

        CheckAndInvokeDestroy();
    }
    public void HandleCollisionEnter(Collision collision, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Collision)
            return;

        if (_useSpeedThreshold)
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed < _speedThreshold)
                return;
        }

        GameObject obj = collision.gameObject;

        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);

            TargetTransform? foundTransform = GetTargetEvent(sender);
            TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
            TargetLayer? foundLayer = GetLayerEvent(sender);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            foundLayer?.Events.OnEnterUnityEvent?.Invoke();
            _events.OnEnterUnityEvent?.Invoke();
        }
    }
    public void HandleCollisionStay(Collision collision, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Collision)
            return;

        if (_useSpeedThreshold)
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed < _speedThreshold)
                return;
        }

        GameObject obj = collision.gameObject;

        if (activeObjects.Contains(obj) && !stayTriggeredObjects.Contains(obj))
        {
            stayTriggeredObjects.Add(obj);

            TargetTransform? foundTransform = GetTargetEvent(sender);
            TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
            TargetLayer? foundLayer = GetLayerEvent(sender);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            foundLayer?.Events.OnEnterUnityEvent?.Invoke();
            _events.OnStayUnityEvent?.Invoke();
        }
    }
    public void HandleCollisionExit(Collision collision, GameObject sender = null)
    {
        if (_detectionType != DetectionType.Collision)
            return;

        GameObject obj = collision.gameObject;

        TargetTransform? foundTransform = GetTargetEvent(sender);
        TargetTag? foundTag = GetTagEvent(sender ? sender.tag : obj.tag);
        TargetLayer? foundLayer = GetLayerEvent(sender);

        foundTransform?.Events.OnEnterUnityEvent?.Invoke();
        foundTag?.Events.OnEnterUnityEvent?.Invoke();
        foundLayer?.Events.OnEnterUnityEvent?.Invoke();
        _events.OnExitUnityEvent?.Invoke();

        activeObjects.Remove(obj);
        stayTriggeredObjects.Remove(obj);

        CheckAndInvokeDestroy();
    }

    #endregion

    #region Private

    private TargetTransform? GetTargetEvent(GameObject obj)
    {
        if (_targetTransform == null || _targetTransform.Length == 0)
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
    private TargetLayer? GetLayerEvent(GameObject obj)
    {
        if (_targetLayer == null || _targetLayer.Length == 0)
            return null;

        int layerHit = obj.layer;

        for (int i = _targetLayer.Length - 1; i >= 0; i--)
        {
            if (_targetLayer[i].Target == obj.gameObject.layer)
            {
                return _targetLayer[i];
            }
        }
        return null;
    }
    private TargetTag? GetTagEvent(string tag)
    {
        if (_targetTag == null || _targetTag.Length == 0)
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
    private void CheckAndInvokeDestroy()
    {
        if (_currentUseType == UseType.Once)
        {
            if (_delay > 0f)
                Destroy(gameObject, _delay);
            else
                Destroy(gameObject);
        }
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(CollisionEventHandler))]
public class CollisionEventHandlerEditor : Editor
{
    private SerializedProperty _detectionType;
    private SerializedProperty _currentUseType;
    private SerializedProperty _delay;
    private SerializedProperty _useSpeedThreshold;
    private SerializedProperty _speedThreshold;
    private SerializedProperty _speedMultiplier;

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
        _detectionType = serializedObject.FindProperty("_detectionType");
        _currentUseType = serializedObject.FindProperty("_currentUseType");
        _delay = serializedObject.FindProperty("_delay");
        _useSpeedThreshold = serializedObject.FindProperty("_useSpeedThreshold");
        _speedThreshold = serializedObject.FindProperty("_speedThreshold");
        _speedMultiplier = serializedObject.FindProperty("_speedMultiplier");

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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_detectionType"), new GUIContent("Detection Type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentUseType"), new GUIContent("Use Type"));
        SerializedProperty useDelay = serializedObject.FindProperty("_currentUseType");
        if (!useDelay.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_delay"), new GUIContent("Delay (Seconds)"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_useSpeedThreshold"), new GUIContent("Use Speed Threshold"));

        SerializedProperty useSpeedThresholdProp = serializedObject.FindProperty("_useSpeedThreshold");

        if (useSpeedThresholdProp.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_speedThreshold"), new GUIContent("Speed Threshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_speedMultiplier"), new GUIContent("Speed Multiplier"));
        }

        EditorGUILayout.Space(10);
        DrawEventToggle(0, ref showTargetTransform, "Target Transform", _targetTransform);
        DrawEventToggle(1, ref showTargetTag, "Tag Transform", _targetTag);
        DrawEventToggle(2, ref showTargetLayer, "Layer Transform", _targetLayer);
        DrawEventToggle(3, ref showEvents, "Open Events", _events);

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