//|=====================Summary========================|0|
//|         Initiates the module start process         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;
using FFT.Controllers.Interfaces;
using BepInEx.Logging;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StartModule : IStartModule
    {
        private readonly ManualLogSource _logger;
        private readonly ModuleController _moduleController;
        public Module_VentValve ModuleVentValve { get; }

        public StartModule(
            ManualLogSource logger,
            ModuleController moduleController,
            Module_VentValve moduleVentValve)
        {
            _logger = logger ?? BepInEx.Logging.Logger.CreateLogSource("StartModule: ");
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            ModuleVentValve = moduleVentValve ?? throw new ArgumentNullException(nameof(moduleVentValve));
        }

        public void StartVentValve()
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();

            if (_moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                ActivateModule(ModuleController.ModuleType.ModuleVentValve);
                _logger.LogInfo("StartVentValve called");
            }

            if (Utility.ActiveVessel == null)
                return;
        }

        public void ActivateModule(ModuleController.ModuleType moduleType)
        {
            if (moduleType == ModuleController.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Activate();
                _logger.LogInfo("ActivateModule called");
            }
        }

        public void DeactivateModule(ModuleController.ModuleType moduleType)
        {
            if (moduleType == ModuleController.ModuleType.ModuleVentValve && ModuleVentValve != null)
            {
                ModuleVentValve.Deactivate();
                _logger.LogInfo("DeactivateModule called");
            }
        }
    }
}