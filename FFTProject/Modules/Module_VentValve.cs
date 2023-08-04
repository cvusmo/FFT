using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
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

        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;
        public ParticleSystem ParticleSystem;
        public float ASLValve;
        public float AGLValve;
        public bool ActivateModuleVentValve = false;

        public RefreshVesselData refreshVesselData;
        public RefreshActiveVessel refreshActiveVessel;

        public VentValveDefinitions VentValveDefinitions { get; private set; }
        public override bool IsActive => FFTPlugin.Instance._isActiveVessel.GetValueBool();

        public object RefreshVesselData { get; private set; }

        public override void OnInitialize()
        {
            base.OnInitialize();

            InitializeDataEntries();
            AddDataModules();

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
            AssignVentValveDefinitions();
            AssignParticleSystem();
            AssignComponents();
            FFTPlugin.Logger.LogInfo("Module_VentValve has started.");
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
            FFTPlugin.Logger.LogInfo("OnModuleFixedUpdate has started.");
            base.OnModuleFixedUpdate(fixedDeltaTime);

            refreshActiveVessel.RefreshData();
            VesselComponent activeVessel = refreshActiveVessel.ActiveVessel;

            refreshVesselData.activeVessel = activeVessel;           
            refreshVesselData.altitudeAgl.RefreshData(activeVessel);
            refreshVesselData.altitudeAsl.RefreshData(activeVessel);

            ASLValve = (float)refreshVesselData.altitudeAsl.altitudeAsl;
            AGLValve = (float)refreshVesselData.altitudeAgl.altitudeAgl;

            float aslCurve = DataVentValve.VFXASLCurve?.Evaluate(ASLValve) ?? 0;
            float aglCurve = DataVentValve.VFXAGLCurve?.Evaluate(AGLValve) ?? 0;

            Animator?.SetFloat("ASLValve", aslCurve);
            Animator?.SetFloat("AGLValve", aglCurve);

           
            if (IsActive)
            {
                if (!MaxAltitude())
                {
                    StopVFX();
                    FFTPlugin.Logger.LogInfo("StopVFX Confirmed.");
                }
                else if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("VentValveVFX_LOOP"))
                {
                    StartVFX();
                    FFTPlugin.Logger.LogInfo("StartVFX Confirmed.");
                }
            }
            else
            {
                StopVFX();
                FFTPlugin.Logger.LogInfo("StopVFX Confirmed.");
            }
        }
        internal void StartVFX()
        {
            EnableEmission();
            ParticleSystem.Play();
            TriggerVFX.enabled = true;
            GravityForVFX.enabled = true;
        }
        internal void StopVFX()
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
        internal void AssignVentValveDefinitions()
        {
            VentValveDefinitions = GetComponentInParent<VentValveDefinitions>();
        }
        internal void AssignParticleSystem()
        {
            if (VentValveVFX != null)
            {
                ParticleSystem = VentValveVFX.GetComponent<ParticleSystem>();
                if (ParticleSystem != null)
                {
                    FFTPlugin.Logger.LogInfo("Successfully retrieved ParticleSystem for VentValve.");
                }
                else
                {
                    FFTPlugin.Logger.LogError("Could not find any ParticleSystem on VentValveVFX.");
                }
            }
            else
            {
                FFTPlugin.Logger.LogError("VentValveVFX GameObject is not assigned.");
            }
        }
        internal void AssignComponents()
        {
            TriggerVFX = GetComponent<TriggerVFXFromAnimation>();
            GravityForVFX = GetComponent<DynamicGravityForVFX>();
            Animator = GetComponent<Animator>();
        }
        internal void InitializeDataEntries()
        {
            DataValveParts = new Data_ValveParts();
            DataVentValve = new Data_VentValve();
            DataVentValve.VFXASLCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            DataVentValve.VFXAGLCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            refreshVesselData = new RefreshVesselData();
            refreshActiveVessel = new RefreshActiveVessel();
        }
        internal bool MaxAltitude()
        {
            return AGLValve > 1000 || ASLValve > 1000;
        }
        public void Activate()
        {
            ActivateModuleVentValve = true;
        }
    }
}