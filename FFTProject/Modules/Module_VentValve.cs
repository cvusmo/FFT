//|=====================Summary========================|0|
//|               Fancy Fuel Tanks 0.1.4.1             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using KSP.Animation;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;
using VFX;
using FFT.Managers;
using FFT.Utilities;
using static FFT.Utilities.RefreshVesselData;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
        [JsonProperty]
        public int IsFlightActive;
        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

        [SerializeField]
        public Data_VentValve DataVentValve;   
        [SerializeField]
        public GameObject VentValveVFX;
        [SerializeField]
        public GameObject CoolingVFX;

        internal Data_FuelTanks DataFuelTanks;
        internal Animator Animator;
        internal ParticleSystem PSVentValveVFX, PSCoolingVFX;
        internal DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;
        internal delegate void ModuleActivationChangedHandler(bool isActive);
        internal event ModuleActivationChangedHandler OnModuleActivationChanged;

        internal float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        internal bool InAtmo = true;
        internal bool ActivateModule;
        internal bool isVFXActive = false;
        internal bool wasActive = true;

        private float updateFrequency = 0.5f;
        private float timeSinceLastUpdate = 0.0f;
        internal event Action VFXConditionsMet = delegate { };
        private static Module_VentValve _instance;
        internal RefreshVesselData RefreshVesselData { get; private set; }
        internal static Module_VentValve Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Module_VentValve();

                return _instance;
            }
        }
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                StartModule startModule = new StartModule();
                startModule.ModuleVentValve = this;
            }
        }
        internal void InitializeVFX()
        {
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
            FFTPlugin.Logger.LogInfo("Module_VentValveVFX has started.");
        }
        public override void AddDataModules()
        {
            base.AddDataModules();

            if (this.DataVentValve == null)
            {
                this.DataVentValve = new Data_VentValve();
                this.DataModules.TryAddUnique<Data_VentValve>(this.DataVentValve, out this.DataVentValve);
            }
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            if (!ActivateModule) return;
            base.OnModuleFixedUpdate(fixedDeltaTime);

            timeSinceLastUpdate += fixedDeltaTime;

            if (timeSinceLastUpdate >= updateFrequency)
            {
                RefreshVesselData.refreshActiveVessel.RefreshData();
                var activeVessel = RefreshVesselData.refreshActiveVessel.ActiveVessel;
                RefreshVesselData.RefreshAll(activeVessel);

                UpdateVFX();

                timeSinceLastUpdate = 0.0f;
            }
        }
        private float GetCurveValue(AnimationCurve curve, float inputValue) => curve.Evaluate(inputValue);
        internal void UpdateVFX()
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

            var altitudeasl = RefreshVesselData.altitudeAsl.altitudeAsl;
            var altitudeagl = RefreshVesselData.altitudeAgl.altitudeAgl;
            bool shouldActivateVFX = (altitudeasl < 1000 && fuelPercentage > 95) || (altitudeagl < 1000 && fuelPercentage > 95) && InAtmo;

            if (shouldActivateVFX && !isVFXActive && !wasActive)
            {
                VFXConditionsMet.Invoke();
                //StartVFX();
                isVFXActive = true;
            }
            else if (!shouldActivateVFX && isVFXActive)
            {
                //StopVFX();
                isVFXActive = false;
                wasActive = false;
            }

        }
        internal void StartVFX()
        {

            if (PSVentValveVFX != null)
            {
                var main = PSVentValveVFX.main;
                main.loop = true;
                DynamicGravityVent.enabled = true;
                PSVentValveVFX.Play();
                Animator.SetBool("VentValveVFXActive", true);
            }
            if (PSCoolingVFX != null)
            {
                var main = PSCoolingVFX.main;
                main.loop = true;
                DynamicGravityCooling.enabled = true;
                PSCoolingVFX.Play();
                Animator.SetBool("CoolingVFXActive", true);
            }
            isVFXActive = true;
        }
        internal void StopVFX()
        {
            if (PSVentValveVFX != null)
            {
                var main = PSVentValveVFX.main;
                main.loop = false;
                DynamicGravityVent.enabled = false;
                PSVentValveVFX.Stop();
                Animator.SetBool("VentValveVFXActive", false);
            }
            if (PSCoolingVFX != null)
            {
                var main = PSCoolingVFX.main;
                main.loop = false;
                DynamicGravityCooling.enabled = false;
                PSCoolingVFX.Stop();
                Animator.SetBool("CoolingVFXActive", false);              
            }
            isVFXActive = false;
            Deactivate();
        }
        internal void Activate()
        {
            ActivateModule = true;
            OnModuleActivationChanged?.Invoke(true);
        }
        internal void Deactivate()
        {
            ActivateModule = false;
            OnModuleActivationChanged?.Invoke(false);
        }
    }
}