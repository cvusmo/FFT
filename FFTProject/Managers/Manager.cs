//|=====================Summary========================|0|
//|                   Switchboard                      |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;

namespace FFT.Managers
{
    public class Manager
    {
        private List<LoadModule> Modules { get; } = new List<LoadModule>();
        internal ManualLogSource Logger { get; }
        private readonly StartModule _startmodule;
        private readonly MessageManager _messageManager;
        private readonly ModuleController _moduleController;
        private readonly LoadModule _loadModule;

        public Manager(
            StartModule startmodule,
            MessageManager messageManager,
            ModuleController moduleController,
            LoadModule loadModule)
        {
            _startmodule = startmodule ?? throw new ArgumentNullException(nameof(startmodule));
            _messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _loadModule = loadModule ?? throw new ArgumentNullException(nameof(loadModule));

            Logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Manager: ");
            _messageManager.ModuleReadyToLoad += HandleModuleReadyToLoad;
        }
        public void Update()
        {
            Utility.RefreshGameManager();
            _messageManager.Update();
            Logger.LogDebug("MessageManager.Update called");
        }
        internal void LoadModuleForFlight()
        {
            if (!Modules.Contains(_loadModule))
            {
                Modules.Add(_loadModule);
            }
        }
        internal List<LoadModule> InitializeModules()
        {
            return new List<LoadModule> { _loadModule };
        }
        internal void OnModuleReset()
        {
            _moduleController.ResetAllModuleStates();
            Logger.LogInfo("Module_VentValve Reset");
        }
        public void HandleModuleReadyToLoad(ModuleController.ModuleType moduleType)
        {
            if (IsStartingModule())
            {
                _startmodule.StartVentValve();
                if (Utility.ActiveVessel == null)
                {
                    return;
                }
            }
        }
        internal bool IsStartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
    }
}