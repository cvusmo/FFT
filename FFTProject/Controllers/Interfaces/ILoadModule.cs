namespace FFT.Controllers.Interfaces
{
    public interface ILoadModule
    {
        void Boot();
        void PreLoad();
        void Load();
        void InitializeModuleComponents();
    }
    public interface IResetModule
    {
        void Reset();
        void Unload();
    }
    public interface IStartModule
    {
        void StartVentValve();
        void ActivateModule(ModuleController.ModuleType moduleType);
        void DeactivateModule(ModuleController.ModuleType moduleType);
    }

}