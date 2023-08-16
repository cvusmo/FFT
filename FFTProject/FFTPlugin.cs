//|=====================Summary========================|0|
//|                Main plugin for FFT                 |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Managers;
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
        internal LoadModule LoadModule { get; private set; }
        public new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }
        public override void OnInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
            base.OnInitialized();
            messagemanager.SubscribeToMessages();
            Logger = base.Logger;
            Logger.LogInfo("Loaded");

            // configuration
            FFTConfig = Config.Bind
                ("Fancy Fuel Tanks Settings",
                "Fancy Fuel Tanks v0.1.4.1",
                LoadModule.EnableVFX, "Fancy Fuel Tanks adds Dynamic Environmental Effects to fuel tanks");
            UpdateConditions();
        }
        public void UpdateConditions()
        {
            manager.Update();
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}