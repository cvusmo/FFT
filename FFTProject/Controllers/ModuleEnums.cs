//|=====================Summary========================|0|
//|             dictionary for moduletypes             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

namespace FFT.Controllers
{
    public class ModuleEnums
    {
        internal static Dictionary<ModuleType, Action> moduleListeners = new Dictionary<ModuleType, Action>();
        internal bool IsVentValve = false;
        public enum ModuleType
        {
            ModuleVentValve = 0,
            ModuleOne = 1,
            ModuleTwo = 2,
            ModuleThree = 3,
            ModuleFour = 4
        }
        public static ModuleType CurrentModule { get; set; } = ModuleType.ModuleVentValve;
        public static ModuleType GetModuleTypeFromMessage(/* GameMessage or other argument */)
        {
            return CurrentModule;
        }
    }
}
