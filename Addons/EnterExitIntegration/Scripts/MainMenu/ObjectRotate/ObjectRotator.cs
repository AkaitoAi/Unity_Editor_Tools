using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectRotator : MonoBehaviour
{
    public bool useAutoRotate = false;

    private float x = 0;
    public float cameraRotateSpeed = 5f;
    public float autoCameraRotateSpeed = .01f;
    public Transform vehicleRoot;

    private int interval = 1;

    public void OnDrag(PointerEventData pointerData)
    {
        x = Mathf.Lerp(x, pointerData.delta.x * cameraRotateSpeed, Time.deltaTime * 5.0f);

        transform.RotateAround(vehicleRoot.position, Vector3.up, -x);
    }

    private void Update()
    {
        if (Time.frameCount % interval == 0)
        {
            if (!useAutoRotate) return;

            x = Mathf.Lerp(x, cameraRotateSpeed * autoCameraRotateSpeed, Time.deltaTime * 5.0f);

            transform.RotateAround(vehicleRoot.position, Vector3.up, -x);
        }
    }
}
