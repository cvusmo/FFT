//CV401 (CorePartData, Module_Color, Module_Drag, Module_ResourceCapacities, Animator)
//└── model()
//└── CV - 401(mesh renderer)
//└── Smoke(ParticleSystem, TriggerController, Module_ToggleCrossfeed, TriggerVFXFromAnimation)


using BepInEx.Logging;
using SpaceWarp;
using FFT;
using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Modules;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    private Module_ResourceCapacities _moduleResourceCapacities;
    private TriggerVFXFromAnimation _triggerVFX;
    private ResourceDefinitionID _fuelResourceId;
    private Animator _animator;
    public ParticleSystem _particleSystem;
    private IsActiveVessel _isActiveVessel;
    public ViewController _viewController;
    internal VesselComponent _vesselComponent;
    private GameObject _coolingVFX;
    private bool _wasActive;
    private bool _coolingVFXON;
    private bool _coolingVFXLOOP;
    private bool _coolingVFXOFF = false;
    private string _activeVesselGuid = "";
    internal new static ManualLogSource Logger { get; set; }
    public bool IsActive { get; set; }

    private void Start()
    {
        _moduleResourceCapacities = GetComponentInParent<Module_ResourceCapacities>();
        if (_moduleResourceCapacities == null)
        {
            Logger.LogError("Module_ResourceCapacities not found in parent");
            return;
        }

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

        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        _fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual fuel resource.
        Logger.LogError("_fuelResourceId" + _fuelResourceId);

        _animator = GetComponentInParent<Animator>();
        if (_animator == null)
        {
            Logger.LogError("Animator not found in parent GameObject" + _animator);
            return;
        }

        _isActiveVessel = new IsActiveVessel();

        Logger = FFTPlugin.Logger;

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

        {
            if (_moduleResourceCapacities == null)
            {
                _moduleResourceCapacities = GetComponentInParent<Module_ResourceCapacities>();
                if (_moduleResourceCapacities == null)
                {
                    Logger.LogError("Module_ResourceCapacities not found in parent");
                    return;
                }
            }

            if (_animator == null)
            {
                _animator = GetComponentInParent<Animator>();
                if (_animator == null)
                {
                    Logger.LogError("Animator not found in parent GameObject");
                    return;
                }
            }
        }
        if (_moduleResourceCapacities == null || _moduleResourceCapacities.OABPart == null || _moduleResourceCapacities.OABPart.Containers == null)
        {
            Logger.LogError("_moduleResourceCapacities or _moduleResourceCapacities.OABPart or _moduleResourceCapacities.OABPart.Containers is null");
            return;
        }

        IResourceContainer fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];
        Logger.LogInfo("fuelContainer: " + fuelContainer);

        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        _animator.SetFloat("FuelLevel", fuelLevel);

        Logger.LogInfo("fuelLevel: " + fuelLevel);
        Logger.LogInfo("_animator: " + _animator);
        Logger.LogInfo("_fuelResourceId" + _fuelResourceId);

        bool _isActive = _isActiveVessel.GetValueBool();

        Logger.LogInfo("_isActive: " + _isActive);
        Logger.LogInfo("_wasActive: " + _wasActive);

        if (_isActive)
        {
            if (!_wasActive)
            {
                _coolingVFXOFF = false;
                EnableEmission();
                if (fuelLevel > 0.8f)
                {
                    _animator.Play("CoolingVFX_ON");
                    Logger.LogInfo("CoolingVFX_ON: " + _coolingVFXON);
                }
                else
                {
                    _animator.Play("CoolingVFX_LOOP");
                    Logger.LogInfo("CoolingVFX_LOOP: " + _coolingVFXLOOP);
                    StartParticleSystem();
                }
            }
        }
        else if (_wasActive && !_coolingVFXOFF && fuelLevel < 0.8f)
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

