using KSP.Animation;
using KSP.Game;
using KSP.Modules;
using KSP.Sim.ResourceSystem;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    private Module_ResourceCapacities moduleResourceCapacities;
    private TriggerVFXFromAnimation triggerVFX;
    private ResourceDefinitionID fuelResourceId;
    private Animator animator;
    private ParticleSystem particleSystem;

    private void Start()
    {
        moduleResourceCapacities = GetComponent<Module_ResourceCapacities>();
        triggerVFX = GetComponent<TriggerVFXFromAnimation>();
        ResourceDefinitionDatabase definitionDatabase = GameManager.Instance.Game.ResourceDefinitionDatabase;
        fuelResourceId = definitionDatabase.GetResourceIDFromName("Methalox"); // Replace "Fuel" with the actual name of your fuel resource.
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        IResourceContainer fuelContainer = moduleResourceCapacities.OABPart.Containers[0];
        float fuelLevel = (float)fuelContainer.GetResourceStoredUnits(fuelResourceId);
        animator.SetFloat("FuelLevel", fuelLevel);

        if (fuelLevel > 0.8)
        {
            triggerVFX.VFX01_ON();

            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            Debug.Log("VFX01_ON called");
            animator.SetBool("IsFuelAbove80", true);
        }
        else
        {
            triggerVFX.VFX01_OFF();

            if (particleSystem != null)
            {
                particleSystem.Stop();
            }
            Debug.Log("VFX01_OFF called");
            animator.SetBool("IsFuelAbove80", false);
        }
    }
}
