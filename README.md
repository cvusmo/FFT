## Fancy Fuel Tanks v0.1.4.1 Patch Notes

### Updates and Additions

### Enhancements and Optimizations

- **Opacity Calculation**:

  - Adjusted the opacity calculation for the fuel tanks, ensuring a linear relationship between fuel level and opacity. The opacity now gradually decreases as the fuel level decreases from 1 to 0.95.
- **Fuel Percentage Conversion**:

  - Modified the `fuelPercentage` value to be within the range of 0 to 1 by dividing by 100, aligning it with the expected input for the opacity calculation.
- **Module Consolidation**:

  - Integrated `Module_TriggerVFX` into `Module_VentValve`, streamlining the control of visual effects and improving maintainability.
- **Singleton Pattern**:

  - Replaced dependency injection in all classes with the Singleton pattern to manage instances. This change streamlines instance management, ensuring that only one instance of a class is instantiated and accessed globally.

- **Event-Driven Logic**:

  - Refactored event handlers and delegates to establish an event-driven architecture, particularly for module activation and data refreshes.
- **Performance Improvement**:

  - Expanded the `RefreshVesselData` method, reducing calculations and optimizing performance through more efficient update processes and conditional animations.
  - 
- **ASL and AGL Curve Update**:

  - Updated the `VFXASLCurve` and `VFXAGLCurve` with new keyframes to reflect a linear relationship with the altitude values. Set the curves to match points (0, 0) and (1, 1), ensuring a direct and linear mapping between altitude and the visual effects.
- **Added Additional Curves**:

  - Vertical Velocity, Horizontal Velocity, Dynamic Pressure, Static Pressure, Atmospheric Temperature, External Temperature, and other environmental factors have been added to make the VFX react to the environment.
- **Cooling VFX Control**:

  - Replaced multiple conditions for controlling Cooling VFX with a single Boolean parameter. This change simplifies the logic for turning the Cooling VFX on and off based on various factors like altitude and fuel level.
- **Module Refactoring**:

  - Extensively refactored every class to better align with modern software design principles and to simplify the codebase.

  - Eliminated the use of `CurrentModule` in favor of a more dynamic and extensible approach.
- **Interface Introduction**:

  - Introduced `ILoadModule` interface to standardize module loading operations, enhancing the maintainability and flexibility of the system.
- **Dynamic Module Identification**:

  - Overhauled `ModuleController` to provide more dynamic module identification. Instead of relying on static properties, it now employs methods that can adapt as the system grows and changes.
- **Utility Class Initialization**:

  - Ensured that the `Utility` class is properly initialized within the `FFTPlugin` to enable its functionality throughout the application.
### Bug Fixes

- Resolved an issue where the fuel calculation at launch incorrectly displayed 0 fuel when the fuel level was 100% full, ensuring accurate and intuitive representation of fuel status
- Addressed redundancy issues in the Manager and ModuleController update methods to prevent unnecessary computations.
- Resolved issue where VFX played in the OAB.
## Next Steps

- Develop a Fuel Tank Selector, providing a tailored and user-friendly selection experience.
- Continue to refine performance and enhance the visual appearance of VFX, aiming for seamless integration and realistic effects.

If you encounter any issues or have suggestions, please feel free to contribute or open an issue on [GitHub](https://github.com/cvusmo/FFT).

## Responsibilities

### FFTPlugin

- **Responsibility**: Initializes all classes.

- **Actions**:

  - Initializes components.

  - Starts `MessageManager` for event listening.

  - Ensures `Manager.Update()` is executed.

  

### MessageManager

  

- **Responsibility**: Captures specific messages/events.

- **Actions**:

  - Routes received messages to `ConditionsManager`.

  

### ConditionsManager

  

- **Responsibility**: Verifies message conditions.

- **Actions**:

  - Directs approved messages to `Manager`.

  

### Manager

  

- **Responsibility**: Serves as a communication hub.

- **Actions**:

  - Accepts directives from `ConditionsManager`.

  - Relays instructions to `LoadModule`.

  

### ILoadModule

  

- **Responsibility**: Defines a standardized approach for module loading.

- **Actions**:

  - Ensures modules adhere to a consistent loading mechanism, fostering modularity and extensibility.

  

### LoadModule

  

- **Responsibility**: Oversees module activation.

- **Actions**:

  - Validates module readiness.

  - Tasks `StartModule` with module initiation.

  

### StartModule

  

- **Responsibility**: Manages module kickoff.

- **Actions**:

  - Initiates the specified module.

  - Alerts `ResetModule` to reset the `ConditionsManager`.

  

### ResetModule

  

- **Responsibility**: Restores `ConditionsManager` to its default status.

- **Actions**:

  - Reverts `ConditionsManager` values.

  - Confirms reset to default state.

  

### ModuleController

  

- **Responsibility**: Dictates and discerns module varieties.

- **Actions**:

  - Adapts to dynamically identify the active module type.

  - Generates module-specific properties as queries.

  

### Module_VentValve

  

- **Responsibility**: Orchestrates visual effects tied to vent valves.

- **Actions**:

  - Sets up vent valve-specific data.

  - Directs vent valve visuals rooted in fuel tank conditions and external surroundings.

  - Refreshes animation according to environment using various curves.

  - Activates or deactivates visual sequences based on conditions.

  - Initiates events in relation to module activation and visual effect prerequisites.

  

### RefreshVesselData

  

- **Responsibility**: Renews vessel-focused data.

- **Actions**:

  - Updates data for the active vessel.

  - Renews metrics for altitude, velocities, pressures, temperatures, and fuel conditions.

  

### Data_VentValve

  

- **Responsibility**: Maintains vent valve-associated data.

- **Actions**:

  - Houses settings, thresholds, and curves relevant to vent valve visual effects.


### Sequence of Operations:

1. **Preliminary Condition Assessment**:
    - The `ConditionsManager` meticulously evaluates the prevailing system conditions to determine the appropriate course of action.

2. **Conditional Module Initialization**:
    - Should the conditions be deemed optimal, the `ConditionsManager` seamlessly delegates the task to the `Manager`, instructing it to engage the requisite module via the `LoadModule` mechanism.

3. **Integrity Verification**:
    - Subsequent to a successful module load, the `Manager` seeks affirmation from the `ConditionsManager`, ensuring alignment of the initialized module with the anticipated operations. The `ConditionsManager`, in turn, ascertains the accuracy of the loaded module.

4. **Module Operationalization**:
    - With all verifications confirming congruence, the `ConditionsManager` authorizes the `Manager` to activate the selected module utilizing the `StartModule` protocol.

5. **Operational Acknowledgment**:
    - In acknowledgment of the commencement, the `Manager` conveys, "Initiating the specified module."
