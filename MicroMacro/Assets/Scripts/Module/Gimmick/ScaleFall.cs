using System;
using Module.Management;
using Module.Scaling;
using UnityEngine;
using Module.Scaling;

namespace Module.Gimmick
{
    public class ScaleFall : MonoBehaviour
    {
        [SerializeField] private GameObject rope;
        
        [SerializeField] private Scaler scaler;
        [SerializeField] private HingeJoint joint;
        [SerializeField] private Rigidbody rigidbody;
        
        [Header("落下時の重量")]
        [SerializeField] private float newMassValue = 10f;
        
        public event Action OnFall;

        private void Start()
        {
            if (scaler != null)
            {
                scaler.OnScaleCompleted += ScaleCheck;
            }
        }

        private void ScaleCheck(ScaleEventArgs args)
        {
            if (args.CurrentStep == scaler.MaxStep)
            {
                FallObject();
                OnFall?.Invoke();
            }
        }

        private void FallObject()
        {
            Destroy(rope);
            Destroy(joint);
            // Jointが有効な状態でfreezeすると固まるので落下時に設定
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                                    RigidbodyConstraints.FreezePositionZ;
            
            SoundManager.instance.Play("糸切り");
            
            // TODO スケールの値によって重さを動的に変化させるクラスを作成後、置き換え
            rigidbody.mass = newMassValue;  // 落下時に重さ変更して重量感を
        }
    }
}