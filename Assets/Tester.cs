using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {
        FluidSimulator simulator = new FluidSimulator(3, 4, 5);
        simulator.CreateData();
        simulator.DoFlow();
    }
}
