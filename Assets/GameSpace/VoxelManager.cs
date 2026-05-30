using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelManager : IDisposable
{
    public readonly int width, height, depth;
    public int[] data;
    public ComputeShader updateShader;
    public ComputeBuffer dataBuffer;
    public bool realtimeUpdate;
    private bool disposed = false;

    public VoxelManager(int Width, int Height, int Depth)
    {
        width = Width;
        height = Height;
        depth = Depth;
        data = new int[(width + 2) * (height + 2) * (depth + 2)];
    }

    public virtual void Initialize()
    {
        CreateData();
    }

    protected virtual void CreateData()
    {
        dataBuffer = new ComputeBuffer((width + 2) * (height + 2) * (depth + 2), sizeof(int));
    }

    protected virtual void GetKernel() { }

    protected virtual void BindBuffers() { }

    protected virtual void SetValues() { }

    public virtual void Refresh() { }

    // Not recommend function, it'll cause performance issues, unless necessary.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetIndex(int x, int y, int z)
    {
        return x * (depth + 2) * (height + 2) + y * (depth + 2) + z;
    }

    // Not recommend function, it'll cause performance issues, unless necessary.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetValue(int x, int y, int z, int[] array)
    {
        return array[x * (depth + 2) * (height + 2) + y * (depth + 2) + z];
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
        disposed = true;
    }
}
