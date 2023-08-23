//|=====================Summary========================|0|
//|     Validates messages meet specific conditions    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;

namespace FFT.Managers
{
    public class ConditionsManager
    {
        internal readonly ManualLogSource _logger;
        internal readonly MessageManager _messageManager;
        internal static ConditionsManager _instance;
        internal static readonly object _lock = new object();

        public delegate void ModuleConditionsMetDelegate();
        public event ModuleConditionsMetDelegate ModuleConditionsMet;

        public delegate void ModuleStartedDelegate();
        public event ModuleStartedDelegate OnModuleStarted;
        public static ConditionsManager Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new ConditionsManager(MessageManager.Instance, Logger.CreateLogSource("FFT.ConditionsManager"));
                }
            }
        }
        internal ConditionsManager(MessageManager messageManager, ManualLogSource logger)
        {
            _messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messageManager.GameStateEntered += GameStateEnteredHandler;
            _messageManager.GameStateLeft += GameStateLeftHandler;
            _messageManager.VesselSituationChanged += HandleVesselSituationChanged;
        }
        ~ConditionsManager()
        {
            _messageManager.GameStateEntered -= GameStateEnteredHandler;
            _messageManager.GameStateLeft -= GameStateLeftHandler;
            _messageManager.VesselSituationChanged -= HandleVesselSituationChanged;
        }
        internal void CheckConditionsAndInformManager()
        {
            if (ConditionsReady())
            {
                ModuleConditionsMet?.Invoke();
            }
        }
        public void VerifyAndStartModule()
        {
            _logger.LogDebug("Verifying loaded module...");
            Manager.Instance.UpdateStartModule();
        }
        public void OnModuleLoaded()
        {
            VerifyAndStartModule();
        }
        public void HandleModuleLoaded()
        {
            ResetModuleState();
        }
        public void ResetModuleState()
        {
            Manager.Instance.OnModuleReset();
        }
        internal void GameStateEnteredHandler(MessageCenterMessage obj)
        {
            try
            {
                if (obj is GameStateEnteredMessage gameStateMessage)
                {
                    Utility.GameState = gameStateMessage.StateBeingEntered;
                    _logger.LogDebug($"Entered New GameState: {Utility.GameStateToString(Utility.GameState)}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling GameStateEntered: {ex}");
            }
        }
        internal void GameStateLeftHandler(MessageCenterMessage obj)
        {
            try
            {
                if (obj is GameStateLeftMessage gameStateMessage)
                {
                    Utility.GameState = gameStateMessage.StateBeingLeft;
                    _logger.LogDebug($"Left Previous GameState: {Utility.GameStateToString(Utility.GameState)}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling GameStateLeft: {ex}");
            }
        }
        internal void HandleVesselSituationChanged(VesselSituationChangedMessage msg)
        {
            try
            {
                _logger.LogDebug($"Vessel situation changed from {Utility.SituationToString(msg.OldSituation)} to {Utility.SituationToString(msg.NewSituation)}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling VesselSituationChanged: {ex}");
            }
        }
        internal bool ConditionsReady()
        {
            try
            {
                _logger.LogDebug("Checking conditions...");

                bool conditionsMet =
                    Utility.VesselSituations == KSP.Sim.impl.VesselSituations.PreLaunch ||
                    Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Landed ||
                    Utility.VesselSituations == KSP.Sim.impl.VesselSituations.Flying ||
                    Utility.GameState == GameState.FlightView ||
                    Utility.GameState == GameState.VehicleAssemblyBuilder;

                _logger.LogDebug(conditionsMet
                    ? $"Conditions Ready! Vessel Situation: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}."
                    : $"Conditions not met: {Utility.SituationToString(Utility.VesselSituations)}, Game State: {Utility.GameStateToString(Utility.GameState)}.");

                if (conditionsMet)
                {
                    ModuleConditionsMet.Invoke();
                }

                return conditionsMet;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking conditions: {ex}");
                return false;
            }
        }
    }
}