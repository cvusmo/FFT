//|=====================Summary========================|0|
//|                   Switchboard                      |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;
using System;
using System.Collections.Generic;

namespace FFT.Managers
{
    public class Manager
    {
        private List<LoadModule> Modules { get; } = new List<LoadModule>();
        internal ManualLogSource Logger { get; private set; }

        private static Manager _instance;
        private static readonly object _lock = new object();

        private StartModule _startmodule => StartModule.Instance;
        private MessageManager _messageManager => MessageManager.Instance;
        private ModuleController _moduleController => ModuleController.Instance;
        private LoadModule _loadModule => LoadModule.Instance;
        public static Manager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = Manager.Instance;
                    }
                    return _instance;
                }
            }
        }
        private Manager()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Manager: ");
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
            return new List<LoadModule> { _loadModule };
        }
        internal void OnModuleReset()
        {
            _moduleController.ResetAllModuleStates();
            Logger.LogInfo("Module_VentValve Reset");
        }
        public void HandleModuleReadyToLoad(ModuleController.ModuleType moduleType)
        {
            if (IsStartingModule())
            {
                _startmodule.StartVentValve();
                if (Utility.ActiveVessel == null)
                {
                    return;
                }
            }
        }
        internal bool IsStartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
    }
}

