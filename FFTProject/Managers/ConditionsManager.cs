//|=====================Summary========================|0|
//|     Validates messages meet specific conditions    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;

namespace FFT.Managers
{
    public class ConditionsManager
    {
        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.MessageHandler");

        internal event Action<GameStateEnteredMessage> GameStateEntered = delegate { };
        internal event Action<GameStateLeftMessage> GameStateLeft = delegate { };
        internal event Action<ModuleEnums.ModuleType> ModuleReadyToLoad = delegate { };
        internal event Action<VesselSituationChangedMessage> VesselSituations = delegate { };
        internal bool inFlightViewState = false;
        internal bool inPreLaunchState = false;

        internal void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingEntered;
                _logger.LogDebug($"Entered GameState. New GameState: {Utility.GameState}.");
                GameStateEntered.Invoke(gameStateMessage);
            }
        }
        internal void GameStateLeftHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingLeft;
                _logger.LogDebug($"Left GameState. Previous GameState: {Utility.GameState}.");
                GameStateLeft.Invoke(gameStateMessage);
            }
        }
        internal void HandleVesselSituationChanged(VesselSituationChangedMessage msg)
        {
            _logger.LogDebug($"Vessel situation changed from {Utility.SituationToString(msg.OldSituation)} to {Utility.SituationToString(msg.NewSituation)}.");
            VesselSituations.Invoke(msg);
        }
        internal bool ConditionsReady()
        {
            inPreLaunchState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.PreLaunch;
            inFlightViewState = Utility.GameState == GameState.FlightView;
            return inPreLaunchState && inFlightViewState;
        }
    }
}

