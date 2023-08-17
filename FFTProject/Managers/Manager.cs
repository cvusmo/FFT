using BepInEx.Logging;
using FFT.Controllers;
using FFT.Modules;
using FFT.Utilities;
using KSP.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFT.Managers
{
    public class Manager
    {
        internal List<LoadModule> Modules { get; private set; }
        internal ManualLogSource Logger { get; private set; } = BepInEx.Logging.Logger.CreateLogSource("Manager: ");
        internal event Action<Module_VentValve> ModuleActivationRequested = delegate { };
        internal StartModule startmodule => StartModule.Instance;
        internal MessageManager MessageManager => MessageManager.Instance;

        private static readonly Lazy<Manager> _lazyInstance = new Lazy<Manager>(() => new Manager());
        public static Manager Instance => _lazyInstance.Value;
        internal Manager()
        {
            Modules = new List<LoadModule>();
            MessageManager.ModuleReadyToLoad += HandleModuleReadyToLoad;
        }
        public void Update()
        {
            Utility.RefreshGameManager();
            MessageManager.Update();
            Logger.LogDebug("MessageManager.Update called");
        }
        internal void LoadModuleForFlight()
        {
            if (!Modules.Contains(LoadModule.Instance))
            {
                Modules.Add(LoadModule.Instance);
            }
        }
        internal List<LoadModule> InitializeModules()
        {
            List<LoadModule> localModules = new List<LoadModule>();
            try
            {
                LoadModule loadModule = LoadModule.Instance;
                localModules.Add(loadModule);

                if (loadModule is ResetModule resetModule)
                {
                    resetModule.ModuleReset += OnModuleReset;
                }
                return localModules;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error creating a LoadModule. Full exception: " + ex);
                return null;
            }
        }
        internal void OnModuleReset()
        {
            Logger.LogInfo("Module_VentValve Reset");
        }
        public void HandleModuleReadyToLoad(ModuleEnums.ModuleType moduleType)
        {
            if (StartingModule())
            {
                startmodule.StartVentValve();
                if (Utility.ActiveVessel == null)
                {
                    return;
                }
            }
        }
        internal bool StartingModule()
        {
            bool isFlightActive = Modules.FirstOrDefault()?.RefreshActiveVessel?.IsFlightActive ?? false;
            return (Utility.GameState == GameState.FlightView && isFlightActive);
        }
    }
}
