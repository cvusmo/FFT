using KSP.Sim;
using KSP.Sim.Definitions;
using UnityEngine;

namespace FFT.Modules
{
    [Serializable]
    public class Data_VentValve : ModuleData
    {
        public override Type ModuleType => typeof(Module_VentValve);

        // Altitude related curves
        [KSPState] public AnimationCurve VFXASLCurve = DefaultASLCurve();
        public static AnimationCurve DefaultASLCurve() => new AnimationCurve(new Keyframe(0, 0), new Keyframe(1000, 1));

        [KSPState] public AnimationCurve VFXAGLCurve = DefaultAGLCurve();     
        public static AnimationCurve DefaultAGLCurve() => new AnimationCurve(new Keyframe(0, 0), new Keyframe(1000, 1));
        
        // Velocity related curves
        [KSPState] public AnimationCurve VFXVerticalVelocity = DefaultVerticalVelocityCurve();
        public static AnimationCurve DefaultVerticalVelocityCurve() => new AnimationCurve(new Keyframe(0, 0), new Keyframe(200, 1));
        [KSPState]public AnimationCurve VFXHorizontalVelocity = DefaultHorizontalVelocityCurve();
        public static AnimationCurve DefaultHorizontalVelocityCurve() => new AnimationCurve(new Keyframe(0, 0), new Keyframe(200, 1));

        // Pressure and temperature related curves
        [KSPState] public AnimationCurve VFXDynamicPressure = DefaultDynamicPressureCurve();
        public static AnimationCurve DefaultDynamicPressureCurve() => new AnimationCurve(new Keyframe(101.325f, 0), new Keyframe(0.100f, 1));
        [KSPState] public AnimationCurve VFXStaticPressure = DefaultStaticPressureCurve();
        public static AnimationCurve DefaultStaticPressureCurve() => new AnimationCurve(new Keyframe(99.65f, 0), new Keyframe(0, 1));
        [KSPState] public AnimationCurve VFXAtmosphericTemperature = DefaultAtmosphericTemperatureCurve();
        public static AnimationCurve DefaultAtmosphericTemperatureCurve() => new AnimationCurve(new Keyframe(287.24f, 0), new Keyframe(0, 1));
        [KSPState] public AnimationCurve VFXExternalTemperature = DefaultExternalTemperatureCurve();
        public static AnimationCurve DefaultExternalTemperatureCurve() => new AnimationCurve(new Keyframe(287.24f, 0), new Keyframe(0, 1));

        //Fuel
        [KSPState] public AnimationCurve VFXFuelPercentage = DefaultFuelPercentageCurve();
        public static AnimationCurve DefaultFuelPercentageCurve() => new AnimationCurve(new Keyframe(1, 1), new Keyframe(0.95f, 0));
    }
}