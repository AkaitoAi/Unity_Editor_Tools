using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! Attach this script to camera
public class DetectTouchInput : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "cube")
                {

                    var objectScript = hit.collider.GetComponent<DragAndRotate>();
                    objectScript.isActive = !objectScript.isActive;
                }
            }
        }
    }
}
