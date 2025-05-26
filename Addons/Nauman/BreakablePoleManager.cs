using UnityEngine;
using UnityEngine.Events;

public class BreakablePoleManager : MonoBehaviour
{
    #region Blueprints
    enum PoleHitType { Trigger, Collision }
    #endregion

    #region Inspector
    [SerializeField] private PoleHitType _currentPoleHitType = PoleHitType.Trigger;
    [Header("Break Settings")]
    [SerializeField] private float _breakForceThreshold = 3f;
    [SerializeField] private float _mass = 5f;
    [SerializeField] private float _drag = 0.01f;
    [SerializeField] private float _explosionForce = 10f;
    [SerializeField] private float _explosionRadius = 2f;

    [Header("Break Settings")]
    [SerializeField] private bool _resetOnTime = false;
    [SerializeField] private float _brokenLifetime = 3f;

    [Header("Player Settings")]
    [SerializeField] private bool _applyOpposingForceToPlayer = false;
    [SerializeField] private float _opposingForce = 0f;

    [Header("Reset Transform")]
    [SerializeField] private bool _canResetToInitialPosition = true;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    [SerializeField] private Collider _collider;
    private Rigidbody _rigidBody;
    private bool _isBroken = false;

    [Header("Events")]
    public UnityEvent _onBreak = new UnityEvent();
    public UnityEvent _onReset = new UnityEvent();
    #endregion

    #region Unity
    private void Start() { Init(); }
    #endregion

    #region Public
    public void OnCollision(Collision collision = null, Rigidbody playerRb = null)
    {
        if (_isBroken) return;

        if (_rigidBody == null)
        {
            _rigidBody = gameObject.AddComponent<Rigidbody>();
        }

        Vector3 forceDirection = Vector3.back * 500f;
        Vector3 impactPoint = Vector3.zero;

        if (collision != null)
        {
            impactPoint = collision.contacts[0].point;
        }

        if (_applyOpposingForceToPlayer && playerRb != null)
        {
            forceDirection = -playerRb.velocity.normalized * _opposingForce;
            playerRb.AddForce(forceDirection, ForceMode.Impulse);
        }

        if (_currentPoleHitType == PoleHitType.Trigger)
        {
            if (_collider != null) { _collider.isTrigger = false; }
            BreakPole(impactPoint, playerRb);
            return;
        }

        if (_currentPoleHitType == PoleHitType.Collision)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > _breakForceThreshold)
            {
                BreakPole(impactPoint, playerRb);
                return;
            }
        }
    }

    [ContextMenu("Reset")]
    public void ResetPole()
    {
        if (_rigidBody != null) { Destroy(_rigidBody); }
        if (_collider != null)
        {
            if (_currentPoleHitType == PoleHitType.Trigger) { _collider.isTrigger = true; }
            if (_currentPoleHitType == PoleHitType.Collision) { _collider.isTrigger = false; }
        }

        transform.localPosition = _initialPosition;
        transform.localRotation = _initialRotation;
        _onReset?.Invoke();
        _isBroken = false;
    }
    #endregion

    #region Private
    private void Init()
    {
        if (_rigidBody == null) { _rigidBody = GetComponent<Rigidbody>(); }
        if (_collider == null) { _collider = GetComponent<Collider>(); }

        if (_collider != null)
        {
            if (_currentPoleHitType == PoleHitType.Trigger) { _collider.isTrigger = true; }
            if (_currentPoleHitType == PoleHitType.Collision) { _collider.isTrigger = false; }
        }

        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
    }
    private void BreakPole(Vector3 impactPoint, Rigidbody playerRb)
    {
        if (_rigidBody != null)
        {
            _rigidBody.mass = _mass;
            _rigidBody.drag = _drag;
            _rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;

            Vector3 forceOrigin = impactPoint == Vector3.zero ? transform.position : impactPoint;
            if (playerRb != null)
            {
                Vector3 forceDirection = playerRb.velocity.normalized;
                _rigidBody.AddForce(forceDirection * _explosionForce, ForceMode.Impulse);
            }
            else
            {
                _rigidBody.AddExplosionForce(_explosionForce, forceOrigin, _explosionRadius, 1f, ForceMode.Impulse);
            }
        }
        _onBreak?.Invoke();
        _isBroken = true;
        if (_resetOnTime) { Invoke(nameof(DestroyOrReset), _brokenLifetime); }
    }
    private void DestroyOrReset()
    {
        if (_canResetToInitialPosition) { ResetPole(); }
        else
        { Destroy(gameObject); }
    }

    #endregion
}