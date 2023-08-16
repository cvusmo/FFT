//|=====================Summary========================|0|
//|     manage messages, state changes, & delegates    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using BepInEx.Logging;
using FFT.Utilities;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;

namespace FFT.Managers
{
    public class MessageManager : RefreshVesselData
    {
        private ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.MessageManager");     
        internal event Action GameStateEnteredEvent = delegate { };
        internal event Action VesselStateChangedEvent = delegate { };
        internal event Action<ModuleEnums.ModuleType> ModuleReadyToLoad = delegate { };
        internal ModuleEnums ModuleEnums { get; private set; }
        internal Manager manager => Manager.Instance;
        private static MessageManager _instanceMM;
        internal LoadModule loadmodule => LoadModule.Instance;
        public static MessageManager InstanceMM
        {
            get
            {
                if (_instanceMM == null)
                {
                    _instanceMM = new MessageManager();
                    _instanceMM.GameStateEnteredEvent += _instanceMM.HandleGameStateEntered;
                }
                return _instanceMM;
            }
        }
        static MessageManager()
        {
            ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleVentValve] = () => { /* ModuleVentValve's listening logic here */ };
            ModuleEnums.moduleListeners[ModuleEnums.ModuleType.ModuleOne] = () => { /* ModuleOne's listening logic here */ };
        }
        public void SubscribeToMessages()
        {
            Utility.RefreshGameManager();
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(msg => this.GameStateEntered((GameStateEnteredMessage)msg));
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(msg => this.GameStateLeft((GameStateLeftMessage)msg));
            Utility.MessageCenter.Subscribe<VesselSituationChangedMessage>(msg => this.HandleVesselSituationChanged((VesselSituationChangedMessage)msg));
            ModuleReadyToLoad += HandleModuleReadyToLoad;
            //ModuleReadyToLoad += HandleVesselSituationChanged;
        }
        internal ModuleEnums.ModuleType GetCurrentModule(ModuleEnums.ModuleType moduleType)
        {
            return FFT.Utilities.ModuleEnums.CurrentModule;
        }
        internal void SetCurrentModule(ModuleEnums.ModuleType moduleType)
        {
            FFT.Utilities.ModuleEnums.CurrentModule = moduleType;
        }
        internal void VesselStateChange(MessageCenterMessage moduletype)
        {
            if (moduletype is VesselSituationChangedMessage vesselSituationChanged)
            {
                Utility.vesselSituations = vesselSituationChanged.NewSituation;
                _logger.LogDebug($"Vessel situation changed from {vesselSituationChanged.OldSituation} to {vesselSituationChanged.NewSituation}.");

                VesselStateChangedEvent.Invoke();
            }
        }
        internal void GameStateEntered(MessageCenterMessage obj)
        {
            if (obj is GameStateEnteredMessage gameStateMessage)
            {
                GameStateConfig.GameState = gameStateMessage.StateBeingEntered;
                _logger.LogDebug($"Entered GameStateEntered. New GameState: {GameStateConfig.GameState}.");

                if (GameStateConfig.GameState == GameState.FlightView)
                {
                    // Handle operations specific to the FlightView state
                }
                else if (GameStateConfig.GameState == GameState.VehicleAssemblyBuilder)
                {
                    // Handle operations specific to the VehicleAssemblyBuilder state
                }

                GameStateEnteredEvent.Invoke();
            }
        }  
        internal void GameStateLeft(MessageCenterMessage obj)
        {
            if (obj is GameStateLeftMessage gameStateMessage)
            {
                GameStateConfig.GameState = gameStateMessage.StateBeingLeft;
                _logger.LogDebug($"Entered GameStateEntered. New GameState: {GameStateConfig.GameState}.");

                if (GameStateConfig.GameState == GameState.FlightView)
                {
                    // Handle operations specific to the FlightView state
                }
                else if (GameStateConfig.GameState == GameState.VehicleAssemblyBuilder)
                {
                    // Handle operations specific to the VehicleAssemblyBuilder state
                }

                GameStateEnteredEvent.Invoke();
            }
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
        internal void RequestLoadModuleForFlight()
        {
            manager.LoadModuleForFlight();
        }
        internal void HandleVesselSituationChanged(VesselSituationChangedMessage msg)
        {
            _logger.LogDebug($"Vessel situation changed from {msg.OldSituation} to {msg.NewSituation}.");

            switch (msg.NewSituation)
            {
                case VesselSituations.Landed:
                    break;
                case VesselSituations.PreLaunch:
                    break;
                case VesselSituations.SubOrbital:
                    break;
                case VesselSituations.Orbiting:
                    break;

                default:
                    _logger.LogDebug($"Unhandled VesselSituation: {msg.NewSituation}.");
                    break;
            }
            if (msg.NewSituation == VesselSituations.Landed)
            {
                VesselStateChangedEvent.Invoke();
            }
            if (msg.NewSituation == VesselSituations.PreLaunch)
            {
                VesselStateChangedEvent.Invoke();
            }


        }
        internal void HandleGameStateEntered()
        {
            Manager.Instance.Update();
        }
        internal void HandleModuleReadyToLoad(ModuleEnums.ModuleType moduleType)
        {
            Utility.RefreshGameManager();

            var currentModuleType = InstanceMM.GetCurrentModule(moduleType);
            bool isModuleTypeFound = StartListening(currentModuleType);

            if (!isModuleTypeFound) return;

            if (FlightReady())
            {
                InstanceMM.ModuleReadyToLoad += HandleModuleReadyToLoad;
                //InstanceMM.VesselStateChangedEvent += HandleVesselSituationChanged;

                if (ConditionsReady())
                {
                    ModuleReadyToLoad.Invoke(0);
                }         
            }
        }      
        internal void OnDestroy()
        {
            //VesselStateChangedEvent -= HandleVesselSituationChanged;
            GameStateEnteredEvent -= HandleGameStateEntered;
            InstanceMM.ModuleReadyToLoad -= HandleModuleReadyToLoad;

        }
        internal bool ConditionsReady()
        {
            return Utility.GameState == GameState.FlightView
                || Utility.GameState == GameState.VehicleAssemblyBuilder;
        }
        internal bool FlightReady()
        {
            return Utility.GameState == GameState.FlightView;
        }
    }
}