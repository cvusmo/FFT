namespace FFT.Utilities
{
    public class ModuleEnums
    {
        internal static Dictionary<ModuleEnums.ModuleType, Action> moduleListeners = new Dictionary<ModuleEnums.ModuleType, Action>();
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
            // Convert the game message into a ModuleType and return it.
            return CurrentModule; // return the default.
        }
    }
}
