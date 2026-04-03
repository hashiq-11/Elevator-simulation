using UnityEngine;

/// <summary>
/// Acts as the central dispatcher. Uses a directional cost-calculation algorithm 
/// to ensure elevators behave logically based on their current state and momentum.
/// </summary>
public class ElevatorManager : MonoBehaviour
{
    [SerializeField] private Elevator[] elevators;

    private void OnEnable() => ElevatorEvents.OnFloorCallRequested += HandleFloorCall;
    private void OnDisable() => ElevatorEvents.OnFloorCallRequested -= HandleFloorCall;

    private void HandleFloorCall(int requestedFloor, int requestedDirection)
    {
        Elevator bestElevator = null;
        int lowestCost = int.MaxValue;

        foreach (Elevator el in elevators)
        {
            // Edge Case 1: An elevator is already sitting idle at the requested floor.
            if (el.currentFloor == requestedFloor && el.currentState != Elevator.ElevatorState.Moving)
            {
                // Instantly resolve the request and turn off the UI button lights
                ElevatorEvents.OnElevatorArrived?.Invoke(requestedFloor);
                return;
            }

            // Edge Case 2: Ignore if this floor is already in this elevator's queue
            if (el.floorQueue.Contains(requestedFloor))
                return;

            // Calculate the efficiency cost of sending this specific elevator
            int cost = CalculateRoutingCost(el, requestedFloor, requestedDirection);

            if (cost < lowestCost)
            {
                lowestCost = cost;
                bestElevator = el;
            }
        }

        // Dispatch the most efficient elevator found
        if (bestElevator != null)
        {
            bestElevator.floorQueue.Add(requestedFloor);
        }
    }

    private int CalculateRoutingCost(Elevator el, int targetFloor, int targetDir)
    {
        // Base cost is the physical distance
        int distance = Mathf.Abs(el.currentFloor - targetFloor);
        int cost = distance;

        if (el.currentState == Elevator.ElevatorState.Idle)
        {
            cost += 0; // Idle elevators are preferred
        }
        else if (el.currentState == Elevator.ElevatorState.Moving)
        {
            int currentDir = (el.floorQueue[0] > el.currentFloor) ? 1 : -1;

            // Check if the elevator is moving towards the user
            bool isMovingTowards = (currentDir == 1 && targetFloor > el.currentFloor) ||
                                   (currentDir == -1 && targetFloor < el.currentFloor);

            // If it's moving towards the user AND the user wants to go in the same direction, it's a valid pickup
            if (isMovingTowards && currentDir == targetDir)
            {
                cost += 1;
            }
            else
            {
                // Elevator is moving away or passenger intends to go the wrong way. Massive penalty.
                cost += 100;
            }
        }

        // Add a penalty for how many stops it already has queued (prevents one elevator from doing all the work)
        cost += (el.floorQueue.Count * 3);

        return cost;
    }
}