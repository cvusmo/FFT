using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using KSP.UI;
using UnityEngine;

namespace FFT.Modules
{
    public class Module_TriggerVFX : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_TriggerVFX);

        [SerializeField]
        public Data_TriggerVFX _dataTriggerVFX;
        [SerializeField]
        public GameObject CoolingVFX;
        [SerializeField]

        public TriggerVFXFromAnimation _triggerVFX;
        public Animator animator;
        public ParticleSystem _particleSystem;
        public IsActiveVessel _isActiveVessel;
        public VesselComponent _vesselComponent;
        public bool _wasActive;
        private float _fuelLevel;
        public GameState _gameState { get; private set; }
        public bool IsActive => _isActiveVessel.GetValueBool();

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

            _isActiveVessel = new IsActiveVessel();
            _vesselComponent = new VesselComponent();

            FFTPlugin.Logger.LogInfo("Module_TriggerVFX has started.");
        }
        public override void AddDataModules()
        {
            base.AddDataModules();
            this._dataTriggerVFX ??= new Data_TriggerVFX();
            this.DataModules.TryAddUnique<Data_TriggerVFX>(this._dataTriggerVFX, out this._dataTriggerVFX);
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);
            double fillRatioSum = 0;
            int count = 0;

            foreach (var container in part.Model.Containers)
            {
                foreach (var resourceID in container)
                {
                    FFTPlugin.Logger.LogInfo("ResourceID Before: " + resourceID);
                    FFTPlugin.Logger.LogInfo("opacity Before: " + count);
                    count++;
                    fillRatioSum += container.GetResourceFillRatio(resourceID);
                    FFTPlugin.Logger.LogInfo("ResourceID After: " + resourceID);
                    FFTPlugin.Logger.LogInfo("opacity After: " + count);
                }
            }

            double fillRatioAverage = fillRatioSum / count;

            float opacity = _dataTriggerVFX.VFXOpacityCurve.Evaluate((float)fillRatioAverage);

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
                    _triggerVFX.enabled = true;
                    FFTPlugin.Logger.LogInfo("_triggerVFX: " + _triggerVFX);
                }
            }
            else
            {
                StopVFX();
            }
        }

        private void StartVFX()
        {
            FFTPlugin.Logger.LogInfo("StopVFX FuelLevel: " + _fuelLevel);
            EnableEmission();
            FFTPlugin.Logger.LogInfo("EnableEmission");
            StartParticleSystem();
            FFTPlugin.Logger.LogInfo("StartParticleSystem"); 
            animator.Play("CoolingVFX_ON");
            FFTPlugin.Logger.LogInfo("CoolingVFX_ON: " + animator);
        }

        private void StopVFX()
        {
            FFTPlugin.Logger.LogInfo("StopVFX FuelLevel: " + _fuelLevel);
            StopParticleSystem();
            animator.Play("CoolingVFX_OFF");
            FFTPlugin.Logger.LogInfo("Successfully played CoolingVFX_OFF.");
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
                 DisableEmission();
                _particleSystem.Stop();    
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in _particleSystem.Stop");
            }
        }
        public bool FuelLevelExceedsThreshold()
        {
            if (_fuelLevel < 0.8f)
            {
                return false;
            }
            return true;

        }
    }
}
