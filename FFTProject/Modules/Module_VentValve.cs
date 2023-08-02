using FFT.Modules.Core;
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

        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;
        public ParticleSystem ParticleSystem;
        internal float ASLValve;
        internal float AGLValve;
        internal bool ActivateModuleVentValve = false;

        public VesselDataModule _vesselDataModule;
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
            base.OnModuleFixedUpdate(fixedDeltaTime);

            _vesselDataModule.altitudeAgl.RefreshData();
            _vesselDataModule.altitudeAsl.RefreshData();

            ASLValve = (float)_vesselDataModule.altitudeAsl.altitudeAsl;
            AGLValve = (float)_vesselDataModule.altitudeAgl.altitudeAgl;

            float aslCurve = DataVentValve.VFXASLCurve.Evaluate(ASLValve);
            float aglCurve = DataVentValve.VFXAGLCurve.Evaluate(AGLValve);
            Animator.SetFloat("ASLValve", aslCurve);
            Animator.SetFloat("AGLValve", aglCurve);

            if (IsActive)
            {
                if (ASLValve > 1000 || AGLValve > 1000)
                {
                    StartVFX();
                }
                else if (ASLValve > 0 || AGLValve > 0)
                {
                    StopVFX();
                }
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
            DataVentValve.VFXAGLCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));;
            _vesselDataModule = new VesselDataModule();
        }
        public void Activate()
        {
            ActivateModuleVentValve = true;
        }
    }
}