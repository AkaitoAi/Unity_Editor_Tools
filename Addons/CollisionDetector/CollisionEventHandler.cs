using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEventHandler : MonoBehaviour
{
    [System.Serializable]
    public class TransformTarget
    {
        [Tooltip("Target GameObject that will trigger these events.")]
        public GameObject Target;
        public Event Events;
    }
    [System.Serializable]
    public class TagTarget
    {
        [Tooltip("Tag that will trigger these events.")]
        public string Target;
        public Event Events;
    }
    [System.Serializable]
    public class Event
    {
        public UnityEvent OnEnterUnityEvent;
        public UnityEvent OnStayUnityEvent;
        public UnityEvent OnExitUnityEvent;
    }
    public enum DetectionType { Trigger, Collision }
    public enum UseType { Once, Multiple }

    [Header("Detection Settings")]
    [Tooltip("Choose whether to use trigger or collision events.")]
    public DetectionType detectionType = DetectionType.Trigger;

    [Header("General Settings")]
    [Tooltip("Use 'Once' to fire events a single time; 'Multiple' allows repeated invocations.")]
    public UseType currentUseType = UseType.Once;
    [Tooltip("Delay (in seconds) before destroying the object if using 'Once' mode.")]
    public float delay = 5f;

    [Header("Speed Settings (Optional)")]
    [Tooltip("Enable to require a minimum speed before triggering events.")]
    public bool useSpeedThreshold = false;
    public float speedThreshold = 5f;
    [Tooltip("If using speed, you can scale the effect if desired.")]
    public float speedMultiplier = 1f;

    [Header("Targeted")]
    public List<TransformTarget> targetTransform = new List<TransformTarget>();
    public List<TagTarget> tagTransform = new List<TagTarget>();
    public Event Events;

    // These hash sets keep track of which objects have already triggered enter or stay events.
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private HashSet<GameObject> stayTriggeredObjects = new HashSet<GameObject>();

    #region Public methods

    public void HandleTriggerEnter(Collider other, GameObject sender = null)
    {
        if (detectionType != DetectionType.Trigger)
            return;

        if (useSpeedThreshold)
        {
            Rigidbody rb = other.attachedRigidbody;
            float speed = rb != null ? rb.velocity.magnitude : 0f;
            if (speed < speedThreshold)
                return;
        }

        GameObject obj = other.gameObject;

        // Fire enter event only if this object hasn't been processed yet.
        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);
            Debug.Log("Trigger Enter from: " + obj.name);

            TransformTarget foundTransform = GetTargetEvent(sender);
            TagTarget foundTag = GetTagEvent(sender.tag);

            if (foundTag != null)
                Debug.Log("Matched tag target: " + foundTag.Target);
            else
                Debug.Log("No matching tag target found for tag: " + obj.tag);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            Events.OnEnterUnityEvent?.Invoke();
        }
    }

    public void HandleTriggerStay(Collider other, GameObject sender = null)
    {
        if (detectionType != DetectionType.Trigger)
            return;

        if (useSpeedThreshold)
        {
            Rigidbody rb = other.attachedRigidbody;
            float speed = rb != null ? rb.velocity.magnitude : 0f;
            if (speed < speedThreshold)
                return;
        }

        GameObject obj = other.gameObject;

        // Fire stay event only once per object per collision cycle.
        if (activeObjects.Contains(obj) && !stayTriggeredObjects.Contains(obj))
        {
            stayTriggeredObjects.Add(obj);
            Debug.Log("Trigger Stay from: " + obj.name);

            TransformTarget foundTransform = GetTargetEvent(sender);
            TagTarget foundTag = GetTagEvent(sender.tag);

            foundTransform?.Events.OnStayUnityEvent?.Invoke();
            foundTag?.Events.OnStayUnityEvent?.Invoke();
            Events.OnStayUnityEvent?.Invoke();
        }
    }

    public void HandleTriggerExit(Collider other, GameObject sender = null)
    {
        if (detectionType != DetectionType.Trigger)
            return;

        GameObject obj = other.gameObject;
        Debug.Log("Trigger Exit from: " + obj.name);

        TransformTarget foundTransform = GetTargetEvent(sender);
        TagTarget foundTag = GetTagEvent(sender.tag);

        foundTransform?.Events.OnExitUnityEvent?.Invoke();
        foundTag?.Events.OnExitUnityEvent?.Invoke();
        Events.OnExitUnityEvent?.Invoke();

        // Reset the state so that events can trigger again on re-entry.
        activeObjects.Remove(obj);
        stayTriggeredObjects.Remove(obj);

        CheckAndInvokeDestroy();
    }

    public void HandleCollisionEnter(Collision collision, GameObject sender = null)
    {
        if (detectionType != DetectionType.Collision)
            return;

        if (useSpeedThreshold)
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed < speedThreshold)
                return;
        }

        GameObject obj = collision.gameObject;

        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);
            Debug.Log("Collision Enter from: " + obj.name);

            TransformTarget foundTransform = GetTargetEvent(sender);
            TagTarget foundTag = GetTagEvent(sender.tag);

            foundTransform?.Events.OnEnterUnityEvent?.Invoke();
            foundTag?.Events.OnEnterUnityEvent?.Invoke();
            Events.OnEnterUnityEvent?.Invoke();
        }
    }

    public void HandleCollisionStay(Collision collision, GameObject sender = null)
    {
        if (detectionType != DetectionType.Collision)
            return;

        if (useSpeedThreshold)
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            if (collisionSpeed < speedThreshold)
                return;
        }

        GameObject obj = collision.gameObject;

        if (activeObjects.Contains(obj) && !stayTriggeredObjects.Contains(obj))
        {
            stayTriggeredObjects.Add(obj);
            Debug.Log("Collision Stay from: " + obj.name);

            TransformTarget foundTransform = GetTargetEvent(sender);
            TagTarget foundTag = GetTagEvent(sender.tag);

            foundTransform?.Events.OnStayUnityEvent?.Invoke();
            foundTag?.Events.OnStayUnityEvent?.Invoke();
            Events.OnStayUnityEvent?.Invoke();
        }
    }

    public void HandleCollisionExit(Collision collision, GameObject sender = null)
    {
        if (detectionType != DetectionType.Collision)
            return;

        GameObject obj = collision.gameObject;
        Debug.Log("Collision Exit from: " + obj.name);

        TransformTarget foundTransform = GetTargetEvent(sender);
        TagTarget foundTag = GetTagEvent(sender.tag);

        foundTransform?.Events.OnExitUnityEvent?.Invoke();
        foundTag?.Events.OnExitUnityEvent?.Invoke();
        Events.OnExitUnityEvent?.Invoke();

        // Reset the state so that the events can trigger again if the object re-collides.
        activeObjects.Remove(obj);
        stayTriggeredObjects.Remove(obj);

        CheckAndInvokeDestroy();
    }

    

    #endregion

    #region Private methods

    private TransformTarget GetTargetEvent(GameObject obj)
    {
        foreach (TransformTarget target in targetTransform)
        {
            if (target.Target == obj)
            {
                Debug.Log("Matched transform target: " + target.Target.name);
                return target;
            }
        }
        return null;
    }

    private TagTarget GetTagEvent(string tag)
    {
        foreach (TagTarget target in tagTransform)
        {
            if (target.Target == tag)
            {
                Debug.Log("Matched tag target: " + target.Target);
                return target;
            }
        }
        return null;
    }
    private void CheckAndInvokeDestroy()
    {
        if (currentUseType == UseType.Once)
        {
            if (delay > 0f)
                Destroy(gameObject, delay);
            else
                Destroy(gameObject);
        }
    }
    #endregion
}
