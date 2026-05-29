using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MCMultiChunkManager : MonoBehaviour
{
    [HideInInspector]
    public List<MCMultiChunk> chunks;

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
    public int isoLevel = 50000;

    public FluidSimulator simulator;
    public FluidSimController simController;
    [SerializeField]
    private ComputeShader fluidSimShader;
    private ComputeBuffer prop;
    
    public void Initialize()
    {
        mapSizeX = chunkCountOfX * chunkSize;
        mapSizeY = chunkCountOfY * chunkSize;
        mapSizeZ = chunkCountOfZ * chunkSize;
        simulator = new FluidSimulator(mapSizeX, mapSizeY, mapSizeZ);
        simController = new FluidSimController();
        simController.simulator = simulator;
        simulator.CreateData();
        prop = new ComputeBuffer(16, sizeof(int));
        prop.SetData(new int[] { 0, 0, 0, 0, isoLevel, 0, 0,
            (simulator.depth + 2) * (simulator.height + 2),
            (simulator.depth + 2) * (simulator.height + 2) + (simulator.depth+2),
            (simulator.depth + 2),
            1 + (simulator.depth + 2) * (simulator.height + 2),
            1 + (simulator.depth + 2) * (simulator.height + 2) + (simulator.depth + 2),
            1 + (simulator.depth + 2),
            (16 - 1) * 16 * 16,
            16 * (16 - 1) * 16,
            16 * 16 * (16 - 1)
        });

        //simulator.data[simulator.GetIndex(1, 1, 1)] = 50_000000;

        for (int i = 0; i < 10; i++)
        {
            //break;
            simulator.data[Random.Range(0, simulator.data.Length)] = Random.Range(50000,100000);
        }

        simController.fluidSimShader = fluidSimShader;
        simController.width = mapSizeX;
        simController.height = mapSizeY;
        simController.depth = mapSizeZ;
        simController.Awake();
        simController.CreateData(mapSizeX, mapSizeY, mapSizeZ);
        for (int x = 0; x < chunkCountOfX; x++)
        for (int y = 0; y < chunkCountOfY; y++)
        for (int z = 0; z < chunkCountOfZ; z++)
                {
                    var chunk = new GameObject($"Chunk {x},{y},{z}");
                    var mc = chunk.AddComponent(typeof(MCMultiChunk)) as MCMultiChunk;
                    chunk.transform.parent = transform;
                    
                    mc.chunkSize = chunkSize;
                    mc.transform.position = new Vector3(x, y, z) * (chunkSize - 1);
                    mc.simulator = simulator;
                    mc.isoLevel = isoLevel;
                    mc.Initialize(x * (chunkSize - 1), y * (chunkSize - 1), z * (chunkSize - 1));
                    mc.MCShader.SetBuffer(mc.kernelMain, "DataBuffer", simController.dataBuffer);
                    mc.MCShader.SetBuffer(mc.kernelMain, "prop", prop);

                    chunks.Add(mc);
                }
    }

    public void Awake()
    {
        Initialize();
        StartCoroutine(UpdateAllChunks());
    }

    public void FixedUpdate()
    {
        simController.FixedUpdate();
    }

    public void Update()
    {
        foreach (var i in chunks)
        {
            i.myUpdate();
        }
    }

    public void OnDestroy()
    {
        simController.Dispose();
        prop.Dispose();
    }

    private IEnumerator UpdateAllChunks()
    {
        while (true)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                if (chunk.updateDone)
                {
                    chunk.updateDone = false;
                    chunk.isPending = false;
                    chunk.WaitForRequest();
                }

                if (chunk.isPending)
                {
                    chunk.WaitForUpdate();
                }
            }

            yield return null;
        }
    }
}
