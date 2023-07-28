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

        public GameInstance _gameInstance;
        public VesselComponent _vesselComponent;
        public IsActiveVessel _isActiveVessel;
        public GameState? _state;
        public FuelTankDefinitions _fuelTankDefinitions;
        public Data_FuelTanks _dataFuelTanks;
        public Module_TriggerVFX Module_TriggerVFX { get; private set; }
        public static FFTPlugin Instance { get; set; }
        public new static ManualLogSource Logger { get; set; }
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
            _fuelTankDefinitions = new FuelTankDefinitions();
        }

        public void Update()
        {
            _state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (_state == GameState.Launchpad || _state == GameState.FlightView || _state == GameState.Runway)
            {
                if (_fuelTankDefinitions == null)
                {
                    _fuelTankDefinitions = FindObjectOfType<FuelTankDefinitions>();
                    return;  // Exit if FuelTankDefinitions is still not available
                } else if (_fuelTankDefinitions != null && _dataFuelTanks != null)
                {
                    _fuelTankDefinitions.PopulateFuelTanks(_dataFuelTanks);
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