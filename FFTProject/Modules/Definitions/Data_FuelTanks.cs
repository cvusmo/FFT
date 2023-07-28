using UnityEngine;

public class Data_FuelTanks : MonoBehaviour
{
    public Dictionary<string, GameObject> fuelTankDict;

    public void Awake()
    {
        fuelTankDict = new Dictionary<string, GameObject>();

        fuelTankDict.Add("CV401", CV401);
        fuelTankDict.Add("CV411", CV411);
        fuelTankDict.Add("CV421", CV421);
        fuelTankDict.Add("SP701", SP701);
        fuelTankDict.Add("SR812", SR812);
        fuelTankDict.Add("SR812A", SR812A);
        fuelTankDict.Add("SR813", SR813);
    }
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
}