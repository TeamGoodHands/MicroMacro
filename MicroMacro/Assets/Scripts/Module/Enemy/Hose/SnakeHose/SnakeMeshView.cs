using Module.Enemy.Hose.SnakeHose;
using UnityEngine;

namespace Module.Enemy.Hose
{
    [RequireComponent(typeof(SnakeController))]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteAlways] // エディタ反映
    public class SnakeMeshView : MonoBehaviour
    {
        [Header("Mesh Settings")]
        public int lengthSegments = 50;
        public int radialSegments = 8;
        public float textureTiling = 1.0f;

        private SnakeController controller;
        private Mesh mesh;
        private Vector3[] vertices;
        private Vector2[] uvs;
        private int[] triangles;

        private void OnEnable()
        {
            controller = GetComponent<SnakeController>();
            InitializeMesh();
        }

        private void LateUpdate()
        {
            if (controller == null) controller = GetComponent<SnakeController>();
            if (controller.splineContainer == null) return;

            // エディタでの操作中やスクリプト更新でMeshが消えることがあるためチェック
            if (mesh == null)
            {
                InitializeMesh();
            }

            UpdateMesh();
        }

        private void InitializeMesh()
        {
            // すでにメッシュがある場合は使い回す（メモリリーク防止）
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "SnakeMesh";
                mf.mesh = mesh;
            }
            
            mesh.MarkDynamic();

            int vertexCount = (lengthSegments + 1) * (radialSegments + 1);
            
            // 配列確保（長さが変わった場合のみ再確保するのが理想だが、エディタ用なので簡易実装）
            if (vertices == null || vertices.Length != vertexCount)
            {
                vertices = new Vector3[vertexCount];
                uvs = new Vector2[vertexCount];
                triangles = new int[lengthSegments * radialSegments * 6];
            }

            int triIndex = 0;
            for (int i = 0; i < lengthSegments; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int current = i * (radialSegments + 1) + j;
                    int next = current + radialSegments + 1;

                    triangles[triIndex++] = current;
                    triangles[triIndex++] = current + 1;
                    triangles[triIndex++] = next;
                    triangles[triIndex++] = next;
                    triangles[triIndex++] = current + 1;
                    triangles[triIndex++] = next + 1;
                }
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }

        private void UpdateMesh()
        {
            if (mesh == null) return;

            float segmentLen = controller.snakeLength / lengthSegments;

            for (int i = 0; i <= lengthSegments; i++)
            {
                float offsetFromHead = controller.snakeLength - (i * segmentLen);
                
                controller.GetSampleAtOffset(offsetFromHead, out Vector3 centerPos, out Quaternion rotation, out float radiusScale);

                Vector3 right = rotation * Vector3.right;
                Vector3 up = rotation * Vector3.up;
                float currentRadius = controller.baseRadius * radiusScale;

                float vRate = (float)i / lengthSegments;

                for (int j = 0; j <= radialSegments; j++)
                {
                    float uRate = (float)j / radialSegments;
                    float angle = uRate * Mathf.PI * 2;
                    
                    float x = Mathf.Cos(angle) * currentRadius;
                    float y = Mathf.Sin(angle) * currentRadius;

                    Vector3 worldPos = centerPos + (right * x) + (up * y);
                    
                    int vertIndex = i * (radialSegments + 1) + j;
                    vertices[vertIndex] = transform.InverseTransformPoint(worldPos);
                    uvs[vertIndex] = new Vector2(uRate, vRate * textureTiling);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            
            // 計算コスト節約のため、BoundsとNormalsは必要に応じて更新頻度を下げることも検討
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}