//|=====================Summary========================|0|
//|     Validates messages meet specific conditions    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;
using System;

namespace FFT.Managers
{
    public class ConditionsManager
    {
        private readonly ManualLogSource _logger = Logger.CreateLogSource("FFT.ConditionsManager");
        private readonly Manager _manager;
        private readonly MessageManager _messagemanager;
        private readonly ModuleController _modulecontroller;
        private readonly LoadModule _loadmodule;
        private readonly StartModule _startmodule;
        private readonly ResetModule _resetmodule;

        public event Action<GameStateEnteredMessage> GameStateEntered = delegate { };
        public event Action<GameStateLeftMessage> GameStateLeft = delegate { };
        public event Action<ModuleController.ModuleType> ModuleReadyToLoad = delegate { };
        public event Action<VesselSituationChangedMessage> VesselSituations = delegate { };

        public bool InFlightViewState { get; private set; }
        public bool InOABState { get; private set; }
        public bool IsFlyingState { get; private set; }
        public bool IsLandedState { get; private set; }
        public bool InPreLaunchState { get; private set; }
        public ConditionsManager(Manager manager, MessageManager messageManager, ModuleController moduleController, LoadModule loadModule, StartModule startModule, ResetModule resetModule)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _messagemanager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _modulecontroller = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _loadmodule = loadModule ?? throw new ArgumentNullException(nameof(loadModule));
            _startmodule = startModule ?? throw new ArgumentNullException(nameof(startModule));
            _resetmodule = resetModule ?? throw new ArgumentNullException(nameof(resetModule));
        }
        internal void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingEntered;
                _logger.LogDebug($"Entered New GameState: {Utility.GameStateToString(Utility.GameState)}.");
                GameStateEntered?.Invoke(gameStateMessage);
            }
        }
        internal void GameStateLeftHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingLeft;
                _logger.LogDebug($"Left Previous GameState: {Utility.GameStateToString(Utility.GameState)}.");
                GameStateLeft?.Invoke(gameStateMessage);
            }
        }
        internal void HandleVesselSituationChanged(VesselSituationChangedMessage msg)
        {
            _logger.LogDebug($"Vessel situation changed from {Utility.SituationToString(msg.OldSituation)} to {Utility.SituationToString(msg.NewSituation)}.");
            VesselSituations?.Invoke(msg);
        }
        internal bool ConditionsReady()
        {
            _logger.LogDebug("Checking conditions...");

            InPreLaunchState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.PreLaunch;
            IsLandedState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Landed;
            IsFlyingState = Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Flying;
            InFlightViewState = Utility.GameState == GameState.FlightView;
            InOABState = Utility.GameState == GameState.VehicleAssemblyBuilder;

            LogStates();

            bool conditionsMet = InPreLaunchState || IsLandedState || IsFlyingState || InFlightViewState || InOABState;

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
        private void LogStates()
        {
            _logger.LogDebug($"InPreLaunchState: {InPreLaunchState}");
            _logger.LogDebug($"IsLandedState: {IsLandedState}");
            _logger.LogDebug($"IsFlyingState: {IsFlyingState}");
            _logger.LogDebug($"InFlightViewState: {InFlightViewState}");
            _logger.LogDebug($"InOABState: {InOABState}");
        }
        public void ResetStates()
        {
            if (InPreLaunchState || IsLandedState || IsFlyingState)
            {
                InPreLaunchState = false;
                IsLandedState = false;
                IsFlyingState = false;
            }
            if (InFlightViewState || InOABState)
            {
                InFlightViewState = false;
                InOABState = false;
            }
        }
    }
}