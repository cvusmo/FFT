//|=====================Summary========================|0|
//| Resets the ConditionsManager to its default states |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
using Newtonsoft.Json;
using System;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResetModule : IResetModule
    {
        public event Action ModuleResetRequested = delegate { };

        private readonly ConditionsManager _conditionsmanager;
        private readonly Manager _manager;
        private readonly ModuleController _modulecontroller;

        public ResetModule(
            ConditionsManager conditionsManager,
            Manager manager,
            ModuleController moduleController)
        {
            _conditionsmanager = conditionsManager ?? throw new ArgumentNullException(nameof(conditionsManager));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _modulecontroller = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
        }

        public void Reset()
        {
            if (!_modulecontroller.ShouldResetModule) return;

            _conditionsmanager.ResetStates();

            if (_conditionsmanager.InFlightViewState && _conditionsmanager.InPreLaunchState)
            {
                ModuleResetRequested.Invoke();
            }

            _modulecontroller.ShouldResetModule = false;
            _modulecontroller.IsModuleLoaded = false;
        }

        public void Unload()
        {
            if (!RefreshVesselData.IsFlightActive())
            {
                _manager.Logger.LogInfo("Unloading Module");

                if (_modulecontroller.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
                {
                    Reset();
                    _manager.Logger.LogInfo("Reset Module_VentValve");
                }
            }
        }
    }
}