using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimController
{
    public int width, height, depth;

    public ComputeShader fluidSimShader;
    public ComputeBuffer dataBuffer;
    public ComputeBuffer nextBuffer;
    public ComputeBuffer resuBuffer;
    public ComputeBuffer propBuffer;
    public int kernelInit;
    [HideInInspector]
    public int kernelMain;
    [HideInInspector]
    public int kernelX;
    [HideInInspector]
    public int kernelY;
    [HideInInspector]
    public int kernelZ;

    public FluidSimulator simulator;

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
        fluidSimShader.SetBuffer(kernelMain, "next", nextBuffer);

        fluidSimShader.SetBuffer(kernelX, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelX, "next", nextBuffer);

        fluidSimShader.SetBuffer(kernelY, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelY, "next", nextBuffer);

        fluidSimShader.SetBuffer(kernelZ, "data", dataBuffer);
        fluidSimShader.SetBuffer(kernelZ, "next", nextBuffer);
    }

    public void SetValues()
    {
        int[] prop = new int[9]
            { width, height, depth, width + 2, height + 2, depth + 2,
            1/*StrideZ*/, depth+2, (depth+2)*(height+2) };
        /*fluidSimShader.SetInt("width", width);
        fluidSimShader.SetInt("height", height);
        fluidSimShader.SetInt("depth", depth);
        fluidSimShader.SetInt("fullW", width + 2);
        fluidSimShader.SetInt("fullH", height + 2);
        fluidSimShader.SetInt("fullD", depth + 2);*/
        propBuffer = new ComputeBuffer(9, sizeof(int));
        propBuffer.SetData(prop);
        fluidSimShader.SetBuffer(kernelMain, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelX, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelY, "prop", propBuffer);
        fluidSimShader.SetBuffer(kernelZ, "prop", propBuffer);

        fluidSimShader.SetBuffer(kernelMain, "prop", propBuffer);
        resuBuffer = new ComputeBuffer(simulator.data.Length, sizeof(int));
        fluidSimShader.SetBuffer(kernelMain, "resu", resuBuffer);
        buf = new int[simulator.data.Length];
    }

    int[] buf;
    public void GetValue()
    {
        //resuBuffer.GetData(buf);
        //Debug.Log(buf[0]);
        //Debug.Log(buf[1]);
        //Debug.Log(buf[2]);
        //Debug.Log(buf[3]);
        return;
        for (int i = 0; i < buf.Length; i++)
        {
            if (buf[i] == 0)
            {
                var x = i / ((depth + 2) * (height + 2));
                var y = (i / (depth + 2)) % (height + 2);
                var z = i % (depth + 2);
                if (x == 0 || y == 0 || z == 0 || x == 66 || y == 66 || z == 66) continue;
                Debug.LogFormat("!({0},{1},{2}):{3}",x,y,z,buf[i]);
            }
        }
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
        //dataBuffer.GetData(simulator.data);
        GetValue();
    }

    public void OnDestroy()
    {
        dataBuffer.Release();
        nextBuffer.Release();
    }
}
