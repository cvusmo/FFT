//|=====================Summary========================|0|
//|             dictionary for moduletypes             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

namespace FFT.Controllers
{
    public class ModuleEnums
    {
        internal static Dictionary<ModuleType, Action> moduleListeners = new Dictionary<ModuleType, Action>();

        public static bool IsVentValve { get; set; } = false;
        public static bool IsModuleOne { get; set; } = false;
        public static bool IsModuleTwo { get; set; } = false;
        public static bool IsModuleThree { get; set; } = false;
        public static bool IsModuleFour { get; set; } = false;
        public enum ModuleType
        {
            Default = 00,
            ModuleVentValve = 0,
            ModuleOne = 1,
            ModuleTwo = 2,
            ModuleThree = 3,
            ModuleFour = 4
        }
        public static ModuleType CurrentModule { get; set; }
        public static ModuleType ParseModuleName(string moduleName)
        {
            if (Enum.TryParse(moduleName, out ModuleType parsedModuleType))
            {
                CurrentModule = parsedModuleType;
                return parsedModuleType;
            }
            else
            {
                return default;
            }
        }
    }
}


