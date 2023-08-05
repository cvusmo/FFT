using KSP.Game;
using KSP.Messages;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using static KSP.Rendering.Planets.PQSData;

namespace FFT.Modules
{
    public class RefreshVesselData
    {
        public VesselComponent VesselComponent;
        public ManeuverNodeData CurrentManeuver;
        public GameStateConfiguration GameState;
        public MessageCenter MessageCenter;
        public VesselDeltaVComponent VesselDeltaVComponentOAB;
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
        public void RefreshGameManager(VesselComponent activeVessel)
        {
            GameState = GameManager.Instance?.Game?.GlobalGameState?.GetGameState();
            MessageCenter = GameManager.Instance?.Game?.Messages;
        }
        public void RefreshStagesOAB(VesselComponent activeVessel)
        {
            VesselDeltaVComponentOAB = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.VesselDeltaV;
        }
        public string SituationToString(VesselSituations situation)
        {
            return situation switch
            {
                VesselSituations.PreLaunch => "Pre-Launch",
                VesselSituations.Landed => "Landed",
                VesselSituations.Splashed => "Splashed down",
                VesselSituations.Flying => "Flying",
                VesselSituations.SubOrbital => "Suborbital",
                VesselSituations.Orbiting => "Orbiting",
                VesselSituations.Escaping => "Escaping",
                _ => "UNKNOWN",
            };
        }
        public string BiomeToString(BiomeSurfaceData biome)
        {
            string result = biome.type.ToString().ToLower().Replace('_', ' ');
            return result.Substring(0, 1).ToUpper() + result.Substring(1);
        }
        public class RefreshActiveVessel
        {
            public VesselComponent ActiveVessel { get; private set; }

            public void RefreshData()
            {
                ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
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
        public RefreshVesselData()
        {
            this.refreshActiveVessel = new RefreshActiveVessel();
            this.altitudeAgl = new AltitudeAgl();
            this.altitudeAsl = new AltitudeAsl();
            this.altitudeFromScenery = new AltitudeFromScenery();
            this.verticalVelocity = new VerticalVelocity();
            this.horizontalVelocity = new HorizontalVelocity();
            this.dynamicPressure_KPa = new DynamicPressure_kPa();
            this.staticPressure_KPa = new StaticPressure_kPa();
            this.atmosphericTemperature = new AtmosphericTemperature();
            this.externalTemperature = new ExternalTemperature();
        }
        public void RefreshAll(VesselComponent activeVessel)
        {
            this.refreshActiveVessel.RefreshData();
            this.altitudeAgl.RefreshData(activeVessel);
            this.altitudeAsl.RefreshData(activeVessel);
            this.altitudeFromScenery.RefreshData(activeVessel);
            this.verticalVelocity.RefreshData(activeVessel);
            this.horizontalVelocity.RefreshData(activeVessel);
            this.dynamicPressure_KPa.RefreshData(activeVessel);
            this.staticPressure_KPa.RefreshData(activeVessel);
            this.atmosphericTemperature.RefreshData(activeVessel);
            this.externalTemperature.RefreshData(activeVessel);
        }
    }
}