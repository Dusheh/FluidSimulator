using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FluidSimController : IDisposable
{
    public int width, height, depth;

    public FluidSimulator simulator;
    public ComputeShader fluidSimShader;
    public ComputeBuffer dataBuffer;
    public ComputeBuffer nextBuffer;
    public ComputeBuffer resuBuffer;
    public ComputeBuffer propBuffer;
    public int kernelMain;
    public int kernelX;
    public int kernelY;
    public int kernelZ;

    private bool disposed = false;

    public void Awake()
    {
        GetKernel();
        SetValues();
    }

    public void GetKernel()
    {
        kernelMain = fluidSimShader.FindKernel("BoundFlow");
        kernelX = fluidSimShader.FindKernel("EdgeFlow_X");
        kernelY = fluidSimShader.FindKernel("EdgeFlow_Y");
        kernelZ = fluidSimShader.FindKernel("EdgeFlow_Z");
    }

    public void CreateData(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        dataBuffer = new ComputeBuffer((width + 2) * (height + 2) * (depth + 2), sizeof(int));
        nextBuffer = new ComputeBuffer((width + 2) * (height + 2) * (depth + 2), sizeof(int));
        dataBuffer.SetData(simulator.data);
        nextBuffer.SetData(simulator.data);
        BindBuffers();
    }

    public void BindBuffers()
    {
        fluidSimShader.SetBuffer(kernelMain, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelX, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelY, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelZ, "data", dataBuffer);

        fluidSimShader.SetBuffer(kernelMain, "next", nextBuffer);
        fluidSimShader.SetBuffer(kernelX, "next", nextBuffer);
        fluidSimShader.SetBuffer(kernelY, "next", nextBuffer);
        fluidSimShader.SetBuffer(kernelZ, "next", nextBuffer);
    }

    public void SetValues()
    {
        int[] prop = new int[9]
            { width, height, depth, width + 2, height + 2, depth + 2, 1, depth+2, (depth+2)*(height+2) };
        propBuffer = new ComputeBuffer(9, sizeof(int));
        resuBuffer = new ComputeBuffer(simulator.data.Length, sizeof(int));
        propBuffer.SetData(prop);

        fluidSimShader.SetBuffer(kernelMain, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelX, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelY, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelZ, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelMain, "resu", resuBuffer);
    }

    void DoFlowGPU()
    {
        fluidSimShader.Dispatch(kernelMain, width / 8, height / 8, depth / 16);
        fluidSimShader.Dispatch(kernelX, height / 8, depth / 8, 1);
        fluidSimShader.Dispatch(kernelY, width / 8, depth / 8, 1);
        fluidSimShader.Dispatch(kernelZ, width / 8, height / 8, 1);

        (dataBuffer, nextBuffer) = (nextBuffer, dataBuffer);
        BindBuffers();
    }

    public void FixedUpdate()
    {
        DoFlowGPU();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        dataBuffer?.Dispose();
        nextBuffer?.Dispose();
        propBuffer?.Dispose();
        resuBuffer?.Dispose();
        disposed = true;
    }
}
