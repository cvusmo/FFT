//|=====================Summary========================|0|
//|                   Initializer                      |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
using KSP.Messages;
using SpaceWarp;
using SpaceWarp.API.Mods;
using System;

namespace FFT
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
    internal class FFTPlugin : BaseSpaceWarpPlugin, IModuleController
    {
        private static readonly object _lock = new object();
        private static FFTPlugin _instance;
        public ConfigEntry<bool> FFTConfig { get; private set; }
        public string Path { get; private set; }
        public static FFTPlugin Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new FFTPlugin();
                        }
                    }
                }
                return _instance;
            }
        }

        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFTPlugin");

        private Manager _manager;
        private ConditionsManager _conditionsManager;
        private MessageManager _messageManager;
        private LoadModule _loadModule;
        private StartModule _startModule;
        private ResetModule _resetModule;
        private ModuleController _moduleController;
        private Module_VentValve _moduleVentValve;
        public override void OnPreInitialized()
        {
            base.OnPreInitialized();
            _logger.LogDebug("FFTPlugin OnPreInitialized called.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();
            _logger.LogDebug("Initializing FFTPlugin...");

            Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );

            try
            {
                InitializeDependencies();
            }
            catch (Exception ex)
            {
                HandleException(ex, "FFT Initialization");
            }

            _logger.LogDebug("Initialized FFTPlugin.");
        }
        public void SetLoadModule(ILoadModule loadModule)
        {
            _loadModule = (LoadModule)loadModule;
        }
        private void InitializeDependencies()
        {
            _logger.LogInfo("Subscribing to messages.... ");
            _messageManager = MessageManager.Instance;
        }
        public override void OnPostInitialized()
        {
            _logger.LogDebug("Calling OnPostInitialized...");
            base.OnPostInitialized();

            try
            {
                _logger.LogDebug("Attempting to initialize ConditionsManager...");
                _conditionsManager = ConditionsManager.Instance;
                _logger.LogDebug("ConditionsManager initialized successfully.");

                _logger.LogDebug("Attempting to initialize ModuleController...");
                _moduleController = ModuleController.Instance;
                _logger.LogDebug("ModuleController initialized successfully.");

                _logger.LogDebug("Attempting to initialize StartModule...");
                _startModule = StartModule.Instance;
                _logger.LogDebug("StartModule initialized successfully.");

                _logger.LogDebug("Attempting to initialize ResetModule...");
                _resetModule = ResetModule.Instance;
                _logger.LogDebug("ResetModule initialized successfully.");

                _logger.LogDebug("Attempting to initialize Manager...");
                _manager = Manager.Instance;
                _logger.LogDebug("Manager initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during OnPostInitialized: {ex.Message}");
            }
        }
        private void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            TryInitializeModuleVentValve();
        }
        private void GameStateLeftHandler(MessageCenterMessage obj)
        {
            TryInitializeModuleVentValve();
        }
        private void VesselSituationChangedHandler(VesselSituationChangedMessage msg)
        {
            TryInitializeModuleVentValve();
        }
        private void TryInitializeModuleVentValve()
        {
            if (_conditionsManager.ConditionsReady() && _moduleVentValve == null)
            {
                _logger.LogDebug("Attempting to initialize ModuleVentValve...");
                _moduleVentValve = Module_VentValve.Instance;
                _logger.LogDebug("ModuleVentValve initialized successfully.");
            }
        }
        private void HandleException(Exception ex, string context)
        {
            _logger.LogError($"Error in {context}: {ex}");
        }

        ~FFTPlugin()
        {
            _messageManager.GameStateEntered -= GameStateEnteredHandler;
            _messageManager.GameStateLeft -= GameStateLeftHandler;
            _messageManager.VesselSituationChanged -= VesselSituationChangedHandler;
        }
    }
}