//|=====================Summary========================|0|
//|                Main plugin for FFT                 |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace FFT
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
    public class FFTPlugin : BaseSpaceWarpPlugin
    {
        public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
        public const string ModName = MyPluginInfo.PLUGIN_NAME;
        public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

        public ConfigEntry<bool> FFTConfig;
        internal Manager manager { get; private set; }
        internal ConditionsManager conditionsmanager { get; private set; }
        internal MessageManager messagemanager { get; private set; }
        internal LoadModule loadmodule { get; private set; }
        internal StartModule startmodule { get; private set; }
        internal ResetModule resetmodule { get; private set; }
        internal ModuleEnums moduleenums { get; private set; }
        internal Module_VentValve moduleventvalve { get; private set; }
        internal RefreshVesselData refreshvesseldata { get; private set; }
        public static string Path { get; private set; }

        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FancyFuelTanks: ");
        public FFTPlugin()
        {
            Initialize();
        }
        public void Initialize()
        {
            //initialization
            _logger.LogInfo("Initializing FFTPlugin...");
            manager = Manager.Instance;
            messagemanager = MessageManager.Instance;
            conditionsmanager = ConditionsManager.Instance;
            loadmodule = LoadModule.Instance;
            startmodule = StartModule.Instance;
            resetmodule = ResetModule.Instance;
            moduleenums = ModuleEnums.Instance;
            moduleventvalve = new Module_VentValve(conditionsmanager);
            refreshvesseldata = new RefreshVesselData();
            _logger.LogInfo("FFTPlugin initialized");

            _logger.LogInfo("Subscribing to messages... ");
            messagemanager.SubscribeToMessages();
            _logger.LogInfo("Subscribed to messages.");
        }
        public override void OnPreInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
            _logger.LogDebug("FFTPlugin OnPreInitialized called.");
        }
        public override void OnInitialized()
        {
            base.OnInitialized();

            // Configuration
            FFTConfig = Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                loadmodule.EnableVFX = true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );

            UpdateManager();
        }
        public void UpdateManager()
        {
            manager.Update();
            _logger.LogInfo("Manager.Update() is called");
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}