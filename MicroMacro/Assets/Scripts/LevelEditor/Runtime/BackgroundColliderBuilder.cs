using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BackgroundColliderBuilder : MonoBehaviour
{
    [SerializeField] private float weldThreshold = 0.0005f;     // 頂点を同一点扱いする距離
    [SerializeField] private float gridSnap = 0.0005f;           // スナップ格子サイズ（T字対策）
    [SerializeField] private bool includeMeshFilters = true;
    [SerializeField] private bool includeMeshColliders = true;

    [ContextMenu("Build Clean NonConvex MeshCollider")]
    private void Build()
    {
        List<CombineInstance> combines = new List<CombineInstance>(1024);

        if (includeMeshFilters)
        {
            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null)
                {
                    continue;
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    transform = mf.transform.localToWorldMatrix
                };
                combines.Add(ci);
            }
        }

        if (includeMeshColliders)
        {
            foreach (MeshCollider mc in GetComponentsInChildren<MeshCollider>())
            {
                if (mc.sharedMesh == null)
                {
                    continue;
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = mc.sharedMesh,
                    transform = mc.transform.localToWorldMatrix
                };
                combines.Add(ci);
            }
        }

        if (combines.Count == 0)
        {
            Debug.LogWarning("No meshes found to build collider.");

            return;
        }

        // 1) まずは素直に結合
        Mesh combined = new Mesh
        {
            indexFormat = IndexFormat.UInt32
        };
        combined.CombineMeshes(combines.ToArray(), true, true, false);

        // 2) Weld＋クリーン（衝突用特化）
        Mesh cleaned = WeldAndCleanForCollision(combined, weldThreshold, gridSnap);

        // 3) MeshCollider に適用（非凸・静的）
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }

        // Cooking オプション（物理側の Weld／クリーンも使う）
        collider.cookingOptions =
              MeshColliderCookingOptions.CookForFasterSimulation
            | MeshColliderCookingOptions.EnableMeshCleaning
            | MeshColliderCookingOptions.WeldColocatedVertices;

        collider.sharedMesh = null;
        collider.convex = false;
        collider.sharedMesh = cleaned;

        Debug.Log($"Background collider rebuilt. Vertices={cleaned.vertexCount}");
    }

    private static Mesh WeldAndCleanForCollision(Mesh source, float threshold, float snap)
    {
        Vector3[] srcPositions = source.vertices;
        int subMeshCount = source.subMeshCount;

        // すべてのインデックスを 1 本化（三角形のみ想定）
        List<int> all = new List<int>();
        for (int s = 0; s < subMeshCount; s++)
        {
            int[] idx = source.GetIndices(s);
            all.AddRange(idx);
        }

        // 位置をワールド空間からそのまま扱っている想定なので、ここではローカルのまま再構築する。
        // スナップ → ハッシュ Weld
        int count = srcPositions.Length;
        Dictionary<Vector3Int, List<int>> cellToList = new Dictionary<Vector3Int, List<int>>(count);
        List<Vector3> newPositions = new List<Vector3>(count);
        int[] oldToNew = new int[count];

        float inv = 1.0f / Mathf.Max(snap, 1e-12f);
        float thresholdSq = threshold * threshold;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = srcPositions[i];

            // 格子スナップ（T字の微小隙間を同一点化）
            Vector3 snapped = new Vector3(
                Mathf.Round(p.x * inv) / inv,
                Mathf.Round(p.y * inv) / inv,
                Mathf.Round(p.z * inv) / inv
            );

            Vector3Int cell = new Vector3Int(
                Mathf.FloorToInt(snapped.x / threshold),
                Mathf.FloorToInt(snapped.y / threshold),
                Mathf.FloorToInt(snapped.z / threshold)
            );

            int found = -1;
            if (cellToList.TryGetValue(cell, out List<int> list))
            {
                for (int k = 0; k < list.Count; k++)
                {
                    int cand = list[k];
                    if ((newPositions[cand] - snapped).sqrMagnitude <= thresholdSq)
                    {
                        found = cand;
                        break;
                    }
                }
            }

            if (found < 0)
            {
                found = newPositions.Count;
                newPositions.Add(snapped);
                if (list == null)
                {
                    list = new List<int>(4);
                    cellToList[cell] = list;
                }
                list.Add(found);
            }

            oldToNew[i] = found;
        }

        // 三角形の張り替え＋重複・退化除去
        HashSet<(int, int, int)> uniqueTris = new HashSet<(int, int, int)>();
        List<int> finalIndices = new List<int>(all.Count);

        for (int i = 0; i < all.Count; i += 3)
        {
            int a = oldToNew[all[i + 0]];
            int b = oldToNew[all[i + 1]];
            int c = oldToNew[all[i + 2]];

            // 退化
            if (a == b || b == c || c == a)
            {
                continue;
            }

            // 面積ほぼゼロ除去
            Vector3 pa = newPositions[a];
            Vector3 pb = newPositions[b];
            Vector3 pc = newPositions[c];
            if (Vector3.Cross(pb - pa, pc - pa).sqrMagnitude <= 1e-24f)
            {
                continue;
            }

            // 重複三角形除去（頂点番号の順序に依らず同一判定）
            int x = a, y = b, z = c;
            if (x > y) { int tmp = x; x = y; y = tmp; }
            if (y > z) { int tmp = y; y = z; z = tmp; }
            if (x > y) { int tmp = x; x = y; y = tmp; }

            (int, int, int) key = (x, y, z);
            if (!uniqueTris.Add(key))
            {
                continue;
            }

            finalIndices.Add(a);
            finalIndices.Add(b);
            finalIndices.Add(c);
        }

        Mesh dst = new Mesh
        {
            indexFormat = (newPositions.Count > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16
        };
        dst.SetVertices(newPositions);
        dst.SetIndices(finalIndices, MeshTopology.Triangles, 0, true);
        dst.RecalculateBounds();

        return dst;
    }
}