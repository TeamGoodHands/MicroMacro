using System.Collections.Generic;
using Module.Enemy.Hose.SnakeHose;
using UnityEngine;

namespace Module.Enemy.Hose
{
    [RequireComponent(typeof(SnakeController))]
    [ExecuteAlways] // エディタ反映
    public class SnakeAttachmentView : MonoBehaviour
    {
        public enum Axis { X, Y, Z, NegativeX, NegativeY, NegativeZ }

        [System.Serializable]
        public class EndPointAttachment
        {
            public Transform target;
            [Tooltip("頭からの距離 (0=頭, Length=尻尾)")]
            public float offsetFromHead;
            [Tooltip("モデルの正面軸")]
            public Axis forwardAxis = Axis.Z;
            [Tooltip("追加の回転オフセット (Euler Angles)")]
            public Vector3 rotationOffset; // ★変更: Vector3で全軸対応
        }

        [System.Serializable]
        public class BodyPartAttachment
        {
            public Transform target;
            [Tooltip("頭からの距離")]
            public float offsetFromHead;
            [Tooltip("追加の回転オフセット (Euler Angles)")]
            public Vector3 rotationOffset; // ★変更: Vector3で全軸対応
        }

        [Header("Special Attachments")]
        public EndPointAttachment head;
        public EndPointAttachment tail;

        [Header("Body Attachments (Fixed X-Axis)")]
        [Tooltip("ここに追加するパーツは、自動的に「X軸」を進行方向に向けます")]
        public List<BodyPartAttachment> bodyParts = new List<BodyPartAttachment>();

        private SnakeController controller;
        private Quaternion xAxisCorrection;

        private void OnEnable()
        {
            controller = GetComponent<SnakeController>();
            InitializeOffsets();
        }

        private void Start()
        {
            InitializeOffsets();
        }

        private void InitializeOffsets()
        {
            // X軸(Vector3.right)をZ軸(Vector3.forward)に向けるための回転
            xAxisCorrection = Quaternion.FromToRotation(Vector3.forward, Vector3.right);

            if (controller == null)
            {
                controller = GetComponent<SnakeController>();
            }

            // Head/TailのOffset自動設定
            if (head != null && head.target != null)
            {
                head.offsetFromHead = controller.snakeLength;
            }
            if (tail != null && tail.target != null && controller != null)
            {
                tail.offsetFromHead = 0f;
            }
        }

        private void LateUpdate()
        {
            if (controller == null)
            {
                controller = GetComponent<SnakeController>();
            }
            if (controller.splineContainer == null)
            {
                return;
            }

            UpdateEndPoint(head);
            UpdateEndPoint(tail);

            if (bodyParts != null)
            {
                foreach (BodyPartAttachment part in bodyParts)
                {
                    UpdateBodyPart(part);
                }
            }
        }

        private void UpdateEndPoint(EndPointAttachment att)
        {
            if (att == null || att.target == null)
            {
                return;
            }

            Vector3 pos;
            Quaternion rot;
            float r;

            controller.GetSampleAtOffset(att.offsetFromHead, out pos, out rot, out r);

            // モデルの向き補正
            Quaternion correction = Quaternion.FromToRotation(Vector3.forward, GetAxisVector(att.forwardAxis));
            
            // ★変更: 3軸のオイラー角オフセットを適用
            // スプラインの回転(rot)に対し、ローカルでオフセット回転を加えます
            Quaternion offsetRot = Quaternion.Euler(att.rotationOffset);

            att.target.position = pos;
            // 適用順序: スプライン回転 -> オフセット回転 -> モデル軸補正の逆
            att.target.rotation = rot * offsetRot * Quaternion.Inverse(correction);
        }

        private void UpdateBodyPart(BodyPartAttachment part)
        {
            if (part == null || part.target == null)
            {
                return;
            }

            Vector3 pos;
            Quaternion rot;
            float r;

            controller.GetSampleAtOffset(part.offsetFromHead, out pos, out rot, out r);

            // ★変更: 3軸のオイラー角オフセットを適用
            Quaternion offsetRot = Quaternion.Euler(part.rotationOffset);

            part.target.position = pos;
            part.target.rotation = rot * offsetRot * Quaternion.Inverse(xAxisCorrection);
        }

        private Vector3 GetAxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.X: return Vector3.right;
                case Axis.Y: return Vector3.up;
                case Axis.Z: return Vector3.forward;
                case Axis.NegativeX: return Vector3.left;
                case Axis.NegativeY: return Vector3.down;
                case Axis.NegativeZ: return Vector3.back;
                default: return Vector3.forward;
            }
        }
    }
}