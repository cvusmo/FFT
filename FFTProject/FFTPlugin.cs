using BepInEx;
using BepInEx.Logging;
using FFT.Managers;
using FFT.Modules;
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
        public FFT.Managers.Manager manager { get; private set; }
        public new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }
        public override void OnInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
            base.OnInitialized();
            MessageManager.Instance.SubscribeToMessages();
            Logger = base.Logger;
            Logger.LogInfo("Loaded");

            manager = new FFT.Managers.Manager();

            UpdateConditions();
        }
        public void UpdateConditions()
        {
            manager.Update();
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}