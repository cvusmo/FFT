using KSP.Sim.Definitions;
using UnityEngine;

namespace FFT.Modules
{
    [Serializable]
    public class Data_ValveParts : ModuleData
    {
        public override Type ModuleType => typeof(Module_VentValve);
        public Dictionary<string, GameObject> ventValveDict;

        public void Awake()
        {
            ventValveDict = new Dictionary<string, GameObject>();

            ventValveDict.Add("RF1", RF1);
            ventValveDict.Add("RF2", RF2);
        }

        [SerializeField]
        public GameObject RF1;
        [SerializeField]
        public GameObject RF2;
    }
}