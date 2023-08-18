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
using System;
using System.Collections.Generic;
using static FFT.Controllers.ModuleController;

namespace FFT.Managers
{
    public class MessageManager : RefreshVesselData, IListener.IGameStateListener, IListener.IVesselSituationListener
    {
        private static readonly ManualLogSource _logger = Logger.CreateLogSource("FFT.MessageManager");

        internal event Action HandleGameStateActions = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleType> ModuleReadyToLoad = delegate { };
        public event Action<VesselSituationChangedMessage> VesselSituationChanged;
        public event Action<GameStateEnteredMessage> GameStateEntered;
        public event Action<GameStateLeftMessage> GameStateLeft;

        private readonly Manager _manager;
        private readonly ConditionsManager _conditionsmanager;
        private readonly ModuleController _modulecontroller;
        private readonly ILoadModule _loadmodule;
        private readonly IStartModule _startmodule;
        private readonly IResetModule _resetmodule;
        private readonly IListener.IGameStateListener _gameStateListener;
        private readonly IListener.IVesselSituationListener _vesselSituationListener;

        private Dictionary<ModuleType, Action> ModuleListeners { get; } = new Dictionary<ModuleType, Action>
        {
            { ModuleType.ModuleVentValve, () => _logger.LogDebug("Listening to ModuleVentValve.") }
        };
        public MessageManager(
            Manager manager,
            ConditionsManager conditionsManager,
            ModuleController moduleController,
            ILoadModule loadModule,
            IStartModule startModule,
            IResetModule resetModule,
            IListener.IGameStateListener gameStateListener,
            IListener.IVesselSituationListener vesselSituationListener)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _conditionsmanager = conditionsManager ?? throw new ArgumentNullException(nameof(conditionsManager));
            _modulecontroller = moduleController ?? throw new ArgumentNullException(nameof(moduleController));
            _loadmodule = loadModule ?? throw new ArgumentNullException(nameof(loadModule));
            _startmodule = startModule ?? throw new ArgumentNullException(nameof(startModule));
            _resetmodule = resetModule ?? throw new ArgumentNullException(nameof(resetModule));
            _gameStateListener = gameStateListener ?? throw new ArgumentNullException(nameof(gameStateListener));
            _vesselSituationListener = vesselSituationListener ?? throw new ArgumentNullException(nameof(vesselSituationListener));
        }

        public void SubscribeToMessages()
        {
            _logger.LogDebug($"Subscribing To Messages... ");
            SubscribeToGameStateMessages();
            ModuleReadyToLoad += HandleModuleReadyToLoad;
            _logger.LogDebug("Subscribed to ModuleReadyToLoad event.");
        }
        internal void Update()
        {
            _logger.LogDebug("MessageManager.Update() triggered.");
            Utility.RefreshGameManager();

            if (Utility.GameState == GameState.FlightView && Utility.ActiveVessel != null)
            {
                Utility.RefreshActiveVesselAndCurrentManeuver();
            }
        }
        internal void OnDestroy()
        {
            _logger.LogDebug("OnDestroy triggered.");
            UnsubscribeFromGameStateMessages();
            ModuleReadyToLoad -= HandleModuleReadyToLoad;
        }
        private void SubscribeToGameStateMessages()
        {
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(HandleGameStateEnteredMessage);
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(HandleGameStateLeftMessage);
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(HandleVesselSituationChangedMessage);
            _logger.LogDebug($"Subscribed to: {nameof(GameStateEnteredMessage)}, {nameof(GameStateLeftMessage)}, {nameof(VesselSituationChangedMessage)}");
        }
        private void UnsubscribeFromGameStateMessages()
        {
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(HandleGameStateEnteredMessage);
            Utility.MessageCenter.Unsubscribe<GameStateLeftMessage>(HandleGameStateLeftMessage);
            Utility.MessageCenter.Unsubscribe<VesselSituationChangedMessage>(HandleVesselSituationChangedMessage);
            _logger.LogDebug($"Unsubscribed from: {nameof(GameStateEnteredMessage)}, {nameof(GameStateLeftMessage)}, {nameof(VesselSituationChangedMessage)}");
        }
        private void HandleGameStateEnteredMessage(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                _conditionsmanager.GameStateEnteredHandler(gameStateMessage);
                HandleGameStateActions?.Invoke();
            }
        }
        private void HandleGameStateLeftMessage(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                _conditionsmanager.GameStateLeftHandler(gameStateMessage);
                HandleGameStateActions?.Invoke();
            }
        }
        private void HandleVesselSituationChangedMessage(MessageCenterMessage obj)
        {
            if (obj is VesselSituationChangedMessage situationMessage)
            {
                _conditionsmanager.HandleVesselSituationChanged(situationMessage);
                VesselStateChangedEvent?.Invoke();
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

            if (CheckAndUpdateManager() && _modulecontroller.GetModuleState(ModuleType.ModuleVentValve))
            {
                ModuleReadyToLoad.Invoke(ModuleType.ModuleVentValve);
                _modulecontroller.SetModuleState(ModuleType.ModuleVentValve, false);
            }
        }
    }
}
