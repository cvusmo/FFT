//|=====================Summary========================|0|
//|      Refreshes Data & gets values to update VFX    |1|
//|by cvusmo===========================================|4|
//|====================================================|1|

using FFT.Managers;
using KSP.Game;
using KSP.Sim.impl;
using System;
using System.Collections.Generic;

namespace FFT.Utilities
{
    public class RefreshVesselData
    {
        private static readonly Lazy<RefreshVesselData> _instance = new Lazy<RefreshVesselData>(() => new RefreshVesselData());
        public static RefreshVesselData Instance => _instance.Value;

        private DateTime _lastRefreshTime;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(3);
        private Dictionary<string, object> _cache = new Dictionary<string, object>();
        public RefreshActiveVessel RefreshActiveVesselInstance { get; } = new RefreshActiveVessel();
        internal RefreshVesselData() => RefreshActiveVesselInstance.RefreshData();
        public T GetCachedValue<T>(string key, Func<VesselComponent, T> fetchValue)
        {
            if (_cache.ContainsKey(key) && DateTime.UtcNow - _lastRefreshTime <= _refreshInterval)
            {
                return (T)_cache[key];
            }
            else
            {
                var value = fetchValue(RefreshActiveVesselInstance.ActiveVessel);
                _cache[key] = value;
                return value;
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
        public double AltitudeAgl => GetCachedValue("AltitudeAgl", vessel => vessel.AltitudeFromScenery);
        public double AltitudeAsl => GetCachedValue("AltitudeAsl", vessel => vessel.AltitudeFromSeaLevel);
        public double AltitudeFromScenery => GetCachedValue("AltitudeFromScenery", vessel => vessel.AltitudeFromTerrain);
        public double VerticalVelocity => GetCachedValue("VerticalVelocity", vessel => vessel.VerticalSrfSpeed);
        public double HorizontalVelocity => GetCachedValue("HorizontalVelocity", vessel => vessel.HorizontalSrfSpeed);
        public double DynamicPressure_kPa => GetCachedValue("DynamicPressure_kPa", vessel => vessel.DynamicPressure_kPa);
        public double StaticPressure_kPa => GetCachedValue("StaticPressure_kPa", vessel => vessel.StaticPressure_kPa);
        public double AtmosphericTemperature => GetCachedValue("AtmosphericTemperature", vessel => vessel.AtmosphericTemperature);
        public double ExternalTemperature => GetCachedValue("ExternalTemperature", vessel => vessel.ExternalTemperature);
        public bool IsInAtmosphere => GetCachedValue("IsInAtmosphere", vessel => vessel.IsInAtmosphere);
        public double FuelPercentage => GetCachedValue("FuelPercentage", vessel => vessel.FuelPercentage);
    }
}
