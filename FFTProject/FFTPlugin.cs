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

        public new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }
        public override void OnPreInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
        }
        public override void OnInitialized()
        {
            base.OnInitialized();
            Logger = base.Logger;
            Logger.LogDebug("Initializing FFTPlugin...");

            //initialize
            manager = new Manager();
            messagemanager = new MessageManager();
            conditionsmanager = new ConditionsManager();
            loadmodule = new LoadModule();
            startmodule = new StartModule();
            resetmodule = new ResetModule();
            moduleenums = new ModuleEnums();
            moduleventvalve = new Module_VentValve();
            refreshvesseldata = new RefreshVesselData();


            messagemanager.SubscribeToMessages();

            // Configuration
            FFTConfig = Config.Bind(
                "Fancy Fuel Tanks Settings",
                "Enable VFX",
                loadmodule.EnableVFX = true,
                "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks"
            );

            UpdateConditions();
            Logger.LogDebug("FFTPlugin initialized");
        }
        public void UpdateConditions()
        {
            manager.Update();
            Logger.LogDebug("Manager.Update() is called");
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}