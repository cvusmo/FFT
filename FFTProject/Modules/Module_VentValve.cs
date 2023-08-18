//|=====================Summary========================|0|
//|               Fancy Fuel Tanks 0.1.4.1             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using FFT;
using FFT.Controllers;
using FFT.Managers;
using FFT.Modules;
using FFT.Utilities;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;
using MVV = BepInEx.Logging.Logger;

public class Module_VentValve : PartBehaviourModule
{
    private static Module_VentValve _instance;
    public static Module_VentValve Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Module_VentValve();
            }
            return _instance;
        }
    }
    public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

    [SerializeField] private Data_VentValve DataVentValve;
    [SerializeField] private GameObject VentValveVFX;
    [SerializeField] private GameObject CoolingVFX;

    private Animator Animator;
    private ParticleSystem PSVentValveVFX, PSCoolingVFX;
    private DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;

    private readonly ManualLogSource _logger = MVV.CreateLogSource("FFT.Module_VentValve: ");
    private ModuleController _moduleController;
    private ConditionsManager _conditionsManager;

    private delegate void ModuleActivationChangedHandler(bool isActive);
    private event ModuleActivationChangedHandler OnModuleActivationChanged;
    private event System.Action VFXConditionsMet = delegate { };

    public FuelTankDefinitions FuelTankDefinitions { get; private set; }
    public VentValveDefinitions VentValveDefinitions { get; private set; }
    public Data_FuelTanks DataFuelTanks { get; private set; } = new Data_FuelTanks();
    public Data_ValveParts DataValveParts { get; private set; }

    private float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
    private bool InAtmo = true;
    private bool ActivateModule;
    private bool isVFXActive = false;
    private bool wasActive = true;
    private float updateFrequency = 0.5f;
    private float timeSinceLastUpdate = 0.0f;
    private Module_VentValve()
    {
        if (_instance != null && _instance != this)
        {
            throw new System.Exception("Multiple singletons of Module_VentValve detected.");
        }
        _instance = this;
    }
    public override void OnInitialize()
    {
        base.OnInitialize();
        InitializeData();
        if (PartBackingMode == PartBackingModes.Flight)
        {
            InitializeVFX();
        }
    }
    internal void InitializeData()
    {
        FuelTankDefinitions.PopulateFuelTanks(DataFuelTanks);
        VentValveDefinitions.PopulateVentValve(DataValveParts);
        InitializeVFX();
    }
    internal void InitializeVFX()
    {
        _logger.LogInfo("Module_VentValveVFX has started.");

        if (VentValveVFX != null)
        {
            PSVentValveVFX = VentValveVFX.GetComponentInChildren<ParticleSystem>();
            DynamicGravityVent = VentValveVFX.GetComponentInChildren<DynamicGravityForVFX>();
        }

        if (CoolingVFX != null)
        {
            PSCoolingVFX = CoolingVFX.GetComponentInChildren<ParticleSystem>();
            DynamicGravityCooling = CoolingVFX.GetComponentInParent<DynamicGravityForVFX>();
        }

        Animator = GetComponentInParent<Animator>();
    }
    public override void OnModuleFixedUpdate(float fixedDeltaTime)
    {
        if (!ActivateModule) return;
        base.OnModuleFixedUpdate(fixedDeltaTime);

        timeSinceLastUpdate += fixedDeltaTime;

        if (timeSinceLastUpdate >= updateFrequency)
        {
            RefreshDataAndVFX();
            timeSinceLastUpdate = 0.0f;
        }
    }
    private void RefreshDataAndVFX()
    {
        try
        {
            RefreshVesselData.Instance.RefreshActiveVesselInstance.RefreshData();
            InitializeData();
            InitializeVFX();
            UpdateVFX();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to refresh data and VFX: {ex.Message}");
        }
    }
    private float GetCurveValue(AnimationCurve curve, float inputValue)
    {
        if (curve == null)
        {
            Debug.LogWarning("Curve is null. Defaulting to 0.");
            return 0f;
        }

        return curve.Evaluate(inputValue);
    }
    private void UpdateVFX()
    {
        var vesselData = RefreshVesselData.Instance;

        ASL = GetCurveValue(DataVentValve.VFXASLCurve, (float)vesselData.AltitudeAsl);
        Animator.SetFloat("ASL", ASL);

        AGL = GetCurveValue(DataVentValve.VFXAGLCurve, (float)vesselData.AltitudeAgl);
        Animator.SetFloat("AGL", AGL);

        VV = GetCurveValue(DataVentValve.VFXVerticalVelocity, (float)vesselData.VerticalVelocity);
        Animator.SetFloat("VV", VV);

        HV = GetCurveValue(DataVentValve.VFXHorizontalVelocity, (float)vesselData.HorizontalVelocity);
        Animator.SetFloat("HV", HV);

        DP = GetCurveValue(DataVentValve.VFXDynamicPressure, (float)vesselData.DynamicPressure_kPa);
        Animator.SetFloat("DP", DP);

        SP = GetCurveValue(DataVentValve.VFXStaticPressure, (float)vesselData.StaticPressure_kPa);
        Animator.SetFloat("SP", SP);

        AT = GetCurveValue(DataVentValve.VFXAtmosphericTemperature, (float)vesselData.AtmosphericTemperature);
        Animator.SetFloat("AT", AT);

        ET = GetCurveValue(DataVentValve.VFXExternalTemperature, (float)vesselData.ExternalTemperature);
        Animator.SetFloat("ET", ET);

        InAtmo = vesselData.IsInAtmosphere;

        var fuelPercentage = vesselData.FuelPercentage;
        double scaledFuelPercentage = vesselData.FuelPercentage / 100.0;
        FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
        Animator.SetFloat("FL", FL);

        if (InAtmo)
        {
            ActivateModule = false;
        }

        if (ActivateModule)
        {
            VFXConditionsMet.Invoke();
        }
    }
    internal void OnPartModuleUpdate()
    {
        if (this.IsActive)
        {
            RefreshDataAndVFX();
        }
    }
    internal void OnPartModuleFixedUpdate(float fixedDeltaTime)
    {
        if (this.IsActive)
        {
            if (PartBackingMode == PartBackingModes.Flight)
            {
                timeSinceLastUpdate += fixedDeltaTime;

                if (timeSinceLastUpdate >= updateFrequency)
                {
                    RefreshDataAndVFX();
                    timeSinceLastUpdate = 0.0f;
                }
            }
        }
    }
    public void Activate()
    {
        ActivateModule = true;
        _logger.LogInfo("Module_VentValve activated.");
    }

    public void Deactivate()
    {
        ActivateModule = false;
        _logger.LogInfo("Module_VentValve deactivated.");
    }
}
