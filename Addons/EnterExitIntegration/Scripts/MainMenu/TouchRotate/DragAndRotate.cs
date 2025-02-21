using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! Attach it to the object
public class DragAndRotate : MonoBehaviour
{
    public bool isActive = false;

    void Update()
    {
        if (isActive)
        {
            if (Input.touchCount == 1)
            {
                Touch screenTouch = Input.GetTouch(0);

                if (screenTouch.phase == TouchPhase.Moved)
                {
                    transform.Rotate(0f, screenTouch.deltaPosition.x, 0f);
                }

                if (screenTouch.phase == TouchPhase.Ended)
                {
                    isActive = false;
                }
            }
        }
    }
}
