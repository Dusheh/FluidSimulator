using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCEdge : MonoBehaviour
{
    public MCMultiChunkManager manager;
    public enum Direction { Left, Right, Up, Down, Front, Back }
    public Direction dir;

    public void Initialize(Direction dir)
    {
        
    }

    public void myUpdate()
    {
        Debug.LogWarning("Undone");
    }
}
