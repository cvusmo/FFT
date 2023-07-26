using FFT.Modules;
using KSP.Sim.impl;
using System;

namespace FFT.Modules
{
    internal class PartComponentModule_TriggerVFX : PartComponentModule
    {
        public override Type PartBehaviourModuleType => typeof(Module_TriggerVFX);
    }
}