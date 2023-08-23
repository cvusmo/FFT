using System.Collections.Generic;
using UnityEngine;

namespace FFT.Modules
{
    public class FuelTankDefinitions : MonoBehaviour
    {
        [SerializeField]
        public List<GameObject> fuelTankDefinitions;
        [SerializeField]
        public Data_VentValve _dataVentvalve;
        [SerializeField]
        public Data_FuelTanks _dataFuelTanks;
        public Dictionary<string, GameObject> fuelTanksDict = new Dictionary<string, GameObject>();
        public bool isInitialized;

        public void PopulateFuelTanks(Data_FuelTanks data)
        {
            if (this.isInitialized)
                return;
            this.fuelTanksDict["CV401"] = data.CV401;
            this.fuelTanksDict["CV411"] = data.CV411;
            this.fuelTanksDict["CV421"] = data.CV421;
            this.fuelTanksDict["SP701"] = data.SP701;
            this.fuelTanksDict["SR812"] = data.SR812;
            this.fuelTanksDict["SR812A"] = data.SR812A;
            this.fuelTanksDict["SR813"] = data.SR813;
            this.isInitialized = true;
        }

        public GameObject GetFuelTank(string tankName)
        {
            GameObject gameObject;
            return this.fuelTanksDict.TryGetValue(tankName, out gameObject) ? gameObject : (GameObject)null;
        }

        public Module_VentValve GetmoduleVentValve(string tankName)
        {
            GameObject gameObject;
            return this.fuelTanksDict.TryGetValue(tankName, out gameObject) ? gameObject.GetComponent<Module_VentValve>() : (Module_VentValve)null;
        }
    }
}