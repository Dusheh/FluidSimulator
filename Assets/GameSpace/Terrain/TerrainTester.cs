using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTester : MonoBehaviour
{
    public MCMultiChunkManager chunkManager;
    public TerrainSimulator terrain;

    public void Start()
    {
        terrain = (chunkManager.voxelManager as TerrainSimulator);
        if (terrain == null) Debug.LogError("Terrain is null");
    }

    public void SetPlaneBottom()
    {

    }

    public void SetPlanet()
    {
        var w = terrain.width;
        var h = terrain.height;
        var d = terrain.depth;
        int r = (int)(terrain.width * 0.2f);
        var centerX = w / 2;
        var centerY = h / 2;
        var centerZ = d / 2;

        for (int x = 1; x <= w; x++)
        {
            for(int y = 1; y <= h; y++)
            {
                for(int z = 1; z <= d; z++)
                {
                    var dis = Mathf.Sqrt(Mathf.Pow(centerX - x, 2) + Mathf.Pow(centerY - y, 2) + Mathf.Pow(centerZ - z, 2));
                    if(dis < r)
                    {
                        terrain.data[terrain.GetIndex(x, y, z)] = 1;
                    }
                }
            }
        }
        terrain.dataBuffer.SetData(terrain.data);
        chunkManager.Refresh();
    }

    public void SetHeightMap()
    {
        FastNoiseLite noise = new FastNoiseLite(Random.Range(0,2048));
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

        for(int x = 1; x <= terrain.width; x++)
        {
            for (int z = 1; z <= terrain.depth; z++)
            {
                var height = terrain.height * (noise.GetNoise(x,z) + 1) / 2;
                for(int y = 1; y <= terrain.height; y++)
                {
                    if(y <= height)
                    {
                        terrain.data[terrain.GetIndex(x, y, z)] = 1;
                    }
                    else
                    {
                        terrain.data[terrain.GetIndex(x, y, z)] = 0;
                    }
                }
            }
        }
        terrain.dataBuffer.SetData(terrain.data);
        chunkManager.Refresh();
    }
}
