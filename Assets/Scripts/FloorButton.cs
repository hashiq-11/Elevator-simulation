using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FloorButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private int floorIndex;
    [SerializeField] private int direction;

    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color defaultColor = new Color(0.2f, 0.6f, 1f); // Blue
    [SerializeField] private Color activeColor = new Color(1f, 0.6f, 0f); // Orange

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(RequestCall);

        if (buttonImage == null) buttonImage = GetComponent<Image>();
        buttonImage.color = defaultColor;
    }

    private void OnEnable() => ElevatorEvents.OnElevatorArrived += ResetButtonColor;
    private void OnDisable() => ElevatorEvents.OnElevatorArrived -= ResetButtonColor;

    private void RequestCall()
    {
        // 1. Update UI state FIRST to prevent synchronous event returns from overwriting the color
        buttonImage.color = activeColor;

        // 2. Broadcast the call to the Manager
        ElevatorEvents.OnFloorCallRequested?.Invoke(floorIndex, direction);
    }

    private void ResetButtonColor(int arrivedFloor)
    {
        // Turn the light off if an elevator has serviced this specific floor
        if (arrivedFloor == floorIndex)
        {
            buttonImage.color = defaultColor;
        }
    }
}