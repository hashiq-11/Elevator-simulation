# Elevator Management System (2D Unity Simulation)

**Live WebGL Build (Play in Browser):** [Insert your Itch.io Link Here]
**Video Demo (Optional):** [Insert YouTube/Loom Link if you have one, or delete this line]

## Overview
This is a 2D elevator simulation built in Unity 6, designed to handle 3 elevators servicing 4 floors (Ground, 1, 2, and 3). 

Rather than relying on basic "closest distance" logic, this project implements a robust dispatcher algorithm and an Event-Driven architecture to simulate how real-world commercial elevator systems manage multiple calls efficiently.

## Core Architecture & Approach

### 1. Event-Driven UI (Observer Pattern)
To ensure the codebase remains scalable and decoupled, the UI components (`FloorButton.cs`) do not hold direct references to the `ElevatorManager.cs`. Instead, I utilized a static Action-based event class (`ElevatorEvents.cs`). 
* Buttons broadcast an `OnFloorCallRequested` event.
* Elevators broadcast an `OnElevatorArrived` event.
* This allows the Manager to listen and react without tight coupling, making it incredibly easy to add new UI elements or emergency override systems in the future without breaking existing logic.

### 2. Smart Routing Dispatcher
The central `ElevatorManager` acts as the dispatcher. When a floor is called, it calculates a "Routing Cost" for every available elevator to find the most efficient candidate. The algorithm weighs:
* **Distance:** The base physical distance to the target.
* **Momentum & Intent:** If an elevator is already moving *towards* the caller, and the caller wants to go in the *same direction*, it receives a favorable cost. If it is moving away, it receives a massive penalty so the system picks a different car.
* **Current Workload:** A penalty is added based on the size of an elevator's current `floorQueue`. This prevents the system from overloading one single elevator while others sit idle.

### 3. Bulletproof WebGL UI Scaling
A common issue with WebGL builds is that dynamically resizing the browser window causes UI Layout Groups to shift, leaving world-space objects (like the Elevators) floating outside their designated shafts. 
* **The Fix:** I implemented a "Magnet" logic system inside the `Elevator.cs` update loop and movement coroutine. It forces the elevator to continuously lock its X-coordinate to the dynamic UI Waypoint, ensuring it never drifts out of the shaft during window resizing.

### 4. Edge Case Handling
* **Instant Resolution:** If a user requests an elevator from a floor where a car is already sitting idle, the system instantly resolves the request and clears the UI button state without triggering unnecessary movement logic.
* **Synchronous Event Protection:** UI button visual states update *before* firing the broadcast event, ensuring that instant-reply events from the Manager don't get overwritten by race conditions.

## How to Test
1. Click the **Up/Down** buttons on the left panel to call an elevator.
2. Notice how the system prioritizes idle elevators or elevators already moving in your intended direction.
3. Try resizing your browser window—the elevators will dynamically snap to their correct Layout Group positions without breaking.
