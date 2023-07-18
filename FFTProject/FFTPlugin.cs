using BepInEx;
using UnityEngine;
using UnityEngine.Sprites;
using HarmonyLib;
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
        public static FFTPlugin Instance { get; set; }
        public static string Path { get; private set; }

        public override void OnPreInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
        }
        public override void OnInitialized()
        {
            base.OnInitialized();

        }
        public override void OnPostInitialized() => base.OnPostInitialized();

    }
}