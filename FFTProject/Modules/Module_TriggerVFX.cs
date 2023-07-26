﻿using KSP.Sim.Definitions;
using UnityEngine;
using KSP.Animation;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using SpaceWarp.API.Game;

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

        public float FuelLevel
        {
            get
            {
                _vesselComponent.RefreshFuelPercentages();
                double fuelPercentage = _vesselComponent.StageFuelPercentage;

                if (fuelPercentage < 0)
                {
                    FFTPlugin.Logger.LogError("Out of Fuel." + fuelPercentage);
                    return 0f;
                }
                else
                {
                    float level = (float)fuelPercentage;
                    animator.SetFloat("FuelLevel", level);
                    FFTPlugin.Logger.LogInfo("Fuel level: " + level);
                    return level;
                }
            }
        }
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                Awake();
            }
        }
        public void Awake()
        {
            FFTPlugin.Logger.LogInfo("Attached to GameObject: " + gameObject.name);

            _triggerVFX = GetComponent<TriggerVFXFromAnimation>();
            _particleSystem = CoolingVFX.GetComponent<ParticleSystem>();
            Animator animator = GetComponentInParent<Animator>();
            if (animator == null)
            {
                FFTPlugin.Logger.LogError("Animator component not found in parents.");
            }
            _isActiveVessel = new IsActiveVessel();
            _vesselComponent = new VesselComponent();

            FFTPlugin.Logger.LogInfo("TriggerController has started.");
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
                    FillRatio = container.GetResourceFillRatio(ResourceID) / count;
                }
            }

            float opacity = _dataTriggerVFX.VFXOpacityCurve.Evaluate((float)FillRatio);

            IsActive = opacity > 0;
        }
        public bool IsActive
        {
            get { return _isActiveVessel.GetValueBool(); }
            set
            {
                bool active = _isActiveVessel.GetValueBool();
                if (active)
                {
                    FFTPlugin.Logger.LogInfo("IsActive set to true");
                    if (!_wasActive)
                    {
                        FFTPlugin.Logger.LogInfo("Was not previously active");
                        if (FuelLevelExceedsThreshold())
                        {
                            FFTPlugin.Logger.LogInfo("Fuel level exceeds threshold");
                            EnableEmission();
                            _triggerVFX.enabled = true;
                            StartParticleSystem();
                            animator.Play("CoolingVFX_ON");
                            FFTPlugin.Logger.LogInfo("CoolingVFX_ON: ");
                        }
                    }
                }
                else
                {
                    FFTPlugin.Logger.LogInfo("IsActive set to false");
                    DisableEmission();
                    _triggerVFX.enabled = false;
                }
                _wasActive = active;
            }
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
        public void FixedUpdate()
        {
            _vesselComponent = Vehicle.ActiveSimVessel;

            if (animator != null)
            {
                animator.SetBool("_isActive", _isActiveVessel.GetValueBool());
            }
            else
            {
                FFTPlugin.Logger.LogError("animator is null in FixedUpdate");
                return;
            }

            if (_isActiveVessel != null)
            {
                FFTPlugin.Logger.LogInfo("_isActiveVessel.GetValueBool(): " + _isActiveVessel.GetValueBool());
                IsActive = _isActiveVessel.GetValueBool();
            }
            else
            {
                FFTPlugin.Logger.LogError("_isActiveVessel is null in FixedUpdate");
                return;
            }

            if (_wasActive && !_isActiveVessel.GetValueBool())
            {
                DisableCoolingEffects();
            }

            _wasActive = _isActiveVessel.GetValueBool();
        }
        public bool FuelLevelExceedsThreshold()
        {
            return FuelLevel > 0.8f;
        }
        public void HandleActiveVessel(IResourceContainer __fuelContainer)
        {
            if (FuelLevelExceedsThreshold() && !animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
            {
                _triggerVFX.enabled = true;
                StartParticleSystem();
                animator.Play("CoolingVFX_LOOP");
                FFTPlugin.Logger.LogInfo("CoolingVFX_LOOP: ");
                FFTPlugin.Logger.LogInfo("__fuelContainer: " + __fuelContainer);
            }
            else if (!FuelLevelExceedsThreshold() && animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
            {
                DisableCoolingEffects();
            }
        }
        public void DisableCoolingEffects()
        {
            animator.Play("CoolingVFX_OFF");
            _triggerVFX.enabled = true;
            StopParticleSystem();
            DisableEmission();
        }
    }
}