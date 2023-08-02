using BepInEx;
using BepInEx.Logging;
using FFT.Modules;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
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

        public GameInstance _gameInstance;
        public IsActiveVessel _isActiveVessel;
        public VesselComponent _vesselComponent;
        public GameState? _state;
        public FuelTankDefinitions _fuelTankDefinitions;
        public Data_FuelTanks _dataFuelTanks;
        public VentValveDefinitions _ventValveDefinitions;
        public Data_ValveParts _dataValveParts;
        public Module_TriggerVFX Module_TriggerVFX { get; private set; }
        public Module_VentValve Module_VentValve { get; private set; }
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
            _dataFuelTanks = new Data_FuelTanks();
            _ventValveDefinitions = new VentValveDefinitions();
            _dataValveParts = new Data_ValveParts();
        }
        public void Update()
        {
            _state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (_state == GameState.Launchpad || _state == GameState.FlightView || _state == GameState.Runway)
            {
                if (_fuelTankDefinitions == null)
                {
                    _fuelTankDefinitions = FindObjectOfType<FuelTankDefinitions>();
                }
                if (_ventValveDefinitions == null)
                {
                    _ventValveDefinitions = FindObjectOfType<VentValveDefinitions>();
                }

                if (_fuelTankDefinitions != null && _dataFuelTanks != null)
                {
                    _fuelTankDefinitions.PopulateFuelTanks(_dataFuelTanks);
                }

                if (_ventValveDefinitions != null && _dataValveParts != null)
                {
                    _ventValveDefinitions.PopulateVentValve(_dataValveParts);
                }

                foreach (var module in FindObjectsOfType<Module_VentValve>())
                {
                    module.Activate();
                }
                foreach (var module in FindObjectsOfType<Module_TriggerVFX>())
                {
                    module.Activate();
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