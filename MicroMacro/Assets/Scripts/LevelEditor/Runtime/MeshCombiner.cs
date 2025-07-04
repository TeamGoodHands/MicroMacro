﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LevelEditor.Runtime
{
    public class MeshCombiner
    {
        public GameObject CombineMeshes(List<MeshFilter> meshFilters)
        {
            if (meshFilters == null || meshFilters.Count == 0)
            {
                Debug.LogWarning("MeshCombiner: メッシュフィルターのリストが空です");
                return null;
            }

            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Count];

            for (var i = 0; i < meshFilters.Count; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    Debug.LogWarning($"MeshCombiner: インデックス{i}のメッシュフィルターが無効です");
                    continue;
                }

                combineInstances[i] = new CombineInstance()
                {
                    mesh = meshFilter.sharedMesh,
                    transform = meshFilter.transform.localToWorldMatrix
                };

                // 非表示にする
                meshFilter.gameObject.SetActive(false);
            }

            Mesh combinedMesh = new Mesh();

            combinedMesh.CombineMeshes(combineInstances, true, true);
            combinedMesh.Optimize();
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateBounds();

            GameObject combinedObject = new GameObject("CombinedObject");

            MeshFilter filter = combinedObject.AddComponent<MeshFilter>();
            filter.mesh = combinedMesh;

            MeshRenderer renderer = combinedObject.AddComponent<MeshRenderer>();
            renderer.enabled = false;

            MeshCollider collider = combinedObject.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
            collider.convex = true;

            return combinedObject;
        }
    }
}