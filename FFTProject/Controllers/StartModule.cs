//|=====================Summary========================|0|
//|         Initiates the module start process         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;
using FFT.Controllers.Interfaces;
using BepInEx.Logging;
using System;
using FFT.Managers;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StartModule : IStartModule
    {
        private readonly ManualLogSource _logger;
        private readonly ModuleController _moduleController;
        private readonly MessageManager _messageManager;
        private static Module_VentValve _moduleVentValve;
        public Module_VentValve ModuleVentValve { get; private set; }

        private static StartModule _instance;
        private static readonly object _lock = new object();

        private readonly TimeSpan ThrottleTimeSpan = TimeSpan.FromSeconds(2);
        private DateTime _lastEventTriggerTime = DateTime.MinValue;
        public static StartModule Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new StartModule(
                            BepInEx.Logging.Logger.CreateLogSource("StartModule: "),
                            ModuleController.Instance,
                            _moduleVentValve = new Module_VentValve());
                    }
                    return _instance;
                }
            }
        }
        private StartModule(
            ManualLogSource logger,
            ModuleController moduleController,
            Module_VentValve moduleVentValve)
        {
            _logger = logger;
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            ModuleVentValve = moduleVentValve ?? throw new ArgumentNullException(nameof(moduleVentValve));
        }
        internal StartModule(MessageManager messageManager, ManualLogSource logger)
        {
            _messageManager = messageManager;
            _logger = logger;
        }
        public void StartVentValve()
        {
            if (DateTime.Now - _lastEventTriggerTime < ThrottleTimeSpan) return;
            _lastEventTriggerTime = DateTime.Now;

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