using System;

/// <summary>
/// Event-driven architecture (Observer Pattern) keeps the UI completely decoupled from the Elevator Manager logic.
/// </summary>
public static class ElevatorEvents
{
    // Triggered by UI buttons. Passes the requested floor index (0,1,2,3) and direction (+1 UP, -1 DOWN).
    public static Action<int, int> OnFloorCallRequested;

    // Triggered by the Manager or Elevator when a floor is successfully serviced. Passes the floor index.
    public static Action<int> OnElevatorArrived;
}