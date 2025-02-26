using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private OnDragCameraRotater cameraRotator;
    //[SerializeField] private GameObject header;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private string uiFadeOut, uiFadeIn;
    
    private Transform cameraTransform;
    private Animator _animator;
    private Vector3 defaultCamPosition;
    private Vector3 defaultCamRotation;

    private bool isPressing = false;
    //private MenuManager menuManager;

    public bool IsPressing => isPressing;

    private void Start()
    {
        cameraTransform = cameraRotator.transform;
        defaultCamPosition = cameraTransform.position;
        defaultCamRotation = cameraTransform.localEulerAngles;
        //menuManager = MenuManager.GetInstance();
        //header.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        cameraRotator.OnDrag(eventData);

        //menuManager.vehicleSelectScreen.SetActive(false);
        //header.SetActive(true);

        if (uiAnimator) uiAnimator.Play(uiFadeOut);

        if (cameraRotator.TryGetComponent<Animator>(out _animator))
            _animator.enabled = false;

        isPressing = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isPressing = false;

        OnBackButton();
    }

    public void OnBackButton()
    {
        //TODO Sound Calling
        //SoundManager.Instance.PlayOnButtonSound();

        //menuManager.vehicleSelectScreen.SetActive(true);
        //header.SetActive(false);

        if (uiAnimator) uiAnimator.Play(uiFadeIn);

        cameraRotator.RotationX = cameraRotator.Reset_Rotation.y;
        cameraRotator.RotationY = cameraRotator.Reset_Rotation.x;

        cameraTransform.position = defaultCamPosition;
        cameraTransform.localEulerAngles = defaultCamRotation;

        if (cameraRotator.TryGetComponent<Animator>(out _animator))
            _animator.enabled = true;
    }
}