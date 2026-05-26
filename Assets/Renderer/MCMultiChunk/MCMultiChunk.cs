using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public partial class MCMultiChunk : MonoBehaviour
{
    [Header("区块大小")]
    public int chunkSize;
    //public Vector3Int size = new Vector3Int(16, 16, 16);
    public Mesh mesh;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public int cX, cY, cZ;

    public int startX, startY, startZ;

    public void Initialize()
    {
        /*simulator = new FluidSimulator(chunkSize, chunkSize, chunkSize);
        simulator.CreateData();
        simulator.data[simulator.GetIndex(3, 4, 4)] = 5000;*/
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        CreateVertices();
    }

    public void CreateVertices()
    {
        cX = (chunkSize - 1) * chunkSize * chunkSize;
        cY = chunkSize * (chunkSize - 1) * chunkSize;
        cZ = chunkSize * chunkSize * (chunkSize - 1);
        Vector3[] vertices = new Vector3[cX + cY + cZ];
        int idx = 0;
        for (int z = 0; z < chunkSize; z++)
            for (int y = 0; y < chunkSize; y++)
                for (int x = 0; x < chunkSize - 1; x++)
                    vertices[idx++] = new Vector3(x + 0.5f, y, z);

        for (int z = 0; z < chunkSize; z++)
            for (int y = 0; y < chunkSize - 1; y++)
                for (int x = 0; x < chunkSize; x++)
                    vertices[idx++] = new Vector3(x, y + 0.5f, z);

        for (int z = 0; z < chunkSize - 1; z++)
            for (int y = 0; y < chunkSize; y++)
                for (int x = 0; x < chunkSize; x++)
                    vertices[idx++] = new Vector3(x, y, z + 0.5f);

        mesh.vertices = vertices;
    }

    public int isoLevel;
    private List<int> triangles = new List<int>();

    public void RefreshIndices()
    {
        //triangles.Clear();
        
        int cursor = 0;
        int arrayCount = triangles.Count;
        int sizex = chunkSize;
        int sizey = chunkSize;
        int sizez = chunkSize;

        for (int vx = 0; vx < sizex - 1; vx++)
        {
            for (int vy = 0; vy < sizey - 1; vy++)
            {
                for (int vz = 0; vz < sizez - 1; vz++)
                {
                    int cubeIndex = 0;

                    int nx = vx + startX, ny = vy + startY, nz = vz + startZ;

                    int baseIndex = (nx + 1) * (simulator.depth + 2) * (simulator.height + 2) + (ny + 1) * (simulator.depth + 2) + (nz + 1);

                    cubeIndex |= simulator.data[baseIndex] > isoLevel ? 1 : 0;
                    cubeIndex |= simulator.data[baseIndex + (simulator.depth + 2) * (simulator.height + 2)] > isoLevel ? 2 : 0;
                    cubeIndex |= simulator.data[baseIndex + (simulator.depth + 2) * (simulator.height + 2) + (simulator.depth + 2)] > isoLevel ? 4 : 0;
                    cubeIndex |= simulator.data[baseIndex + (simulator.depth + 2)] > isoLevel ? 8 : 0;
                    cubeIndex |= simulator.data[baseIndex + 1] > isoLevel ? 16 : 0;
                    cubeIndex |= simulator.data[baseIndex + 1 + (simulator.depth + 2) * (simulator.height + 2)] > isoLevel ? 32 : 0;
                    cubeIndex |= simulator.data[baseIndex + 1 + (simulator.depth + 2) * (simulator.height + 2) + (simulator.depth + 2)] > isoLevel ? 64 : 0;
                    cubeIndex |= simulator.data[baseIndex + 1 + (simulator.depth + 2)] > isoLevel ? 128 : 0;

                    if (cubeIndex == 0 || cubeIndex == 255) continue;

                    for (int i = 0; i < 16; i += 3)
                    {
                        int e0 = triTable[cubeIndex, i];
                        if (e0 < 0) break; 

                        int e1 = triTable[cubeIndex, i + 2];
                        int e2 = triTable[cubeIndex, i + 1];

                        int v;
                        int gx = vx + edgeInfo[e0].dx;
                        int gy = vy + edgeInfo[e0].dy;
                        int gz = vz + edgeInfo[e0].dz;
                        if (edgeInfo[e0].dir == 0)
                            v = gz * (chunkSize * (chunkSize - 1)) + gy * (chunkSize - 1) + gx;
                        else if (edgeInfo[e0].dir == 1)
                            v = cX + gz * (chunkSize * (chunkSize - 1)) + gy * chunkSize + gx;
                        else
                            v = cX + cY + gz * (chunkSize * chunkSize) + gy * chunkSize + gx;
                        if (cursor < arrayCount)
                        {
                            triangles[cursor++] = v;//triangles.Add(v);
                        }
                        else { triangles.Add(v); cursor++; arrayCount++; }

                        gx = vx + edgeInfo[e1].dx;
                        gy = vy + edgeInfo[e1].dy;
                        gz = vz + edgeInfo[e1].dz;
                        if (edgeInfo[e1].dir == 0)
                            v = gz * (chunkSize * (chunkSize - 1)) + gy * (chunkSize - 1) + gx;
                        else if (edgeInfo[e1].dir == 1)
                            v = cX + gz * (chunkSize * (chunkSize - 1)) + gy * chunkSize + gx;
                        else
                            v = cX + cY + gz * (chunkSize * chunkSize) + gy * chunkSize + gx;
                        if (cursor < arrayCount) triangles[cursor++] = v;//triangles.Add(v);
                        else { triangles.Add(v); cursor++; arrayCount++; }

                        gx = vx + edgeInfo[e2].dx;
                        gy = vy + edgeInfo[e2].dy;
                        gz = vz + edgeInfo[e2].dz;
                        if (edgeInfo[e2].dir == 0)
                            v = gz * (chunkSize * (chunkSize - 1)) + gy * (chunkSize - 1) + gx;
                        else if (edgeInfo[e2].dir == 1)
                            v = cX + gz * (chunkSize * (chunkSize - 1)) + gy * chunkSize + gx;
                        else
                            v = cX + cY + gz * (chunkSize * chunkSize) + gy * chunkSize + gx;
                        if (cursor < arrayCount) triangles[cursor++] = v;//triangles.Add(v);
                        else { triangles.Add(v); cursor++; arrayCount++; }
                    }
                }
            }
        }

        mesh.SetTriangles(triangles, 0);
        //mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();       // 可选
    }

    public void myUpdate()
    {
        //RefreshIndices();
    }

    // 这里有强耦合代码

    public FluidSimulator simulator;

    struct EdgeInfo { public int dir; public int dx, dy, dz; }
    EdgeInfo[] edgeInfo = new EdgeInfo[12]
    {
    new EdgeInfo{ dir=0, dx=0, dy=0, dz=0 }, // 0: X边, 体素内起点(0,0,0)
    new EdgeInfo{ dir=1, dx=1, dy=0, dz=0 }, // 1: Y边
    new EdgeInfo{ dir=0, dx=0, dy=1, dz=0 }, // 2: X边
    new EdgeInfo{ dir=1, dx=0, dy=0, dz=0 }, // 3: Y边
    new EdgeInfo{ dir=0, dx=0, dy=0, dz=1 }, // 4: X边
    new EdgeInfo{ dir=1, dx=1, dy=0, dz=1 }, // 5: Y边
    new EdgeInfo{ dir=0, dx=0, dy=1, dz=1 }, // 6: X边
    new EdgeInfo{ dir=1, dx=0, dy=0, dz=1 }, // 7: Y边
    new EdgeInfo{ dir=2, dx=0, dy=0, dz=0 }, // 8: Z边
    new EdgeInfo{ dir=2, dx=1, dy=0, dz=0 }, // 9: Z边
    new EdgeInfo{ dir=2, dx=1, dy=1, dz=0 }, //10: Z边
    new EdgeInfo{ dir=2, dx=0, dy=1, dz=0 }  //11: Z边
    };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    int GetVertexIndex(int vx, int vy, int vz, int edge)
    {
        var info = edgeInfo[edge];
        int gx = vx + info.dx;
        int gy = vy + info.dy;
        int gz = vz + info.dz;
        if (info.dir == 0)      // X边
            return gz * (chunkSize * (chunkSize - 1)) + gy * (chunkSize - 1) + gx;
        else if (info.dir == 1) // Y边
            return cX + gz * (chunkSize * (chunkSize - 1)) + gy * chunkSize + gx;
        else                    // Z边
            return cX + cY + gz * (chunkSize * chunkSize) + gy * chunkSize + gx;
    }
    int XEdgeIndex(int x, int y, int z) => z * (chunkSize * (chunkSize - 1)) + y * (chunkSize - 1) + x;
    int YEdgeIndex(int x, int y, int z) => cX + z * (chunkSize * (chunkSize - 1)) + y * chunkSize + x;
    int ZEdgeIndex(int x, int y, int z) => cX + cY + z * (chunkSize * chunkSize) + y * chunkSize + x;

    public bool renderEdge = false;

    public void disableOnDrawGizmosSelected()
    {
        if (!renderEdge) return;
        Gizmos.DrawWireCube(Vector3.one * 8 + transform.position, Vector3.one * 15);   
    }
}
