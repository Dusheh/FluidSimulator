using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGUI : MonoBehaviour
{
    public Rect creeperCountRect;
    void Awake()
    {
        creeperCountRect = new Rect(5, 5, 160, 30);    
    }

    public static long creeperCount;
    void OnGUI()
    {
#if DEBUG
        GUI.Label(creeperCountRect, creeperCount.ToString());
#endif
    }
}
