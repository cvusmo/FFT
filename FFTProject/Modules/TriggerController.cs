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
    private bool _coolingVFXOFF = false;
    public bool _isActive;

    internal new static ManualLogSource Logger { get; set; }
    //public bool IsActive { get; set; }

    private void Start()
    {
        Logger = FFTPlugin.Logger;

        _triggerVFX = GetComponent<TriggerVFXFromAnimation>();
        if (_triggerVFX == null)
        {
            Logger.LogError("TriggerVFXFromAnimation not found in same GameObject");
            return;
        }

        _coolingVFX = GameObject.Find("CV401/CoolingVFX");
        if (_coolingVFX == null)
        {
            Logger.LogError("CoolingVFX not found");
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

        IResourceContainer fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];
        Logger.LogInfo("fuelContainer: " + fuelContainer);

        UpdateFuelLevel(fuelContainer);

        IsActive = _isActiveVessel.GetValueBool();

        if (IsActive && _wasActive)
        {
            HandleActiveVessel(fuelContainer);
        }
        else if (_wasActive && !_coolingVFXOFF)
        {
            DisableCoolingEffects();
        }
    }

    private void UpdateFuelLevel(IResourceContainer _fuelContainer)
    {
        float fuelLevel = _fuelLevelWatcher != null ? (float)(_fuelLevelWatcher.GetValueDouble() / 100.0) : (float)_fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        _animator.SetFloat("FuelLevel", fuelLevel);
    }

    private bool FuelLevelExceedsThreshold()
    {
        return _animator.GetFloat("FuelLevel") > 0.8f;
    }

    private void HandleActiveVessel(IResourceContainer _fuelContainer)
    {
        if (FuelLevelExceedsThreshold() && !_animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            _triggerVFX.enabled = true;
            StartParticleSystem();
            _animator.Play("CoolingVFX_LOOP");
            Logger.LogInfo("CoolingVFX_LOOP: ");
        }
        else if (!FuelLevelExceedsThreshold() && !_coolingVFXOFF && _animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            DisableCoolingEffects();
        }
    }

    private void DisableCoolingEffects()
    {
        _animator.Play("CoolingVFX_OFF");
        _coolingVFXOFF = true;
        _triggerVFX.enabled = true;
        StopParticleSystem();
        DisableEmission();
        Logger.LogInfo("CoolingVFX_OFF: " + _coolingVFXOFF);
    }


}