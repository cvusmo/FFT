using BepInEx.Logging;
using FFT;
using KSP.Messages.PropertyWatchers;
using KSP.Animation;
using KSP.Game;
using KSP.Modules;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using UnityEngine;
using System.Linq;

public class TriggerController : MonoBehaviour
{
    private TriggerVFXFromAnimation _triggerVFX;
    private Animator _animator;
    private ParticleSystem _particleSystem;
    private IsActiveVessel _isActiveVessel;
    private GameObject _coolingVFX;
    private VesselFuelLevelPropertyWatcher _fuelLevelWatcher;
    private Module_ResourceCapacities _moduleResourceCapacities;
    private ResourceDefinitionID _fuelResourceId;
    private ResourceDataProvider _resourceDataProvider;

    private bool _wasActive;
    private bool _coolingVFXOFF = false;

    internal new static ManualLogSource Logger { get; set; }
    public bool IsActive { get; set; }

    private void Start()
    {
        Logger = FFTPlugin.Logger;

        _resourceDataProvider = new ResourceDataProvider();

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

    private void FixedUpdate()
    {
        GameState? state = FFTPlugin.Instance.GetGameState();

        IResourceContainer fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];
        Logger.LogInfo("fuelContainer: " + fuelContainer);

        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        _animator.SetFloat("FuelLevel", fuelLevel);

        if (_animator == null)
        {
            _animator = GetComponentInParent<Animator>();
            if (_animator == null)
            {
                Logger.LogError("Animator not found in parent GameObject");
                return;
            }
        }

        if (_fuelLevelWatcher != null)
        {
            fuelLevel = (float)(_fuelLevelWatcher.GetValueDouble() / 100.0);
            _animator.SetFloat("FuelLevel", fuelLevel);
        }

        bool _isActive = _isActiveVessel.GetValueBool();

        if (_isActive)
        {
            if (!_wasActive)
            {
                _coolingVFXOFF = false;
                EnableEmission();
                if (fuelLevel > 0.8f)
                {
                    _animator.Play("CoolingVFX_ON");
                    Logger.LogInfo("CoolingVFX_ON: ");
                }
            }
            else if (fuelLevel > 0.8f && !_animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
            {
                _animator.Play("CoolingVFX_LOOP");
                Logger.LogInfo("CoolingVFX_LOOP: ");
                StartParticleSystem();
            }
            else if (fuelLevel <= 0.8f && !_coolingVFXOFF && _animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
            {
                _animator.Play("CoolingVFX_OFF");
                _coolingVFXOFF = true;
                Logger.LogInfo("CoolingVFX_OFF: " + _coolingVFXOFF);
                StopParticleSystem();
                DisableEmission();
            }
        }
        else if (_wasActive && !_coolingVFXOFF)
        {
            _animator.Play("CoolingVFX_OFF");
            _coolingVFXOFF = true;
            Logger.LogInfo("CoolingVFX_OFF: " + _coolingVFXOFF);
            StopParticleSystem();
            DisableEmission();
        }

        _wasActive = _isActive;
    }
}
