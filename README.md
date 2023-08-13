# Fancy Fancy Fuel Tanks v0.1.4.0 Patch Notes

## Updates and Additions

### New Features
  - Introduced a linear relationship between fuel level and opacity, providing an intuitive and realistic visual representation.
  - Implemented the `Module_VentValve`, controlling the venting process in the fuel system.
  - Added the `RF2 Vent Valve`, a specialized component for venting operations.
  - Remodeled all existing hydrogen tanks, enhancing visual quality and consistency.
  - Added a new hydrogen tank, expanding the selection available to players.
  - Implemented error logging for failure scenarios, aiding in future debugging.

### Enhancements and Optimizations
  - **Fuel Level Calculation**: Refined the logic to calculate fuel levels accurately across different resources.
  - **Opacity Calculation**: Modified the opacity calculation using an animation curve, creating a gradual decrease in opacity as fuel levels decrease from 1 to 0.8.
  - **Modeling and Texturing**: Improved the aesthetic appeal and realism of the hydrogen tanks through careful remodeling and texturing.
  -  Improved performance by only having necessary methods called during Updates/FixedUpdates

### Bug Fixes
  - Fixed an issue causing the fuel level to count up instead of down.
  - Corrected the curve setup, aligning it with the intended relationship between fuel level and opacity.
  - Fixed an issue where SP-701 would angle when attached radially

## Next Steps
- Continuously monitor and adjust the visual effects for further fine-tuning, ensuring that they stay aligned with fuel consumption in the vessel.
- Focus on improving the performance and resource usage of the system.
- Identify and resolve any new bugs or issues to enhance gameplay.

These changes mark a significant update to the fuel system's visual and functional aspects. The addition of new components and a more refined look for the hydrogen tanks provides players with a more immersive experience. If you encounter any issues or have suggestions, please feel free to contribute or open an issue on GitHub.
