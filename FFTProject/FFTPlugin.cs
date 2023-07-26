using BepInEx;
using BepInEx.Logging;
using FFT.Modules;
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
        public FuelTankDefinitions fuelTankDefintions;
        public GameObject CV401;
        internal Module_TriggerVFX Module_TriggerVFX { get; private set; }
        public static FFTPlugin Instance { get; set; }
        internal new static ManualLogSource Logger { get; set; }
        public static string Path { get; private set; }
        public void GetFuelTanks()
        {
            CV401 = fuelTankDefintions.GetFuelTank("CV401");
        }
        void Awake()
        {
            fuelTankDefintions = FindObjectOfType<FuelTankDefinitions>();
            GetFuelTanks();
            if (CV401 == null)
            {
                Logger.LogInfo("CV401 not found in FuelTanks.");
            }
        }
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
                    Logger.LogInfo("CV401 not found in FuelTanks.");
                    return;
                }

                GameObject CoolingVFX = CV401.transform.Find("CoolingVFX")?.gameObject;

                if (CoolingVFX != null)
                {
                    if (CoolingVFX.GetComponent<Module_TriggerVFX>() == null)
                    {
                        Module_TriggerVFX = CoolingVFX.AddComponent<Module_TriggerVFX>();
                        Logger.LogInfo("Module_TriggerVFX added to CoolingVFX");
                    }
                }
                else
                {
                    Logger.LogInfo("CoolingVFX not found");
                }

                if (Module_TriggerVFX != null && _isActiveVessel.GetValueBool())
                {
                    Module_TriggerVFX.IsActive = true;
                    Logger.LogInfo("Module_TriggerVFX IsActive = True");
                }
            }
            else
            {
                if (Module_TriggerVFX != null)
                {
                    Module_TriggerVFX.IsActive = false;
                    Logger.LogInfo("Module_TriggerVFX IsActive = False");
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