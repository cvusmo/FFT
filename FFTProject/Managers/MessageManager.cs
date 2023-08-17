//|=====================Summary========================|0|
//|         listens for specific messages/events       |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Controllers;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;

namespace FFT.Managers
{
    public class MessageManager : RefreshVesselData
    {
        internal ManualLogSource _logger = Logger.CreateLogSource("FFT.MessageManager");
        internal static ManualLogSource _logger2 = Logger.CreateLogSource("FFT.Listener");
        internal event Action HandleGameStateActions = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleEnums.ModuleType> ModuleReadyToLoad = delegate { };

        internal Manager manager => Manager.Instance;
        internal ConditionsManager conditionsmanager => ConditionsManager.Instance;
        internal ModuleEnums moduleenums => ModuleEnums.Instance;
        internal LoadModule loadmodule => LoadModule.Instance;
        internal StartModule startmodule => StartModule.Instance;
        internal ResetModule resetmodule => ResetModule.Instance;


        private static readonly Lazy<MessageManager> _lazyInstance = new Lazy<MessageManager>(() => new MessageManager());
        public static MessageManager Instance => _lazyInstance.Value;
        private MessageManager() { }
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
        static MessageManager()
        {
            ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleVentValve] = () => { _logger2.LogDebug("Listening to ModuleVentValve."); };
            //ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleOne] = () => { /* ModuleOne's listening logic here */ };
        }
        internal ModuleEnums.ModuleType GetCurrentModule(ModuleEnums.ModuleType moduleType)
        {
            return ModuleEnums.CurrentModule;
        }
        internal void SetCurrentModule(ModuleEnums.ModuleType moduleType)
        {
            ModuleEnums.CurrentModule = moduleType;
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
        internal bool StartListening(ModuleEnums.ModuleType moduleType)
        {
            if (ModuleEnums.moduleListeners.TryGetValue(moduleType, out Action listenerAction))
            {
                listenerAction.Invoke();
                ModuleEnums.IsVentValve = true;
                return true;
            }
            return false;
        }
        internal bool CheckAndUpdateManager()
        {
            _logger.LogDebug("CheckAndUpdateManager triggered.");
            if (conditionsmanager.ConditionsReady())
            {
                Manager.Instance.Update();
                return true;
            }
            return false;
        }
        internal void HandleModuleReadyToLoad(ModuleEnums.ModuleType moduleType)
        {
            _logger.LogDebug("HandleModuleReadyToLoad triggered.");
            Utility.RefreshGameManager();

            var currentModuleType = GetCurrentModule(moduleType);
            bool isModuleTypeFound = StartListening(currentModuleType);

            if (!isModuleTypeFound) return;

            if (CheckAndUpdateManager())
            {
                if (ModuleEnums.IsVentValve)
                {
                    ModuleReadyToLoad.Invoke(0);
                    ModuleEnums.IsVentValve = false;
                }
            }
        }
        internal void OnDestroy()
        {
            _logger.LogDebug("OnDestroy triggered.");
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(conditionsmanager.GameStateEnteredHandler);
            ModuleReadyToLoad -= HandleModuleReadyToLoad;
        }
    }
}
