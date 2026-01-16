using System.Collections.Generic;
using UnityEngine;
using Module.Enemy.Hose.SnakeHose; // 修正: namespace修正

namespace Module.Enemy.Hose
{
    [RequireComponent(typeof(SnakeController))]
    [ExecuteAlways] // エディタ反映
    public class SnakeColliderView : MonoBehaviour
    {
        [Header("Collider Settings")]
        public int colliderSegments = 20;
        public float radiusMultiplier = 1.0f;

        private SnakeController controller;
        private List<SphereCollider> colliders = new List<SphereCollider>();

        private void OnEnable()
        {
            controller = GetComponent<SnakeController>();
            // エディタではListがクリアされることがあるため、既存コンポーネントを取得し直す
            RebuildCollidersList();
        }

        private void LateUpdate()
        {
            if (controller == null) controller = GetComponent<SnakeController>();
            if (controller.splineContainer == null) return;

            // 設定数と現在のCollider数が合わない場合は再生成（エディタ操作用）
            if (colliders.Count != colliderSegments + 1)
            {
                RebuildCollidersList();
            }

            UpdateColliders();
        }

        // 既存のColliderを探してリストに入れ、足りなければ足し、多ければ消す
        private void RebuildCollidersList()
        {
            colliders.Clear();
            var existingColliders = GetComponents<SphereCollider>();
            colliders.AddRange(existingColliders);

            int targetCount = colliderSegments + 1;

            // 足りない分を追加
            while (colliders.Count < targetCount)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.hideFlags = HideFlags.None; // Inspectorで見えるようにするならNone, 隠すならHideInInspector
                colliders.Add(col);
            }

            // 多い分を削除（後ろから）
            while (colliders.Count > targetCount)
            {
                var col = colliders[colliders.Count - 1];
                colliders.RemoveAt(colliders.Count - 1);

                if (UnityEngine.Application.isPlaying)
                {
                    Destroy(col);
                }
                else
                {
                    DestroyImmediate(col); // エディタ中はImmediate
                }
            }
        }

        private void UpdateColliders()
        {
            if (colliders.Count == 0) return;

            float segmentLen = controller.snakeLength / colliderSegments;

            for (int i = 0; i <= colliderSegments; i++)
            {
                if (i >= colliders.Count) break;
                if (colliders[i] == null) continue;

                float offsetFromHead = controller.snakeLength - (i * segmentLen);

                controller.GetSampleAtOffset(offsetFromHead, out Vector3 pos, out Quaternion rot, out float radiusScale);

                colliders[i].center = transform.InverseTransformPoint(pos);
                colliders[i].radius = controller.baseRadius * radiusScale * radiusMultiplier;
            }
        }
    }
}