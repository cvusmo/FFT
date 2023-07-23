using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Modules;
using KSP.Sim.ResourceSystem;
using UnityEngine;
using BepInEx.Logging;
using FFT;
using KSP.Sim.impl;
using static KSP.Api.UIDataPropertyStrings.View;

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
    private bool _wasActive;
    private bool _coolingVFXON;
    private bool _coolingVFXLOOP;
    private bool _coolingVFXOFF = false;
    private string _activeVesselGuid = "";
    internal new static ManualLogSource Logger { get; set; }
    public bool IsActive { get; set; }

    private void Start()
    {
        _moduleResourceCapacities = GetComponent<Module_ResourceCapacities>();
        _triggerVFX = GetComponentInChildren<TriggerVFXFromAnimation>();
        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        _fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual fuel resource.
        _animator = GetComponent<Animator>();
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

    private void FixedUpdate()
    {

        IResourceContainer fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];

        Logger.LogInfo("fuelContainer: " + fuelContainer);

        _vesselComponent = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);


        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        _animator.SetFloat("FuelLevel", fuelLevel);

        Logger.LogInfo("fuelLevel: " + fuelLevel);

        bool _isActive = _isActiveVessel.GetValueBool();

        Logger.LogInfo("_isActive: " + _isActive);
        Logger.LogInfo("_wasActive: " + _wasActive);

        if (_isActive && !_wasActive)
        {
            if (fuelLevel > 0.8f)
            {
                _animator.Play("CoolingVFX_ON");
                Logger.LogInfo("CoolingVFX_ON: " + _coolingVFXON);
            }
            else
            {
                _animator.Play("CoolingVFX_LOOP");
                Logger.LogInfo("CoolingVFX_LOOP: " + _coolingVFXLOOP);
            }

            _coolingVFXOFF = false;
        }
        else if (!_isActive && _wasActive && !_coolingVFXOFF)
        {
            if (fuelLevel < 0.8f)
            {
                _animator.Play("CoolingVFX_OFF");
                _coolingVFXOFF = true;
                Logger.LogInfo("CoolingVFX_OFF: " + _coolingVFXOFF);
            }
        }

        _wasActive = _isActive;
    }
}
