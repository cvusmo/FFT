using BepInEx.Logging;
using FFT;
using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Modules;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
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
    private GameObject _coolingVFX;
    private VesselFuelLevelPropertyWatcher _fuelLevelWatcher;
    private Module_ResourceCapacities _moduleResourceCapacities;
    private ResourceDefinitionID _fuelResourceId;
    private ResourceDataProvider _resourceDataProvider;
    private bool _wasActive;
    public bool _isActive;
    public float FuelLevel { get; private set; }

    internal new static ManualLogSource Logger { get; set; }
    //public bool IsActive { get; set; }

    private void Start()
    {
        Logger = FFTPlugin.Logger;
        Logger.LogInfo("Attached to GameObject: " + gameObject.name);

        _triggerVFX = GetComponent<TriggerVFXFromAnimation>();
        if (_triggerVFX == null)
        {
            Logger.LogError("TriggerVFXFromAnimation not found in same GameObject");
            return;
        }

        _particleSystem = _coolingVFX.GetComponent<ParticleSystem>();
        if (_particleSystem == null)
        {
            Logger.LogError("ParticleSystem not found in same GameObject");
            return;
        }

        _animator = GetComponentInParent<Animator>();
        if (_animator == null)
        {
            Logger.LogError("Animator not found in parent GameObject");
            return;
        }

        _isActiveVessel = new IsActiveVessel();
        _fuelLevelWatcher = new VesselFuelLevelPropertyWatcher();
        _resourceDataProvider = new ResourceDataProvider();
        _vesselComponent = new VesselComponent();
        _moduleResourceCapacities = new Module_ResourceCapacities();
        _fuelResourceId = new ResourceDefinitionID();


        Logger.LogInfo("TriggerController has started.");
    }
    public void EnableEmission()
    {
        var emission = _particleSystem.emission;
        emission.enabled = true;
    }
    public void DisableEmission()
    {
        var emission = _particleSystem.emission;
        emission.enabled = false;
    }
    public void StartParticleSystem()
    {
        _particleSystem.Play();
    }
    public void StopParticleSystem()
    {
        _particleSystem.Stop();
    }
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (_isActive)
            {
                if (!_wasActive && FuelLevelExceedsThreshold())
                {
                    EnableEmission();
                    _triggerVFX.enabled = true;
                    StartParticleSystem();
                    _animator.Play("CoolingVFX_ON");
                    Logger.LogInfo("CoolingVFX_ON: ");
                }
            }
            else
            {
                DisableEmission();
                _triggerVFX.enabled = false;
            }

            _wasActive = _isActive;
        }
    }
    private void FixedUpdate()
    {
        VesselComponent _vesselComponent = Vehicle.ActiveSimVessel;
        _vesselComponent.RefreshFuelPercentages();

        Logger.LogInfo("ActiveSimVessel: " + Vehicle.ActiveSimVessel);

        IResourceContainer _fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];
        Logger.LogInfo("_fuelContainer: " + _fuelContainer);

        UpdateFuelLevel(_fuelContainer);

        IsActive = _isActiveVessel.GetValueBool();

        if (IsActive && _wasActive)
        {
            HandleActiveVessel(_fuelContainer);
        }
        else if (_wasActive)
        {
            DisableCoolingEffects();
        }
    }

    private void UpdateFuelLevel(IResourceContainer _fuelContainer)
    {
        float storedFuel = (float)_fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        float capacityFuel = (float)_fuelContainer.GetResourceCapacityUnits(_fuelResourceId);
        FuelLevel = _fuelLevelWatcher != null ? (float)(_fuelLevelWatcher.GetValueDouble() / 100.0) : storedFuel / capacityFuel;
        _animator.SetFloat("FuelLevel", FuelLevel);
        Logger.LogInfo("_fuelLevelWatcher" + _fuelLevelWatcher);
        Logger.LogInfo("_fuelContainer: " + _fuelContainer);
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