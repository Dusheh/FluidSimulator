using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulator : VoxelManager
{
    public int[] next;

    public ComputeBuffer nextBuffer;
    public ComputeBuffer resuBuffer;
    public ComputeBuffer propBuffer;
    public int kernelMain;
    public int kernelX;
    public int kernelY;
    public int kernelZ;

    public FluidSimulator(int Width, int Height, int Depth) : base(Width, Height, Depth) { realtimeUpdate = true; }

    public override void Initialize()
    {
        CreateData();
        GetKernel();
        BindBuffers();
        SetValues();
    }

    protected override void CreateData()
    {
        base.CreateData();
        nextBuffer = new ComputeBuffer((width + 2) * (height + 2) * (depth + 2), sizeof(int));
        nextBuffer.SetData(data);
    }

    protected override void GetKernel()
    {
        base.GetKernel();
        kernelMain = updateShader.FindKernel("BoundFlow");
        kernelX = updateShader.FindKernel("EdgeFlow_X");
        kernelY = updateShader.FindKernel("EdgeFlow_Y");
        kernelZ = updateShader.FindKernel("EdgeFlow_Z");
    }

    protected override void BindBuffers()
    {
        base.BindBuffers();

        updateShader.SetBuffer(kernelMain, "data", dataBuffer);
        updateShader.SetBuffer(kernelX, "data", dataBuffer);
        updateShader.SetBuffer(kernelY, "data", dataBuffer);
        updateShader.SetBuffer(kernelZ, "data", dataBuffer);

        updateShader.SetBuffer(kernelMain, "next", nextBuffer);
        updateShader.SetBuffer(kernelX, "next", nextBuffer);
        updateShader.SetBuffer(kernelY, "next", nextBuffer);
        updateShader.SetBuffer(kernelZ, "next", nextBuffer);
    }

    protected override void SetValues()
    {
        base.SetValues();

        for (int i = 0; i < 10; i++)
        {
            data[Random.Range(0, data.Length)] = Random.Range(2000, 10000);
        }
        dataBuffer.SetData(data);

        int[] prop = new int[9]
            { width, height, depth, width + 2, height + 2, depth + 2, 1, depth + 2, (depth+2) * (height+2) };
        propBuffer = new ComputeBuffer(9, sizeof(int));
        resuBuffer = new ComputeBuffer(data.Length, sizeof(int));
        propBuffer.SetData(prop);

        updateShader.SetBuffer(kernelMain, "prop", propBuffer);
        updateShader.SetBuffer(kernelX, "prop", propBuffer);
        updateShader.SetBuffer(kernelY, "prop", propBuffer);
        updateShader.SetBuffer(kernelZ, "prop", propBuffer);
        updateShader.SetBuffer(kernelMain, "resu", resuBuffer);
    }

    public void BindTerrainBuffer(ComputeBuffer buffer)
    {
        updateShader.SetBuffer(kernelMain, "terrain", buffer);
    }

    protected void DispatchGPU()
    {
        updateShader.Dispatch(kernelMain, width / 8, height / 8, depth / 16);
        updateShader.Dispatch(kernelX, height / 8, depth / 8, 1);
        updateShader.Dispatch(kernelY, width / 8, depth / 8, 1);
        updateShader.Dispatch(kernelZ, width / 8, height / 8, 1);

        (dataBuffer, nextBuffer) = (nextBuffer, dataBuffer);
        BindBuffers();
    }

    public override void Refresh()
    {
        base.Refresh();
        DispatchGPU();
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        nextBuffer?.Dispose();
        propBuffer?.Dispose();
        resuBuffer?.Dispose();
        base.Dispose(disposing);
    }
}
