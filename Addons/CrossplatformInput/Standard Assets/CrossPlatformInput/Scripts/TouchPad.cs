using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
    [RequireComponent(typeof(Image))]
    public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum AxisOption { Both, OnlyHorizontal, OnlyVertical }
        public enum ControlStyle { Absolute, Relative, Swipe }

        public AxisOption axesToUse = AxisOption.Both;
        public ControlStyle controlStyle = ControlStyle.Absolute;
        public string horizontalAxisName = "Horizontal";
        public string verticalAxisName = "Vertical";
        public float Xsensitivity = 1f;
        public float Ysensitivity = 1f;
        // Multiplier to scale touch delta for mobile devices
        public float mobileSensitivityMultiplier = 0.1f;
        // Maximum allowed mobile delta to clamp fast swipes
        public float maxMobileDelta = 50f;

        bool m_UseX;
        bool m_UseY;
        CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
        CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;
        bool m_Dragging;
        int m_Id = -1;

#if UNITY_EDITOR
        Vector3 m_PreviousMouse;
#endif

        void OnEnable()
        {
            CreateVirtualAxes();
        }

        void CreateVirtualAxes()
        {
            m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
            m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

            if (m_UseX)
            {
                m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            }
            if (m_UseY)
            {
                m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
            }
        }

        void UpdateVirtualAxes(Vector3 value)
        {
            if (m_UseX)
                m_HorizontalVirtualAxis.Update(value.x);
            if (m_UseY)
                m_VerticalVirtualAxis.Update(value.y);
        }

        public void OnPointerDown(PointerEventData data)
        {
            m_Dragging = true;
            m_Id = data.pointerId;
#if UNITY_EDITOR
            m_PreviousMouse = Input.mousePosition;
#endif
        }

        void Update()
        {
            if (!m_Dragging)
                return;

            Vector2 pointerDelta;

#if !UNITY_EDITOR
            if (Input.touchCount > m_Id && m_Id >= 0)
            {
                // Get the built-in deltaPosition which gives the frame-to-frame change.
                pointerDelta = Input.touches[m_Id].deltaPosition;
                // Scale by the mobile multiplier and sensitivity settings.
                pointerDelta.x *= mobileSensitivityMultiplier * Xsensitivity;
                pointerDelta.y *= mobileSensitivityMultiplier * Ysensitivity;
                // Clamp the delta to avoid excessive speed on fast swipes.
                pointerDelta = Vector2.ClampMagnitude(pointerDelta, maxMobileDelta);
            }
            else
            {
                return;
            }
#else
            pointerDelta.x = Input.mousePosition.x - m_PreviousMouse.x;
            pointerDelta.y = Input.mousePosition.y - m_PreviousMouse.y;
            m_PreviousMouse = Input.mousePosition;
#endif
            UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));
        }

        public void OnPointerUp(PointerEventData data)
        {
            m_Dragging = false;
            m_Id = -1;
            UpdateVirtualAxes(Vector3.zero);
        }

        void OnDisable()
        {
            if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
        }
    }
}
