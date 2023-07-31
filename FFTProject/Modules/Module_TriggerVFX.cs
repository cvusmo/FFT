using KSP.Animation;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;
using VFX;

namespace FFT.Modules
{
    public class Module_TriggerVFX : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_TriggerVFX);

        [SerializeField]
        public Data_TriggerVFX _dataTriggerVFX;
        [SerializeField]
        public Data_FuelTanks _dataFuelTanks;
        [SerializeField]
        public GameObject CoolingVFX;

        public TriggerVFXFromAnimation _triggerVFX;
        public DynamicGravityForVFX _gravityForVFX;
        public Animator animator;
        public ParticleSystem _particleSystem;
        public bool _wasActive;
        private float _fuelLevel;
        public GameState _gameState { get; private set; }
        public override bool IsActive => FFTPlugin.Instance._isActiveVessel.GetValueBool();
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
                _particleSystem = CoolingVFX.GetComponentInChildren<ParticleSystem>();
                if (_particleSystem != null)
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

            animator = GetComponentInParent<Animator>();

            FFTPlugin.Instance._isActiveVessel = new IsActiveVessel();
            FFTPlugin.Instance._vesselComponent = new VesselComponent();
            FFTPlugin.Logger.LogInfo("Module_TriggerVFX has started.");
        }
        public override void AddDataModules()
        {
            base.AddDataModules();
            this._dataTriggerVFX ??= new Data_TriggerVFX();
            this.DataModules.TryAddUnique<Data_TriggerVFX>(this._dataTriggerVFX, out this._dataTriggerVFX);
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

            float opacity = _dataTriggerVFX.VFXOpacityCurve.Evaluate((float)fillRatioAverage);

            _fuelLevel = opacity;
            animator.SetFloat("FuelLevel", _fuelLevel);

            if (_fuelLevel < 0)
            {
                FFTPlugin.Logger.LogError("Out of Fuel. Fuel level: " + _fuelLevel);
            }

            if (IsActive)
            {
                if (!FuelLevelExceedsThreshold())
                {
                    StopVFX();
                }
                else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("CoolingVFX_LOOP"))
                {
                    StartVFX();
                }
            }
            else
            {
                StopVFX();
            }
        }
        internal void StartVFX()
        {
            EnableEmission();
            _particleSystem.Play();
            _triggerVFX.enabled = true;
            _gravityForVFX.enabled = true;
        }
        internal void StopVFX()
        {
            _particleSystem.Stop();
        }
        internal void EnableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = true;
            }
        }
        internal void DisableEmission()
        {
            if (_particleSystem != null)
            {
                var emission = _particleSystem.emission;
                emission.enabled = false;
            }
        }
        internal bool FuelLevelExceedsThreshold()
        {
            return _fuelLevel > 0.8f;
        }
    }
}