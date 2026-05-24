using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCMultiChunkManager : MonoBehaviour
{
    public List<MCMultiChunk> chunks;
    public Material material;

    [Min(1)]
    public int chunkCountOfX;
    [Min(1)]
    public int chunkCountOfY;
    [Min(1)]
    public int chunkCountOfZ;

    [HideInInspector]
    public int mapSizeX;
    [HideInInspector]
    public int mapSizeY;
    [HideInInspector]
    public int mapSizeZ;

    [Min(1)]
    public int chunkSize = 16;

    [Header("Water Coord")]
    public Vector3Int waterCoord;

    public FluidSimulator simulator;

    public void Initialize()
    {
        mapSizeX = chunkCountOfX * chunkSize;
        mapSizeY = chunkCountOfY * chunkSize;
        mapSizeZ = chunkCountOfZ * chunkSize;
        simulator = new FluidSimulator(mapSizeX, mapSizeY, mapSizeZ);
        simulator.CreateData();
        simulator.data[simulator.GetIndex(mapSizeX / 2, mapSizeY / 2, mapSizeZ / 2)] = 5000;
        //simulator.data[simulator.GetIndex(waterCoord.x, waterCoord.y, waterCoord.z)] = 100000;
        for (int x = 0; x < chunkCountOfX; x++)
        for(int y = 0; y < chunkCountOfY; y++)
        for(int z = 0; z < chunkCountOfZ; z++)
                {
                    var chunk = new GameObject($"Chunk {x},{y},{z}");
                    var mc = chunk.AddComponent(typeof(MCMultiChunk)) as MCMultiChunk;
                    chunk.transform.parent = transform;
                    
                    // TODO
                    mc.chunkSize = chunkSize;
                    mc.GetComponent<MeshRenderer>().material = material;
                    mc.transform.position = new Vector3(x, y, z) * (chunkSize - 1);
                    mc.simulator = simulator;
                    mc.startX = x * (chunkSize - 1);
                    mc.startY = y * (chunkSize - 1);
                    mc.startZ = z * (chunkSize - 1);
                    mc.Initialize();
                    
                }
    }

    public void Awake()
    {
        Initialize();
    }

    public void FixedUpdate()
    {
        simulator.DoFlow();
    }
}
