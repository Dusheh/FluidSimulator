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

    public FluidSimulator simulator;

    public void Initialize()
    {
        mapSizeX = chunkCountOfX * 16;
        mapSizeY = chunkCountOfY * 16;
        mapSizeZ = chunkCountOfZ * 16;
        for(int x = 0; x < chunkCountOfX; x++)
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
                    mc.Initialize();
                    
                }
    }

    public void Awake()
    {
        Initialize();
    }
}
