//|=====================Summary========================|0|
//|             Dictionary for module types            |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

namespace FFT.Controllers
{
    public sealed class ModuleController
    {
        private readonly Dictionary<ModuleType, bool> moduleStates = new Dictionary<ModuleType, bool>();
        private static ModuleController _instance;
        private static readonly object _lock = new object();
        public enum ModuleType
        {
            Default = 0,
            ModuleVentValve = 1,
            ModuleOne = 2,
            ModuleTwo = 3,
            ModuleThree = 4,
            ModuleFour = 5
        }
        internal bool IsModuleLoaded { get; set; }
        internal bool ShouldResetModule { get; set; }
        internal static ModuleController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ModuleController();
                        }
                    }
                }
                return _instance;
            }
        }
        private ModuleController()
        {
            InitializeModuleStates();
        }
        private void InitializeModuleStates()
        {
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                moduleStates[moduleType] = false;
            }
        }
        internal void SetModuleState(ModuleType type, bool state)
        {
            if (!moduleStates.ContainsKey(type))
            {
                throw new ArgumentException($"Unsupported ModuleType: {type}");
            }

            moduleStates[type] = state;
        }
        internal bool GetModuleState(ModuleType type)
        {
            if (moduleStates.TryGetValue(type, out bool state))
            {
                return state;
            }
            throw new ArgumentException($"Unsupported ModuleType: {type}");
        }
        internal void SetVentValveState(bool state)
        {
            SetModuleState(ModuleType.ModuleVentValve, state);
        }
        internal void ResetAllModuleStates()
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