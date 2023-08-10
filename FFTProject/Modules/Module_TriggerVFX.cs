using KSP.Animation;
using KSP.Game;
using KSP.Sim.Definitions;
using UnityEngine;
using VFX;

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
        internal float _fuelLevel;
        internal bool activateTriggerVFX;
        public GameState _gameState { get; private set; }
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

            Animator = CoolingVFX.GetComponentInParent<Animator>();

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
            int totalResourceCount = 0;

            foreach (var container in part.Model.Containers)
            {
                foreach (var resourceID in container)
                {
                    totalResourceCount++;
                    fillRatioSum += container.GetResourceFillRatio(resourceID);
                }
            }

            _fuelLevel = (float)(fillRatioSum / totalResourceCount);
            float opacity = dataTriggerVFX.VFXOpacityCurve.Evaluate(_fuelLevel);

            Animator.SetFloat("FuelLevel", _fuelLevel);

            if (FFTPlugin.Instance._state == GameState.Launchpad)
            {
                if (FuelLevelExceedsThreshold())
                {
                    StopVFX();
                }
                else if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
                {
                    StartVFX();
                }
            }
        }
        public void StartVFX()
        {
            particleSystem.Play();
            EnableEmission();
            GravityForVFX.enabled = true;
        }

        public void StopVFX()
        {
            particleSystem.Stop(); ;
            DisableEmission();
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