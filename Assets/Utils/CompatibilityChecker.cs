using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CompatibilityChecker
{
    public static bool MemoryEnoughToPlay(int mapWidth, int mapHeight, int mapDepth)
    {
        if (SystemInfo.graphicsMemorySize < mapWidth * mapHeight * mapDepth * 4 / 1024 / 1024) return false;
        return true;
    }
}
