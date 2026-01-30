using UnityEngine;

public class TransformLocker : MonoBehaviour
{
    [Header("固定設定")]
    public bool lockPosition = true;
    public bool lockRotation = true;

    [Header("固定する値")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private Vector3 targetRotationEuler = Vector3.zero;

    private Quaternion targetRotation;

    void Start()
    {
        // 事前にQuaternionに変換して計算負荷を軽減
        targetRotation = Quaternion.Euler(targetRotationEuler);
    }

    void LateUpdate()
    {
        // ロックされていない場合は、現在の値を維持する
        Vector3 finalPos = lockPosition ? targetPosition : transform.position;
        Quaternion finalRot = lockRotation ? targetRotation : transform.rotation;

        // 座標と回転を1回で適用
        transform.SetPositionAndRotation(finalPos, finalRot);
    }
}