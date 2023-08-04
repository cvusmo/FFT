using KSP.Animation;
using KSP.Game;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;
using static FFT.Modules.RefreshVesselData;

namespace FFT.Modules
{
    public class Module_TriggerVFX : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_TriggerVFX);

        [SerializeField]
        public Data_TriggerVFX dataTriggerVFX;
        [SerializeField]
        public Data_FuelTanks _dataFuelTanks;
        [SerializeField]
        public GameObject CoolingVFX;

        public TriggerVFXFromAnimation TriggerVFX;
        public DynamicGravityForVFX GravityForVFX;
        public Animator Animator;
        public ParticleSystem particleSystem;
        public bool _wasActive;
        internal float _fuelLevel;
        internal bool activateTriggerVFX;

        public RefreshActiveVessel refreshActiveVessel;
        public GameState _gameState { get; private set; }
        public override bool IsActive => FFTPlugin.Instance.isActiveVessel.GetValueBool();
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (PartBackingMode == PartBackingModes.Flight)
            {
                if (CoolingVFX)
                {
                    Awake();
                }
            }
        }
        public void Awake()
        {
            FFTPlugin.Logger.LogInfo("Attached to GameObject: " + gameObject.name);

            if (CoolingVFX != null)
            {
                particleSystem = CoolingVFX.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null)
                {
                    FFTPlugin.Logger.LogInfo("Successfully retrieved ParticleSystem on CoolingVFX.");
                }
                else
                {
                    FFTPlugin.Logger.LogError("Could not find ParticleSystem on CoolingVFX.");
                }
            }
            else
            {
                FFTPlugin.Logger.LogError("CoolingVFX GameObject is not assigned.");
            }

            Animator = GetComponentInParent<Animator>();

            refreshActiveVessel = new RefreshActiveVessel();

            FFTPlugin.Logger.LogInfo("ModuleTriggerVFX has started.");
        }
        public override void AddDataModules()
        {
            base.AddDataModules();
            this.dataTriggerVFX ??= new Data_TriggerVFX();
            this.DataModules.TryAddUnique<Data_TriggerVFX>(this.dataTriggerVFX, out this.dataTriggerVFX);
        }
        public override void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);
            double fillRatioSum = 0;
            int count = 0;

            foreach (var container in part.Model.Containers)
            {
                foreach (var resourceID in container)
                {
                    count++;
                    fillRatioSum += container.GetResourceFillRatio(resourceID);
                }
            }

            double fillRatioAverage = fillRatioSum / count;
            float opacity = dataTriggerVFX.VFXOpacityCurve.Evaluate((float)fillRatioAverage);
            _fuelLevel = opacity;
            Animator.SetFloat("FuelLevel", _fuelLevel);

            if (_fuelLevel < 0)
            {
                FFTPlugin.Logger.LogError("Out of Fuel. Fuel level: " + _fuelLevel);
            }

            if (IsActive)
            {
                if (!FuelLevelExceedsThreshold())
                {
                    StopVFX();

                    Debug.Log("StopVFX: " + (StopVFX == null ? "is null" : "is not null"));
                }
                else if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
                {
                    StartVFX();
                    Debug.Log("StartVFX: " + (StartVFX == null ? "is null" : "is not null"));
                }
            }
            else
            {
                StopVFX();
            }
        }
        public void StartVFX()
        {
            EnableEmission();
            TriggerVFX.VFX01_ON();
            GravityForVFX.enabled = true;
            particleSystem.Play();
        }

        public void StopVFX()
        {
            TriggerVFX.VFX01_OFF();
            particleSystem.Stop(); ;
        }
        internal void EnableEmission()
        {
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = false;
            }
        }
        internal bool FuelLevelExceedsThreshold()
        {
            return _fuelLevel > 0.8f;
        }
        public void Activate()
        {
            activateTriggerVFX = true;
        }
    }
}