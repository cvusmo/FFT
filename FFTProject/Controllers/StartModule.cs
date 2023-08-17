//|=====================Summary========================|0|
//|         Initiates the module start process         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;
using KSP.Sim.impl;
using BepInEx.Logging;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StartModule
    {
        internal static StartModule Instance { get; } = new StartModule();
        internal Module_VentValve ModuleVentValve { get; set; }
        private ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("StartModule: ");
        internal StartModule() { }
        internal void StartVentValve()
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();

            if (ModuleEnums.IsVentValve)
            {
                ActivateModule(ModuleEnums.ModuleType.ModuleVentValve);
                _logger.LogInfo("StartVentValve called");
            }

            if (Utility.ActiveVessel == null)
                return;
        }
        internal void ActivateModule(ModuleEnums.ModuleType moduleType)
        {
            if (moduleType == ModuleEnums.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Activate();
                _logger.LogInfo("ActivateModule called");
            }
        }
        internal void DeactivateModule(ModuleEnums.ModuleType moduleType)
        {
            if (moduleType == ModuleEnums.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Deactivate();
                _logger.LogInfo("DeactivateModule called");
            }
        }
    }
}
