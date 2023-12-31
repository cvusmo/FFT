﻿//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule : ILoadModule
    {
        [JsonProperty]
        public bool EnableVFX { get; private set; } = true;

        private static readonly object _lock = new object();
        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("LoadModule: ");

        internal RefreshVesselData.RefreshActiveVessel RefreshActiveVessel => RefreshVesselData.Instance.RefreshActiveVesselInstance;

        private Manager _manager;
        private MessageManager _messageManager;
        private ConditionsManager _conditionsManager;
        private ModuleController _moduleController;
        private StartModule _startModule;
        private ResetModule _resetModule;
        private Module_VentValve _moduleVentValve;
        private Data_ValveParts _dataValveParts;
        private Data_VentValve _dataVentValve;
        private Data_FuelTanks _dataFuelTanks;
        private FuelTankDefinitions _fuelTankDefinitions;
        private VentValveDefinitions _ventValveDefinitions;

        public event Action ModuleResetRequested;

        private static LoadModule _instance;
        private static readonly Lazy<LoadModule> _lazyInstance = new Lazy<LoadModule>(() => new LoadModule());
        public static LoadModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = _lazyInstance.Value;
                    _instance.InitializeDependencies();
                }
                return _instance;
            }
        }
        private LoadModule() { }
        public void InitializeDependencies()
        {
            _manager = Manager.Instance;
            _messageManager = MessageManager.Instance;
            _conditionsManager = ConditionsManager.Instance;
            _moduleController = ModuleController.Instance;
            _startModule = StartModule.Instance;
            _resetModule = ResetModule.Instance;
            _moduleVentValve = new Module_VentValve();
            _dataValveParts = new Data_ValveParts();
            _dataVentValve = new Data_VentValve();
            _dataFuelTanks = new Data_FuelTanks();
            _fuelTankDefinitions = new FuelTankDefinitions();
            _ventValveDefinitions = new VentValveDefinitions();
        }
        public void Boot()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _logger.LogInfo("Booting Module_VentValve");
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
                _moduleVentValve = new Module_VentValve();
                Load();
            }
        }
        public void Load()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && _moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Loading Module_VentValve");
                _messageManager.SubscribeToMessages();
                _moduleController.IsModuleLoaded = true;
            }
        }
    }
}