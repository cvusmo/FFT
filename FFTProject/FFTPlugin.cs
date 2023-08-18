using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
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
        private FFTPlugin()
        {
            try
            {
                InitializeDependencies();
                InitializeConfig();
                Initialize();
            }
            catch (Exception ex)
            {
                HandleException(ex, "Initialization");
            }
        }
        public override void OnPreInitialized()
        {
            base.OnPreInitialized();
            _logger.LogDebug("FFTPlugin OnPreInitialized called.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();
            _logger.LogInfo("Initializing FFTPlugin...");

            _messageManager.SubscribeToMessages();
            _manager.Update();
            _logger.LogInfo("Subscribed to messages.");
        }
        public void SetLoadModule(ILoadModule loadModule)
        {
            _loadModule = (LoadModule)loadModule;
        }
        private void InitializeDependencies()
        {
            _messageManager = MessageManager.Instance;
            _moduleController = ModuleController.Instance;
            _conditionsManager = new ConditionsManager(_messageManager, _logger);
            _loadModule = LoadModule.Instance;
            _startModule = new StartModule(_messageManager, _logger);
            _resetModule = ResetModule.Instance;
            _manager = Manager.Instance;

            _moduleVentValve = Module_VentValve.Instance;
        }
        private void InitializeConfig()
        {
            Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );
        }
        private void Initialize()
        {
            _logger.LogInfo("Initializing FFTPlugin...");
            _messageManager.SubscribeToMessages();
            _logger.LogInfo("Subscribed to messages.");
        }
        public override void OnPostInitialized()
        {
            base.OnPostInitialized();
            _logger.LogInfo("FFTPlugin OnPostInitialized called.");
        }
        private void HandleException(Exception ex, string context)
        {
            _logger.LogError($"Error in {context}: {ex}");
        }
    }
}
