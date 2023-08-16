//|=====================Summary========================|0|
//|  manages modules, module states, & initialization  |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT.Modules;
using FFT.Utilities;
using KSP.Game;

namespace FFT.Managers
{
    public class Manager
    {
        private static Manager _instance;
        internal List<LoadModule> Modules;
        internal event Action<Module_VentValve> ModuleActivationRequested = delegate { };

        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Manager");
        internal Manager()
        {
            Modules = InitializeModules();
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
            MessageManager.StartListening(ModuleEnums);

            bool isFlightActive = Modules.OfType<LoadModule>().FirstOrDefault().RefreshActiveVessel.IsFlightActive;

            if (Utility.GameState == GameState.FlightView && isFlightActive)
            {
                Utility.RefreshActiveVesselAndCurrentManeuver();
                StartModule.Instance.ActivateModule(ModuleEnums.ModuleType);
                if (Utility.ActiveVessel == null)
                    return;
            }
        }
        internal void LoadModuleForFlight()
        {
            var loadModule = new LoadModule(this);
            Modules.Add(loadModule);
        }
        internal List<LoadModule> InitializeModules()
        {
            try
            {
                LoadModule loadModule = new LoadModule(this)
                {
                    ModuleList = Utility.ModuleList,
                    IsFlightActive = false,
                };

                Modules.Add(loadModule);
                if (loadModule is ResetModule resetModule)
                {
                    resetModule.ModuleReset += OnModuleReset;
                }

                return Modules;
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
    }
}
