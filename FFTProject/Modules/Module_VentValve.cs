using KSP.Animation;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;

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
        public ParticleSystem ParticleSystem;

        //internal FFT scripts
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel;
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL;
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public RefreshVesselData RefreshVesselData { get; private set; }
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
            ParticleSystem = VentValveVFX.GetComponentInChildren<ParticleSystem>();
            DataValveParts = new Data_ValveParts();
            DataVentValve = new Data_VentValve();
            RefreshVesselData = new RefreshVesselData();
            Animator = GetComponentInParent<Animator>();
            TriggerVFX = GetComponentInParent<TriggerVFXFromAnimation>();
            GravityForVFX = GetComponentInParent<DynamicGravityForVFX>();

            FFTPlugin.Logger.LogInfo("Module_VentValveVFX has started.");
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

            RefreshVesselData.refreshActiveVessel.RefreshData();
            var activeVessel = RefreshVesselData.refreshActiveVessel.ActiveVessel;
            RefreshVesselData.RefreshAll(activeVessel);

            var altitudeSeaLevel = RefreshVesselData.altitudeAsl.altitudeAsl;
            float ASLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)altitudeSeaLevel);
            ASL = ASLFromCurve;
            Animator.SetFloat("ASL", ASL);

            var altitudeGroundLevel = RefreshVesselData.altitudeAgl.altitudeAgl;
            float AGLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)altitudeGroundLevel);
            AGL = AGLFromCurve;
            Animator.SetFloat("AGL", AGL);

            if (MaxAltitudeAchieved())
            {
                StartVFX();
            }
            else if (ASL == 0 && AGL == 0)
            {
                StopVFX();
            }

        }
        public void StartVFX()
        {
            EnableEmission();
            ParticleSystem.Play();
        }
        public void StopVFX()
        {
            DisableEmission();
            ParticleSystem.Stop();
        }
        internal void EnableEmission()
        {
            if (ParticleSystem != null)
            {
                var emission = ParticleSystem.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (ParticleSystem != null)
            {
                var emission = ParticleSystem.emission;
                emission.enabled = false;
            }
        }

        internal bool MaxAltitudeAchieved()
        {
            return ASL > 0 || AGL > 0;
        }
        public void Activate()
        {
            activateModuleVentValve = true;
        }
    }
}