using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class GridMapGenerator : MonoBehaviour
{
    // マップデータ（1が壁、0が床など）
    private int[,] mapData = new int[,]
    {
        { 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 1 },
        { 1, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1 },
    };

    private float size = 1.0f; 
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        int width = mapData.GetLength(0);
        int depth = mapData.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (mapData[x, z] == 1)
                {
                    // 中心座標ではなく、左下を基準にした位置
                    float xPos = x * size;
                    float zPos = z * size;
                    
                    // 各方向のチェック
                    // 上面 (Top)
                    AddFace(
                        new Vector3(xPos, size, zPos), 
                        new Vector3(xPos, size, zPos + size), 
                        new Vector3(xPos + size, size, zPos + size), 
                        new Vector3(xPos + size, size, zPos)
                    );

                    // 北面 (North, Z+)
                    if (z + 1 >= depth || mapData[x, z + 1] == 0)
                    {
                        AddFace(
                            new Vector3(xPos + size, 0, zPos + size),
                            new Vector3(xPos + size, size, zPos + size),
                            new Vector3(xPos, size, zPos + size),
                            new Vector3(xPos, 0, zPos + size)
                        );
                    }

                    // 南面 (South, Z-)
                    if (z - 1 < 0 || mapData[x, z - 1] == 0)
                    {
                        AddFace(
                            new Vector3(xPos, 0, zPos),
                            new Vector3(xPos, size, zPos),
                            new Vector3(xPos + size, size, zPos),
                            new Vector3(xPos + size, 0, zPos)
                        );
                    }

                    // 東面 (East, X+)
                    if (x + 1 >= width || mapData[x + 1, z] == 0)
                    {
                        AddFace(
                            new Vector3(xPos + size, 0, zPos),
                            new Vector3(xPos + size, size, zPos),
                            new Vector3(xPos + size, size, zPos + size),
                            new Vector3(xPos + size, 0, zPos + size)
                        );
                    }

                    // 西面 (West, X-)
                    if (x - 1 < 0 || mapData[x - 1, z] == 0)
                    {
                        AddFace(
                            new Vector3(xPos, 0, zPos + size),
                            new Vector3(xPos, size, zPos + size),
                            new Vector3(xPos, size, zPos),
                            new Vector3(xPos, 0, zPos)
                        );
                    }
                    
                    // 底面は通常見えないので省略可能ですが、必要なら追加してください
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // 4つの頂点を受け取って四角形（三角形2つ）を作る
    void AddFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;

        // 頂点を追加
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // 三角形を追加 (時計回り)
        // 1つ目の三角形
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        // 2つ目の三角形
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }
}