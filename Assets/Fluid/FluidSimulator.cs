using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulator
{
    public readonly int width, height, depth;
    public int[] data, next;
    public FluidSimulator(int Width, int Height, int Depth)
    {
        width = Width;
        height = Height;
        depth = Depth;
    }
    public void CreateData()
    {
        data = new int[(width + 2) * (height + 2) * (depth + 2)];
        next = new int[(width + 2) * (height + 2) * (depth + 2)];
    }

    // Not recommend function, it'll cause performance issues, unless necessary.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetIndex(int x, int y, int z)
    {
        return x * (depth + 2) * (height + 2) + y * (depth + 2) + z; 
    }

    // Not recommend function, it'll cause performance issues, unless necessary.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetValue(int x, int y, int z)
    {
        return data[x * (depth + 2) * (height + 2) + y * (depth + 2) + z];
    }
}
