using KSP.Animation;
using UnityEngine;
using VFX;
using FFT.Utilities;
using FFT;

namespace FFT.Modules
{
    public class VentValveDefinitions : MonoBehaviour
    {
        public static VentValveDefinitions Instance { get; private set; }

        [SerializeField] public List<GameObject> ventValveDefinitions;
        [SerializeField] public Data_VentValve _dataVentValve;
        [SerializeField] public Data_ValveParts _dataValveParts;

        public Dictionary<string, GameObject> ventValveDict = new Dictionary<string, GameObject>();
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
        public void PopulateVentValve(Data_ValveParts data)
        {
            if (isInitialized) return;

            ventValveDict["RF1"] = data.RF1;
            ventValveDict["RF2"] = data.RF2;

            isInitialized = true;
        }
        public GameObject GetVentValve(string valveName)
        {
            if (ventValveDict.TryGetValue(valveName, out var vent))
            {
                return vent;
            }

            return null;
        }
        public Module_VentValve GetVentValveModule(string valveName)
        {
            if (ventValveDict.TryGetValue(valveName, out var vent))
            {
                return vent.GetComponent<Module_VentValve>();
            }

            return null;
        }
    }
}