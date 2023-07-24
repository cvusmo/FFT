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
        internal GameState? _state;
        private GameObject CV401;
        internal TriggerController TriggerController { get; private set; } 
        public static FFTPlugin Instance { get; set; }

        internal new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }

        public override void OnPreInitialized()
        {
            FFTPlugin.Path = this.PluginFolderPath;
        }

        private void Start()
        {
            CV401 = GameObject.Find("CV401");
            Logger.LogInfo("CV401 not found at Start");
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            Instance = this;
            Logger = base.Logger;
            Logger.LogInfo("Loaded");

            _gameInstance = GameManager.Instance.Game;
            _isActiveVessel = new IsActiveVessel();
            _vesselComponent = new VesselComponent();
            Logger.LogInfo("gameInstance" + _gameInstance);
            Logger.LogInfo("_isActiveVessel" + _isActiveVessel);
            Logger.LogInfo("_vesselComponent" + _vesselComponent);
        }
        public void Update()
        {
            _state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (_state == GameState.Launchpad || _state == GameState.FlightView || _state == GameState.Runway)
            {
                GameObject controllableObject = GameObject.Find("CV401");
                if (controllableObject != null)
                {
                    Logger.LogInfo("CV401 found: " + controllableObject);
                }
                else
                {
                    Logger.LogInfo("CV401 not found at Update");
                }

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
        public GameState? GetGameState()
        {
            Logger.LogInfo("_state" + _state);
            return _state; 
        }

        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}
