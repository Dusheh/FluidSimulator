using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulator
{
    public readonly int width, height, depth;
    public FluidSimulator(int Width, int Height, int Depth)
    {
        width = Width;
        height = Height;
        depth = Depth;
    }

    public int[] data;
    public int[] next;
    public void CreateData()
    {
        data = new int[(width + 2) * (height + 2) * (depth + 2)];
        next = new int[(width + 2) * (height + 2) * (depth + 2)];
    }

    private void BoundsFlow()
    {
        SimpleGUI.creeperCount = 0;

        int cursor = 0;
        cursor += (height + 2) * (depth + 2);
        for (int x = 1; x <= width; x++)
        {
            cursor += (depth + 2);
            for (int y = 1; y <= height; y++)
            {
                cursor++;
                for (int z = 1; z <= depth; z++)
                {
                    next[cursor] = (int)(
                        data[cursor] - (int)(data[cursor] / 7) * 6 +
                        data[cursor + 1] / 7 +
                        data[cursor - 1] / 7 +
                        data[cursor + (depth + 2)] / 7 +
                        data[cursor - (depth + 2)] / 7 +
                        data[cursor + (depth + 2) * (height + 2)] / 7 +
                        data[cursor - (depth + 2) * (height + 2)] / 7
                    );
                    SimpleGUI.creeperCount += next[cursor];

                    /*next[cursor] = (int)(
                        (data[cursor] + 
                        data[cursor + 1] +
                        data[cursor - 1] +
                        data[cursor + (depth + 2)] +
                        data[cursor - (depth + 2)] +
                        data[cursor + (depth + 2) * (height + 2)] +
                        data[cursor - (depth + 2) * (height + 2)]) / 7);*/

                    //int index = GetIndex(x, y, z);
                    /*if(cursor != GetIndex(x,y,z))
                    {
                        Debug.LogFormat("cursor({0})!=GetIndex({1})", cursor, index);
                        return;
                    }*/
                    cursor++;
                }
                cursor++;
            }
            cursor += (depth + 2);
        }
        //cursor += (height + 2) * (depth + 2);
    }
    private bool CheckCursorEqualsIndex(int cursor, int x, int y, int z)
    {
        int index = GetIndex(x, y, z);
        if (cursor != index)
        {
            Debug.LogFormat("cursor({0})!=GetIndex({1})", cursor, index);
            return false;
        }
        return true;
    }
    private void OutputCursorCoord(int cursor)
    {
        Debug.LogFormat("Cursor({0}):" + new Vector3Int(cursor / ((depth + 2) * (height + 2)), (cursor / (depth + 2)) % (height + 2), cursor % (depth + 2)).ToString(),cursor);
    }
    private void EdgeFlowAndToBound()
    {
        int x, y, z, cursor = 0, ncursor, offset;
        cursor += (depth + 2);
        for (y = 1; y <= height; y++)
        {
            //cursor += (depth + 2);
            cursor++;
            for (z = 1; z <= depth; z++)
            { 
                //x = 0;
                offset = (depth + 2) * (height + 2);
                next[cursor] = data[cursor + offset] / 7;
                next[cursor + offset] += next[cursor];
                SimpleGUI.creeperCount += next[cursor];
                //x = width + 1
                ncursor = cursor + offset * (width + 1);
                next[ncursor] = data[ncursor - offset] / 7;
                next[ncursor - offset] += next[ncursor];
                SimpleGUI.creeperCount += next[ncursor];

                cursor++;
            }
            cursor++;
        }
        cursor = 0;
        cursor += (height + 2) * (depth + 2);
        for (x = 1; x <= width; x++)
        {
            cursor += (depth + 2);
            for (y = 1; y <= height; y++)
            {
                //z = 0;
                offset = 1;
                next[cursor] = data[cursor + offset] / 7;
                next[cursor + offset] += next[cursor];
                SimpleGUI.creeperCount += next[cursor];
                //z = depth + 1
                ncursor = cursor + offset * (depth + 1);
                next[ncursor] = data[ncursor - offset] / 7;
                next[ncursor - offset] += next[ncursor];
                SimpleGUI.creeperCount += next[ncursor];
                //CheckCursorEqualsIndex(ncursor, x, y, depth+1);
                //CheckCursorEqualsIndex(ncursor, x, y, depth + 1);
                //CheckCursorEqualsIndex(ncursor - offset, x, y, depth);
                //CheckCursorEqualsIndex(ncursor, x, y, depth+1);
                cursor += depth+2;
            }
            cursor += (depth + 2);
        }
        cursor = 0;
        cursor += (height + 2) * (depth + 2);
        for (x = 1; x <= width; x++)
        { 
            for (z = 1; z <= depth; z++)
            {
                cursor++;
                //y = 0
                offset = (depth + 2);
                next[cursor] = data[cursor + offset] / 7;
                next[cursor + offset] += next[cursor];
                SimpleGUI.creeperCount += next[cursor];
                /*if (!CheckCursorEqualsIndex(cursor, x, 0, z))
                {
                    OutputCursorCoord(cursor);
                    Debug.Log(new Vector3Int(x, 0, z));
                }*/
                //CheckCursorEqualsIndex(cursor, x, 0, z);
                //y = height + 1
                ncursor = cursor + offset * (height + 1);
                next[ncursor] = data[ncursor - offset] / 7;
                next[ncursor - offset] += next[ncursor];
                SimpleGUI.creeperCount += next[ncursor];

            }
            cursor++;
            cursor++;
            cursor += (depth + 2) * (height + 1);
        }
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

    int[] old;
    public void DoFlow()
    {
        BoundsFlow();
        EdgeFlowAndToBound();
        old = data;
        data = next;
        next = old;
    }
}
