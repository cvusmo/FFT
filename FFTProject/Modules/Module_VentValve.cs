using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

        [SerializeField]
        public Data_VentValve _dataVentValve;
        [SerializeField]
        public Data_ValveParts _dataValveParts;
        [SerializeField]
        public GameObject VentValveVFX;

        public TriggerVFXFromAnimation _triggerVFX;
        public DynamicGravityForVFX _gravityForVFX;
        public Animator animator;
        public ParticleSystem _particleSystem;
        internal float _ASLValve;
        internal float _AGLValve;
        public GameState _gameState { get; private set; }
        public VentValveDefinitions _ventValveDefinitions { get; private set; }
        public TelemetryComponent _telemetryComponent { get; private set; }

        public override bool IsActive => FFTPlugin.Instance._isActiveVessel.GetValueBool();
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
            FFTPlugin.Logger.LogInfo("Attached to GameObject: " + gameObject.name);

            _ventValveDefinitions = GetComponentInParent<VentValveDefinitions>();

            if (VentValveVFX != null)
            {
                _particleSystem = VentValveVFX.GetComponent<ParticleSystem>();
                FFTPlugin.Logger.LogError("ParticleSystem assigned.");

                if (_particleSystem != null)
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

            float aslCurve = _dataVentValve.VFXAGLCurve.Evaluate((float)_ASLValve);
            float aglCurve = _dataVentValve.VFXAGLCurve.Evaluate((float)_AGLValve);           

            _ASLValve = aslCurve;
            _AGLValve = aglCurve;

            animator.SetFloat("ASL", _ASLValve);
            animator.SetFloat("AGL", _AGLValve);


            FFTPlugin.Instance._isActiveVessel = new IsActiveVessel();
            FFTPlugin.Instance._vesselComponent = new VesselComponent();
            FFTPlugin.Instance._telemetryComponent = new TelemetryComponent();
            FFTPlugin.Logger.LogInfo("Module_VentValve has started.");
        }
        public override void AddDataModules()
        {
            base.AddDataModules();

            if (this._dataVentValve == null)
            {
                this._dataVentValve = new Data_VentValve();
                this.DataModules.TryAddUnique<Data_VentValve>(this._dataVentValve, out this._dataVentValve);
            }
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);

            double AltitudeFromSeaLevel = _telemetryComponent.AltitudeFromSeaLevel;
            double AltitudeFromTerrain = _telemetryComponent.AltitudeFromTerrain;
            int count = 1000;

            if (AltitudeFromSeaLevel > 1000 || AltitudeFromTerrain > 1000)
            {
                StopVFX();
                return;
            }

            double altitudeSealLevel = AltitudeFromSeaLevel / count;
            double altitudeTerrain = AltitudeFromTerrain / count;

            float ASL = _dataVentValve.VFXASLCurve.Evaluate((float)altitudeSealLevel);
            float AGL = _dataVentValve.VFXAGLCurve.Evaluate((float)altitudeTerrain);
            
            _ASLValve = ASL;
            animator.SetFloat("ASLValve", _ASLValve);

            _AGLValve = AGL;
            animator.SetFloat("AGLValve", _AGLValve);

            if (_ASLValve < 0 || _AGLValve < 0)
            {
                FFTPlugin.Logger.LogError("_ASLValve closed: " + _ASLValve);
                FFTPlugin.Logger.LogError("_AGLValve closed: " + _AGLValve);
            }

            if (IsActive)
            {
                if (!MaxAltitude())
                {
                    StopVFX();
                }
                else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("VentValve_LOOP"))
                {
                    StartVFX();
                }
            }
            else
            {
                StopVFX();
            }
        }
        internal void StartVFX()
        {
            EnableEmission();
            _particleSystem.Play();
            _triggerVFX.enabled = true;
            _gravityForVFX.enabled = true;
        }
        internal void StopVFX()
        {
            _particleSystem.Stop();
        }
        internal void EnableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = false;
            }
        }
        internal bool MaxAltitude()
        {
            return _ASLValve > 1000 || _AGLValve > 1000;
        }
    }
}