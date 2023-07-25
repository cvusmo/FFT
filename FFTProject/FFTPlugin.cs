using BepInEx;
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine;

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
        public override void OnInitialized()
        {
            base.OnInitialized();

            Instance = this;
            Logger = base.Logger;
            Logger.LogInfo("Loaded");

            _gameInstance = GameManager.Instance.Game;
            _isActiveVessel = new IsActiveVessel();
            _vesselComponent = new VesselComponent();

            //CV401 = GameObject.Find("CV401");

            Logger.LogInfo("gameInstance" + _gameInstance);
            Logger.LogInfo("_isActiveVessel" + _isActiveVessel);
            Logger.LogInfo("_vesselComponent" + _vesselComponent);
        }

        public void Update()
        {
            _state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (_state == GameState.Launchpad || _state == GameState.FlightView || _state == GameState.Runway)
            {
                if (CV401 == null)
                {
                    CV401 = GameObject.Find("CV401");
                    if (CV401 != null)
                    {
                        Logger.LogInfo("CV401 found: " + CV401);
                    }
                    else
                    {
                        Logger.LogInfo("CV401 not found");
                    }
                }

                GameObject CoolingVFX = CV401.transform.Find("CoolingVFX")?.gameObject;

                if (CoolingVFX != null)
                {
                    if (CoolingVFX.GetComponent<TriggerController>() == null)
                    {
                        TriggerController = CoolingVFX.AddComponent<TriggerController>();
                    }
                    else
                    {
                        Logger.LogInfo("TriggerController already exists on CoolingVFX");
                    }
                }
                else
                {
                    Logger.LogInfo("CoolingVFX not found");
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
