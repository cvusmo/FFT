using FFT.Utilities;
using KSP.Sim.impl;

namespace FFT.Modules.Core
{


    public class VesselDataModule
    {
        internal AltitudeAgl altitudeAgl;
        internal AltitudeAsl altitudeAsl;
        internal AltitudeFromScenery altitudeFromScenery;
        internal VerticalVelocity verticalVelocity;
        internal HorizontalVelocity horizontalVelocity;
        internal DynamicPressure_kPa dynamicPressure_kPa;
        internal StaticPressure_kPa staticPressure_kPa;
        internal AtmosphericTemperature atmosphericTemperature;
        internal ExternalTemperature externalTemperature;

        public VesselComponent _isActiveVessel;
        public VesselDataModule()
        {
            RefreshVessel();

            altitudeAgl = new AltitudeAgl();
            altitudeAsl = new AltitudeAsl();
            altitudeFromScenery = new AltitudeFromScenery();
            verticalVelocity = new VerticalVelocity();
            horizontalVelocity = new HorizontalVelocity();
            dynamicPressure_kPa = new DynamicPressure_kPa();
            staticPressure_kPa = new StaticPressure_kPa();
            atmosphericTemperature = new AtmosphericTemperature();
            externalTemperature = new ExternalTemperature();
        }

        public void RefreshVessel()
        {
            Utility.RefreshActiveVesselAndCurrentManeuver();
            this._isActiveVessel = Utility.vesselComponent;
        }

        public void FixedUpdate()
        {
            //if (_isActiveVessel.IsEnabled)
            //{
                altitudeAgl.RefreshData();
                altitudeAsl.RefreshData();
                altitudeFromScenery.RefreshData();
                verticalVelocity.RefreshData();
                horizontalVelocity.RefreshData();
                dynamicPressure_kPa.RefreshData();
                staticPressure_kPa.RefreshData();
                atmosphericTemperature.RefreshData();
                externalTemperature.RefreshData();
           // }
        }
    }
}
