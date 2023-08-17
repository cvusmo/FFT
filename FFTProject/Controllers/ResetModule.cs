//|=====================Summary========================|0|
//| Resets the ConditionsManager to its default states |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Managers;
using FFT.Utilities;
using Newtonsoft.Json;
using System;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResetModule
    {
        public event Action ModuleResetRequested = delegate { };

        private static readonly Lazy<ResetModule> _lazyInstance = new Lazy<ResetModule>(() => new ResetModule());
        public static ResetModule Instance => _lazyInstance.Value;
        private ResetModule() { }
        public void Reset()
        {
            ConditionsManager.Instance.ResetStates();

            if (ConditionsManager.Instance.inFlightViewState && ConditionsManager.Instance.inPreLaunchState)
            {
                ModuleResetRequested.Invoke();
            }
        }
        public void Unload()
        {
            if (!RefreshVesselData.IsFlightActive())
            {
                Manager.Instance.Logger.LogInfo("Unloading Module");

                if (ModuleEnums.IsVentValve)
                {
                    Reset();
                    Manager.Instance.Logger.LogInfo("Reset Module_VentValve");
                }
            }
        }
    }
}

