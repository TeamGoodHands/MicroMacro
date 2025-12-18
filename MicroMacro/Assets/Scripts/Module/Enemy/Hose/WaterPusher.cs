using System;
using Constants;
using Module.Enemy.Hose;
using UnityEngine;

public class WaterPusher : MonoBehaviour
{
    [SerializeField] private HoseBehaviour hoseBehaviour;
    [SerializeField] private float centeringStrength = 5.0f; // 座標を寄せる強さ

    [SerializeField, Header("水を抜けたときに水流方向に吹き飛ばす強さ")]
    private float exitPower = 1.5f;

    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(Tag.Handle.Player))
        {
            ApplyWaterForce();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HoseWater hoseWater = hoseBehaviour.HoseWater;
        Vector3 verticalForce = hoseWater.CalculateVerticalForce(true);
        rigidBody.AddForce(verticalForce * exitPower);
    }

    private void ApplyWaterForce()
    {
        // --- 追加: 横方向の速度を殺す ---
        // 現在の速度を、このオブジェクトのY軸（進行方向）に投影する
        // これにより「進行方向成分」だけが残り、横ブレの速度が 0 になります
        Vector3 velocityAlongAxis = Vector3.Project(rigidBody.linearVelocity, transform.up);

        // 速度を上書き（横方向の慣性を消滅させる）
        rigidBody.linearVelocity = velocityAlongAxis;
        // -----------------------------

        HoseWater hoseWater = hoseBehaviour.HoseWater;
        Vector3 verticalForce = hoseWater.CalculateVerticalForce(true);
        Vector3 horizontalForce = hoseWater.CalculateHorizontalForce(true, rigidBody.position);

        // --- 座標を強制的に軸へ寄せる（前回の処理） ---
        Vector3 vectorToPlayer = rigidBody.position - transform.position;
        Vector3 projectionOnAxis = Vector3.Project(vectorToPlayer, transform.up);
        Vector3 targetAxisPoint = transform.position + projectionOnAxis;

        Vector3 newPosition = Vector3.Lerp(rigidBody.position, targetAxisPoint, centeringStrength * Time.deltaTime);
        rigidBody.position = newPosition;
        // ----------------------------------------

        // 最後に力を加える（これにより進行方向へは加速する）
        Vector3 force = verticalForce + horizontalForce;
        rigidBody.AddForce(force);
    }
}