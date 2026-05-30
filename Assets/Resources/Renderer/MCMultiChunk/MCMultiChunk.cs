using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public partial class MCMultiChunk : MonoBehaviour
{
    [Header("区块大小")]
    public int chunkSize;

    [Header("区块设置")]
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    public int isoLevel;

    [HideInInspector]
    private int cX, cY, cZ;
    [HideInInspector]
    private int startX, startY, startZ;

    public ComputeShader MCShader;
    private ComputeBuffer priv;
    private ComputeBuffer indices;
    private ComputeBuffer indexCounter;
    [HideInInspector]
    public int kernelMain;
    [HideInInspector]
    public Material material;

    private AsyncGPUReadbackRequest r1, r2;
    [HideInInspector]
    public bool isPending = false, updateDone = true;
    private readonly int[] counterZero = { 0 };

    [Header("调试设置")]
    [SerializeField]
    private bool renderEdge = true;

    public void Initialize(int startX, int startY, int startZ)
    {
        this.startX = startX;
        this.startY = startY;
        this.startZ = startZ;
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        CreateVertices();
        MCShader = Instantiate(Resources.Load<ComputeShader>("Renderer/MCMultiChunk/MCShader"));
        if (MCShader == null) Debug.LogError("Shader not found");
        kernelMain = MCShader.FindKernel("CSMain");
        meshRenderer.material = material;//Resources.Load<Material>("Materials/Blue");
        BindBuffer();

        mesh.SetIndexBufferParams(50625, IndexFormat.UInt32);
    }

    public void BindBuffer()
    { 
        indexCounter = new ComputeBuffer(1, sizeof(int));
        indices = new ComputeBuffer(50625, sizeof(int));
        priv = new ComputeBuffer(3, sizeof(int));
        priv.SetData(new int[] { startX, startY, startZ });
        MCShader.SetBuffer(kernelMain, "IndexCounter", indexCounter);
        MCShader.SetBuffer(kernelMain, "Indices", indices);
        MCShader.SetBuffer(kernelMain, "priv", priv);

        //MCShader.SetBuffer(kernelMain, "DataBuffer", databuf);
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

    public void RefreshIndicesGPU()
    {
        indexCounter.SetData(counterZero);
        MCShader.Dispatch(kernelMain, 16 / 8, 16 / 8, 16 / 8);
    }

    public void WaitForRequest()
    {
        if (isPending) return;
        r1 = AsyncGPUReadback.Request(indices);
        r2 = AsyncGPUReadback.Request(indexCounter);
        isPending = true;
    }

    public void WaitForUpdate()
    {
        if (!r1.done || !r2.done) return;
        if (!r1.hasError && !r2.hasError)
        {
            NativeArray<int> indi = r1.GetData<int>();
            NativeArray<int> coun = r2.GetData<int>();

            mesh.SetIndexBufferData(indi, 0, 0, coun[0]);

            mesh.SetSubMesh(0, new SubMeshDescriptor
            {
                indexStart = 0,
                indexCount = coun[0],
                topology = MeshTopology.Triangles,
                baseVertex = 0
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
;
            mesh.RecalculateNormals();
        }
        else
        {
            Debug.LogError("GPU readback error occurred");
        }
        updateDone = true;
    }

    public void myUpdate()
    {
        RefreshIndicesGPU();
    }

    public void OnDrawGizmosSelected()
    {
        if (!renderEdge) return;
        Gizmos.DrawWireCube(Vector3.one * 8 + transform.position, Vector3.one * 15);   
    }

    public void OnDestroy()
    {
        indexCounter.Dispose();
        indices.Dispose();
        priv.Dispose();
    }
}
