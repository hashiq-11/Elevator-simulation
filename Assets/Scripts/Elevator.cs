using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles physical movement and local UI updates. Acts purely as a "dumb" actor 
/// executing the queue assigned by the ElevatorManager.
/// </summary>
public class Elevator : MonoBehaviour
{
    public enum ElevatorState { Idle, Moving, DoorsOpen }

    [Header("State")]
    public ElevatorState currentState = ElevatorState.Idle;
    public int currentFloor = 0;
    public List<int> floorQueue = new List<int>();

    [Header("Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float doorOpenDuration = 1.5f;
    [SerializeField] private Transform[] floorWaypoints;
    [SerializeField] private TextMeshProUGUI floorDisplayUI;
    [SerializeField] private TextMeshProUGUI statusDisplayUI;

    private void Start()
    {
        SetState(ElevatorState.Idle);
    }

    private void Update()
    {
        // Basic State Machine execution
        if (currentState == ElevatorState.Idle && floorQueue.Count > 0)
        {
            StartCoroutine(ProcessNextFloor());
        }
    }

    private IEnumerator ProcessNextFloor()
    {
        SetState(ElevatorState.Moving);
        int targetFloor = floorQueue[0];

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, floorWaypoints[targetFloor].position.y, startPos.z);

        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;

        // Move smoothly towards the target waypoint using interpolation
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;

            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, fractionOfJourney));

            UpdateDynamicFloorDisplay();
            yield return null;
        }

        // Arrival sequence
        transform.position = targetPos;
        currentFloor = targetFloor;

        // Announce arrival so Hall Buttons can reset their color
        ElevatorEvents.OnElevatorArrived?.Invoke(currentFloor);

        SetState(ElevatorState.DoorsOpen);
        yield return new WaitForSeconds(doorOpenDuration);

        // Cleanup
        floorQueue.RemoveAt(0);
        SetState(ElevatorState.Idle);
    }

    private void UpdateDynamicFloorDisplay()
    {
        float closestDist = float.MaxValue;
        int closestFloor = currentFloor;

        // Find which physical waypoint we are currently closest to
        for (int i = 0; i < floorWaypoints.Length; i++)
        {
            float dist = Mathf.Abs(transform.position.y - floorWaypoints[i].position.y);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestFloor = i;
            }
        }

        floorDisplayUI.text = (closestFloor == 0) ? "G" : closestFloor.ToString();
    }

    private void SetState(ElevatorState newState)
    {
        currentState = newState;

        if (statusDisplayUI != null)
        {
            switch (currentState)
            {
                case ElevatorState.Idle: statusDisplayUI.text = "IDLE"; break;
                case ElevatorState.Moving: statusDisplayUI.text = "MOVING"; break;
                case ElevatorState.DoorsOpen: statusDisplayUI.text = "OPEN"; break;
            }
        }
    }
}