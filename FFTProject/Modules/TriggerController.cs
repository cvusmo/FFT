using KSP.Modules;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using UnityEngine;
using KSP.Animation;
using KSP.Game;

public class TriggerController : MonoBehaviour
{
    private Module_ResourceCapacities moduleResourceCapacities;
    private TriggerVFXFromAnimation triggerVFX;
    private ResourceDefinitionID fuelResourceId;
    private Animator animator;

    private void Start()
    {
        moduleResourceCapacities = GetComponent<Module_ResourceCapacities>();

        triggerVFX = GetComponent<TriggerVFXFromAnimation>();

        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual name of your fuel resource.

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        IResourceContainer fuelContainer = moduleResourceCapacities.OABPart.Containers[0];

        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(fuelResourceId);

        if (fuelLevel > 0.8)
        {
            triggerVFX.VFX01_ON();
            animator.SetBool("IsFuelAbove80", true);
        }
        else
        {
            triggerVFX.VFX01_OFF();
            animator.SetBool("IsFuelAbove80", false);
        }
    }
}
