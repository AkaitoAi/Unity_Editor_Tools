using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool isCivilian;

    private Vector3 startPosition, startRotation;

    public static event Action OnDoorOpenAction;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation.eulerAngles;
    }

    public void DoorOpen()
    { 
        if(animator == null) return;

        animator.CrossFadeInFixedTime("OpenDoor", .2f);

        OnDoorOpenAction?.Invoke();
    }

    public void ActivateDriver()
    { 
        if(animator == null) return;

        animator.SetBool("SitDirect", false);
    }

    private void OnEnable()
    {
        NavigateToTarget.OnDestinationReachedAction += DoorOpen;
    }

    private void OnDisable()
    {
        NavigateToTarget.OnDestinationReachedAction -= DoorOpen;
    }

}
