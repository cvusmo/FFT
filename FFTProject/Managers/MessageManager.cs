﻿//|=====================Summary========================|0|
//|         listens for specific messages/events       |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Controllers.Interfaces;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;
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
                    return _instance ??= new MessageManager(_logger);
                }
            }
        }

        private ConditionsManager _conditionsManager;
        internal ConditionsManager conditionsmanager => _conditionsManager ??= ConditionsManager.Instance;

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
            SubscribeToMessages();
        }
        public void SubscribeToMessages()
        {
            _logger.LogDebug($"Subscribing To Messages...");
            Utility.RefreshGameManager();

            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(msg => conditionsmanager.GameStateEnteredHandler(msg));
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(msg => conditionsmanager.GameStateLeftHandler(msg));
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(msg => conditionsmanager.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
            _logger.LogDebug($"Subscribed to: {nameof(GameStateEnteredMessage)}, {nameof(GameStateLeftMessage)}, {nameof(VesselSituationChangedMessage)}");
        }
        internal void OnDestroy()
        {
            UnsubscribeFromMessages();
        }
        private void UnsubscribeFromMessages()
        {
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(conditionsmanager.GameStateEnteredHandler);
            Utility.MessageCenter.Unsubscribe<GameStateLeftMessage>(conditionsmanager.GameStateLeftHandler);
            Utility.MessageCenter.Unsubscribe<VesselSituationChangedMessage>(msg => conditionsmanager.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
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
        internal bool AreConditionsReady()
        {
            return conditionsmanager.ConditionsReady();
        }
        internal void HandleModuleReadyToLoad(ModuleType moduleType)
        {
            try
            {
                Utility.RefreshGameManager();

                if (StartListening(moduleType) && AreConditionsReady() && ModuleController.Instance.GetModuleState(ModuleType.ModuleVentValve))
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