using KSP.Animation;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;

namespace FFT.Modules
{
    internal class Module_TriggerVFX : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_TriggerVFX);

        [SerializeField]
        public Data_TriggerVFX _dataTriggerVFX;
        [SerializeField]
        public GameObject CoolingVFX;

        public TriggerVFXFromAnimation _triggerVFX;
        public Animator animator;
        public ParticleSystem _particleSystem;
        public IsActiveVessel _isActiveVessel;
        public VesselComponent _vesselComponent;
        public bool _wasActive;
        private float _fuelLevel;

        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                if (CoolingVFX)
                {
                    Awake();
                }
            }
        }
        public void Awake()
        {
            FFTPlugin.Logger.LogInfo("Attached to GameObject: " + gameObject.name);

            _triggerVFX = GetComponent<TriggerVFXFromAnimation>();

            if (CoolingVFX != null)
            {
                _particleSystem = CoolingVFX.GetComponentInChildren<ParticleSystem>();
                if (_particleSystem != null)
                {
                    FFTPlugin.Logger.LogInfo("Successfully retrieved ParticleSystem.");
                }
                else
                {
                    FFTPlugin.Logger.LogError("Could not find ParticleSystem on CoolingVFX.");
                }
            }
            else
            {
                FFTPlugin.Logger.LogError("CoolingVFX GameObject is not assigned.");
            }

            animator = GetComponentInParent<Animator>();
            if (animator == null)
            {
                FFTPlugin.Logger.LogError("Animator component not found in parents.");
            }

            _isActiveVessel = new IsActiveVessel();
            _vesselComponent = new VesselComponent();

            FFTPlugin.Logger.LogInfo("Module_TriggerVFX has started.");
        }

        public void AddDataModules()
        {
            base.AddDataModules();
            this._dataTriggerVFX ??= new Data_TriggerVFX();
            this.DataModules.TryAddUnique<Data_TriggerVFX>(this._dataTriggerVFX, out this._dataTriggerVFX);
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);
            double FillRatio = 0;
            int count = 0;

            foreach (var container in part.Model.Containers)
            {
                foreach (var ResourceID in container)
                {
                    count++;
                    FillRatio += container.GetResourceFillRatio(ResourceID);
                }
            }

            if (count != 0)
            {
                FillRatio /= count;
            }

            float opacity = _dataTriggerVFX.VFXOpacityCurve.Evaluate((float)FillRatio);

            _fuelLevel = opacity;
            FFTPlugin.Logger.LogInfo("opacity: " + opacity);

            animator.SetFloat("FuelLevel", _fuelLevel);
            FFTPlugin.Logger.LogInfo("Fuel level: " + _fuelLevel);

            if (_fuelLevel < 0)
            {
                FFTPlugin.Logger.LogError("Out of Fuel. Fuel level: " + _fuelLevel);
            }

            if (IsActive)
            {
                if (!FuelLevelExceedsThreshold())
                {
                    StopVFX();
                }
                else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
                {
                    StartVFX();
                }
            }
            else
            {
                StopVFX();
            }
        }

        public bool IsActive
        {
            get { return _isActiveVessel.GetValueBool(); }
        }
        private void StartVFX()
        {
            EnableEmission();
            StartParticleSystem();
            _triggerVFX.enabled = true;
            animator.Play("CoolingVFX_ON");
            FFTPlugin.Logger.LogInfo("CoolingVFX_ON");
        }

        private void StopVFX()
        {
            DisableEmission();
            _triggerVFX.enabled = false;
            StopParticleSystem();
        }
        public void EnableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = true;
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in EnableEmission");
            }
        }
        public void DisableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = false;
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in DisableEmission");
            }
        }
        public void StartParticleSystem()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in _particleSystem.Play");
            }
        }
        public void StopParticleSystem()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in _particleSystem.Stop");
            }
        }
        public bool FuelLevelExceedsThreshold()
        {
            return _fuelLevel > 0.8f;
        }
    }
}
