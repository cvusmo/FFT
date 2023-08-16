﻿//|=====================Summary========================|0|
//| Resets the ConditionsManager to its default states |1|
//|by cvusmo===========================================|4|
//|====================================================|1|


using FFT.Managers;
using FFT.Modules;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResetModule : LoadModule
    {

        public event Action ModuleReset = delegate { };
        public void Reset()
        {
            if (IsFlightActive)
            {
                ModuleReset.Invoke();
            }
        }
        public void Unload()
        {
            if (!IsFlightActive)
            {
                Manager._logger.LogInfo("Unloading Module");
                if (IsVentValve)
                {
                    Reset();
                    Manager._logger.LogInfo("Reset Module_VentValve");
                }
            }
        }
    }
}