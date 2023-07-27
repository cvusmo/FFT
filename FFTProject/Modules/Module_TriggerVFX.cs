using KSP.Animation;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
    public class Module_TriggerVFX : PartBehaviourModule
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

            if (IsActive)
            {
                if (FuelLevelExceedsThreshold())
                {
                    if (_particleSystem.isPlaying)
                    {
                        FFTPlugin.Logger.LogInfo("Looping VFX. FuelLevel: " + _fuelLevel);
                        LoopVFX();
                    }
                    else // If particle system is not playing, start the particle system
                    {
                        FFTPlugin.Logger.LogInfo("Starting VFX. FuelLevel: " + _fuelLevel);
                        StartVFX();
                    }
                }
                else
                {
                    FFTPlugin.Logger.LogInfo("Stopping VFX. FuelLevel: " + _fuelLevel);
                    StopVFX();
                }
            }
        }
        private void StartVFX()
        {
            StartParticleSystem();
            FFTPlugin.Logger.LogInfo("Successfully started VFX");
        }

        private void LoopVFX()
        {
            FFTPlugin.Logger.LogInfo("LoopVFX FuelLevel: " + _fuelLevel);
            EnableEmission();
            StartParticleSystem();
            _triggerVFX.enabled = true;
            animator.Play("CoolingVFX_LOOP");
            FFTPlugin.Logger.LogInfo("Successfully started LoopVFX");
        }

        private void StopVFX()
        {
            StopParticleSystem();
            FFTPlugin.Logger.LogInfo("Successfully stopped VFX");
            FFTPlugin.Logger.LogInfo("StopVFX FuelLevel: " + _fuelLevel);
        }

        public void EnableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = true;
                FFTPlugin.Logger.LogInfo("Emission enabled");
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
                FFTPlugin.Logger.LogInfo("Emission disabled");
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
                EnableEmission();
                animator.Play("CoolingVFX_OFF");
                _triggerVFX.enabled = true;
                FFTPlugin.Logger.LogInfo("Particle system started");
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in StartParticleSystem");
            }
        }

        public void StopParticleSystem()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
                FFTPlugin.Logger.LogInfo("Particle system stopped");
            }
            else
            {
                FFTPlugin.Logger.LogError("_particleSystem is null in StopParticleSystem");
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
