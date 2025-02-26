using UnityEngine;
using UnityEngine.EventSystems;

public class OnDragCameraRotater : MonoBehaviour
{
    public bool cameraRotating = false;

    [SerializeField]
    private float _dragSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    [SerializeField] private Vector2 reset_Rotation = new Vector2();

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _distanceFromTarget = 3.0f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    [SerializeField]
    private float _smoothTime = 0.2f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    [SerializeField]
    private float rotationSpeed = 5f;

    public bool useAutoCameraRotation = false;

    private bool isPositive = true;
    private float recentDrag;

    public float RotationY { get => _rotationY; set => _rotationY = value; }
    public float RotationX { get => _rotationX; set => _rotationX = value; }
    public Vector2 Reset_Rotation { get => reset_Rotation;}

    public void OnDrag(PointerEventData pointerData)
    {
        float orbitX = pointerData.delta.x * _dragSensitivity;
        float orbitY = pointerData.delta.y * _dragSensitivity;

        _rotationY += orbitX;
        _rotationX += orbitY;

        recentDrag = _rotationY;

        if (_rotationY > recentDrag)
            isPositive = true;
        else if (_rotationY < recentDrag)
            isPositive = false;

        CameraRotation();

        cameraRotating = true;
    }

   
    private void Update()
    {
        if (!cameraRotating) return;

         float orbitX = Time.deltaTime * rotationSpeed;

        if (isPositive)
            _rotationY += orbitX;
        else if (!isPositive)
            _rotationY -= orbitX;

        if (useAutoCameraRotation) CameraRotation();
    }

    private void CameraRotation()
    {
        // Apply clamping for x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(-_rotationX, _rotationY);

        // Apply damping between rotation changes
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
        transform.localEulerAngles = _currentRotation;

        // Substract forward vector of the GameObject to point its forward vector to the target
        transform.position = _target.position - transform.forward * _distanceFromTarget;
    }

}
