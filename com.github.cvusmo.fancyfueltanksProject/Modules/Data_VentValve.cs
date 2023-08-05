using FFT.Modules;
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
        public AnimationCurve VFXASLCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1000, 0));
        [KSPState]
        public AnimationCurve VFXAGLCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1000, 0));
        [KSPState]
        public AnimationCurve VFXVerticalSpeedCurve;
        [KSPState]
        public AnimationCurve VFXHorizontalSpeedCurve;
    }
}