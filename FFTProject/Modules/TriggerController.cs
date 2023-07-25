using FFT;
using KSP.Animation;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using SpaceWarp.API.Game;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    public TriggerVFXFromAnimation _triggerVFX;
    public Animator animator;
    public ParticleSystem _particleSystem;
    public IsActiveVessel _isActiveVessel;
    public VesselComponent _vesselComponent;
    public bool _wasActive;
    public bool _isActive;

    [SerializeField]
    private GameObject CoolingVFX;
    //internal new static ManualLogSource FFTPlugin.Logger { get; set; }
    public float FuelLevel
    {
        get
        {
            _vesselComponent.RefreshFuelPercentages();
            double fuelPercentage = _vesselComponent.StageFuelPercentage;

            if (fuelPercentage < 0)
            {
                FFTPlugin.Logger.LogError("Out of Fuel." + fuelPercentage);
                return 0f;
            }
            else
            {
                float level = (float)fuelPercentage;
                animator.SetFloat("FuelLevel", level);
                FFTPlugin.Logger.LogInfo("Fuel level: " + level);
                return level;
            }
        }
    }
    public void Awake()
    {
        FFTPlugin.Logger.LogInfo("Attached to GameObject: " + gameObject.name);

        _triggerVFX = GetComponent<TriggerVFXFromAnimation>();
        _particleSystem = CoolingVFX.GetComponent<ParticleSystem>();
        Animator animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            FFTPlugin.Logger.LogError("Animator component not found in parents.");
        }
        _isActiveVessel = new IsActiveVessel();
        _vesselComponent = new VesselComponent();

        FFTPlugin.Logger.LogInfo("TriggerController has started.");
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
            FFTPlugin.Logger.LogError("_particleSystem is null in EnableEmission");
        }
    }
    public void DisableEmission()
    {
        if (_particleSystem != null)
        {
            var emission = _particleSystem.emission;
            emission.enabled = false;
        }
        else
        {
            FFTPlugin.Logger.LogError("_particleSystem is null in DisableEmission");
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
            FFTPlugin.Logger.LogError("_particleSystem is null in _particleSystem.Play");
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
            FFTPlugin.Logger.LogError("_particleSystem is null in _particleSystem.Stop");
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
                FFTPlugin.Logger.LogInfo("IsActive set to true");
                if (!_wasActive)
                {
                    FFTPlugin.Logger.LogInfo("Was not previously active");
                    if (FuelLevelExceedsThreshold())
                    {
                        FFTPlugin.Logger.LogInfo("Fuel level exceeds threshold");
                        EnableEmission();
                        _triggerVFX.enabled = true;
                        StartParticleSystem();
                        animator.Play("CoolingVFX_ON");
                        FFTPlugin.Logger.LogInfo("CoolingVFX_ON: ");
                    }
                }
            }
            else
            {
                FFTPlugin.Logger.LogInfo("IsActive set to false");
                DisableEmission();
                _triggerVFX.enabled = false;
            }
            _wasActive = _isActive;
        }
    }
    private void FixedUpdate()
    {

        _vesselComponent = Vehicle.ActiveSimVessel;

        if (animator != null)
        {
            animator.SetBool("_isActive", _isActive);
        }
        else
        {
            FFTPlugin.Logger.LogError("animator is null in FixedUpdate");
            return;
        }

        if (_isActiveVessel != null)
        {
            FFTPlugin.Logger.LogInfo("_isActiveVessel.GetValueBool(): " + _isActiveVessel.GetValueBool());
            IsActive = _isActiveVessel.GetValueBool();
        }
        else
        {
            FFTPlugin.Logger.LogError("_isActiveVessel is null in FixedUpdate");
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
        if (FuelLevelExceedsThreshold() && !animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            _triggerVFX.enabled = true;
            StartParticleSystem();
            animator.Play("CoolingVFX_LOOP");
            FFTPlugin.Logger.LogInfo("CoolingVFX_LOOP: ");
            FFTPlugin.Logger.LogInfo("__fuelContainer: " + __fuelContainer);
        }
        else if (!FuelLevelExceedsThreshold() && animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
        {
            DisableCoolingEffects();
        }
    }
    private void DisableCoolingEffects()
    {
        animator.Play("CoolingVFX_OFF");
        _triggerVFX.enabled = true;
        StopParticleSystem();
        DisableEmission();
    }
}
