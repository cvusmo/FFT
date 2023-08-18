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
        private readonly ManualLogSource _logger;
        private readonly Manager _manager;
        private readonly MessageManager _messagemanager;
        private readonly ModuleController _modulecontroller;
        private readonly LoadModule _loadmodule;
        private readonly StartModule _startmodule;
        private readonly ResetModule _resetmodule;

        public event Action<GameStateEnteredMessage> GameStateEntered = delegate { };
        public event Action<GameStateLeftMessage> GameStateLeft = delegate { };
        public event Action<VesselSituationChangedMessage> VesselSituationChanged = delegate { };
        public event Action<ModuleController.ModuleType> ModuleReadyToLoad = delegate { };

        public bool InFlightViewState { get; private set; }
        public bool InOABState { get; private set; }
        public bool IsFlyingState { get; private set; }
        public bool IsLandedState { get; private set; }
        public bool InPreLaunchState { get; private set; }
        public ConditionsManager(
            Manager manager,
            MessageManager messageManager,
            ModuleController moduleController,
            LoadModule loadModule,
            StartModule startModule,
            ResetModule resetModule)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _messagemanager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _modulecontroller = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _loadmodule = loadModule ?? throw new ArgumentNullException(nameof(loadModule));
            _startmodule = startModule ?? throw new ArgumentNullException(nameof(startModule));
            _resetmodule = resetModule ?? throw new ArgumentNullException(nameof(resetModule));

            _logger = Logger.CreateLogSource("FFT.ConditionsManager");
            _messagemanager.GameStateEntered += GameStateEnteredHandler;
            _messagemanager.GameStateLeft += GameStateLeftHandler;
            _messagemanager.VesselSituationChanged += HandleVesselSituationChanged;
        }
        internal void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingEntered;
                LogGameStateChange($"Entered New GameState: {Utility.GameStateToString(Utility.GameState)}.");
                GameStateEntered?.Invoke(gameStateMessage);
            }
        }
        internal void GameStateLeftHandler(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                Utility.GameState = gameStateMessage.StateBeingLeft;
                LogGameStateChange($"Left Previous GameState: {Utility.GameStateToString(Utility.GameState)}.");
                GameStateLeft?.Invoke(gameStateMessage);
            }
        }
        internal void HandleVesselSituationChanged(VesselSituationChangedMessage msg)
        {
            LogVesselSituationChange($"Vessel situation changed from {Utility.SituationToString(msg.OldSituation)} to {Utility.SituationToString(msg.NewSituation)}.");
            VesselSituationChanged?.Invoke(msg);
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
                LogConditions($"Conditions Ready! Vessel Situation: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}.");
            }
            else
            {
                LogConditions($"Conditions not met: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}.");
            }
            return conditionsMet;
        }
        private void LogStates()
        {
            LogConditionState($"InPreLaunchState: {InPreLaunchState}");
            LogConditionState($"IsLandedState: {IsLandedState}");
            LogConditionState($"IsFlyingState: {IsFlyingState}");
            LogConditionState($"InFlightViewState: {InFlightViewState}");
            LogConditionState($"InOABState: {InOABState}");
        }
        private void LogGameStateChange(string message)
        {
            _logger.LogDebug(message);
        }
        private void LogVesselSituationChange(string message)
        {
            _logger.LogDebug(message);
        }
        private void LogConditions(string message)
        {
            _logger.LogDebug(message);
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
        private void LogConditionState(string message)
        {
            _logger.LogDebug(message);
        }
    }
}
