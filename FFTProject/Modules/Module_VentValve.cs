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
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;
        public TriggerVFXFromAnimation TriggerVFXFromAnimation;
        public ParticleSystem PSVentValveVFX;
        public ParticleSystem PSCoolingVFX;

        //internal FFT scripts
        internal float dynamicPressure, atmosphericTemp, externalTemp, verticalSpeed, horizontalSpeed, altitudeSeaLevel, altitudeGroundLevel, isInAtmosphere, fuelCheck;
        internal bool activateModuleVentValve = false;
        internal float ASL, AGL, FL, AT;
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
            }

            if (CoolingVFX != null)
            {
                PSCoolingVFX = CoolingVFX.GetComponentInChildren<ParticleSystem>();
            }

            RefreshVesselData = new RefreshVesselData();
            Animator = GetComponentInParent<Animator>();
            TriggerVFXFromAnimation = GetComponentInParent<TriggerVFXFromAnimation>();
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

            //var atmosphericTemperature = RefreshVesselData.atmosphericTemperature.atmosphericTemperature;
            //AT = DataVentValve.VFXAtmosphericTemperature.Evaluate((float)atmosphericTemperature);
            //Animator.SetFloat("AT", AT);

            var fuelPercentage = RefreshVesselData.fuelPercentage.fuelPercentage;
            double scaledFuelPercentage = fuelPercentage / 100.0;
            FL = DataVentValve.VFXOpacityCurve.Evaluate((float)scaledFuelPercentage);
            Animator.SetFloat("FL", FL);

            var isInAtmosphere = RefreshVesselData.isInAtmosphere.isInAtmosphere;
            InAtmo = isInAtmosphere;
            Animator.SetBool("InAtmo", InAtmo);
        }
        public void StartVFX()
        {
            if (PSVentValveVFX != null)
            {
                PSVentValveVFX.Play();
                var emission = PSVentValveVFX.emission;
                emission.enabled = true;
            }
            if (PSCoolingVFX != null)
            {

                PSCoolingVFX.Play();
                var emission = PSCoolingVFX.emission;
                emission.enabled = true;
            }
        }
        public void StopVFX()
        {
            if (PSVentValveVFX != null)
            {
                PSVentValveVFX.Stop();
                var emission = PSVentValveVFX.emission;
                emission.enabled = false;
            }
            if (PSCoolingVFX != null)
            {
                PSCoolingVFX.Stop();
                var emission = PSCoolingVFX.emission;
                emission.enabled = false;
            }
        }
        public void Activate()
        {
            activateModuleVentValve = true;
        }
        internal void VFXConditions()
        {
            if ((ASL < 0.98 && FL > 0.95 || AGL < 0.98 && FL > 0.95))
            {
                StartVFX();
            }
            else if ((ASL >= 0.99 && FL < 0.95 || AGL >= 0.99 && FL < 0.95))
            {
                StopVFX();
            }
        }
    }
}