using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBindHelper : MonoBehaviour
{
    public MCMultiChunkManager terrain;
    public MCMultiChunkManager fluid;
    public void Start()
    {
        var flu = (fluid.voxelManager as FluidSimulator);
        var ter = (terrain.voxelManager as TerrainSimulator);
        flu.BindTerrainBuffer(ter.dataBuffer);
    }
}
