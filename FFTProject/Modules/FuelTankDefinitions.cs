﻿using KSP.Animation;
using UnityEngine;
using System.Collections.Generic;
using VFX;

namespace FFT.Modules
{
    public class FuelTankDefinitions : MonoBehaviour
    {
        public static FuelTankDefinitions Instance { get; private set; }

        [SerializeField] public List<GameObject> fuelTankDefintions;
        [SerializeField] public Data_FuelTanks DataFuelTanks;

        public Dictionary<string, GameObject> fuelTanksDict = new Dictionary<string, GameObject>();
        public bool isInitialized = false;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
        public void PopulateFuelTanks(Data_FuelTanks data)
        {
            if (isInitialized) return;

            fuelTanksDict["CV401"] = data.CV401;
            fuelTanksDict["CV411"] = data.CV411;
            fuelTanksDict["CV421"] = data.CV421;
            fuelTanksDict["SP701"] = data.SP701;
            fuelTanksDict["SR812"] = data.SR812;
            fuelTanksDict["SR812A"] = data.SR812A;
            fuelTanksDict["SR813"] = data.SR813;

            isInitialized = true;
        }
        public GameObject GetFuelTank(string tankName)
        {
            if (fuelTanksDict.TryGetValue(tankName, out var tank))
            {
                return tank;
            }

            return null;
        }
        public Module_VentValve GetCoolingVFX(string tankName)
        {
            if (fuelTanksDict.TryGetValue(tankName, out var tank))
            {
                return tank.GetComponent<Module_VentValve>();
            }

            return null;
        }
    }
}