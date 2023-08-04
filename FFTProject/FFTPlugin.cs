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

        public GameInstance gameInstance;
        public IsActiveVessel isActiveVessel;
        public VesselComponent vesselComponent;
        public GameState? _state;
        public FuelTankDefinitions fuelTankDefinitions;
        public Data_FuelTanks dataFuelTanks;
        public VentValveDefinitions ventValveDefinitions;
        public Data_ValveParts dataValveParts;
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

            gameInstance = GameManager.Instance.Game;
            isActiveVessel = new IsActiveVessel();
            vesselComponent = new VesselComponent();
            fuelTankDefinitions = new FuelTankDefinitions();
            dataFuelTanks = new Data_FuelTanks();
            ventValveDefinitions = new VentValveDefinitions();
            dataValveParts = new Data_ValveParts();
        }
        public void Update()
        {
            _state = BaseSpaceWarpPlugin.Game?.GlobalGameState?.GetState();

            if (_state == GameState.Launchpad || _state == GameState.FlightView || _state == GameState.Runway)
            {
                if (fuelTankDefinitions == null)
                {
                    fuelTankDefinitions = FindObjectOfType<FuelTankDefinitions>();
                }
                if (ventValveDefinitions == null)
                {
                    ventValveDefinitions = FindObjectOfType<VentValveDefinitions>();
                }

                if (fuelTankDefinitions != null && dataFuelTanks != null)
                {
                    fuelTankDefinitions.PopulateFuelTanks(dataFuelTanks);
                }

                if (ventValveDefinitions != null && dataValveParts != null)
                {
                    ventValveDefinitions.PopulateVentValve(dataValveParts);
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
        public VesselComponent GetActiveVessel()
        {
            return GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
        }
        public override void OnPostInitialized() => base.OnPostInitialized();
    }
}