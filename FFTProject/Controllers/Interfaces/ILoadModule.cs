namespace FFT.Controllers.Interfaces
{
    public interface ILoadModule
    {
        void Boot();
        void PreLoad();
        void Load();
    }
}