using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the physical movement and local UI updates.
/// executing the queue assigned to it by the ElevatorManager.
/// </summary>
public class Elevator : MonoBehaviour
{
    public enum ElevatorState { Idle, Moving, DoorsOpen }

    [Header("Elevator State")]
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
        if (currentState == ElevatorState.Idle)
        {
            if (floorQueue.Count > 0)
            {
                StartCoroutine(ProcessNextFloor());
            }
            else if (floorWaypoints.Length > 0)
            {
                // WebGL Fix: If the user resizes their browser window, the UI Layout Group might move the shafts.
                // By constantly snapping the X/Y position here, the elevator acts like a magnet to the waypoint 
                // and won't accidentally float outside the shaft.
                Vector3 snapPos = floorWaypoints[currentFloor].position;
                transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
            }
        }
    }

    private IEnumerator ProcessNextFloor()
    {
        SetState(ElevatorState.Moving);
        int targetFloor = floorQueue[0];

        Vector3 startPos = transform.position;
        float startTime = Time.time;

        // Move smoothly towards the target waypoint
        while (Vector3.Distance(transform.position, GetTargetPos(targetFloor)) > 0.01f)
        {
            Vector3 targetPos = GetTargetPos(targetFloor);
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float fractionOfJourney = ((Time.time - startTime) * speed) / journeyLength;

            // Mathf.SmoothStep gives the movement a nice ease-in, ease-out mechanical feel
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, fractionOfJourney));

            // Lock the X position to the waypoint to prevent horizontal drifting if the screen resizes mid-movement
            nextPos.x = targetPos.x;
            transform.position = nextPos;

            UpdateDynamicFloorDisplay();
            yield return null;
        }

        // Arrival sequence
        transform.position = GetTargetPos(targetFloor);
        currentFloor = targetFloor;

        // Broadcast to the building that we arrived so the Hall Buttons can turn off their lights
        ElevatorEvents.OnElevatorArrived?.Invoke(currentFloor);

        SetState(ElevatorState.DoorsOpen);
        yield return new WaitForSeconds(doorOpenDuration);

        // Cleanup and finish
        floorQueue.RemoveAt(0);
        SetState(ElevatorState.Idle);
    }

    private Vector3 GetTargetPos(int floorIndex)
    {
        return floorWaypoints[floorIndex].position;
    }

    private void UpdateDynamicFloorDisplay()
    {
        float closestDist = float.MaxValue;
        int closestFloor = currentFloor;

        // Check which physical waypoint we are currently passing
        for (int i = 0; i < floorWaypoints.Length; i++)
        {
            float dist = Mathf.Abs(transform.position.y - floorWaypoints[i].position.y);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestFloor = i;
            }
        }

        // Visual polish: Display "G" instead of "0" for the ground floor
        floorDisplayUI.text = (closestFloor == 0) ? "G" : closestFloor.ToString();
    }

    private void SetState(ElevatorState newState)
    {
        currentState = newState;

        // Keep the local UI car text updated with what the machine is currently doing
        if (statusDisplayUI != null)
        {
            statusDisplayUI.text = currentState.ToString().ToUpper();
        }
    }
}