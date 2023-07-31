using KSP.Sim;
using KSP.Sim.Definitions;
using UnityEngine;

namespace FFT.Modules
{
    public class Data_ValveParts : MonoBehaviour
    {
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
