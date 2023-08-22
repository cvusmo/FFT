using FFT.Modules;
using System.Collections.Generic;
using KSP.Sim;
using KSP.Sim.Definitions;
using System;
using UnityEngine;

namespace FFT.Modules
{
    [Serializable]
    public class Data_VentValve : ModuleData
    {
        public override Type ModuleType => typeof(Module_VentValve);

        [KSPState]
        public AnimationCurve VFXASLCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1000, 1));
        [KSPState]
        public AnimationCurve VFXAGLCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1000, 1));
        [KSPState]
        public AnimationCurve VFXVerticalVelocity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(200, 1));
        [KSPState]
        public AnimationCurve VFXHorizontalVelocity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(200, 1));
        [KSPState]
        public AnimationCurve VFXDynamicPressure = new AnimationCurve(new Keyframe(101.325f, 0), new Keyframe(0.100f, 1));
        [KSPState]
        public AnimationCurve VFXStaticPressure = new AnimationCurve(new Keyframe(99.65f, 0), new Keyframe(0, 1));
        [KSPState]
        public AnimationCurve VFXAtmosphericTemperature = new AnimationCurve(new Keyframe(287.24f, 0), new Keyframe(0, 1));
        [KSPState]
        public AnimationCurve VFXExternalTemperature = new AnimationCurve(new Keyframe(287.24f, 0), new Keyframe(0, 1));
        [KSPState]
        public AnimationCurve VFXOpacityCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(0.95f, 0));
    }
}