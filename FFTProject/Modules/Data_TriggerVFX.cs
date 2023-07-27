using FFT.Modules;
using KSP.Sim;
using KSP.Sim.Definitions;
using System;
using UnityEngine;

namespace FFT.Modules
{
    [Serializable]
    public class Data_TriggerVFX : ModuleData
    {
        public override Type ModuleType => typeof(Module_TriggerVFX);

        [KSPState]
        public AnimationCurve VFXOpacityCurve;
    }
}