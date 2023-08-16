//|=====================Summary========================|0|
//|          receives instructions & delegates         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Controllers;
using FFT.Modules;
using FFT.Utilities;
using KSP.Game;

namespace FFT.Managers
{
    public class Manager
    {
        private static Manager _instance;
        internal List<LoadModule> Modules;
        internal MessageManager MessageManager { get; private set; }
        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Manager");
        internal event Action<Module_VentValve> ModuleActivationRequested = delegate { };
        internal Manager()
        {
            Modules = new List<LoadModule>();
            MessageManager = new MessageManager();

            MessageManager.ModuleReadyToLoad += HandleModuleReadyToLoad;
        }
        internal static Manager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Manager();

                return _instance;
            }
        }
        public void Update()
        {
            Utility.RefreshGameManager();
        }
        internal void LoadModuleForFlight()
        {
            var loadModule = new LoadModule();
            Modules.Add(loadModule);
        }
        internal List<LoadModule> InitializeModules()
        {
            List<LoadModule> localModules = new List<LoadModule>();
            try
            {
                LoadModule loadModule = new LoadModule();
                localModules.Add(loadModule);

                if (loadModule is ResetModule resetModule)
                {
                    resetModule.ModuleReset += OnModuleReset;
                }

                return localModules;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating a LoadModule. Full exception: " + ex);
                return null;
            }
        }
        private void OnModuleReset()
        {
            _logger.LogInfo("Module_VentValve Reset");
        }
        private void HandleModuleReadyToLoad(ModuleEnums.ModuleType moduleType)
        {
            if (StartingModule())
            {
                StartModule startModuleInstance = new StartModule();
                startModuleInstance.StartVentValve();

                if (Utility.ActiveVessel == null)
                    return;
            }
        }
        private bool StartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
    }
}