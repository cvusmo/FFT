//|=====================Summary========================|0|
//|            Module for Cooling/Vent VFX             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
using FFT.Utilities;
using KSP.Animation;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
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

        [SerializeField] public Data_VentValve DataVentValve;
        [SerializeField] public Data_ValveParts DataValveParts;
        [SerializeField] public GameObject VentValveVFX;
        [SerializeField] public GameObject CoolingVFX;

        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;
        public Animator Animator;
        internal ParticleSystem PSVentValveVFX, PSCoolingVFX;
        internal Data_FuelTanks DataFuelTanks;

        private event System.Action VFXConditionsMet = delegate { };

        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel;
        internal bool activateModuleVentValve = false;
        private float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        private bool InAtmo = true;
        private bool ActivateModule;
        private float updateFrequency = 0.5f;
        private float timeSinceLastUpdate = 0.0f;
        internal VentValveDefinitions VentValveDefinitions { get; private set; }
        internal FuelTankDefinitions FuelTankDefinitions { get; private set; }
        public RefreshVesselData RefreshVesselData { get; private set; }
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
        internal void StartVFX()
        {
            EnableEmission();
            PSVentValveVFX.Play();
        }
        internal void StopVFX()
        {
            DisableEmission();
            if (VentValveVFX != null)
            {
                PSVentValveVFX.Stop();
            }
            if (CoolingVFX != null)
            {
                PSCoolingVFX.Stop();
            }
        }
        internal void EnableEmission()
        {
            if (VentValveVFX != null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = true;
            }
            if (CoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (VentValveVFX != null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = false;
            }
            if (CoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = false;
            }
        }
        internal void Activate()
        {
            ActivateModule = true;
            FFTPlugin.Instance._logger.LogInfo("Module_VentValve activated.");
            StartVFX();
        }
        internal void Deactivate()
        {
            ActivateModule = false;
            FFTPlugin.Instance._logger.LogInfo("Module_VentValve deactivated.");
            StopVFX();
        }
    }
}