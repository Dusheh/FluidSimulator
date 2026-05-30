using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSimulator : VoxelManager
{
    public int kernelMain;
    public ComputeBuffer propBuffer;

    public TerrainSimulator(int Width, int Height, int Depth) : base(Width, Height, Depth) { realtimeUpdate = false; }

    public override void Initialize()
    {
        CreateData();
        GetKernel();
        BindBuffers();
        SetValues();
    }

    protected override void GetKernel()
    {
        base.GetKernel();
        kernelMain = updateShader.FindKernel("Refresh");
    }

    protected override void BindBuffers()
    {
        base.BindBuffers();
        updateShader.SetBuffer(kernelMain, "data", dataBuffer);
    }

    protected override void SetValues()
    {
        base.SetValues();

        FastNoiseLite fastNoise = new FastNoiseLite(Random.Range(0, 2048));
        fastNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        
        fastNoise.SetFrequency(0.01f);

        for (int x = 1; x <= width; x++)
        {
            for (int y = 1; y <= height; y++)
            {
                for (int z = 1; z <= depth; z++)
                {
                    data[GetIndex(x, y, z)] = (int)((fastNoise.GetNoise(x, y, z) + 1.5) / 2);
                }
            }
        }

        int[] prop = new int[9]
            { width, height, depth, width + 2, height + 2, depth + 2, 1, depth + 2, (depth+2) * (height+2) };
        propBuffer = new ComputeBuffer(9, sizeof(int));
        propBuffer.SetData(prop);
        dataBuffer.SetData(data);
        updateShader.SetBuffer(kernelMain, "prop", propBuffer);
    }

    protected void DispatchGPU()
    {
        updateShader.Dispatch(kernelMain, width / 8, height / 8, depth / 16);
    }

    public override void Refresh()
    {
        base.Refresh();
        DispatchGPU();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) return;
        propBuffer?.Dispose();
        base.Dispose(disposing);
    }
}
