//|=====================Summary========================|0|
//|            Module for Cooling/Vent VFX             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using FFT.Utilities;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

        [SerializeField] private GameObject VentValveDefinitions;
        [SerializeField] private GameObject FuelTankDefinitions;
        [SerializeField] public GameObject VentValveVFX;
        [SerializeField] public GameObject CoolingVFX;

        public DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;
        public Animator Animator;
        public ParticleSystem PSVentValveVFX, PSCoolingVFX;
        private Data_FuelTanks _dataFuelTanks;
        private Data_ValveParts _dataValveParts;
        private Data_VentValve _dataVentValve;
        private FuelTankDefinitions _fuelTankDefinitions;
        private VentValveDefinitions _ventValveDefinitions;

        internal float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        internal bool InAtmo = true;
        internal bool ActivateModule;
        internal float updateFrequency = 0.5f;
        internal float timeSinceLastUpdate = 0.0f;
        public RefreshVesselData RefreshVesselData { get; private set; }
        public override void OnInitialize()
        {
            AddDataModules();
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
            if (this._dataVentValve == null)
                this._dataVentValve = new Data_VentValve();
            this.DataModules.TryAddUnique<Data_VentValve>(this._dataVentValve, out this._dataVentValve);
        }
        internal void InitializeData()
        {
            if (_fuelTankDefinitions == null)
            {
                _fuelTankDefinitions = FindObjectOfType<FuelTankDefinitions>();
                FFTPlugin.Instance._logger.LogDebug("FuelTankDefinitions is null.");
            }
            if (_ventValveDefinitions == null)
            {
                _ventValveDefinitions = FindObjectOfType<VentValveDefinitions>();
                FFTPlugin.Instance._logger.LogDebug("VentValveDefinitions is null.");
            }

            if (_fuelTankDefinitions != null && _dataFuelTanks != null)
            {
                _fuelTankDefinitions.PopulateFuelTanks(_dataFuelTanks);
                FFTPlugin.Instance._logger.LogDebug("_dataFuelTanks is null.");
            }

            if (_ventValveDefinitions != null && _dataValveParts != null)
            {
                _ventValveDefinitions.PopulateVentValve(_dataValveParts);
                FFTPlugin.Instance._logger.LogDebug("_dataValveParts is null.");
            }
        }
        internal void InitializeVFX()
        {
            FFTPlugin.Instance._logger.LogInfo("Module_VentValveVFX has started.");

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
            FFTPlugin.Instance._logger.LogDebug("InitializeVFX successfully started");
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
                InitializeData();
                InitializeVFX();
                UpdateVFX();
            }
            catch (Exception ex)
            {
                FFTPlugin.Instance._logger.LogError($"Failed to refresh data and VFX: {ex.Message}");
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

            ASL = GetCurveValue(_dataVentValve.VFXASLCurve, (float)vesselData.AltitudeAsl);
            Animator.SetFloat("ASL", ASL);

            AGL = GetCurveValue(_dataVentValve.VFXAGLCurve, (float)vesselData.AltitudeAgl);
            Animator.SetFloat("AGL", AGL);

            VV = GetCurveValue(_dataVentValve.VFXVerticalVelocity, (float)vesselData.VerticalVelocity);
            Animator.SetFloat("VV", VV);

            HV = GetCurveValue(_dataVentValve.VFXHorizontalVelocity, (float)vesselData.HorizontalVelocity);
            Animator.SetFloat("HV", HV);

            DP = GetCurveValue(_dataVentValve.VFXDynamicPressure, (float)vesselData.DynamicPressure_kPa);
            Animator.SetFloat("DP", DP);

            SP = GetCurveValue(_dataVentValve.VFXStaticPressure, (float)vesselData.StaticPressure_kPa);
            Animator.SetFloat("SP", SP);

            AT = GetCurveValue(_dataVentValve.VFXAtmosphericTemperature, (float)vesselData.AtmosphericTemperature);
            Animator.SetFloat("AT", AT);

            ET = GetCurveValue(_dataVentValve.VFXExternalTemperature, (float)vesselData.ExternalTemperature);
            Animator.SetFloat("ET", ET);

            InAtmo = vesselData.IsInAtmosphere;

            var fuelPercentage = vesselData.FuelPercentage;
            double scaledFuelPercentage = vesselData.FuelPercentage / 100.0;
            FL = _dataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
            Animator.SetFloat("FL", FL);
        }
        internal void StartVFX()
        {
            if (VentValveVFX != null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = true;
                PSVentValveVFX.Play();
            }
            if (CoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = true;
                PSCoolingVFX.Play();
            }
            
        }
        internal void StopVFX()
        {
            if (VentValveVFX != null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = false;
                PSVentValveVFX.Stop();
            }
            if (CoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = false;
                PSCoolingVFX.Stop();
            }
        }
        internal void Activate()
        {
            StartVFX();
            ActivateModule = true;
            FFTPlugin.Instance._logger.LogInfo("Module_VentValve activated.");        
        }
        internal void Deactivate()
        {
            StopVFX();
            ActivateModule = false;
            FFTPlugin.Instance._logger.LogInfo("Module_VentValve deactivated."); 
        }
    }
}