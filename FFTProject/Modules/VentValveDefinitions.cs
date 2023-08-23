using System.Collections.Generic;
using UnityEngine;

namespace FFT.Modules
{
    public class VentValveDefinitions : MonoBehaviour
    {
        [SerializeField]
        public List<GameObject> ventValveDefinitions;
        [SerializeField]
        public Data_VentValve DataVentValve;
        [SerializeField]
        public Data_ValveParts _dataValveParts;
        public Dictionary<string, GameObject> ventValveDict = new Dictionary<string, GameObject>();
        public bool isInitialized;

        public void PopulateVentValve(Data_ValveParts data)
        {
            if (this.isInitialized)
                return;
            this.ventValveDict["RF1"] = data.RF1;
            this.ventValveDict["RF2"] = data.RF2;
            this.isInitialized = true;
        }

        public GameObject GetVentValve(string valveName)
        {
            GameObject gameObject;
            return this.ventValveDict.TryGetValue(valveName, out gameObject) ? gameObject : (GameObject)null;
        }

        public Module_VentValve GetVentValveModule(string valveName)
        {
            GameObject gameObject;
            return this.ventValveDict.TryGetValue(valveName, out gameObject) ? gameObject.GetComponent<Module_VentValve>() : (Module_VentValve)null;
        }
    }
}