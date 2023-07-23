using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Modules;
using KSP.Sim.ResourceSystem;
using UnityEngine;
public class TriggerController : MonoBehaviour
{
    private Module_ResourceCapacities moduleResourceCapacities;
    private TriggerVFXFromAnimation triggerVFX;
    private ResourceDefinitionID fuelResourceId;
    private Animator animator;
    private IsActiveVessel _isActiveVessel;
    public bool IsActive { get; set; }

    private void Start()
    {
        moduleResourceCapacities = GetComponent<Module_ResourceCapacities>();
        triggerVFX = GetComponentInChildren<TriggerVFXFromAnimation>();
        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual name of your fuel resource.
        animator = GetComponent<Animator>();
        _isActiveVessel = new IsActiveVessel();
    }

    private void FixedUpdate()
    {
        IResourceContainer fuelContainer = moduleResourceCapacities.OABPart.Containers[0];
        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(fuelResourceId);
        animator.SetFloat("FuelLevel", fuelLevel);

        IsActive = _isActiveVessel.GetValueBool();

        if (!IsActive)
            return;
    }
}
