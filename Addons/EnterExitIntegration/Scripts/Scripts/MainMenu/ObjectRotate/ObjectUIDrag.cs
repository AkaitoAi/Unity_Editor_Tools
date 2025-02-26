using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectUIDrag : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private ObjectRotator objectRotator;
    //private bool isPressing = false;
    public void OnDrag(PointerEventData eventData)
    {
        objectRotator.OnDrag(eventData);

        //MenuManager.Instance.vehicleSelection.SetActive(false);

        //MenuManager.Instance.GarageAnimator("GarageCameraZoomInAnimation");

        //isPressing = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //MenuManager.Instance.vehicleSelection.SetActive(true);

        //MenuManager.Instance.GarageAnimator("GarageCameraZoomOutAnimation");

        //isPressing = false;
    }
}
