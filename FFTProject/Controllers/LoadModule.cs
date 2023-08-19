//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;
using System;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule : ILoadModule
    {
        [JsonProperty]
        public bool EnableVFX { get; private set; } = true;

        private static readonly object _lock = new object();
        private static LoadModule _instance;
        public static LoadModule Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = LoadModule.Instance;
                    }
                    return _instance;
                }
            }
        }

        private readonly ManualLogSource _logger;
        private Module_VentValve Module_VentValve { get; set; }
        internal RefreshVesselData.RefreshActiveVessel RefreshActiveVessel => RefreshVesselData.Instance.RefreshActiveVesselInstance;  
        private FuelTankDefinitions FuelTankDefinitions { get; set; }
        private Data_FuelTanks DataFuelTanks { get; set; }
        private VentValveDefinitions VentValveDefinitions { get; set; }
        private Data_ValveParts DataValveParts { get; set; }
        private Data_VentValve DataVentValve { get; set; }
        private bool ModuleReadyToLoad { get; set; }

        private readonly Manager _manager;
        private readonly MessageManager _messageManager;
        private readonly ConditionsManager _conditionsManager;
        private readonly ModuleController _moduleController;
        private readonly StartModule _startModule;
        private readonly ResetModule _resetModule;

        private event Action _moduleResetRequested;
        public event Action ModuleResetRequested
        {
            add
            {
                _moduleResetRequested += value;
            }
            remove
            {
                _moduleResetRequested -= value;
            }
        }
        private LoadModule(MessageManager _messageManager)
        {
            _logger = BepInEx.Logging.Logger.CreateLogSource("LoadModule: ");
        }
        public void Boot()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _logger.LogInfo("Booting Module_VentValve");
                FuelTankDefinitions = new FuelTankDefinitions();
                DataFuelTanks = new Data_FuelTanks();
                VentValveDefinitions = new VentValveDefinitions();
                DataValveParts = new Data_ValveParts();

                _moduleController.SetModuleState(ModuleController.ModuleType.ModuleVentValve, true);
                PreLoad();
            }
        }
        public void PreLoad()
        {
            Utility.RefreshGameManager();
            if (_moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Preloading Module_VentValve");
                Module_VentValve.InitializeData();
                Module_VentValve.InitializeVFX();
                Load();
            }

        }
        public void Load()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && _moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Loading Module_VentValve");
                ModuleReadyToLoad = true;

                _messageManager.SubscribeToMessages();
                _moduleController.IsModuleLoaded = true;
            }
        }
    }
}
