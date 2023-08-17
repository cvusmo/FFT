//|=====================Summary========================|0|
//|         Initiates the module start process         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;
using KSP.Sim.impl;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StartModule
    {
        internal static StartModule Instance { get; } = new StartModule();
        internal Module_VentValve ModuleVentValve { get; set; }

        internal StartModule() { } // Private constructor for Singleton

        internal void StartVentValve()
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();

            if (ModuleEnums.IsVentValve)
            {
                ActivateModule(ModuleEnums.ModuleType.ModuleVentValve);
            }

            if (Utility.ActiveVessel == null)
                return;
        }
        internal void ActivateModule(ModuleEnums.ModuleType moduleType)
        {
            if (moduleType == ModuleEnums.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Activate();
            }
        }
        internal void DeactivateModule(ModuleEnums.ModuleType moduleType)
        {
            if (moduleType == ModuleEnums.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Deactivate();
            }
        }
    }
}
