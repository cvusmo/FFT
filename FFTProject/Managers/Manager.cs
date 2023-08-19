//|=====================Summary========================|0|
//|                     Manager                        |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFT.Managers
{
    public class Manager
    {
        private readonly List<LoadModule> Modules = new List<LoadModule>();
        internal ManualLogSource Logger { get; }

        private static Manager _instance;
        private static readonly object _lock = new object();
        private readonly ConditionsManager _conditionsManager;

        private readonly StartModule _startmodule;
        private readonly MessageManager _messageManager;
        private readonly ModuleController _moduleController;
        private readonly LoadModule _loadModule;
        private readonly StartModule _startModule;
        private bool isModuleLoaded = false;

        public static Manager Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new Manager();
                }
            }
        }
        private Manager()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Manager: ");
            _conditionsManager = ConditionsManager.Instance ?? throw new InvalidOperationException("ConditionsManager is not initialized.");
            _startModule = StartModule.Instance ?? throw new InvalidOperationException("StartModule is not initialized.");
            _messageManager = MessageManager.Instance ?? throw new InvalidOperationException("MessageManager is not initialized.");
            _moduleController = ModuleController.Instance ?? throw new InvalidOperationException("ModuleController is not initialized.");
            _loadModule = LoadModule.Instance ?? throw new InvalidOperationException("LoadModule is not initialized.");
            _messageManager.ModuleReadyToLoad += HandleModuleReadyToLoad;
            ConditionsManager.Instance.ModuleConditionsMet += HandleModuleConditionsMet;

            InitializeManager();
        }
        private void InitializeManager()
        {
            Utility.RefreshGameManager();
            UpdateMessageManager();
        }
        private void UpdateMessageManager()
        {
            _messageManager.Update();
            Logger.LogDebug("MessageManager.Update called");
        }
        private void HandleModuleConditionsMet()
        {
            LoadModuleForFlight();
            ConditionsManager.Instance.OnModuleLoaded();
        }
        internal void LoadModuleForFlight()
        {
            if (_loadModule == null)
            {
                Logger.LogError("LoadModule is null. Cannot load for flight.");
                return;
            }

            if (!Modules.Contains(_loadModule))
            {
                Modules.Add(_loadModule);
            }
            isModuleLoaded = true;
        }
        internal List<LoadModule> InitializeModules()
        {
            if (_loadModule == null)
            {
                Logger.LogError("LoadModule is null. Cannot initialize modules.");
                return new List<LoadModule>();
            }

            return new List<LoadModule> { _loadModule };
        }
        public void HandleModuleReadyToLoad(ModuleController.ModuleType moduleType)
        {
            if (IsStartingModule())
            {
                LoadModuleForFlight();
                //ModuleLoaded.Invoke(moduleType);
                _startmodule?.StartVentValve();
                if (Utility.ActiveVessel == null)
                {
                    Logger.LogWarning("ActiveVessel is null. HandleModuleReadyToLoad may not function as expected.");
                    return;
                }
            }
        }
        public void UpdateStartModule()
        {
            if (!isModuleLoaded)
            {
                Logger.LogError("Module not loaded. Cannot start.");
                return;
            }

            _startmodule?.StartVentValve();
            Logger.LogDebug("Module started");

            ConditionsManager.Instance.HandleModuleLoaded();
        }
        internal bool IsStartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
        internal void OnModuleReset()
        {
            _moduleController?.ResetAllModuleStates();
            Logger.LogInfo("All Modules have been Reset");
        }    
    }
}