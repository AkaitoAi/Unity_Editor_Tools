using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    [Header("Layer Mask")]
    [Tooltip("Specify one or more layers that are allowed to interact. Defaults to all layers.")]
    [SerializeField] LayerMask _interactionLayers = ~0;

    #region Unity methods
    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject))
            return;
        if (other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleTriggerEnter(other, gameObject); };
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsInLayerMask(other.gameObject))
            return;

        if (other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleTriggerStay(other, gameObject); };
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayerMask(other.gameObject))
            return;

        if (other.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleTriggerExit(other, gameObject); };
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject))
            return;

        if (collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleCollisionEnter(collision, gameObject); };
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject))
            return;

        if (collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleCollisionStay(collision, gameObject); };
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsInLayerMask(collision.gameObject))
            return;

        if (collision.gameObject.TryGetComponent(out CollisionEventHandler collisionEventHandler)) { collisionEventHandler.HandleCollisionExit(collision, gameObject); };
    }
    #endregion

    #region Private methods

    private bool IsInLayerMask(GameObject obj)
    {
        return ((_interactionLayers.value & (1 << obj.layer)) != 0);
    }
    #endregion
}
