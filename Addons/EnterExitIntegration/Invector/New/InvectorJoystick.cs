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

    private bool lockedToRun = false;
    private bool isDragging = false; // Track if the joystick is being dragged

    Vector3 m_StartPos;
    bool m_UseX;
    bool m_UseY;
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;

    private GameObject unlockOverlay; // Transparent overlay for tap detection
    private UnityEngine.UI.Button unlockButton;

    void Awake()
    {
        // Ensure joystick has an Image for raycasting
        if (!GetComponent<UnityEngine.UI.Image>())
        {
            gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0); // Transparent
        }
        GetComponent<UnityEngine.UI.Image>().raycastTarget = true;

        // Create unlock overlay
        unlockOverlay = new GameObject("UnlockOverlay");
        unlockOverlay.transform.SetParent(transform.parent, false);
        var rect = unlockOverlay.AddComponent<RectTransform>();
        rect.sizeDelta = runButton != null ? runButton.sizeDelta : new Vector2(100, 100); // Match runButton size
        rect.anchoredPosition = Vector2.zero; // Will position it later
        var img = unlockOverlay.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(1, 1, 1, 0); // Transparent
        unlockButton = unlockOverlay.AddComponent<UnityEngine.UI.Button>();
        unlockButton.transition = UnityEngine.UI.Selectable.Transition.None;
        unlockOverlay.SetActive(false); // Hidden by default
    }

    IEnumerator Start()
    {
        m_StartPos = transform.localPosition;
        yield return new WaitForEndOfFrame();
        CreateVirtualAxes();

        // Set up unlock button listener
        unlockButton.onClick.RemoveAllListeners();
        unlockButton.onClick.AddListener(() =>
        {
            if (lockedToRun)
            {
                Debug.Log("Unlock overlay tapped. Unlocking and stopping run.");
                UnlockJoystick();
            }
        });
    }

    void UpdateVirtualAxes(Vector3 value)
    {
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
                m_HorizontalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName);
            else
            {
                m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            }
        }
        if (m_UseY)
        {
            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
                m_VerticalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(verticalAxisName);
            else
            {
                m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
            }
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("Joystick OnPointerDown. LockedToRun: " + lockedToRun + ", Position: " + data.position);
        isDragging = true;
        lockedToRun = false;
        unlockOverlay.SetActive(false); // Hide overlay if starting drag
        Debug.Log("Joystick drag started.");
    }

    public void OnDrag(PointerEventData data)
    {
        if (!lockedToRun && isRunning && runButton != null)
        {
            Vector2 runButtonScreenPos = RectTransformUtility.WorldToScreenPoint(data.pressEventCamera, runButton.position);
            if (data.position.y > runButtonScreenPos.y)
            {
                lockedToRun = true;
                Vector3 lockedLocalPos = transform.parent.InverseTransformPoint(runButton.position);
                transform.localPosition = lockedLocalPos;
                UpdateVirtualAxes(lockedLocalPos);

                // Position and activate unlock overlay
                unlockOverlay.GetComponent<RectTransform>().position = runButton.position;
                unlockOverlay.SetActive(true);
                Debug.Log("Joystick locked to run position. Overlay activated.");
                return;
            }
        }

        if (!lockedToRun)
        {
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
    }

    public void OnPointerUp(PointerEventData data)
    {
        isDragging = false;
        if (!lockedToRun)
        {
            transform.localPosition = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
            Debug.Log("Joystick released and reset to start position.");
        }
        else
        {
            UpdateVirtualAxes(transform.localPosition);
            Debug.Log("Joystick released but still locked.");
        }
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
        if (!isRunning)
        {
            if (!isDragging)
            {
                transform.localPosition = m_StartPos;
                UpdateVirtualAxes(m_StartPos);
            }
            lockedToRun = false;
            unlockOverlay.SetActive(false); // Hide overlay when running stops
            Debug.Log("Running stopped externally.");
        }
    }

    private void UnlockJoystick()
    {
        lockedToRun = false;
        isRunning = false;
        transform.localPosition = m_StartPos;
        UpdateVirtualAxes(m_StartPos);
        unlockOverlay.SetActive(false); // Hide overlay on unlock
    }

    public void OnLockToRun(bool value) => lockedToRun = value;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (runButton != null)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(runButton, sp, eventCamera))
                return false;
        }
        return true;
    }
}