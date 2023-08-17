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
        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.ConditionsManager");

        internal event Action<GameStateEnteredMessage> GameStateEntered = delegate { };
        internal event Action<GameStateLeftMessage> GameStateLeft = delegate { };
        internal event Action<ModuleEnums.ModuleType> ModuleReadyToLoad = delegate { };
        internal event Action<VesselSituationChangedMessage> VesselSituations = delegate { };
        internal bool inFlightViewState, inOABState = false;
        internal bool isFlyingState, isLandedState, inPreLaunchState = false;

        internal Manager manager => Manager.Instance;
        internal MessageManager messagemanager => MessageManager.Instance;
        internal ModuleEnums moduleenums => ModuleEnums.Instance;
        internal LoadModule loadmodule => LoadModule.Instance;
        internal StartModule startmodule => StartModule.Instance;
        internal ResetModule resetmodule => ResetModule.Instance;


        private static ConditionsManager _instance;
        private ConditionsManager() { }
        public static ConditionsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConditionsManager();
                return _instance;
            }
        }
        internal void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingEntered;
                _logger.LogDebug($"Entered New GameState: {Utility.GameStateToString(Utility.GameState)}.");
                GameStateEntered.Invoke(gameStateMessage);
            }
        }
        internal void GameStateLeftHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingLeft;
                _logger.LogDebug($"Left Previous GameState: {Utility.GameStateToString(Utility.GameState)}.");
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
            _logger.LogDebug("Checking conditions...");
            inPreLaunchState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.PreLaunch;
            isLandedState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Landed;
            isFlyingState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Flying;
            inFlightViewState = Utility.GameState == GameState.FlightView;
            inOABState = Utility.GameState == GameState.VehicleAssemblyBuilder;
            _logger.LogDebug($"inPreLaunchState: {inPreLaunchState}");
            _logger.LogDebug($"isLandedState: {isLandedState}");
            _logger.LogDebug($"isFlyingState: {isFlyingState}");
            _logger.LogDebug($"inFlightViewState: {inFlightViewState}");
            _logger.LogDebug($"inOABState: {inOABState}");

            bool conditionsMet = (inPreLaunchState || isLandedState || isFlyingState || inFlightViewState || inOABState);

            if (conditionsMet)
            {
                _logger.LogDebug($"Conditions Ready! Vessel Situation: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}.");
            }
            else
            {
                _logger.LogDebug($"Conditions not met: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}.");
            }
            return conditionsMet;
        }
        public void ResetStates()
        {
            if (inPreLaunchState || isLandedState || isFlyingState)
            {
                inPreLaunchState = false;
                isLandedState = false;
                isFlyingState = false;
            }
            if (inFlightViewState || inOABState)
            {
                inFlightViewState = false;
                inOABState = false;
            }
        }
    }
}

