//|=====================Summary========================|0|
//|                   Initializer                      |1|
//|by cvusmo===========================================|4|
//|====================================================|2|
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace FFT
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
    internal class FFTPlugin : BaseSpaceWarpPlugin, IModuleController
    {
        public ConfigEntry<bool> FFTConfig { get; private set; }

        internal readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFTPlugin");
        public static FFTPlugin Instance { get; private set; }
        public static string Path { get; private set; }

        private Manager _manager;
        private ConditionsManager _conditionsManager;
        private MessageManager _messageManager;
        private LoadModule _loadModule;
        private StartModule _startModule;
        private ResetModule _resetModule;
        private RefreshVesselData _refreshVesselData;
        private ModuleController _moduleController;
        private Module_VentValve _moduleVentValve;
        public FFTPlugin()
        {
            if (Instance != null)
            {
                throw new Exception("FFTPlugin is a singleton and cannot have multiple instances!");
            }
            Instance = this;
        }
        public override void OnPreInitialized()
        {
            Path = this.PluginFolderPath;
            base.OnPreInitialized();
            _logger.LogInfo("OnPreInitialized FFTPlugin.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();

            _logger.LogInfo("Initializing FFTPlugin...");

            Config.Bind("Fancy Fuel Tanks Settings", "Enable VFX", true, "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks");

            try
            {
                InitializeDependencies();
            }
            catch (Exception ex)
            {
                HandleException(ex, "FFT Initialization");
            }

            _logger.LogInfo("Initialized FFTPlugin.");
        }
        public void SetLoadModule(ILoadModule loadModule)
        {
            _loadModule = LoadModule.Instance;
        }
        private void InitializeDependencies()
        {
            _logger.LogInfo("Subscribing to messages.... ");
            _messageManager = MessageManager.Instance;
        }
        public override void OnPostInitialized()
        {
            _logger.LogInfo("Calling OnPostInitialized...");
            base.OnPostInitialized();

            try
            {
                _conditionsManager = ConditionsManager.Instance;
                _moduleController = ModuleController.Instance;
                _startModule = StartModule.Instance;

                Manager.InitializeInstance();
                _manager = Manager.Instance;

                _refreshVesselData = RefreshVesselData.Instance;
                _resetModule = new ResetModule(_conditionsManager, _manager, _moduleController, _refreshVesselData);

                _moduleVentValve = new Module_VentValve();

                LogInitializationSuccesses();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during OnPostInitialized: {ex.Message}");
            }
        }
        private void LogInitializationSuccesses()
        {
            _logger.LogInfo("ConditionsManager initialized successfully.");
            _logger.LogInfo("ModuleController initialized successfully.");
            _logger.LogInfo("StartModule initialized successfully.");
            _logger.LogInfo("Manager initialized successfully.");
            _logger.LogInfo("RefreshVesselData initialized successfully.");
            _logger.LogInfo("ResetModule initialized successfully.");
            _logger.LogDebug("ModuleVentValve initialized successfully.");
        }
        private void HandleException(Exception ex, string context)
        {
            _logger.LogError($"Error in {context}: {ex}");
        }
    }
}