using System.Collections.Generic;
using UnityEngine;

public class FuelTankDefinitions : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> fuelTankDefintions;

    private Dictionary<string, GameObject> fuelTanksDict = new Dictionary<string, GameObject>();
    private void Awake()
    {
        foreach (var tank in fuelTankDefintions)
        {
            fuelTanksDict[tank.name] = tank;
        }
    }
    public GameObject GetFuelTank(string tankName)
    {
        if (fuelTanksDict.TryGetValue(tankName, out var tank))
        {
            return tank;
        }

        return null;
    }
}