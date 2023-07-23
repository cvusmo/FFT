using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Modules;
using KSP.Sim.ResourceSystem;
using UnityEngine;
public class TriggerController : MonoBehaviour
{
    private Module_ResourceCapacities _moduleResourceCapacities;
    private TriggerVFXFromAnimation _triggerVFX;
    private ResourceDefinitionID _fuelResourceId;
    private Animator _animator;
    private IsActiveVessel _isActiveVessel;
    private bool _wasActive;
    private bool _coolingVFXOFF = false;
    public bool IsActive { get; set; }

    private void Start()
    {
        _moduleResourceCapacities = GetComponent<Module_ResourceCapacities>();
        _triggerVFX = GetComponentInChildren<TriggerVFXFromAnimation>();
        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        _fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual name of your fuel resource.
        _animator = GetComponent<Animator>();
        _isActiveVessel = new IsActiveVessel();
    }

    private void FixedUpdate()
    {
        IResourceContainer fuelContainer = _moduleResourceCapacities.OABPart.Containers[0];
        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(_fuelResourceId);
        _animator.SetFloat("FuelLevel", fuelLevel);

        bool _isActive = _isActiveVessel.GetValueBool();

        if (_isActive && !_wasActive)
        {
            if (fuelLevel > 0.8f)
                _animator.Play("CoolingVFX_ON");
            else
                _animator.Play("CoolingVFX_LOOP");

            _coolingVFXOFF = false;
        }
        else if (!_isActive && _wasActive && !_coolingVFXOFF)
        {
            if (fuelLevel < 0.8f)
            {
                _animator.Play("CoolingVFX_OFF");
                _coolingVFXOFF = true;
            }
        }

        _wasActive = _isActive;
    }
}
