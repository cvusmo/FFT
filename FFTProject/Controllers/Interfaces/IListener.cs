using KSP.Messages;

namespace FFT.Controllers.Interfaces
{
    public interface IListener
    {
        public interface IGameStateListener
        {
            event Action<GameStateEnteredMessage> GameStateEntered;
            event Action<GameStateLeftMessage> GameStateLeft;
        }

        public interface IVesselSituationListener
        {
            event Action<VesselSituationChangedMessage> VesselSituationChanged;
        }
    }
}
