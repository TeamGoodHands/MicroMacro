using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

[ExecuteAlways] // エディタ上でも動作するようにします
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CloudMeshGenerator : MonoBehaviour
{
    private struct MeshData
    {
        public Vector3[] Vertices;
        public Vector2[] UVs;
        public int[] Triangles;
    }

    [Header("Base Shape Settings")]
    [Tooltip("雲の横幅の倍率")]
    public float WidthScale = 2.5f;

    [Tooltip("雲の高さの倍率")]
    public float HeightScale = 1.0f;

    [Tooltip("底面をどれくらい平らにするか (0～1)")]
    [Range(0f, 1f)]
    public float FlattenBottomAmount = 0.7f;

    [Header("Noise Settings")]
    [Tooltip("ノイズの基本スケール")]
    public float BaseNoiseScale = 3.0f;

    [Tooltip("ノイズによる変形の強さ")]
    public float NoiseStrength = 1.5f;

    [Tooltip("ノイズの重ね合わせ回数")]
    [Range(1, 5)]
    public int Octaves = 3;

    [Tooltip("ノイズの粗さの減衰率")]
    [Range(0.1f, 1.0f)]
    public float Lacunarity = 2.0f;

    [Tooltip("ノイズの強さの減衰率")]
    [Range(0.1f, 1.0f)]
    public float Persistence = 0.5f;

    [Tooltip("ランダムシード")]
    public int Seed = 0;

    [Tooltip("メッシュの分割数")]
    public int Resolution = 80;

    private MeshFilter meshFilter;
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        RequestGeneration();
    }

    /// <summary>
    /// インスペクタの値が変更されたときに呼ばれます
    /// </summary>
    private void OnValidate()
    {
        // パラメータが変更されたら再生成をリクエストします
        // エディタ上での反応を良くするため、遅延なしで呼び出します
        RequestGeneration();
    }

    private void RequestGeneration()
    {
        // 以前の処理が動いていればキャンセルします
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        cancellationTokenSource = new CancellationTokenSource();
        GenerateCloudAsync(cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid GenerateCloudAsync(CancellationToken token)
    {
        if (Resolution < 4)
            Resolution = 4;

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        // スレッドプールに移動して計算
        await UniTask.SwitchToThreadPool();

        // キャンセルされていたら処理を中断
        if (token.IsCancellationRequested)
            return;

        MeshData data = CalculateMeshData();

        if (token.IsCancellationRequested)
            return;

        // メインスレッドに戻って適用
        await UniTask.SwitchToMainThread();

        if (token.IsCancellationRequested || meshFilter == null)
            return;

        ApplyMesh(data);
    }

    private MeshData CalculateMeshData()
    {
        int res = Resolution;
        int numVerts = (res + 1) * (res + 1);
        
        MeshData data = new MeshData();
        data.Vertices = new Vector3[numVerts];
        data.UVs = new Vector2[numVerts];
        data.Triangles = new int[res * res * 6];

        int vertIndex = 0;
        int triIndex = 0;
        Vector3 noiseOffset = new Vector3(Seed * 13.1f, Seed * 17.4f, Seed * 23.6f);

        for (int lat = 0; lat <= res; lat++)
        {
            float v = (float)lat / res;
            float latAngle = v * Mathf.PI;
            float sinLat = Mathf.Sin(latAngle);
            float cosLat = Mathf.Cos(latAngle);

            for (int lon = 0; lon <= res; lon++)
            {
                float u = (float)lon / res;
                float lonAngle = u * 2f * Mathf.PI;
                
                float x = Mathf.Cos(lonAngle) * sinLat;
                float y = cosLat;
                float z = Mathf.Sin(lonAngle) * sinLat;

                Vector3 spherePos = new Vector3(x, y, z);
                
                Vector3 finalPos = GetCloudShape(spherePos, noiseOffset);

                data.Vertices[vertIndex] = finalPos;
                data.UVs[vertIndex] = new Vector2(u, v);

                if (lat < res && lon < res)
                {
                    int current = lat * (res + 1) + lon;
                    int next = current + res + 1;

                    data.Triangles[triIndex++] = current;
                    data.Triangles[triIndex++] = current + 1;
                    data.Triangles[triIndex++] = next + 1;

                    data.Triangles[triIndex++] = current;
                    data.Triangles[triIndex++] = next + 1;
                    data.Triangles[triIndex++] = next;
                }

                vertIndex++;
            }
        }

        return data;
    }

    private Vector3 GetCloudShape(Vector3 spherePos, Vector3 offset)
    {
        Vector3 pos = spherePos;
        pos.x *= WidthScale;
        pos.z *= WidthScale;
        pos.y *= HeightScale;

        if (pos.y < 0)
        {
            float flattenFactor = Mathf.Lerp(1.0f, 0.2f, FlattenBottomAmount);
            pos.y *= flattenFactor;
        }

        float noiseVal = 0f;
        float frequency = BaseNoiseScale;
        float amplitude = 1.0f;
        float totalAmplitude = 0f;

        for (int i = 0; i < Octaves; i++)
        {
            float n = Get3DNoise(spherePos * frequency + offset);
            noiseVal += n * amplitude;
            totalAmplitude += amplitude;
            amplitude *= Persistence;
            frequency *= Lacunarity;
        }

        float bottomMask = Mathf.Clamp01(spherePos.y + 0.5f);
        float displacement = (noiseVal / totalAmplitude) * NoiseStrength * bottomMask;
        
        return pos + (spherePos * displacement);
    }

    private float Get3DNoise(Vector3 pos)
    {
        float xy = Mathf.PerlinNoise(pos.x, pos.y);
        float yz = Mathf.PerlinNoise(pos.y, pos.z);
        float xz = Mathf.PerlinNoise(pos.x, pos.z);
        
        return ((xy + yz + xz) / 3f) - 0.5f;
    }

    private void ApplyMesh(MeshData data)
    {
        // 既存のメッシュがあれば破棄してメモリリークを防ぎます
        if (meshFilter.sharedMesh != null)
        {
            // エディタ実行中とプレイモードで破棄メソッドを使い分けます
            if (Application.isPlaying)
                Destroy(meshFilter.sharedMesh);
            else
                DestroyImmediate(meshFilter.sharedMesh);
        }

        Mesh mesh = new Mesh();
        mesh.name = "CloudMesh"; // 名前をつけておくとデバッグ時に便利です

        if (data.Vertices.Length > 65000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = data.Vertices;
        mesh.uv = data.UVs;
        mesh.triangles = data.Triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void OnDisable()
    {
        // スクリプトが無効化されたり削除されたりしたときはキャンセルします
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}