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
        public AnimationCurve VFXASLCurve;
        [KSPState]
        public AnimationCurve VFXAGLCurve;
    }
}