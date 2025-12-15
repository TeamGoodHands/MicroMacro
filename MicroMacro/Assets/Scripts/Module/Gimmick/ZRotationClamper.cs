using UnityEngine;

// Rigidbodyがないとエラーになるようにする属性
[RequireComponent(typeof(Rigidbody))]
public class ZRotationClamperPhysics : MonoBehaviour
{
    [Header("回転の制限設定")]
    [Tooltip("最小角度（例: -45）")]
    [SerializeField] private float minAngle = -45f;

    [Tooltip("最大角度（例: 45）")]
    [SerializeField] private float maxAngle = 45f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 現在のローカル回転を取得
        Vector3 currentRot = transform.localEulerAngles;

        // 角度の補正（0~360度 を -180~180度 に変換）
        float zAngle = (currentRot.z > 180) ? currentRot.z - 360 : currentRot.z;

        // 指定範囲に入っているか確認
        if (zAngle < minAngle || zAngle > maxAngle)
        {
            // 範囲外ならクランプする
            float clampedZ = Mathf.Clamp(zAngle, minAngle, maxAngle);

            // 新しい回転（Quaternion）を作成
            // 親がいる場合といない場合で計算が少し異なりますが、
            // 基本的にlocalRotationを基準に計算し、それを親の回転と合成してワールド回転にします。
            Quaternion localTargetRot = Quaternion.Euler(currentRot.x, currentRot.y, clampedZ);
            
            Quaternion worldTargetRot;
            if (transform.parent != null)
            {
                // 親の回転 × ローカルの目標回転 ＝ ワールドの目標回転
                worldTargetRot = transform.parent.rotation * localTargetRot;
            }
            else
            {
                worldTargetRot = localTargetRot;
            }

            // Rigidbodyを使って回転位置を修正
            rb.MoveRotation(worldTargetRot);

            // 【重要】制限に達しているのに回り続けようとする「勢い（角速度）」をZ軸だけ消す
            // これをしないと、制限角度でガタガタ震えます
            Vector3 vel = rb.angularVelocity;
            // ローカル座標系の角速度に変換してZを消し、またワールドに戻すなどの計算もできますが
            // 簡易的に「制限に当たったら回転の勢いを止める」処理にします
            rb.angularVelocity = Vector3.Lerp(vel, Vector3.zero, 0.5f); 
        }
    }
}