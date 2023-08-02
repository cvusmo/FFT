using FFT.Utilities;
using FFT;

namespace FFT.Modules
{
    public class AltitudeAgl
    {
        public double altitudeAgl { get; private set; }

        public void RefreshData()
        {
            altitudeAgl = Utility.vesselComponent.AltitudeFromScenery;
        }
    }
    public class AltitudeAsl
    {
        public double altitudeAsl { get; private set; }

        public void RefreshData()
        {
            altitudeAsl = Utility.vesselComponent.AltitudeFromSeaLevel;
        }
    }
    public class AltitudeFromScenery
    {
        public double altitudeFromScenery { get; private set; }

        public void RefreshData()
        {
            altitudeFromScenery = Utility.vesselComponent.AltitudeFromTerrain;
        }
    }
    public class VerticalVelocity
    {
        public double verticalVelocity { get; private set; }

        public void RefreshData()
        {
            verticalVelocity = Utility.vesselComponent.VerticalSrfSpeed;
        }
    }
    public class HorizontalVelocity
    {
        public double horizontalVelocity { get; private set; }

        public void RefreshData()
        {
            horizontalVelocity = Utility.vesselComponent.HorizontalSrfSpeed;
        }
    }
    public class DynamicPressure_kPa
    {
        public double dynamicPressure_kPa { get; private set; }

        public void RefreshData()
        {
            dynamicPressure_kPa = Utility.vesselComponent.DynamicPressure_kPa;
        }
    }
    public class StaticPressure_kPa
    {
        public double staticPressure_kPa { get; private set; }

        public void RefreshData()
        {
            staticPressure_kPa = Utility.vesselComponent.StaticPressure_kPa;
        }
    }
    public class AtmosphericTemperature
    {
        public double atmosphericTemperature { get; private set; }

        public void RefreshData()
        {
            atmosphericTemperature = Utility.vesselComponent.AtmosphericTemperature;
        }
    }
    public class ExternalTemperature
    {
        public double externalTemperature { get; private set; }

        public void RefreshData()
        {
            externalTemperature = Utility.vesselComponent.ExternalTemperature;
        }
    }
}