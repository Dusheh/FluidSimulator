using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCubeRenderer : MonoBehaviour
{
    public FluidSimulator simulator;

    //public Dictionary<Vector3, GameObject> cubes;
    public Dictionary<int, GameObject> cubes;

    public Vector3Int size;

    public void Awake()
    {
        simulator = new FluidSimulator(size.x, size.y, size.z);
        simulator.CreateData();
        cubes = new Dictionary<int, GameObject>();
        int cursor = 0;
        for (int i = 0; i < simulator.width + 2; i++)
            for (int j = 0; j < simulator.height + 2; j++)
                for (int k = 0; k < simulator.depth + 2; k++)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.parent = transform;
                    obj.transform.position = new Vector3Int(i, j, k);
                    obj.transform.localScale = Vector3.one * 0.02f;
                    cubes.Add(cursor, obj);
                    cursor++;
                }
        simulator.data[2 + 2 * (size.z + 2) + 2 * (size.z + 2) * (size.y + 2)] = 50;
    }

    public void FixedUpdate()
    {
        simulator.DoFlow();
        for (int cursor = 0; cursor < (size.x + 2) * (size.y + 2) * (size.z + 2); cursor++)
        {
            cubes[cursor].transform.localScale = Vector3.one * (simulator.data[cursor] * 0.02f);
        }
    }
}
