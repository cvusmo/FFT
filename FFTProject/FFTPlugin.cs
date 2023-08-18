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
    public class FFTPlugin : BaseSpaceWarpPlugin, IModuleController
    {
        private readonly ManualLogSource _logger;

        public FFTPlugin(
            ManualLogSource logger,
            Manager manager,
            ConditionsManager conditionsManager,
            MessageManager messageManager,
            ILoadModule loadModule,
            StartModule startModule,
            ResetModule resetModule,
            ModuleController moduleController,
            Module_VentValve moduleVentValve,
            RefreshVesselData refreshVesselData)
        {
            _logger = logger;
            Manager = manager;
            ConditionsManager = conditionsManager;
            MessageManager = messageManager;
            LoadModule = (LoadModule)loadModule;
            StartModule = startModule;
            ResetModule = resetModule;
            ModuleController = moduleController;
            ModuleVentValve = moduleVentValve;
            RefreshVesselData = refreshVesselData;

            Initialize();
        }
        internal Manager Manager { get; private set; }
        internal ConditionsManager ConditionsManager { get; private set; }
        internal MessageManager MessageManager { get; private set; }
        internal LoadModule LoadModule { get; private set; }
        internal StartModule StartModule { get; private set; }
        internal ResetModule ResetModule { get; private set; }
        internal ModuleController ModuleController { get; private set; }
        internal Module_VentValve ModuleVentValve { get; private set; }
        internal RefreshVesselData RefreshVesselData { get; private set; }
        public override void OnPreInitialized()
        {
            base.OnPreInitialized();
            _logger.LogDebug("FFTPlugin OnPreInitialized called.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();
            _logger.LogInfo("Initializing FFTPlugin...");

            InitializeDependencies();
            InitializeConfig();

            MessageManager.SubscribeToMessages();
            Manager.Update();
            _logger.LogInfo("Subscribed to messages.");
        }
        public void SetLoadModule(ILoadModule loadModule)
        {
            LoadModule = (LoadModule)loadModule;
        }
        private void InitializeDependencies()
        {
            ModuleController = new ModuleController(this);
            ConditionsManager = new ConditionsManager(Manager, MessageManager, ModuleController, LoadModule, StartModule, ResetModule);
            LoadModule = new LoadModule(Manager, MessageManager, ConditionsManager, ModuleController, StartModule, ResetModule, _logger);
            StartModule = new StartModule(_logger, ModuleController, ModuleVentValve);
            ResetModule = new ResetModule(ConditionsManager, Manager, ModuleController);
            Manager = new Manager(StartModule, MessageManager, ModuleController, LoadModule);
            RefreshVesselData = new RefreshVesselData();
            ModuleVentValve = new Module_VentValve();
        }
        private void InitializeConfig()
        {
            Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                LoadModule.EnableVFX = true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );
        }
        private void Initialize()
        {
            _logger.LogInfo("Initializing FFTPlugin...");
            //Utilities.Utility.Initialize();
            MessageManager.SubscribeToMessages();
            _logger.LogInfo("Subscribed to messages.");
        }
        public override void OnPostInitialized()
        {
            base.OnPostInitialized();
            _logger.LogInfo("FFTPlugin OnPostInitialized called.");
        }
    }
}
