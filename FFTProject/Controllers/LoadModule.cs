//|=====================Summary========================|0|
//|     Ensures that the correct module gets loaded    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadModule : ILoadModule
    {
        [JsonProperty]
        public bool EnableVFX { get; private set; } = true;

        private static readonly object _lock = new object();
        private static LoadModule _instance;
        public static LoadModule Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        Initialize();
                    }
                    return _instance;
                }
            }
        }

        private readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("LoadModule: ");

        internal RefreshVesselData.RefreshActiveVessel RefreshActiveVessel => RefreshVesselData.Instance.RefreshActiveVesselInstance;

        private readonly Manager _manager;
        private readonly MessageManager _messageManager;
        private readonly ConditionsManager _conditionsManager;
        private readonly ModuleController _moduleController;
        private readonly StartModule _startModule;
        private readonly ResetModule _resetModule;

        public event Action ModuleResetRequested;

        private LoadModule(
            Manager manager,
            MessageManager messageManager,
            ConditionsManager conditionsManager,
            ModuleController moduleController,
            StartModule startModule,
            ResetModule resetModule)
        {
            _manager = manager;
            _messageManager = messageManager;
            _conditionsManager = conditionsManager;
            _moduleController = moduleController;
            _startModule = startModule;
            _resetModule = resetModule;
        }

        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new LoadModule(
                    Manager.Instance,
                    MessageManager.Instance,
                    ConditionsManager.Instance,
                    ModuleController.Instance,
                    StartModule.Instance,
                    ResetModule.Instance);
            }
        }

        public void Boot()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && EnableVFX)
            {
                _logger.LogInfo("Booting Module_VentValve");
                _moduleController.SetModuleState(ModuleController.ModuleType.ModuleVentValve, true);
                PreLoad();
            }
        }
        public void PreLoad()
        {
            Utility.RefreshGameManager();
            if (_moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Preloading Module_VentValve");
                Module_VentValve.Instance.InitializeData();
                Module_VentValve.Instance.InitializeVFX();
                Load();
            }
        }

        public void Load()
        {
            Utility.RefreshGameManager();
            if (RefreshActiveVessel.IsFlightActive && _moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
            {
                _logger.LogInfo("Loading Module_VentValve");
                _messageManager.SubscribeToMessages();
                _moduleController.IsModuleLoaded = true;
            }
        }
    }
}
