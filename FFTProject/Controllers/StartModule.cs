//|=====================Summary========================|0|
//|         Initiates the module start process         |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Modules;
using FFT.Utilities;

namespace FFT.Controllers
{
    public class StartModule
    {
        internal StartModule Instance { get; set; }
        internal Module_VentValve ModuleVentValve { get; set; }
        internal void StartVentValve()
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();
            ActivateModule(ModuleVentValve);

            if (Utility.ActiveVessel == null)
                return;
        }
        internal void ActivateModule(Module_VentValve module)
        {
            if (module != null)
            {
                module.Activate();
            }
        }
        internal void DeactivateModule(Module_VentValve module)
        {
            if (module != null)
            {
                module.Deactivate();
            }
        }
    }
}
