//|=====================Summary========================|0|
//|             dictionary for moduletypes             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Managers;
using System;
using System.Collections.Generic;

namespace FFT.Controllers
{
    public class ModuleEnums
    {
        internal static Dictionary<ModuleType, Action> moduleListeners = new Dictionary<ModuleType, Action>();

        internal Manager manager => Manager.Instance;
        internal MessageManager messagemanager => MessageManager.Instance;
        internal ConditionsManager conditionsmanager => ConditionsManager.Instance;
        internal LoadModule loadmodule => LoadModule.Instance;
        internal StartModule startmodule => StartModule.Instance;
        internal ResetModule resetmodule => ResetModule.Instance;

        private static readonly Lazy<ModuleEnums> _lazyInstance = new Lazy<ModuleEnums>(() => new ModuleEnums());
        public static ModuleEnums Instance => _lazyInstance.Value;
        private ModuleEnums() { }
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

        public static bool IsVentValve => CurrentModule == ModuleType.ModuleVentValve;
        public static bool IsModuleOne => CurrentModule == ModuleType.ModuleOne;
        public static bool IsModuleTwo => CurrentModule == ModuleType.ModuleTwo;
        public static bool IsModuleThree => CurrentModule == ModuleType.ModuleThree;
        public static bool IsModuleFour => CurrentModule == ModuleType.ModuleFour;
        public static ModuleType ParseModuleName(string moduleName)
        {
            if (Enum.TryParse<ModuleType>(moduleName, out ModuleType parsedModuleType))
            {
                CurrentModule = parsedModuleType;
                return parsedModuleType;
            }
            else
            {
                return ModuleType.Default;
            }
        }
    }
}



