using UnityEngine;
using UnityEngine.Events;

public class VisibilityCheck : MonoBehaviour
{
    #region Blueprints
    #endregion

    #region Inspector
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private bool _isVisible;

    [Header("Events")]
    public UnityEvent _visibleEvent = new UnityEvent();
    public UnityEvent _invisibleEvent = new UnityEvent();
    #endregion

    #region Unity
    private void Start() { Init(); }
    private void Update()
    {
        if (IsInViewFrustum())
        {
            _isVisible = true;
            _visibleEvent?.Invoke();
        }
        else
        {
            _isVisible = false;
            _invisibleEvent?.Invoke();
        }
    }
    #endregion

    #region Public
    public void GetCurrenActiveCamera()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
    }
    #endregion

    #region Private
    private void Init()
    {
    }
    bool IsInViewFrustum()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_targetCamera);
        Bounds bounds = GetComponent<Renderer>().bounds;
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    #endregion
}
