//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule
    {
        [JsonProperty]
        public bool EnableVFX = true;

        private ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("LoadModule: ");
        public FuelTankDefinitions FuelTankDefinitions { get; private set; }
        public Data_FuelTanks DataFuelTanks { get; private set; }
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public Data_ValveParts DataValveParts { get; private set; }
        public Data_VentValve DataVentValve { get; private set; }
        public bool ModuleReadyToLoad { get; private set; }
        public RefreshVesselData.RefreshActiveVessel RefreshActiveVessel { get; private set; }
        internal Manager manager => Manager.Instance;
        internal MessageManager messagemanager => MessageManager.Instance;
        internal ConditionsManager conditionsmanager => ConditionsManager.Instance;
        internal ModuleEnums moduleenums => ModuleEnums.Instance;
        internal StartModule startmodule => StartModule.Instance;
        internal ResetModule resetmodule => ResetModule.Instance;

        private static LoadModule _instance;
        private static readonly object _lock = new object();
        private LoadModule() { }
        public static LoadModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LoadModule();
                        }
                    }
                }
                return _instance;
            }
        }
        internal void Boot()
        {
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _logger.LogInfo("Booting Module_VentValve");
                FuelTankDefinitions = new FuelTankDefinitions();
                DataFuelTanks = new Data_FuelTanks();
                VentValveDefinitions = new VentValveDefinitions();
                DataValveParts = new Data_ValveParts();

                ModuleEnums.IsVentValve = true;
                PreLoad();
            }
        }
        internal void PreLoad()
        {
            Utility.RefreshGameManager();

            if (ModuleEnums.IsVentValve)
            {
                InitializeModuleComponents();
                _logger.LogInfo("Preloading Module_VentValve");
            }

            Load();
        }
        internal void Load()
        {
            if (RefreshActiveVessel.IsFlightActive && ModuleEnums.CurrentModule == ModuleEnums.ModuleType.ModuleVentValve)
            {
                _logger.LogInfo("Loading Module_VentValve");
                ModuleReadyToLoad = true;

                messagemanager.SubscribeToMessages();
            }
        }
        internal void InitializeModuleComponents()
        {
            if (FuelTankDefinitions == null)
            {
                FuelTankDefinitions = UnityEngine.Object.FindObjectOfType<FuelTankDefinitions>();
            }
            if (FuelTankDefinitions != null && DataFuelTanks != null)
            {
                FuelTankDefinitions.PopulateFuelTanks(DataFuelTanks);
            }

            if (VentValveDefinitions == null)
            {
                VentValveDefinitions = UnityEngine.Object.FindObjectOfType<VentValveDefinitions>();
            }
            if (VentValveDefinitions != null && DataValveParts != null)
            {
                VentValveDefinitions.PopulateVentValve(DataValveParts);
            }
        }
        
    }
}