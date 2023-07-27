using FFT.Modules;
using KSP.Sim.impl;
using System;

namespace FFT.Modules
{
    public class PartComponentModule_TriggerVFX : PartComponentModule
    {
        public override Type PartBehaviourModuleType => typeof(Module_TriggerVFX);
    }
}