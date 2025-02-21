using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class InvectorJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, ICanvasRaycastFilter
{
    public enum AxisOption
    {
        Both,
        OnlyHorizontal,
        OnlyVertical
    }

    public int MovementRange = 100;
    public AxisOption axesToUse = AxisOption.Both;
    public string horizontalAxisName = "Horizontal";
    public string verticalAxisName = "Vertical";

    public RectTransform runButton; // Assign the run button's RectTransform in the Inspector.
    public bool isRunning; // Set externally based on your character's state.

    // Internal flag that tracks if the joystick is locked to the run button.
    private bool lockedToRun = false;

    Vector3 m_StartPos;
    bool m_UseX;
    bool m_UseY;
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;

    IEnumerator Start()
    {
        m_StartPos = transform.localPosition;
        yield return new WaitForEndOfFrame();
        CreateVirtualAxes();
    }

    void UpdateVirtualAxes(Vector3 value)
    {
        // Calculate delta relative to the starting position.
        Vector3 delta = m_StartPos - value;
        delta.y = -delta.y;
        delta /= MovementRange;
        if (m_UseX)
            m_HorizontalVirtualAxis.Update(-delta.x);
        if (m_UseY)
            m_VerticalVirtualAxis.Update(delta.y);
    }

    void CreateVirtualAxes()
    {
        m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
        m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

        if (m_UseX)
        {
            if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
            {
                m_HorizontalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName);
            }
            else
            {
                m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            }
        }
        if (m_UseY)
        {
            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
            {
                m_VerticalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(verticalAxisName);
            }
            else
            {
                m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
            }
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        // When the user touches the joystick, clear the lock so normal control resumes.
        lockedToRun = false;
    }

    public void OnDrag(PointerEventData data)
    {
        // When not running and already locked, snap the joystick to maximum range in the locked direction.
        if (!isRunning && lockedToRun)
        {
            Vector3 runButtonLocalPos = transform.parent.InverseTransformPoint(runButton.position);
            Vector3 direction = runButtonLocalPos - m_StartPos;
            if (direction.sqrMagnitude > 0.001f)
            {
                Vector3 newJoystickPos = m_StartPos + direction.normalized * MovementRange;
                transform.localPosition = newJoystickPos;
                UpdateVirtualAxes(newJoystickPos);
            }
            return;
        }

        // When running is true, check if the pointer should lock to the run button.
        if (runButton != null && isRunning)
        {
            Vector2 runButtonScreenPos = RectTransformUtility.WorldToScreenPoint(data.pressEventCamera, runButton.position);
            if (data.position.y > runButtonScreenPos.y || lockedToRun)
            {
                lockedToRun = true; // Lock the joystick.
                Vector3 lockedLocalPos = transform.parent.InverseTransformPoint(runButton.position);
                transform.localPosition = lockedLocalPos;
                UpdateVirtualAxes(lockedLocalPos);
                return;
            }
        }

        // Process drag normally if not locked.
        Vector3 newPos = Vector3.zero;
        if (m_UseX)
        {
            int delta = (int)(data.position.x - m_StartPos.x);
            newPos.x = delta;
        }
        if (m_UseY)
        {
            int delta = (int)(data.position.y - m_StartPos.y);
            newPos.y = delta;
        }

        transform.localPosition = Vector3.ClampMagnitude(
            transform.parent.InverseTransformPoint(new Vector3(newPos.x, newPos.y, 0)),
            MovementRange) + m_StartPos;
        UpdateVirtualAxes(transform.localPosition);
    }

    public void OnPointerUp(PointerEventData data)
    {
        // If not locked, reset to the center; if locked, update axes without resetting.
        if (!lockedToRun)
        {
            transform.localPosition = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
        }
        else
        {
            UpdateVirtualAxes(transform.localPosition);
        }
    }

    // Sets the running state.
    // When isRunning becomes false while locked, snap the joystick to maximum range toward the run button.
    public void SetRunning(bool running)
    {
        isRunning = running;
        if (!isRunning && lockedToRun)
        {
            Vector3 runButtonLocalPos = transform.parent.InverseTransformPoint(runButton.position);
            Vector3 direction = runButtonLocalPos - m_StartPos;
            if (direction.sqrMagnitude > 0.001f)
            {
                Vector3 newJoystickPos = m_StartPos + direction.normalized * MovementRange;
                transform.localPosition = newJoystickPos;
                UpdateVirtualAxes(newJoystickPos);
            }
        }
    }

    // ICanvasRaycastFilter implementation.
    // This allows pointer events over the run button (which is below the joystick) to pass through.
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (runButton != null)
        {
            // If the pointer is over the run button's area, return false so the event passes through.
            if (RectTransformUtility.RectangleContainsScreenPoint(runButton, sp, eventCamera))
                return false;
        }
        return true;
    }
}
