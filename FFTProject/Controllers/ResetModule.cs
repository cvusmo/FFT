//|=====================Summary========================|0|
//| Resets the ConditionsManager to its default states |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using FFT.Controllers.Interfaces;
using FFT.Managers;
using FFT.Utilities;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResetModule : IResetModule
    {
        public event Action ModuleResetRequested = delegate { };

        private readonly ConditionsManager _conditionsManager;
        private readonly Manager _manager;
        private readonly ModuleController _moduleController;
        private readonly RefreshVesselData _vesselData;
        private bool? _isFlightActiveCache;

        private static readonly object _lock = new object();
        private static ResetModule _instance;
        public ResetModule(
            ConditionsManager conditionsManager,
            Manager manager,
            ModuleController moduleController,
            RefreshVesselData vesselData)
        {
            _conditionsManager = conditionsManager;
            _manager = manager;
            _moduleController = moduleController;
            _vesselData = vesselData;
        }
        public static ResetModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ResetModule(ConditionsManager.Instance, Manager.Instance, ModuleController.Instance, new RefreshVesselData());
                        }
                    }
                }
                return _instance;
            }
        }
        public void Reset()
        {
            if (!_moduleController.ShouldResetModule) return;

            if (Utility.VesselSituations == KSP.Sim.impl.VesselSituations.PreLaunch ||
                   Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Landed ||
                   Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Flying)
            {
                ModuleResetRequested.Invoke();
            }

            _moduleController.ShouldResetModule = false;
            _moduleController.IsModuleLoaded = false;
        }
        public void Unload()
        {
            bool isFlightCurrentlyActive = _vesselData.RefreshActiveVesselInstance.IsFlightActive;

            if (_isFlightActiveCache.HasValue && !_isFlightActiveCache.Value ||
                !_isFlightActiveCache.HasValue && !isFlightCurrentlyActive)
            {
                _isFlightActiveCache = false;
                _manager.Logger.LogInfo("Unloading Module");

                if (_moduleController.GetModuleState(ModuleController.ModuleType.ModuleVentValve))
                {
                    Reset();
                    _manager.Logger.LogInfo("Reset Module_VentValve");
                }
            }
            else
            {
                _isFlightActiveCache = true;
            }
        }
    }
}