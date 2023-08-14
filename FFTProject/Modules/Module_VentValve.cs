using KSP.Animation;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;
using static FFT.Modules.RefreshVesselData;

namespace FFT.Modules
{
    public class Module_VentValve : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_VentValve);

        [SerializeField]
        public Data_VentValve DataVentValve;
        [SerializeField]
        public Data_ValveParts DataValveParts;
        [SerializeField]
        public Data_FuelTanks DataFuelTanks;
        [SerializeField]
        public GameObject VentValveVFX;
        [SerializeField]
        public GameObject CoolingVFX;

        //unity scripts
        public Animator Animator;
        public TriggerVFXFromAnimation TriggerCooling, TriggerVentValve;
        public ParticleSystem PSVentValveVFX;
        public ParticleSystem PSCoolingVFX;
        public DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;

        //internal FFT scripts
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel, isInAtmosphere, fuelCheck;
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        internal bool InAtmo = true;

        //update frequency
        private float updateFrequency = 0.15f;
        private float timeSinceLastUpdate = 0.0f;
        public RefreshVesselData RefreshVesselData { get; private set; }
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                InitializeVFX();
            }
        }
        public void InitializeVFX()
        {
            if (VentValveVFX != null)
            {
                PSVentValveVFX = VentValveVFX.GetComponentInChildren<ParticleSystem>();
                DynamicGravityVent = VentValveVFX.GetComponentInChildren<DynamicGravityForVFX>();
                TriggerVentValve = VentValveVFX.GetComponentInParent<TriggerVFXFromAnimation>();
            }

            if (CoolingVFX != null)
            {
                PSCoolingVFX = CoolingVFX.GetComponentInChildren<ParticleSystem>();
                DynamicGravityCooling = CoolingVFX.GetComponentInParent<DynamicGravityForVFX>();
                TriggerCooling = CoolingVFX.GetComponentInParent<TriggerVFXFromAnimation>();
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
            if (this.DataFuelTanks == null)
            {
                this.DataFuelTanks ??= new Data_FuelTanks();
                this.DataModules.TryAddUnique<Data_FuelTanks>(this.DataFuelTanks, out this.DataFuelTanks);
            }
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);

            timeSinceLastUpdate += fixedDeltaTime;

            if (timeSinceLastUpdate >= updateFrequency)
            {
                RefreshVesselData.refreshActiveVessel.RefreshData();
                var activeVessel = RefreshVesselData.refreshActiveVessel.ActiveVessel;
                RefreshVesselData.RefreshAll(activeVessel);

                VFXConditions();
                UpdateVFX();

                timeSinceLastUpdate = 0.0f;
            }           
        }
        private void UpdateVFX()
        {
            var altitudeSeaLevel = RefreshVesselData.altitudeAsl.altitudeAsl;
            float ASLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)altitudeSeaLevel);
            ASL = ASLFromCurve;
            Animator.SetFloat("ASL", ASL);

            var altitudeGroundLevel = RefreshVesselData.altitudeAgl.altitudeAgl;
            float AGLFromCurve = DataVentValve.VFXASLCurve.Evaluate((float)altitudeGroundLevel);
            AGL = AGLFromCurve;
            Animator.SetFloat("AGL", AGL);

            var verticalVelocity = RefreshVesselData.verticalVelocity.verticalVelocity;
            float VVCurve = DataVentValve.VFXVerticalVelocity.Evaluate((float)verticalVelocity);
            VV = VVCurve;
            double roundedverticalVelocity = Math.Round(verticalVelocity, 2);
            FFTPlugin.Logger.LogInfo("verticalVelocity: " + roundedverticalVelocity);
            Animator.SetFloat("VV", VV);

            var horizontalVelocity = RefreshVesselData.horizontalVelocity.horizontalVelocity;
            //float DPCurve = DataVentValve.VFXDynamicPressure.Evaluate((float)dynamicPressure_KPa);
            //DP = DPCurve;
            double roundedhorizontalVelocity = Math.Round(horizontalVelocity, 2);
            FFTPlugin.Logger.LogInfo("horizontalVelocity: " + roundedhorizontalVelocity);
            //Animator.SetFloat("DP", DP);

            var dynamicPressure_KPa = RefreshVesselData.dynamicPressure_KPa.dynamicPressure_kPa;
            float DPCurve = DataVentValve.VFXDynamicPressure.Evaluate((float)dynamicPressure_KPa);
            DP = DPCurve;
            double roundeddynamicPressure_KPa = Math.Round(dynamicPressure_KPa, 2);
            FFTPlugin.Logger.LogInfo("dynamicPressure_KPa: " + roundeddynamicPressure_KPa);
            Animator.SetFloat("DP", DP);

            var staticPressure_KPa = RefreshVesselData.staticPressure_KPa.staticPressure_kPa;
            float SPCurve = DataVentValve.VFXStaticPressure.Evaluate((float)staticPressure_KPa);
            SP = SPCurve;
            double roundedstaticPressure_kPa = Math.Round(staticPressure_KPa, 2);
            FFTPlugin.Logger.LogInfo("staticPressure_KPa: " + roundedstaticPressure_kPa);
            Animator.SetFloat("SP", SP);

            var atmosphericTemperature = RefreshVesselData.atmosphericTemperature.atmosphericTemperature;
            //AT = DataVentValve.VFXAtmosphericTemperature.Evaluate((float)atmosphericTemperature);
            //Animator.SetFloat("AT", AT);
            double roundedatmosphericTemperature = Math.Round(atmosphericTemperature, 2);
            //FFTPlugin.Logger.LogInfo("atmosphericTemperature: " + roundedatmosphericTemperature);

            var externalTemperature = RefreshVesselData.externalTemperature.externalTemperature;
            //AT = DataVentValve.VFXExternalTemperature.Evaluate((float)externalTemperature);
            //Animator.SetFloat("ET", ET);
            double roundedExternalTemperature = Math.Round(externalTemperature, 2);
            //FFTPlugin.Logger.LogInfo("externalTemperature: " + roundedExternalTemperature);

            var isInAtmosphere = RefreshVesselData.isInAtmosphere.isInAtmosphere;
            InAtmo = isInAtmosphere;
            Animator.SetBool("InAtmo", InAtmo);

            var fuelPercentage = RefreshVesselData.fuelPercentage.fuelPercentage;
            double scaledFuelPercentage = fuelPercentage / 100.0;
            FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
            Animator.SetFloat("FL", FL);
        }
        public void StartVFX()
        {
            FFTPlugin.Logger.LogInfo("StartVFX calling");
            if (PSVentValveVFX != null && ((ASL < 0.99 && FL > 0.96 || AGL < 0.99 && FL > 0.96 || InAtmo || VV < 199)))
            {
                TriggerVentValve.VFX01_ON();
                DynamicGravityVent.enabled = true;
                PSVentValveVFX.Play();
                var emission = PSVentValveVFX.emission;
                emission.enabled = true;
            }
            if (PSCoolingVFX != null && ((ASL < 0.99 && FL > 0.96 || AGL < 0.99 && FL > 0.96 || InAtmo || VV < 199)))
            {
                TriggerCooling.VFX01_ON();
                DynamicGravityCooling.enabled = true;
                PSCoolingVFX.Play();
                var emission = PSCoolingVFX.emission;
                emission.enabled = true;
            }
        }      
        public void StopVFX()
        {

            if (PSVentValveVFX != null && ((ASL > 0.98 && FL < 0.95 || AGL > 0.98 && FL < 0.95 || !InAtmo || VV > 200)))
            {
                TriggerVentValve.VFX01_OFF();
                DynamicGravityVent.enabled = false;
                PSVentValveVFX.Stop();
                var emission = PSVentValveVFX.emission;
                emission.enabled = false;
            }
            if (PSCoolingVFX != null && ((ASL > 0.98 && FL < 0.95 || AGL > 0.98 && FL < 0.95 || !InAtmo || VV > 200)))
            {
                TriggerCooling.VFX01_OFF();
                DynamicGravityCooling.enabled = false;
                PSCoolingVFX.Stop();
                var emission = PSCoolingVFX.emission;
                emission.enabled = false;
            }
        }
        internal void VFXConditions()
        {
            if (ASL < 0.99)
            {
                StartVFX();
                
            }
            else if (ASL > 0.98)
            {
                StopVFX();
                
            }
        }
        public void Activate()
        {
            activateModuleVentValve = true;
        }     
    }
}