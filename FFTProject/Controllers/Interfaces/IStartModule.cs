using System;
using System.Collections.Generic;
using System.Text;

namespace FFT.Controllers.Interfaces
{
    public interface IStartModule
    {
        void StartVentValve();
        void ActivateModule(ModuleController.ModuleType moduleType);
        void DeactivateModule(ModuleController.ModuleType moduleType);
    }
}
