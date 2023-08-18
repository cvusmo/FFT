//|=====================Summary========================|0|
//|                Main plugin for FFT                 |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

//|=====================Summary========================|0|
//|                Main plugin for FFT                 |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace FFT
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
    public class FFTPlugin : BaseSpaceWarpPlugin
    {
        public ConfigEntry<bool> FFTConfig { get; private set; }
        public string Path { get; private set; }

        private readonly ManualLogSource _logger;
        public FFTPlugin(
            ManualLogSource logger,
            Manager manager,
            ConditionsManager conditionsManager,
            MessageManager messageManager,
            LoadModule loadModule,
            StartModule startModule,
            ResetModule resetModule,
            ModuleController moduleController,
            ILoadModule iLoadModule,
            Module_VentValve moduleVentValve,
            RefreshVesselData refreshVesselData)
        {
            _logger = logger;
            Manager = manager;
            ConditionsManager = conditionsManager;
            MessageManager = messageManager;
            LoadModule = loadModule;
            StartModule = startModule;
            ResetModule = resetModule;
            ModuleController = moduleController;
            ILoadModule = iLoadModule;
            ModuleVentValve = moduleVentValve;
            RefreshVesselData = refreshVesselData;

            Initialize();
        }
        internal Manager Manager { get; }
        internal ConditionsManager ConditionsManager { get; }
        internal MessageManager MessageManager { get; }
        internal LoadModule LoadModule { get; }
        internal StartModule StartModule { get; }
        internal ResetModule ResetModule { get; }
        internal ModuleController ModuleController { get; }
        internal ILoadModule ILoadModule { get; }
        internal Module_VentValve ModuleVentValve { get; }
        internal RefreshVesselData RefreshVesselData { get; }
        private void Initialize()
        {
            _logger.LogInfo("Initializing FFTPlugin...");

            Utilities.Utility.Initialize();

            MessageManager.SubscribeToMessages();
            _logger.LogInfo("Subscribed to messages.");
        }
        public override void OnPreInitialized()
        {
            Path = this.PluginFolderPath;
            _logger.LogDebug("FFTPlugin OnPreInitialized called.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();
            FFTConfig = Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                LoadModule.EnableVFX = true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );

            UpdateManager();
        }
        private void UpdateManager()
        {
            Manager.Update();
            _logger.LogInfo("Manager.Update() is called");
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}