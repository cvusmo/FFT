//|=====================Summary========================|0|
//|      Refreshes Data & gets values to update VFX    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Managers;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;

namespace FFT.Utilities
{
    public class RefreshVesselData
    {
        private static RefreshVesselData _instance;
        public VesselComponent VesselComponent;
        public CelestialBodyComponent CelestialBodyComponent;
        public ManeuverNodeData CurrentManeuver;
        public MessageCenter MessageCenter;
        public VesselDeltaVComponent VesselDeltaVComponentOAB;
        public GameStateConfiguration GameStateConfig;
        public static double UniversalTime => GameManager.Instance.Game.UniverseModel.UniversalTime;
        public VesselComponent activeVessel { get; set; }
        public RefreshActiveVessel refreshActiveVessel { get; set; }
        public AltitudeAgl altitudeAgl { get; private set; }
        public AltitudeAsl altitudeAsl { get; private set; }
        public AltitudeFromScenery altitudeFromScenery { get; private set; }
        public VerticalVelocity verticalVelocity { get; private set; }
        public HorizontalVelocity horizontalVelocity { get; private set; }
        public DynamicPressure_kPa dynamicPressure_KPa { get; private set; }
        public StaticPressure_kPa staticPressure_KPa { get; private set; }
        public AtmosphericTemperature atmosphericTemperature { get; private set; }
        public ExternalTemperature externalTemperature { get; private set; }
        public FuelPercentage fuelPercentage { get; private set; }
        public IsInAtmosphere isInAtmosphere { get; private set; }
        internal Manager Manager { get; private set; }
        internal static RefreshVesselData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RefreshVesselData();

                return _instance;
            }
        }
        public class RefreshActiveVessel
        {
            public VesselComponent ActiveVessel { get; private set; }
            internal bool IsFlightActive = false;

            public void RefreshData()
            {
                ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
                IsFlightActive = true;
            }
        }
        public class AltitudeAgl
        {
            public double altitudeAgl { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                altitudeAgl = activeVessel.AltitudeFromScenery;
            }
        }
        public class AltitudeAsl
        {
            public double altitudeAsl { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                altitudeAsl = activeVessel.AltitudeFromSeaLevel;
            }
        }
        public class AltitudeFromScenery
        {
            public double altitudeFromScenery { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                altitudeFromScenery = activeVessel.AltitudeFromTerrain;
            }
        }
        public class VerticalVelocity
        {
            public double verticalVelocity { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                verticalVelocity = activeVessel.VerticalSrfSpeed;
            }
        }
        public class HorizontalVelocity
        {
            public double horizontalVelocity { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                horizontalVelocity = activeVessel.HorizontalSrfSpeed;
            }
        }
        public class DynamicPressure_kPa
        {
            public double dynamicPressure_kPa { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                dynamicPressure_kPa = activeVessel.DynamicPressure_kPa;
            }
        }
        public class StaticPressure_kPa
        {
            public double staticPressure_kPa { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                staticPressure_kPa = activeVessel.StaticPressure_kPa;
            }
        }
        public class AtmosphericTemperature
        {
            public double atmosphericTemperature { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                atmosphericTemperature = activeVessel.AtmosphericTemperature;
            }
        }
        public class ExternalTemperature
        {
            public double externalTemperature { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                externalTemperature = activeVessel.ExternalTemperature;
            }
        }
        public class IsInAtmosphere
        {
            public bool isInAtmosphere { get; private set; }

            public void RefreshData(VesselComponent activeVessel)
            {
                isInAtmosphere = activeVessel.IsInAtmosphere;
            }
        }
        public class FuelPercentage
        {
            public double fuelPercentage { get; private set; }
            public void RefreshData(VesselComponent activeVessel)
            {
                fuelPercentage = activeVessel.FuelPercentage;
                //fuelCheck = activeVessel.StageFuelPercentage;
            }
        }
        public RefreshVesselData()
        {
            refreshActiveVessel = new RefreshActiveVessel();
            altitudeAgl = new AltitudeAgl();
            altitudeAsl = new AltitudeAsl();
            altitudeFromScenery = new AltitudeFromScenery();
            verticalVelocity = new VerticalVelocity();
            horizontalVelocity = new HorizontalVelocity();
            dynamicPressure_KPa = new DynamicPressure_kPa();
            staticPressure_KPa = new StaticPressure_kPa();
            atmosphericTemperature = new AtmosphericTemperature();
            externalTemperature = new ExternalTemperature();
            isInAtmosphere = new IsInAtmosphere();
            fuelPercentage = new FuelPercentage();
        }
        public void RefreshAll(VesselComponent activeVessel)
        {
            refreshActiveVessel.RefreshData();
            altitudeAgl.RefreshData(activeVessel);
            altitudeAsl.RefreshData(activeVessel);
            altitudeFromScenery.RefreshData(activeVessel);
            verticalVelocity.RefreshData(activeVessel);
            horizontalVelocity.RefreshData(activeVessel);
            dynamicPressure_KPa.RefreshData(activeVessel);
            staticPressure_KPa.RefreshData(activeVessel);
            atmosphericTemperature.RefreshData(activeVessel);
            externalTemperature.RefreshData(activeVessel);
            isInAtmosphere.RefreshData(activeVessel);
            fuelPercentage.RefreshData(activeVessel);
        }
    }
}