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

    public enum VoxelType { Fluid, Terrain };
    public VoxelType voxelType;

    public VoxelManager voxelManager;
    [SerializeField]
    private ComputeShader updateShader;
    private ComputeBuffer prop;
    public Material meshMaterial;

    public bool realtimeUpdate;
    public bool refreshOnce;
    public bool paused;

    public void Awake()
    {
        Initialize();
        StartCoroutine(UpdateAllChunks());
    }

    public void Initialize()
    {
        mapSizeX = chunkCountOfX * chunkSize;
        mapSizeY = chunkCountOfY * chunkSize;
        mapSizeZ = chunkCountOfZ * chunkSize;
        if (voxelType == VoxelType.Fluid)
        {
            voxelManager = new FluidSimulator(mapSizeX, mapSizeY, mapSizeZ);
            voxelManager.updateShader = updateShader;
        }
        else if (voxelType == VoxelType.Terrain)
        {
            voxelManager = new TerrainSimulator(mapSizeX, mapSizeY, mapSizeZ);
            voxelManager.updateShader = updateShader;
        }
        realtimeUpdate = voxelManager.realtimeUpdate;
        voxelManager.Initialize();
        prop = new ComputeBuffer(16, sizeof(int));
        prop.SetData(new int[] { 0, 0, 0, 0, isoLevel, 0, 0,
            (voxelManager.depth + 2) * (voxelManager.height + 2),
            (voxelManager.depth + 2) * (voxelManager.height + 2) + (voxelManager.depth+2),
            (voxelManager.depth + 2),
            1 + (voxelManager.depth + 2) * (voxelManager.height + 2),
            1 + (voxelManager.depth + 2) * (voxelManager.height + 2) + (voxelManager.depth + 2),
            1 + (voxelManager.depth + 2),
            (16 - 1) * 16 * 16,
            16 * (16 - 1) * 16,
            16 * 16 * (16 - 1)
        });

        for (int x = 0; x < chunkCountOfX; x++)
            for (int y = 0; y < chunkCountOfY; y++)
                for (int z = 0; z < chunkCountOfZ; z++)
                {
                    var chunk = new GameObject($"Chunk {x},{y},{z}");
                    var mc = chunk.AddComponent(typeof(MCMultiChunk)) as MCMultiChunk;
                    chunk.transform.parent = transform;

                    mc.chunkSize = chunkSize;
                    mc.transform.position = new Vector3(x, y, z) * (chunkSize - 1);
                    mc.isoLevel = isoLevel;
                    mc.material = meshMaterial;
                    mc.Initialize(x * (chunkSize - 1), y * (chunkSize - 1), z * (chunkSize - 1));
                    mc.MCShader.SetBuffer(mc.kernelMain, "prop", prop);
                    mc.MCShader.SetBuffer(mc.kernelMain, "DataBuffer", voxelManager.dataBuffer);
                    //mc.MCShader.SetBuffer(mc.kernelMain, "DataBuffer", voxelManager.dataBuffer);

                    chunks.Add(mc);
                }
        //if (!realtimeUpdate)
        {
            voxelManager.Refresh();
            foreach (var i in chunks)
            {
                i.myUpdate();
            }
        }
    }

    public void FixedUpdate()
    {
        if (!realtimeUpdate || paused) return;
        if(voxelType == VoxelType.Fluid)
        {
            if(Time.frameCount % 3 == 0) voxelManager.Refresh();
        }
    }

    public void Update()
    {
        if (!realtimeUpdate || paused) return;
        if (voxelType == VoxelType.Fluid)
        {
            if (Time.frameCount % 3 == 0)
                foreach (var i in chunks)
                {
                    i.myUpdate();
                }
        }
    }

    public void Refresh()
    {
        if(voxelType == VoxelType.Fluid) voxelManager.Refresh();
        foreach (var i in chunks)
        {
            i.myUpdate();
        }
    }

    public void OnDestroy()
    {
        voxelManager.Dispose();
        prop.Dispose();
    }

    private IEnumerator UpdateAllChunks()
    {
        while (true)
        {
            if (!refreshOnce && (!realtimeUpdate || paused)) yield return null;
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
