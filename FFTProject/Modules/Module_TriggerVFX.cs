using KSP.Sim.Definitions;
using UnityEngine;

namespace FFT.Modules
{
    internal class Module_TriggerVFX : PartBehaviourModule
    {
        public override Type PartComponentModuleType => typeof(PartComponentModule_TriggerVFX);
        [SerializeField]
        public Data_TriggerVFX _dataTriggerVFX;
        TriggerController triggerController;
        
        public override void OnInitialize()
        {
            base.OnInitialize();
            if (PartBackingMode == PartBackingModes.Flight)
            {
                triggerController = GetComponentInChildren<TriggerController>();
            }
        }
        public void AddDataModules()
        {
            base.AddDataModules();
            this._dataTriggerVFX ??= new Data_TriggerVFX();
            this.DataModules.TryAddUnique<Data_TriggerVFX>(this._dataTriggerVFX, out this._dataTriggerVFX);
        }
        public void OnModuleFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleFixedUpdate(fixedDeltaTime);
            double FillRatio = 0;
            int count = 0;

            foreach (var container in part.Model.Containers)
            {
                foreach (var ResourceID in container)
                {
                    count++;
                    FillRatio = container.GetResourceFillRatio(ResourceID) / count;
                }
            }

            float opacity = _dataTriggerVFX.VFXOpacityCurve.Evaluate((float)FillRatio);

            //Set VFX Opacity Here
        }
        
    }
}


