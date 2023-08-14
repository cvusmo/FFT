using KSP.Animation;
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
        public Data_ValveParts DataValveParts;
        [SerializeField]
        public Data_FuelTanks DataFuelTanks;
        [SerializeField]
        public GameObject VentValveVFX;
        [SerializeField]
        public GameObject CoolingVFX;

        //unity scripts
        public Animator Animator;
        public ParticleSystem PSVentValveVFX, PSCoolingVFX;
        public DynamicGravityForVFX DynamicGravityVent, DynamicGravityCooling;

        //internal FFT scripts
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL, VV, HV, DP, SP, AT, ET, FL;
        internal bool InAtmo = true;

        //update frequency
        private float updateFrequency = 2f;
        private float timeSinceLastUpdate = 0.0f;
        internal RefreshVesselData RefreshVesselData { get; private set; }
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
            Animator.SetFloat("VV", VV);

            var horizontalVelocity = RefreshVesselData.horizontalVelocity.horizontalVelocity;
            float HVCurve = DataVentValve.VFXHorizontalVelocity.Evaluate((float)horizontalVelocity);
            HV = HVCurve;
            Animator.SetFloat("HV", HV);

            var dynamicPressure_KPa = RefreshVesselData.dynamicPressure_KPa.dynamicPressure_kPa;
            float DPCurve = DataVentValve.VFXDynamicPressure.Evaluate((float)dynamicPressure_KPa);
            DP = DPCurve;
            Animator.SetFloat("DP", DP);

            var staticPressure_KPa = RefreshVesselData.staticPressure_KPa.staticPressure_kPa;
            float SPCurve = DataVentValve.VFXStaticPressure.Evaluate((float)staticPressure_KPa);
            SP = SPCurve;
            Animator.SetFloat("SP", SP);

            var atmosphericTemperature = RefreshVesselData.atmosphericTemperature.atmosphericTemperature;
            float ATCurve = DataVentValve.VFXAtmosphericTemperature.Evaluate((float)atmosphericTemperature);
            AT = ATCurve;
            Animator.SetFloat("AT", AT);

            var externalTemperature = RefreshVesselData.externalTemperature.externalTemperature;
            float ETCurve = DataVentValve.VFXAtmosphericTemperature.Evaluate((float)externalTemperature);
            ET = ETCurve;
            Animator.SetFloat("ET", ET);

            var isInAtmosphere = RefreshVesselData.isInAtmosphere.isInAtmosphere;
            InAtmo = isInAtmosphere;
            Animator.SetBool("InAtmo", InAtmo);

            var fuelPercentage = RefreshVesselData.fuelPercentage.fuelPercentage;
            double scaledFuelPercentage = fuelPercentage / 100.0;
            FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
            Animator.SetFloat("FL", FL);

            bool deactivateVFX = (altitudeSeaLevel > 1000 && fuelPercentage < 95 || altitudeGroundLevel > 1000 && fuelPercentage < 95 || !InAtmo);
            if (!deactivateVFX)
            {
                StartVFX();
            }
            else
            {
                StopVFX();
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

        }
        internal void Activate()
        {
            activateModuleVentValve = true;
        }
    }
}