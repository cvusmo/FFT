
//|=====================Summary========================|0|
//|            helper methods & properties             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using System.Reflection;
using static KSP.Rendering.Planets.PQSData;

namespace FFT.Utilities
{
    public static class Utility
    {
        public static VesselComponent ActiveVessel;
        public static ManeuverNodeData CurrentManeuver;
        public static string LayoutPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModuleList.json");
        public static GameState GameState;
        public static VesselSituations vesselSituations;
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Utility");
        public static MessageCenter MessageCenter;
        public static VesselDeltaVComponent VesselDeltaVComponentOAB;
        public static VesselSituations VesselSituations { get; private set; }
        public static double UniversalTime => GameManager.Instance.Game.UniverseModel.UniversalTime;
        public static void RefreshActiveVesselAndCurrentManeuver()
        {
            ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
            CurrentManeuver = ActiveVessel != null ? GameManager.Instance?.Game?.SpaceSimulation.Maneuvers.GetNodesForVessel(ActiveVessel.GlobalId).FirstOrDefault() : null;
        }
        public static void RefreshGameManager()
        {
            var state = GameManager.Instance?.Game?.GlobalGameState?.GetGameState();
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