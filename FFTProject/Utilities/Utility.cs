//|=====================Summary========================|0|
//|            helper methods & properties             |1|
//|by cvusmo===========================================|4|
//|====================================================|1|
//|====================================================|2|
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using static KSP.Rendering.Planets.PQSData;

namespace FFT.Utilities
{
    public static class Utility
    {
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("FFT.Utility: ");
        public static VesselComponent ActiveVessel;
        public static ManeuverNodeData CurrentManeuver;
        public static GameState GameState;
        public static VesselSituations VesselSituations { get; private set; }
        public static MessageCenter MessageCenter { get; private set; }
        public static double UniverseTime => GameManager.Instance.Game.UniverseModel.UniverseTime;
        public static void Initialize()
        {
            RefreshGameManager();
            RefreshActiveVesselAndCurrentManeuver();
        }
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
        public static string GameStateToString(GameState gamestate)
        {
            return gamestate switch
            {
                GameState.KerbalSpaceCenter => "KSC",
                GameState.Launchpad => "LaunchPad",
                GameState.Runway => "Runway",
                GameState.FlightView => "FlightView",
                GameState.MainMenu => "MainMenu",
                GameState.BaseAssemblyEditor => "BaseAssemblyEditor",
                GameState.Map3DView => "Map3DView",
                GameState.VehicleAssemblyBuilder => "VAB",
                _ => "UNKNOWN GAMESTATE",
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