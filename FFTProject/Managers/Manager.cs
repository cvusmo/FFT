//|=====================Summary========================|0|
//|                   Switchboard                      |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers;
using FFT.Modules;
using FFT.Utilities;
using KSP.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFT.Managers
{
    public class Manager
    {
        internal List<LoadModule> Modules { get; private set; } = new List<LoadModule>();
        internal ManualLogSource Logger { get; private set; }
        internal event Action<Module_VentValve> ModuleActivationRequested = delegate { };

        private readonly StartModule _startmodule;
        private readonly MessageManager _messageManager;
        private readonly ModuleController _moduleController;
        private readonly LoadModule _loadModule;
        public Manager(
            StartModule startmodule,
            MessageManager messageManager,
            ModuleController moduleController,
            LoadModule loadModule)
        {
            _startmodule = startmodule ?? throw new ArgumentNullException(nameof(startmodule));
            _messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _loadModule = loadModule ?? throw new ArgumentNullException(nameof(loadModule));

            Logger = BepInEx.Logging.Logger.CreateLogSource("Manager: ");
            _messageManager.ModuleReadyToLoad += HandleModuleReadyToLoad;
        }
        public void Update()
        {
            Utility.RefreshGameManager();
            _messageManager.Update();
            Logger.LogDebug("MessageManager.Update called");
        }
        internal void LoadModuleForFlight()
        {
            if (!Modules.Contains(_loadModule))
            {
                Modules.Add(_loadModule);
            }
        }
        internal List<LoadModule> InitializeModules()
        {
            List<LoadModule> localModules = new List<LoadModule> { _loadModule };
            return localModules;
        }
        internal void OnModuleReset()
        {
            _moduleController.ResetAllModuleStates();
            Logger.LogInfo("Module_VentValve Reset");
        }
        public void HandleModuleReadyToLoad(ModuleController.ModuleType moduleType)
        {
            if (StartingModule())
            {
                _startmodule.StartVentValve();
                if (Utility.ActiveVessel == null)
                {
                    return;
                }
            }
        }
        internal bool StartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
    }
}