//|=====================Summary========================|0|
//|                   Loads Modules                    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Utilities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule
    {
        [JsonProperty]
        public bool EnableVFX;
        internal static LoadModule _instance;
        internal ModuleEnums ModuleEnums {  get; private set; }
        public FuelTankDefinitions FuelTankDefinitions { get; private set; }
        public Data_FuelTanks DataFuelTanks { get; private set; }
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public Data_ValveParts DataValveParts { get; private set; }
        public Data_VentValve DataVentValve { get; private set; }
        public bool ModuleReadyToLoad { get; private set; }
        public RefreshVesselData.RefreshActiveVessel RefreshActiveVessel { get; private set; }
        internal static LoadModule Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadModule();

                return _instance;
            }
        }
        internal void Boot()
        {
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                FFT.Managers.Manager.Instance._logger.LogInfo("Booting Module_VentValve");
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
                Manager.Instance._logger.LogInfo("Preloading Module_VentValve");
            }
            MessageManager messageManagerInstance = MessageManager.InstanceMM;
            Load(messageManagerInstance);
        }
        internal void Load(MessageManager messageManager)
        {
            if (RefreshActiveVessel.IsFlightActive && ModuleEnums.IsVentValve)
            {
                Manager.Instance._logger.LogInfo("Loading Module_VentValve");
                ModuleReadyToLoad = true;
            }
        }
    }
}