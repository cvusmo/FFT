using KSP.Game;
using KSP.Messages;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using System.Reflection;
using static KSP.Rendering.Planets.PQSData;
using FFT.Modules;

namespace FFT.Utilities
{
    public static class Utility
    {
        public static VesselComponent vesselComponent;
        public static ManeuverNodeData CurrentManeuver;
        public static string LayoutPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FancyFuelTank.json");
        public static int CurrentLayoutVersion = 13;
        public static GameStateConfiguration GameState;
        public static MessageCenter MessageCenter;
        public static VesselDeltaVComponent VesselDeltaVComponentOAB;

        public static double UniversalTime => GameManager.Instance.Game.UniverseModel.UniversalTime;

        public static void RefreshActiveVesselAndCurrentManeuver()
        {
            vesselComponent = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
            CurrentManeuver = vesselComponent != null ? GameManager.Instance?.Game?.SpaceSimulation.Maneuvers.GetNodesForVessel(vesselComponent.GlobalId).FirstOrDefault() : null;
        }

        public static void RefreshGameManager()
        {
            GameState = GameManager.Instance?.Game?.GlobalGameState?.GetGameState();
            MessageCenter = GameManager.Instance?.Game?.Messages;
        }

        public static void RefreshStagesOAB()
        {
            VesselDeltaVComponentOAB = GameManager.Instance?.Game?.OAB?.Current?.Stats?.MainAssembly?.VesselDeltaV;
        }

        public static string SituationToString(VesselSituations situation)
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

        public static string BiomeToString(BiomeSurfaceData biome)
        {
            string result = biome.type.ToString().ToLower().Replace('_', ' ');
            return result.Substring(0, 1).ToUpper() + result.Substring(1);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * PatchedConicsOrbit.Rad2Deg;
        }
    }
}