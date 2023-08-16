//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
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
        public bool EnableVFX;
        internal ModuleEnums ModuleEnums { get; private set; }
        public FuelTankDefinitions FuelTankDefinitions { get; private set; }
        public Data_FuelTanks DataFuelTanks { get; private set; }
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public Data_ValveParts DataValveParts { get; private set; }
        public Data_VentValve DataVentValve { get; private set; }
        public bool ModuleReadyToLoad { get; private set; }
        public RefreshVesselData.RefreshActiveVessel RefreshActiveVessel { get; private set; }

        private static LoadModule _instance;
        private LoadModule _loadmodule => LoadModule.Instance;
        private Manager _manager => Manager.Instance;
        private MessageManager _messageManager => _manager.MessageManager;
        internal static LoadModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoadModule();
                }
                return _instance;
            }
        }
        internal LoadModule()
        {
            ModuleEnums = new ModuleEnums();
        }
        internal void Boot()
        {
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _manager._logger.LogInfo("Booting Module_VentValve");
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
                _manager._logger.LogInfo("Preloading Module_VentValve");
            }

            Load();
        }
        private void InitializeModuleComponents()
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
        internal void Load()
        {
            if (RefreshActiveVessel.IsFlightActive && ModuleEnums.IsVentValve)
            {
                _manager._logger.LogInfo("Loading Module_VentValve");
                ModuleReadyToLoad = true;

                _messageManager.SubscribeToMessages(); // Make sure MessageManager is listening to necessary messages
            }
        }
    }
}