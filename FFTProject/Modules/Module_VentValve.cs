using KSP.Animation;
using KSP.Game;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;
using VFX;
using static FFT.Modules.RefreshVesselData;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

        [SerializeField]
        public Data_VentValve DataVentValve;
        [SerializeField]
        public Data_ValveParts DataValveParts;
        [SerializeField]
        public GameObject VentValveVFX;

        //unity scripts
        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;

        //internal FFT scripts
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel;
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL;
        private RefreshVesselData _refreshVesselData;
        private RefreshActiveVessel _refreshActiveVessel;
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public VesselComponent activeVessel { get; private set; }
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                if (VentValveVFX)
                {
                    Awake();
                }
            }
        }
        public void Awake()
        {
            DataValveParts = new Data_ValveParts();
            DataVentValve = new Data_VentValve();
            Animator = GetComponentInParent<Animator>();
            FFTPlugin.Logger.LogInfo("Module_VentValveVFX has started.");
        }
        public RefreshVesselData refreshVesselData
        {
            get
            {
                if (_refreshVesselData == null)
                    _refreshVesselData = new RefreshVesselData();
                return _refreshVesselData;
            }
        }
        public RefreshActiveVessel refreshActiveVessel
        {
            get
            {
                if (_refreshActiveVessel == null)
                    _refreshActiveVessel = new RefreshActiveVessel();
                return _refreshActiveVessel;
            }
        }
        public override void AddDataModules()
        {
            base.AddDataModules();

            if (this.DataVentValve == null)
            {
                this.DataVentValve = new Data_VentValve();
                this.DataModules.TryAddUnique<Data_VentValve>(this.DataVentValve, out this.DataVentValve);
            }
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);

            var activeVessel = refreshActiveVessel;
            var altitudeSeaLevel = activeVessel.ActiveVessel.AltitudeFromSeaLevel;
            float ASLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)altitudeSeaLevel);
            Animator.SetFloat("ASL", ASLFromCurve);

            if (FFTPlugin.Instance._state == GameState.Launchpad && AltitudeCheck())
            {
                StartVFX();
            }
            else
            {
                StopVFX();
            }
        }
        public void StartVFX()
        {
            TriggerVFX.VFX01_ON();
            GravityForVFX.enabled = true;
        }
        public void StopVFX()
        {
            TriggerVFX.VFX01_OFF();
        }
        public bool AltitudeCheck()
        {
            return altitudeGroundLevel <= 1000 || altitudeSeaLevel <= 1000;
        }
        public void Activate()
        {
            activateModuleVentValve = true;
        }
    }
}