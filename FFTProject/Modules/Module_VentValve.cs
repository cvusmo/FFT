//|=====================Summary========================|0|
//|               Fancy Fuel Tanks 0.1.4.1             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Controllers;
using FFT.Utilities;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
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
        internal RefreshVesselData RefreshVesselData { get; private set; }
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                //
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
            ModuleEnums.IsVentValve = true;
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
            ModuleEnums.IsVentValve = false;
            ActivateModule = false;
            OnModuleActivationChanged?.Invoke(false);

            ManageParticleSystem(PSVentValveVFX, DynamicGravityVent, false);
            ManageParticleSystem(PSCoolingVFX, DynamicGravityCooling, false);

            UpdateAnimator(false);

            isVFXActive = false;
            wasActive = false;
        }
    }
}