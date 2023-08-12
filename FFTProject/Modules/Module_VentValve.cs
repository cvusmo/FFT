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
        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;
        public ParticleSystem PSVentValveVFX;
        public ParticleSystem PSCoolingVFX;

        //internal FFT scripts
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel, isInAtmosphere, fuelCheck;
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL, FL, AT;
        internal bool InAtmo = true;

        //update frequency
        private float updateFrequency = 3.0f;
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
            }

            if (CoolingVFX != null)
            {
                PSCoolingVFX = CoolingVFX.GetComponentInChildren<ParticleSystem>();
            }

            RefreshVesselData = new RefreshVesselData();
            Animator = GetComponentInParent<Animator>();
            TriggerVFX = GetComponentInParent<TriggerVFXFromAnimation>();
            GravityForVFX = GetComponentInParent<DynamicGravityForVFX>();

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

            if (MaxAltitudeAchieved() && FuelLevelExceedsThreshold())
            {
                StartVFX();
                FFTPlugin.Logger.LogInfo("StartVFX");
            }
            else if (Mathf.Approximately(ASL, 0) && Mathf.Approximately(AGL, 0) || FL < 0.95 || InAtmo is false)
            {
                StopVFX();
                FFTPlugin.Logger.LogInfo("StopVFX");
            }
        }
        private void UpdateVFX()
        {
            var altitudeSeaLevel = RefreshVesselData.altitudeAsl.altitudeAsl;
            ASL = DataVentValve.VFXASLCurve.Evaluate((float)altitudeSeaLevel);
            Animator.SetFloat("ASL", ASL);

            var altitudeGroundLevel = RefreshVesselData.altitudeAgl.altitudeAgl;
            AGL = DataVentValve.VFXASLCurve.Evaluate((float)altitudeGroundLevel);
            Animator.SetFloat("AGL", AGL);

            //var atmosphericTemperature = RefreshVesselData.atmosphericTemperature.atmosphericTemperature;
            //AT = DataVentValve.VFXAtmosphericTemperature.Evaluate((float)atmosphericTemperature);
            //Animator.SetFloat("AT", AT);

            var fuelPercentage = RefreshVesselData.fuelPercentage.fuelPercentage;
            double scaledFuelPercentage = fuelPercentage / 100.0;
            FFTPlugin.Logger.LogInfo("fuelPercentage is: " + scaledFuelPercentage);
            FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
            Animator.SetFloat("FL", FL);

            var isInAtmosphere = RefreshVesselData.isInAtmosphere.isInAtmosphere;
            InAtmo = isInAtmosphere;
            Animator.SetBool("InAtmo", InAtmo);
        }
        public void StartVFX()
        {
            EnableEmission();
            if (PSVentValveVFX != null)
            {
                PSVentValveVFX.Play();
                FFTPlugin.Logger.LogInfo("PSVentValveVFX Start");
            }
            if (PSCoolingVFX != null)
            {
                PSCoolingVFX.Play();
                FFTPlugin.Logger.LogInfo("PSCoolingVFX Start");
            }
        }

        public void StopVFX()
        {
            DisableEmission();
            if (PSVentValveVFX != null)
            {
                PSVentValveVFX.Stop();
                FFTPlugin.Logger.LogInfo("PSVentValveVFX Stop");
            }
            if (PSCoolingVFX != null)
            {
                PSCoolingVFX.Stop();
                FFTPlugin.Logger.LogInfo("PSCoolingVFX Stop");
            }
        }
        internal void EnableEmission()
        {
            if (PSCoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = true;
            }
            if (PSVentValveVFX!= null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (PSCoolingVFX != null)
            {
                var emission = PSCoolingVFX.emission;
                emission.enabled = false;
            }
            if (PSVentValveVFX != null)
            {
                var emission = PSVentValveVFX.emission;
                emission.enabled = false;
            }
        }
        internal bool MaxAltitudeAchieved()
        {
            return ASL > 0 || AGL > 0;
        }
        public void Activate()
        {
            activateModuleVentValve = true;
        }
        internal bool FuelLevelExceedsThreshold()
        {
            if (fuelCheck < 0 || fuelCheck > 1)
            {
                FFTPlugin.Logger.LogError("fuelCheck is out of range: " + fuelCheck);
                return false;
            }
            return fuelCheck > 0.95f;
        }
    }
}