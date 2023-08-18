//|=====================Summary========================|0|
//|             Dictionary for module types            |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Controllers.Interfaces;
using System;
using System.Collections.Generic;

namespace FFT.Controllers
{
    public class ModuleController
    {
        private Dictionary<ModuleType, bool> moduleStates = new Dictionary<ModuleType, bool>();
        private IModuleController moduleController;
        public enum ModuleType
        {
            Default = 00,
            ModuleVentValve = 0,
            ModuleOne = 1,
            ModuleTwo = 2,
            ModuleThree = 3,
            ModuleFour = 4
        }
        public bool IsModuleLoaded { get; set; }
        public bool ShouldResetModule { get; set; }
        public ModuleController(IModuleController moduleController)
        {
            this.moduleController = moduleController;
            InitializeModuleStates();
        }
        private void InitializeModuleStates()
        {
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                moduleStates[moduleType] = false;
            }
        }
        public void SetModuleState(ModuleType type, bool state)
        {
            if (!moduleStates.ContainsKey(type))
            {
                throw new ArgumentException($"Unsupported ModuleType: {type}");
            }

            moduleStates[type] = state;
        }
        public bool GetModuleState(ModuleType type)
        {
            if (moduleStates.TryGetValue(type, out bool state))
            {
                return state;
            }
            throw new ArgumentException($"Unsupported ModuleType: {type}");
        }
        public void SetVentValveState(bool state)
        {
            SetModuleState(ModuleType.ModuleVentValve, state);
        }
        public void ResetAllModuleStates()
        {
            foreach (var key in moduleStates.Keys.ToList())
            {
                moduleStates[key] = false;
            }

            IsModuleLoaded = false;
            ShouldResetModule = false;
        } 
    }
}