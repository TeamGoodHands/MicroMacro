using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace Module.Enemy.Water
{
    public class WaterObstacle : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Vector2 initialSpeed;

        // 乗っているオブジェクト（プレイヤーなど）を管理するリスト
        private readonly HashSet<Rigidbody> passengers = new HashSet<Rigidbody>();

        // 前フレームの位置を記憶
        private Vector3 previousPosition;

        // 【追加】慣性を渡すために計算した「現在の速度ベクトル」
        private Vector3 calculatedVelocity;

        private void Start()
        {
            // 初期化時に位置を入れておかないと、最初のフレームで大きな移動と判定される可能性があるため
            previousPosition = rigidBody.position;
        }

        private void FixedUpdate()
        {
            rigidBody.linearVelocity = initialSpeed * Time.fixedDeltaTime;
            
            MovePassengers();
        }

        private void MovePassengers()
        {
            // 今回のフレームで床がどれだけ動いたか（移動ベクトル）
            Vector3 displacement = rigidBody.position - previousPosition;

            // 【追加】移動距離を時間に割って「速度」に変換し、保存しておく
            if (Time.fixedDeltaTime > 0)
            {
                calculatedVelocity = displacement / Time.fixedDeltaTime;
            }

            foreach (var passenger in passengers)
            {
                if (passenger == null)
                    continue;

                // プレイヤー独自の移動に加えて、床の移動分を加算する
                passenger.MovePosition(passenger.position + displacement);
            }

            // 位置更新
            previousPosition = rigidBody.position;
        }

        private void OnCollisionStay(Collision collision)
        {
            TryAddPassenger(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            // 離れたらリストから外す
            Rigidbody r = collision.gameObject.GetComponent<Rigidbody>();
            if (r != null && passengers.Contains(r))
            {
                passengers.Remove(r);

                // 【追加】離れる瞬間に、床の速度をプレイヤーに加算（慣性）
                r.linearVelocity += calculatedVelocity;
            }
        }

        private void TryAddPassenger(Collision collision)
        {
            ContactPoint[] contacts = ArrayPool<ContactPoint>.Shared.Rent(collision.contactCount);
            int contactCount = collision.GetContacts(contacts);

            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint contact = contacts[i];

                // 法線が上向き (Y > 0.5) なら「上に乗っている」とみなす
                // (Unityの衝突法線は「相手→自分」の向きなので、上がマイナスになる判定で正しい)
                if (contact.normal.y < -0.5f)
                {
                    Rigidbody r = collision.gameObject.GetComponent<Rigidbody>();
                    if (r != null)
                    {
                        passengers.Add(r);
                    }
                }
            }

            // 借りたバッファを返す
            ArrayPool<ContactPoint>.Shared.Return(contacts);
        }
    }
}