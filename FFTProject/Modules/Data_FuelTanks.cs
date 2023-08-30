using UnityEngine;
using System.Collections.Generic;
using KSP.Sim.Definitions;

namespace FFT.Modules
{
    [Serializable]
    public class Data_FuelTanks : ModuleData
    {
        public override Type ModuleType => typeof(Module_VentValve);

        [SerializeField]
        public GameObject CV401;
        [SerializeField]
        public GameObject CV411;
        [SerializeField]
        public GameObject CV421;
        [SerializeField]
        public GameObject SP701;
        [SerializeField]
        public GameObject SR812;
        [SerializeField]
        public GameObject SR812A;
        [SerializeField]
        public GameObject SR813;

        public GameObject GetFuelTank(string tankName)
        {
            switch (tankName)
            {
                case "CV401": return CV401;
                case "CV411": return CV411;
                case "CV421": return CV421;
                case "SP701": return SP701;
                case "SR812": return SR812;
                case "SR812A": return SR812A;
                case "SR813": return SR813;
                default: return null;
            }
        }
    }
}