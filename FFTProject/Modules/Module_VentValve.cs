//|=====================Summary========================|0|
//|            Module for Cooling/Vent VFX             |1|
//|by cvusmo===========================================|4|
//|====================================================|2|
using FFT.Controllers;
using FFT.Utilities;
using KSP.Sim.Definitions;
using UnityEngine;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {

        [SerializeField] private FuelTankDefinitions _fuelTankDefinitions;
        [SerializeField] private VentValveDefinitions _ventValveDefinitions;

        public Dictionary<string, GameObject> fuelTankDict = new Dictionary<string, GameObject>();

        public Animator Animator;
        public AnimationBridge animationBridge;
        private Material _cachedMaterial;
        private Data_VentValve _dataVentValve;
        private Data_ValveParts _dataValveParts;
        private Data_FuelTanks _dataFuelTanks;
        private Renderer _cachedRenderer;

        internal float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        internal bool InAtmo = true;
        internal bool ActivateModule;
        internal float updateFrequency = 0.5f;
        internal float timeSinceLastUpdate = 0.0f;

        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);
        public RefreshVesselData RefreshVesselData { get; private set; }
        public override void OnInitialize()
        {
            base.OnInitialize();
            _cachedRenderer = GetComponent<Renderer>();
            _cachedMaterial = _cachedRenderer.material;
            AddDataModules();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                animationBridge = GetComponentInChildren<AnimationBridge>();
                LazyInitializeVFX();
            }
        }
        public override void AddDataModules()
        {
            Debug.Log("AddDataModules called...");
            base.AddDataModules();

            _dataVentValve ??= new Data_VentValve();
            DataModules.TryAddUnique(_dataVentValve, out _dataVentValve);

            _ventValveDefinitions ??= new VentValveDefinitions();
            DataModules.TryAddUnique(_ventValveDefinitions, out _ventValveDefinitions);

            _dataFuelTanks ??= new Data_FuelTanks();
            DataModules.TryAddUnique(_dataFuelTanks, out _dataFuelTanks);

            _fuelTankDefinitions ??= new FuelTankDefinitions();
            DataModules.TryAddUnique(_fuelTankDefinitions, out _fuelTankDefinitions);

            _dataValveParts ??= new Data_ValveParts();
            DataModules.TryAddUnique(_dataValveParts, out _dataValveParts);

            Debug.Log("Added Data Modules.");
        }
        private void LazyInitializeVFX()
        {
            Debug.Log("Module_VentValveVFX has started.");

            if (Animator == null)
            {
                Animator = GetComponentInParent<Animator>();
            }

            Activate();
            Debug.Log("InitializeVFX successfully started");
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            if (ActivateModule)
            {
                base.OnModuleFixedUpdate(fixedDeltaTime);

                timeSinceLastUpdate += fixedDeltaTime;

                if (timeSinceLastUpdate >= updateFrequency)
                {
                    RefreshDataAndVFX();
                    timeSinceLastUpdate = 0.0f;
                }
            }
        }
        private void RefreshDataAndVFX()
        {
            try
            {
                RefreshVesselData.Instance.RefreshActiveVesselInstance.RefreshData();
                UpdateVFX();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to refresh data and VFX: {ex.Message}");
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
            Material vaporMaterial = _cachedRenderer.material;

            ASL = GetCurveValue(_dataVentValve.VFXASLCurve, (float)vesselData.AltitudeAsl);
            AGL = GetCurveValue(_dataVentValve.VFXAGLCurve, (float)vesselData.AltitudeAgl);
            VV = GetCurveValue(_dataVentValve.VFXVerticalVelocity, (float)vesselData.VerticalVelocity);
            HV = GetCurveValue(_dataVentValve.VFXHorizontalVelocity, (float)vesselData.HorizontalVelocity);
            DP = GetCurveValue(_dataVentValve.VFXDynamicPressure, (float)vesselData.DynamicPressure_kPa);
            SP = GetCurveValue(_dataVentValve.VFXStaticPressure, (float)vesselData.StaticPressure_kPa);
            AT = GetCurveValue(_dataVentValve.VFXAtmosphericTemperature, (float)vesselData.AtmosphericTemperature);
            ET = GetCurveValue(_dataVentValve.VFXExternalTemperature, (float)vesselData.ExternalTemperature);
            double scaledFuelPercentage = vesselData.FuelPercentage / 100.0;
            FL = _dataVentValve.VFXFuelPercentage.Evaluate((float)scaledFuelPercentage);

            // Updating Animator
            Animator.SetFloat("ASL", ASL);
            Animator.SetFloat("AGL", AGL);
            Animator.SetFloat("VV", VV);
            Animator.SetFloat("HV", HV);
            Animator.SetFloat("DP", DP);
            Animator.SetFloat("SP", SP);
            Animator.SetFloat("AT", AT);
            Animator.SetFloat("ET", ET);
            Animator.SetFloat("FL", FL);

            // Update _cachedMaterial
            _cachedMaterial.SetFloat("_ASL", ASL);
            _cachedMaterial.SetFloat("_AGL", AGL);
            _cachedMaterial.SetFloat("_VV", VV);
            _cachedMaterial.SetFloat("_HV", HV);
            _cachedMaterial.SetFloat("_DP", DP);
            _cachedMaterial.SetFloat("_SP", SP);
            _cachedMaterial.SetFloat("_AT", AT);
            _cachedMaterial.SetFloat("_ET", ET);
            _cachedMaterial.SetFloat("_FL", FL);

            if (animationBridge != null)
            {
                float adjustedIntensity = animationBridge.intensity * ASL * 0.1f;
                float adjustedStepDistance = animationBridge.stepDistance * AGL * 0.1f;
                animationBridge.UpdateShaderProperties(_cachedMaterial, adjustedIntensity, adjustedStepDistance, ASL, AGL, VV, HV, DP, SP, AT, ET, FL);
            }
        }
        internal void Activate()
        {
            Material vaporMaterial = _cachedRenderer.material;
            _cachedMaterial.SetFloat("_IsVFXActive", 0.0f);
            vaporMaterial.SetFloat("_IsVFXActive", 1.0f);
            RefreshDataAndVFX();
            ActivateModule = true;
            Debug.Log("Module_VentValve activated.");
        }
        internal void Deactivate()
        {
            Material vaporMaterial = _cachedRenderer.material;
            _cachedMaterial.SetFloat("_IsVFXActive", 1.0f);
            vaporMaterial.SetFloat("_IsVFXActive", 0.0f);
            ActivateModule = false;
            Debug.Log("Module_VentValve deactivated.");
        }
    }
}