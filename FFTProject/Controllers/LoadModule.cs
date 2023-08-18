//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule : ILoadModule
    {
        [JsonProperty]
        public bool EnableVFX { get; set; } = true;

        private readonly ManualLogSource _logger;
        public FuelTankDefinitions FuelTankDefinitions { get; private set; }
        public Data_FuelTanks DataFuelTanks { get; private set; }
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public Data_ValveParts DataValveParts { get; private set; }
        public Data_VentValve DataVentValve { get; private set; }
        public bool ModuleReadyToLoad { get; private set; }
        public RefreshVesselData.RefreshActiveVessel RefreshActiveVessel { get; private set; }

        private readonly Manager _manager;
        private readonly MessageManager _messageManager;
        private readonly ConditionsManager _conditionsManager;
        private readonly ModuleController _moduleController;
        private readonly StartModule _startModule;
        private readonly ResetModule _resetModule;

        public event Action ModuleResetRequested = delegate { };

        public LoadModule(
            Manager manager,
            MessageManager messageManager,
            ConditionsManager conditionsManager,
            ModuleController moduleController,
            StartModule startModule,
            ResetModule resetModule,
            ManualLogSource logger)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _conditionsManager = conditionsManager ?? throw new ArgumentNullException(nameof(conditionsManager));
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _startModule = startModule ?? throw new ArgumentNullException(nameof(startModule));
            _resetModule = resetModule ?? throw new ArgumentNullException(nameof(resetModule));
            _logger = logger ?? BepInEx.Logging.Logger.CreateLogSource("LoadModule: ");
        }
        public void Boot()
        {
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _logger.LogInfo("Booting Module_VentValve");
                FuelTankDefinitions = new FuelTankDefinitions();
                DataFuelTanks = new Data_FuelTanks();
                VentValveDefinitions = new VentValveDefinitions();
                DataValveParts = new Data_ValveParts();

                _moduleController.SetModuleState(ModuleController.ModuleType.ModuleVentValve, true);
                PreLoad();
            }
        }
        public void PreLoad()
        {
            Utility.RefreshGameManager();

            if (_moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                InitializeModuleComponents();
                _logger.LogInfo("Preloading Module_VentValve");
            }

            Load();
        }

        public void Load()
        {
            if (RefreshActiveVessel.IsFlightActive && _moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Loading Module_VentValve");
                ModuleReadyToLoad = true;

                _messageManager.SubscribeToMessages();
                _moduleController.IsModuleLoaded = true;
            }
        }
        public void InitializeModuleComponents()
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
