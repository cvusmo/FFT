//|=====================Summary========================|0|
//|         listens for specific messages/events       |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;
using FFT.Controllers.Interfaces;
using System.Collections.Generic;
using static FFT.Controllers.ModuleController;

namespace FFT.Managers
{
    public class MessageManager : RefreshVesselData
    {
        internal static ManualLogSource _logger = Logger.CreateLogSource("FFT.MessageManager");

        internal event Action HandleGameStateActions = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleType> ModuleReadyToLoad = delegate { };

        private readonly Manager _manager;
        private readonly ConditionsManager _conditionsmanager;
        private readonly ModuleController _modulecontroller;
        private readonly ILoadModule _loadmodule;
        private readonly IStartModule _startmodule;
        private readonly IResetModule _resetmodule;

        public Dictionary<ModuleType, Action> ModuleListeners { get; private set; } = new Dictionary<ModuleType, Action>
        {
            { ModuleType.ModuleVentValve, () => { _logger.LogDebug("Listening to ModuleVentValve."); } }
        };

        public MessageManager(Manager manager, ConditionsManager conditionsManager, ModuleController moduleController, ILoadModule loadModule, IStartModule startModule, IResetModule resetModule)
        {
            _manager = manager;
            _conditionsmanager = conditionsManager;
            _loadmodule = loadModule;
            _startmodule = startModule;
            _resetmodule = resetModule;
        }
        public void SubscribeToMessages()
        {
            _logger.LogDebug($"Subscribing To Messages... ");
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(msg => _conditionsmanager.GameStateEnteredHandler((GameStateEnteredMessage)msg));
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(msg => _conditionsmanager.GameStateLeftHandler((GameStateLeftMessage)msg));
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(msg => _conditionsmanager.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
            _logger.LogDebug($"Subscribed to: {nameof(GameStateEnteredMessage)}, {nameof(GameStateLeftMessage)}, {nameof(VesselSituationChangedMessage)}");

            ModuleReadyToLoad += HandleModuleReadyToLoad;
            _logger.LogDebug("Subscribed to ModuleReadyToLoad event.");
        }
        internal void Update()
        {
            _logger.LogDebug("MessageManager.Update() triggered.");
            Utility.RefreshGameManager();

            if (Utility.GameState == GameState.FlightView)
            {
                Utility.RefreshActiveVesselAndCurrentManeuver();
                if (Utility.ActiveVessel == null)
                    return;
            }
        }
        internal bool StartListening(ModuleType moduleType)
        {
            if (ModuleListeners.TryGetValue(moduleType, out Action listenerAction))
            {
                listenerAction.Invoke();
                _modulecontroller.SetModuleState(moduleType, true);
                return true;
            }
            return false;
        }
        internal bool CheckAndUpdateManager()
        {
            _logger.LogDebug("CheckAndUpdateManager triggered.");
            if (_conditionsmanager.ConditionsReady())
            {
                _manager.Update();
                return true;
            }
            return false;
        }
        internal void HandleModuleReadyToLoad(ModuleType moduleType)
        {
            _logger.LogDebug("HandleModuleReadyToLoad triggered.");
            Utility.RefreshGameManager();

            bool isModuleTypeFound = StartListening(moduleType);

            if (!isModuleTypeFound) return;

            if (CheckAndUpdateManager())
            {
                if (_modulecontroller.GetModuleState(ModuleType.ModuleVentValve))
                {
                    ModuleReadyToLoad.Invoke(ModuleType.ModuleVentValve);
                    _modulecontroller.SetModuleState(ModuleType.ModuleVentValve, false);
                }
            }
        }
        internal void OnDestroy()
        {
            _logger.LogDebug("OnDestroy triggered.");
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(_conditionsmanager.GameStateEnteredHandler);
            ModuleReadyToLoad -= HandleModuleReadyToLoad;
        }
    }
}