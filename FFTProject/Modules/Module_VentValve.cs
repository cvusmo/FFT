using KSP.Animation;
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
        public ParticleSystem ParticleSystem;

        //internal FFT scripts
        public RefreshActiveVessel refreshActiveVessel;
        public RefreshVesselData refreshVesselData;
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel;
        internal bool activateModuleVentValve = false;
        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public override bool IsActive => FFTPlugin.Instance.isActiveVessel.GetValueBool();
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
            if (VentValveVFX != null)
            {
                ParticleSystem = VentValveVFX.GetComponentInChildren<ParticleSystem>();
                if (ParticleSystem != null)
                {
                    FFTPlugin.Logger.LogInfo("Successfully retrieved ParticleSystem on VentValveVFX.");
                }
                else
                {
                    FFTPlugin.Logger.LogError("Could not find ParticleSystem on VentValveVFX.");
                }
            }
            else
            {
                FFTPlugin.Logger.LogError("VentValveVFX GameObject is not assigned.");
            }

            DataValveParts = new Data_ValveParts();
            DataVentValve = new Data_VentValve();
            refreshVesselData = new RefreshVesselData();
            refreshActiveVessel = new RefreshActiveVessel();
            Animator = GetComponentInParent<Animator>();

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
            refreshActiveVessel.RefreshData();
            VesselComponent activeVessel = refreshActiveVessel.ActiveVessel;

            bool altitudeCheck = AltitudeCheck();
            float maxASL = 1000;
            float maxAGL = 1000;
            float maxVertical = 300;
            float maxHorizontal = 300;

            refreshVesselData.activeVessel = activeVessel;
            refreshVesselData.RefreshAll(activeVessel);

            altitudeSeaLevel = (float)refreshVesselData.altitudeAsl.altitudeAsl;
            altitudeGroundLevel = (float)refreshVesselData.altitudeAgl.altitudeAgl;
            verticalSpeed = (float)refreshVesselData.verticalVelocity.verticalVelocity;
            horizontalSpeed = (float)refreshVesselData.horizontalVelocity.horizontalVelocity;

            Debug.Log("Animator: " + (Animator == null ? "is null" : "is not null"));

            double normalizedASL = altitudeSeaLevel / maxASL;
            float ASLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)normalizedASL);
            Animator.SetFloat("ASL", ASLFromCurve);
         
            double normalizedAGL = altitudeGroundLevel / maxAGL;
            float AGLFromCurve = DataVentValve.VFXAGLCurve.Evaluate((float)normalizedAGL);
            Animator.SetFloat("AGL", AGLFromCurve);
         
            double normalizedVS = verticalSpeed / maxVertical;
            float VSFromCurve = DataVentValve.VFXVerticalSpeedCurve.Evaluate((float)normalizedVS);
            Animator.SetFloat("VerticalSpeed", VSFromCurve);
          
            double normalizedHorizontal = horizontalSpeed / maxHorizontal;
            float HSFromCurve = DataVentValve.VFXHorizontalSpeedCurve.Evaluate((float)normalizedHorizontal);
            Animator.SetFloat("HorizontalSpeed", HSFromCurve);
         
            if (IsActive)
            {
                if (!altitudeCheck)
                {
                    StopVFX();
                    Debug.Log("StopVFX: " + (StopVFX == null ? "is null" : "is not null"));
                }
                else if (altitudeCheck)
                {
                    StartVFX();
                    Debug.Log("StartVFX: " + (StartVFX == null ? "is null" : "is not null"));
                }
            }
            else
            {
                StopVFX();
            }
        }
        public void StartVFX()
        {
            EnableEmission();
            TriggerVFX.VFX01_ON();
            GravityForVFX.enabled = true;
            ParticleSystem.Play();
        }

        public void StopVFX()
        {
            TriggerVFX.VFX01_OFF();
            ParticleSystem.Stop(); ;
        }
        internal void EnableEmission()
        {
            if (ParticleSystem != null)
            {
                var emission = ParticleSystem.emission;
                emission.enabled = true;
            }
        }
        public void DisableEmission()
        {
            if (ParticleSystem != null)
            {
                var emission = ParticleSystem.emission;
                emission.enabled = false;
            }
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