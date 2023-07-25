using BepInEx.Logging;
using FFT;
using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using KSP.Sim.State;
using SpaceWarp.API.Game;
using UnityEngine;
using static KSP.Api.UIDataPropertyStrings.View;

public class TriggerController : MonoBehaviour
{
    private TriggerVFXFromAnimation _triggerVFX;
    private Animator _animator;
    private ParticleSystem _particleSystem;
    private IsActiveVessel _isActiveVessel;
    private VesselComponent _vesselComponent;
    private bool _wasActive;
    public bool _isActive;

    [SerializeField]
    private GameObject CoolingVFX;
    internal new static ManualLogSource Logger { get; set; }

    public float FuelLevel
    {
        get
        {
            _vesselComponent.RefreshFuelPercentages();
            double fuelPercentage = _vesselComponent.StageFuelPercentage;

            if (fuelPercentage < 0)
            {
                Logger.LogError("Out of Fuel." + fuelPercentage);
                return 0f;
            }
            else
            {
                float level = (float)fuelPercentage;
                _animator.SetFloat("FuelLevel", level);
                Logger.LogInfo("Fuel level: " + level);
                return level;
            }
        }
    }
    private void Start()
    {
        Logger = FFTPlugin.Logger;
        Logger.LogInfo("Attached to GameObject: " + gameObject.name);

        _triggerVFX = GetComponent<TriggerVFXFromAnimation>();
        _particleSystem = CoolingVFX.GetComponent<ParticleSystem>();
        _animator = GetComponentInParent<Animator>();
        _isActiveVessel = new IsActiveVessel();
        _vesselComponent = new VesselComponent();

        if (_triggerVFX == null)
        {
            Logger.LogError("_triggerVFX is null");
        }

        if (_particleSystem == null)
        {
            Logger.LogError("_particleSystem is null");
        }

        if (_animator == null)
        {
            Logger.LogError("_animator is null");
        }

        if (_isActiveVessel == null)
        {
            Logger.LogError("_isActiveVessel is null");
        }

        if (_vesselComponent == null)
        {
            Logger.LogError("_vesselComponent is null");
        }

        Logger.LogInfo("TriggerController has started.");
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
            Logger.LogError("_particleSystem is null in EnableEmission");
        }
    }
    public void DisableEmission()
    {
        if(_particleSystem != null)
        {
            var emission = _particleSystem.emission;
            emission.enabled = false;
        }
        else
        {
            Logger.LogError("_particleSystem is null in DisableEmission");
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
            Logger.LogError("_particleSystem is null in _particleSystem.Play");
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
            Logger.LogError("_particleSystem is null in _particleSystem.Stop");
        }
    }
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (_isActive)
            {
                Logger.LogInfo("IsActive set to true");
                if (!_wasActive)
                {
                    Logger.LogInfo("Was not previously active");
                    if (FuelLevelExceedsThreshold())
                    {
                        Logger.LogInfo("Fuel level exceeds threshold");
                        EnableEmission();
                        _triggerVFX.enabled = true;
                        StartParticleSystem();
                        _animator.Play("CoolingVFX_ON");
                        Logger.LogInfo("CoolingVFX_ON: ");
                    }
                }
            }
            else
            {
                Logger.LogInfo("IsActive set to false");
                DisableEmission();
                _triggerVFX.enabled = false;
            }
            _wasActive = _isActive;
        }
    }
    private void FixedUpdate()
    {
        if (Vehicle.ActiveSimVessel != null)
        {
            _vesselComponent = Vehicle.ActiveSimVessel;
            Logger.LogInfo("ActiveSimVessel: " + Vehicle.ActiveSimVessel);
        }
        else
        {
            Logger.LogError("Vehicle.ActiveSimVessel is null");
            return;
        }

        Logger.LogInfo("_isActive: " + _isActive);

        if (_animator != null)
        {
            _animator.SetBool("_isActive", _isActive);
        }
        else
        {
            Logger.LogError("_animator is null in FixedUpdate");
            return;
        }

        if (_isActiveVessel != null)
        {
            Logger.LogInfo("_isActiveVessel.GetValueBool(): " + _isActiveVessel.GetValueBool());
            IsActive = _isActiveVessel.GetValueBool();
        }
        else
        {
            Logger.LogError("_isActiveVessel is null in FixedUpdate");
            return;
        }

        if (_wasActive && !_isActive)
        {
            DisableCoolingEffects();
        }

        _wasActive = _isActive;
    }


    private bool FuelLevelExceedsThreshold()
    {
        return FuelLevel > 0.8f;
    }

    private void HandleActiveVessel(IResourceContainer __fuelContainer)
    {
        if (FuelLevelExceedsThreshold() && !_animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            _triggerVFX.enabled = true;
            StartParticleSystem();
            _animator.Play("CoolingVFX_LOOP");
            Logger.LogInfo("CoolingVFX_LOOP: ");
            Logger.LogInfo("__fuelContainer: " + __fuelContainer);
        }
        else if (!FuelLevelExceedsThreshold() && _animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            DisableCoolingEffects();
        }
    }
    private void DisableCoolingEffects()
    {
        _animator.Play("CoolingVFX_OFF");
        _triggerVFX.enabled = true;
        StopParticleSystem();
        DisableEmission();
    }
}
