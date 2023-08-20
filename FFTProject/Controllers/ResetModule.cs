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

        private static ResetModule _instance;
        private static readonly object _lock = new object();
        private bool? _isFlightActiveCache;
        private static readonly Lazy<ResetModule> _lazyInstance = new Lazy<ResetModule>(() =>
            new ResetModule(
                ConditionsManager.Instance,
                Manager.Instance,
                ModuleController.Instance,
                RefreshVesselData.Instance));
        public static ResetModule Instance => _lazyInstance.Value;

        private ResetModule(
            ConditionsManager conditionsManager,
            Manager manager,
            ModuleController moduleController,
            RefreshVesselData vesselData)
        {
            _conditionsManager = conditionsManager ?? throw new ArgumentNullException(nameof(conditionsManager));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _moduleController = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _vesselData = vesselData ?? throw new ArgumentNullException(nameof(vesselData));
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