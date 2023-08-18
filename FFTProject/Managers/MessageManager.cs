//|=====================Summary========================|0|
//|         listens for specific messages/events       |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;
using System;
using System.Collections.Generic;
using static FFT.Controllers.ModuleController;

namespace FFT.Managers
{
    internal class MessageManager : RefreshVesselData, IListener.IGameStateListener, IListener.IVesselSituationListener
    {
        private static readonly ManualLogSource _logger = Logger.CreateLogSource("FFT.MessageManager");
        private static readonly object _lock = new object();

        private static MessageManager _instance;
        public static MessageManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MessageManager(_logger);
                    }
                    return _instance;
                }
            }
        }
        internal ConditionsManager conditionsmanager { get; private set; }

        internal event Action HandleGameStateActions = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleType> ModuleReadyToLoad = delegate { };
        public event Action<GameStateEnteredMessage> GameStateEntered;
        public event Action<GameStateLeftMessage> GameStateLeft;
        public event Action<VesselSituationChangedMessage> VesselSituationChanged;
        
        private readonly Dictionary<ModuleType, Action> ModuleListeners;
        private MessageManager(ManualLogSource logger)
        {
            ModuleListeners = new Dictionary<ModuleType, Action>
            {
                { ModuleType.ModuleVentValve, () => _logger.LogDebug("Listening to ModuleVentValve.") }
            };

            conditionsmanager = ConditionsManager.Instance;
            ModuleReadyToLoad = delegate { };
            SubscribeToMessages();
        }
        public void SubscribeToMessages()
        {
            _logger.LogDebug($"Subscribing To Messages... ");
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(msg => conditionsmanager.GameStateEnteredHandler((GameStateEnteredMessage)msg));
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(msg => conditionsmanager.GameStateLeftHandler((GameStateLeftMessage)msg));
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(msg => conditionsmanager.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
            _logger.LogDebug($"Subscribed to: {nameof(GameStateEnteredMessage)}, {nameof(GameStateLeftMessage)}, {nameof(VesselSituationChangedMessage)}");

            ModuleReadyToLoad += HandleModuleReadyToLoad;
            _logger.LogDebug("Subscribed to ModuleReadyToLoad event.");
        }
        internal void Update()
        {
            try
            {
                Utility.RefreshGameManager();

                if (Utility.GameState == GameState.FlightView && Utility.ActiveVessel != null)
                {
                    Utility.RefreshActiveVesselAndCurrentManeuver();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Update: {ex}");
            }
        }
        internal void OnDestroy()
        {
            UnsubscribeFromGameStateMessages();
            ModuleReadyToLoad -= HandleModuleReadyToLoad;
        }
        private void SubscribeToGameStateMessages()
        {
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(HandleGameStateEnteredMessage);
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(HandleGameStateLeftMessage);
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(HandleVesselSituationChangedMessage);
        }
        private void UnsubscribeFromGameStateMessages()
        {
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(HandleGameStateEnteredMessage);
            Utility.MessageCenter.Unsubscribe<GameStateLeftMessage>(HandleGameStateLeftMessage);
            Utility.MessageCenter.Unsubscribe<VesselSituationChangedMessage>(HandleVesselSituationChangedMessage);
        }
        private void HandleGameStateEnteredMessage(MessageCenterMessage obj)
        {
            try
            {
                if (obj is GameStateEnteredMessage gameStateMessage)
                {
                    ConditionsManager.Instance.GameStateEnteredHandler(gameStateMessage);
                    HandleGameStateActions?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling GameStateEntered: {ex}");
            }
        }
        private void HandleGameStateLeftMessage(MessageCenterMessage obj)
        {
            try
            {
                if (obj is GameStateLeftMessage gameStateMessage)
                {
                    ConditionsManager.Instance.GameStateLeftHandler(gameStateMessage);
                    HandleGameStateActions?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling GameStateLeft: {ex}");
            }
        }
        private void HandleVesselSituationChangedMessage(MessageCenterMessage obj)
        {
            try
            {
                if (obj is VesselSituationChangedMessage situationMessage)
                {
                    ConditionsManager.Instance.HandleVesselSituationChanged(situationMessage);
                    VesselStateChangedEvent?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling VesselSituationChanged: {ex}");
            }
        }
        internal bool StartListening(ModuleType moduleType)
        {
            if (ModuleListeners.TryGetValue(moduleType, out Action listenerAction))
            {
                listenerAction.Invoke();
                ModuleController.Instance.SetModuleState(moduleType, true);
                return true;
            }
            return false;
        }
        internal bool CheckAndUpdateManager()
        {
            if (ConditionsManager.Instance.ConditionsReady())
            {
                Manager.Instance.Update();
                return true;
            }
            return false;
        }
        internal void HandleModuleReadyToLoad(ModuleType moduleType)
        {
            try
            {
                Utility.RefreshGameManager();

                if (StartListening(moduleType) && CheckAndUpdateManager() && ModuleController.Instance.GetModuleState(ModuleType.ModuleVentValve))
                {
                    ModuleReadyToLoad.Invoke(ModuleType.ModuleVentValve);
                    ModuleController.Instance.SetModuleState(ModuleType.ModuleVentValve, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling ModuleReadyToLoad: {ex}");
            }
        }
    }
}
