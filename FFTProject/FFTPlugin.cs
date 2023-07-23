using BepInEx;
using SpaceWarp;
using KSP.Game;
using KSP.Sim.impl;
using BepInEx.Logging;
using SpaceWarp.API.Mods;
using UnityEngine;
using KSP.Messages.PropertyWatchers;

namespace FFT
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
    public class FFTPlugin : BaseSpaceWarpPlugin
    {
        public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
        public const string ModName = MyPluginInfo.PLUGIN_NAME;
        public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

        internal GameInstance _gameInstance;
        internal VesselComponent _vesselComponent;
        private IsActiveVessel _isActiveVessel;
        internal TriggerController TriggerController { get; private set; } 
        public static FFTPlugin Instance { get; set; }

        internal new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }

        public override void OnPreInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            Instance = this;
            Logger = base.Logger;
            Logger.LogInfo("Loaded");

            _gameInstance = GameManager.Instance.Game; 
        }
        public void FixedUpdate()
        {
            GameState? state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (state == GameState.Launchpad || state == GameState.FlightView || state == GameState.Runway)
            {
                GameObject controllableObject = GameObject.Find("CV401");

                if (controllableObject != null && controllableObject.GetComponent<TriggerController>() == null)
                {
                    TriggerController = controllableObject.AddComponent<TriggerController>();
                }

                if (TriggerController != null && _isActiveVessel.GetValueBool())
                {
                    TriggerController.IsActive = true;
                    Logger.LogInfo("TriggerController IsActive = True");
                }
            }
            else
            {
                if (TriggerController != null)
                {
                    TriggerController.IsActive = false;
                    Logger.LogInfo("TriggerController IsActive = False");
                }
            }
        }

        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}
