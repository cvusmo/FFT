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
        internal ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.MessageManager");
        internal event Action HandleGameStateActions = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleEnums.ModuleType> ModuleReadyToLoad = delegate { };

        internal ConditionsManager _conditionmanager = new ConditionsManager();
        internal ModuleEnums ModuleEnums { get; private set; }
        internal Manager manager => Manager.Instance;
        internal LoadModule loadmodule => LoadModule.Instance;
        public void SubscribeToMessages()
        {
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(msg => _conditionmanager.GameStateEnteredHandler((GameStateEnteredMessage)msg));
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(msg => _conditionmanager.GameStateLeftHandler((GameStateLeftMessage)msg));
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(msg => _conditionmanager.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
            ModuleReadyToLoad += HandleModuleReadyToLoad;
        }
        static MessageManager()
        {
            ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleVentValve] = () => { /* ModuleVentValve's listening logic here */ };
            ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleOne] = () => { /* ModuleOne's listening logic here */ };
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
            Utility.RefreshGameManager();

            if (GameStateConfig.GameState == GameState.FlightView)
            {
                Utility.RefreshActiveVesselAndCurrentManeuver();
                if (Utility.ActiveVessel == null)
                    return;
            }
        }
        private bool StartListening(ModuleEnums.ModuleType moduleType)
        {
            if (ModuleEnums.moduleListeners.TryGetValue(moduleType, out Action listenerAction))
            {
                listenerAction.Invoke();
                ModuleEnums.IsVentValve = true;
                return true;
            }
            return false;
        }
        private bool CheckAndUpdateManager()
        {
            if (_conditionmanager.ConditionsReady())
            {
                Manager.Instance.Update();
                return true;
            }
            return false;
        }
        internal void HandleModuleReadyToLoad(ModuleEnums.ModuleType moduleType)
        {
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
            Utility.MessageCenter.Unsubscribe<GameStateEnteredMessage>(_conditionmanager.GameStateEnteredHandler);
            ModuleReadyToLoad -= HandleModuleReadyToLoad;
        }
    }
}
