//|=====================Summary========================|0|
//| Resets the ConditionsManager to its default states |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx;
using FFT.Managers;
using Newtonsoft.Json;

namespace FFT.Controllers
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResetModule : LoadModule
    {
        internal ConditionsManager ConditionsManager { get; private set; }

        public event Action ModuleReset = delegate { };
        public ResetModule()
        {
            ConditionsManager = ConditionsManager;
        }
        public void Reset()
        {
            ConditionsManager.ResetStates();

            if (ConditionsManager.inFlightViewState && ConditionsManager.inPreLaunchState)
            {
                ModuleReset.Invoke();
            }
        }
        public void Unload()
        {
            if (!RefreshActiveVessel.IsFlightActive)
            {
                Manager.Instance._logger.LogInfo("Unloading Module");
                if (ModuleEnums.IsVentValve)
                {
                    Reset();
                    Manager.Instance._logger.LogInfo("Reset Module_VentValve");
                }
            }
        }
    }
}
