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
    public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

    [SerializeField] private Data_VentValve DataVentValve;
    [SerializeField] private GameObject VentValveVFX;
    [SerializeField] private GameObject CoolingVFX;

    private RefreshVesselData RefreshVesselData { get; set; }
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

    public Module_VentValve()
    {
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
    public override void AddDataModules()
    {
        base.AddDataModules();

        if (this.DataVentValve == null)
        {
            this.DataVentValve = new Data_VentValve();
            this.DataModules.TryAddUnique<Data_VentValve>(this.DataVentValve, out this.DataVentValve);
        }
        if (this.DataFuelTanks == null)
        {
            //this.DataFuelTanks ??= new Data_FuelTanks();
            //this.DataModules.TryAddUnique<Data_FuelTanks>(this.DataFuelTanks, out this.DataFuelTanks);
        }
    }
    internal void InitializeData()
    {

        if (FuelTankDefinitions == null)
        {
            FuelTankDefinitions = UnityEngine.Object.FindObjectOfType<FuelTankDefinitions>();
        }
        if (FuelTankDefinitions != null && DataFuelTanks != null)
        {
            FuelTankDefinitions.PopulateFuelTanks(DataFuelTanks);
        }

        if (VentValveDefinitions == null)
        {
            VentValveDefinitions = UnityEngine.Object.FindObjectOfType<VentValveDefinitions>();
        }
        if (VentValveDefinitions != null && DataValveParts != null)
        {
            VentValveDefinitions.PopulateVentValve(DataValveParts);
        }
        InitializeVFX();
    }
    internal void InitializeVFX()
    {
        if (_conditionsManager != null && _logger != null)
        {
            _logger.LogInfo("Module_VentValveVFX has started.");
        }
        else
        {
            throw new System.InvalidOperationException("Manager or its logger was not properly initialized.");
        }
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

        RefreshVesselData = new RefreshVesselData();
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
        RefreshVesselData.refreshActiveVessel.RefreshData();
        var activeVessel = RefreshVesselData.refreshActiveVessel.ActiveVessel;
        RefreshVesselData.RefreshAll(activeVessel);

        InitializeData();
        InitializeVFX();
        UpdateVFX();
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
        ASL = GetCurveValue(DataVentValve.VFXASLCurve, (float)RefreshVesselData.altitudeAsl.altitudeAsl);
        Animator.SetFloat("ASL", ASL);

        AGL = GetCurveValue(DataVentValve.VFXAGLCurve, (float)RefreshVesselData.altitudeAgl.altitudeAgl);
        Animator.SetFloat("AGL", AGL);

        VV = GetCurveValue(DataVentValve.VFXVerticalVelocity, (float)RefreshVesselData.verticalVelocity.verticalVelocity);
        Animator.SetFloat("VV", VV);

        HV = GetCurveValue(DataVentValve.VFXHorizontalVelocity, (float)RefreshVesselData.horizontalVelocity.horizontalVelocity);
        Animator.SetFloat("HV", HV);

        DP = GetCurveValue(DataVentValve.VFXDynamicPressure, (float)RefreshVesselData.dynamicPressure_KPa.dynamicPressure_kPa);
        Animator.SetFloat("DP", DP);

        SP = GetCurveValue(DataVentValve.VFXStaticPressure, (float)RefreshVesselData.staticPressure_KPa.staticPressure_kPa);
        Animator.SetFloat("SP", SP);

        AT = GetCurveValue(DataVentValve.VFXAtmosphericTemperature, (float)RefreshVesselData.atmosphericTemperature.atmosphericTemperature);
        Animator.SetFloat("AT", AT);

        ET = GetCurveValue(DataVentValve.VFXExternalTemperature, (float)RefreshVesselData.externalTemperature.externalTemperature);
        Animator.SetFloat("ET", ET);

        var isInAtmosphere = RefreshVesselData.isInAtmosphere.isInAtmosphere;
        InAtmo = isInAtmosphere;
        Animator.SetBool("InAtmo", InAtmo);

        var fuelPercentage = RefreshVesselData.fuelPercentage.fuelPercentage;
        double scaledFuelPercentage = fuelPercentage / 100.0;
        FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
        Animator.SetFloat("FL", FL);

        bool shouldActivateVFX = ShouldActivateVFX(RefreshVesselData.altitudeAsl.altitudeAsl, RefreshVesselData.altitudeAgl.altitudeAgl, RefreshVesselData.fuelPercentage.fuelPercentage);

        if (shouldActivateVFX && !isVFXActive && !wasActive)
        {
            Activate();
        }
        else if (!shouldActivateVFX && isVFXActive)
        {
            Deactivate();
        }
    }
    private void ManageParticleSystem(ParticleSystem ps, DynamicGravityForVFX dynamicGravity, bool state)
    {
        if (ps != null)
        {
            var main = ps.main;
            main.loop = state;
            dynamicGravity.enabled = state;
            if (state)
                ps.Play();
            else
                ps.Stop();
        }
    }
    private bool ShouldActivateVFX(double altitudeasl, double altitudeagl, double fuelPercentage)
    {
        return (altitudeasl < 1000 && fuelPercentage > 95) || (altitudeagl < 1000 && fuelPercentage > 95) && InAtmo;
    }
    private void UpdateAnimator(bool state)
    {
        Animator.SetBool("VentValveVFXActive", state);
        Animator.SetBool("CoolingVFXActive", state);
    }
    internal void Activate()
    {
        _moduleController.SetVentValveState(true);
        ActivateModule = true;
        OnModuleActivationChanged?.Invoke(true);
        VFXConditionsMet.Invoke();

        ManageParticleSystem(PSVentValveVFX, DynamicGravityVent, true);
        ManageParticleSystem(PSCoolingVFX, DynamicGravityCooling, true);

        UpdateAnimator(true);

        isVFXActive = true;
        wasActive = true;
    }
    internal void Deactivate()
    {
        _moduleController.SetVentValveState(false);
        ActivateModule = false;
        OnModuleActivationChanged?.Invoke(false);

        ManageParticleSystem(PSVentValveVFX, DynamicGravityVent, false);
        ManageParticleSystem(PSCoolingVFX, DynamicGravityCooling, false);

        UpdateAnimator(false);

        isVFXActive = false;
        wasActive = false;
    }
}