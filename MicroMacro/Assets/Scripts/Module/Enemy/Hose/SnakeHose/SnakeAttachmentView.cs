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
        }

        [System.Serializable]
        public class BodyPartAttachment
        {
            public Transform target;
            [Tooltip("頭からの距離")]
            public float offsetFromHead;
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

            if (controller == null) controller = GetComponent<SnakeController>();

            // Head/TailのOffset自動設定
            if (head != null && head.target != null)
            {
                head.offsetFromHead = 0f;
            }
            if (tail != null && tail.target != null && controller != null)
            {
                tail.offsetFromHead = controller.snakeLength;
            }
        }

        private void LateUpdate()
        {
            if (controller == null) controller = GetComponent<SnakeController>();
            if (controller.splineContainer == null) return;

            UpdateEndPoint(head);
            UpdateEndPoint(tail);

            if (bodyParts != null)
            {
                foreach (var part in bodyParts)
                {
                    UpdateBodyPart(part);
                }
            }
        }

        private void UpdateEndPoint(EndPointAttachment att)
        {
            if (att == null || att.target == null) return;

            controller.GetSampleAtOffset(att.offsetFromHead, out Vector3 pos, out Quaternion rot, out float r);

            Quaternion correction = Quaternion.FromToRotation(Vector3.forward, GetAxisVector(att.forwardAxis));
            
            att.target.position = pos;
            att.target.rotation = rot * Quaternion.Inverse(correction);
        }

        private void UpdateBodyPart(BodyPartAttachment part)
        {
            if (part == null || part.target == null) return;

            controller.GetSampleAtOffset(part.offsetFromHead, out Vector3 pos, out Quaternion rot, out float r);

            part.target.position = pos;
            part.target.rotation = rot * Quaternion.Inverse(xAxisCorrection);
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