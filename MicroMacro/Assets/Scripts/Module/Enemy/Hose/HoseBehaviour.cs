using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using NaughtyAttributes;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class HoseBehaviour : MonoBehaviour
    {
        [SerializeField] private HoseParameter parameter;
        [SerializeField] private Scaler scaler;
        [SerializeField] private Transform waterPivot;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private Transform waterHead;

        public HoseWater HoseWater { get; private set; }

        private Transform playerTransform;
        private Vector3 waterDefaultScale;
        private Vector3 waterScale;
        private Vector3 scalerDefaultScale;
        private Vector3 hitPoint;
        private float scaleMultiplier = 1f;
        private bool isObjectHit;

        private void Start()
        {
            // プレイヤーのTransformを取得する
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;

            HoseWater = new HoseWater(scaler, waterPivot, rotatePivot, parameter);

            // 初期情報の取得
            waterDefaultScale = waterPivot.localScale;
            waterScale = waterDefaultScale;
            scalerDefaultScale = scaler.transform.localScale;

            // 水のオンオフを切り替える場合は実行
            if (parameter.IsLooping)
            {
                DoLoopWater().Forget();
            }
        }

        [SerializeField, ReadOnly] private float maxMultiplier = 1f;

        private void FixedUpdate()
        {
            // スケールの差を基に水の長さを計算する
            float lengthScale = scaler.transform.localScale.x - scalerDefaultScale.x;
            waterScale = CalculateWaterScale(lengthScale);

            if (isObjectHit)
            {
                // ヒットしていたらヒットした場所まで水を伸ばす
                float distance = Vector3.Distance(waterPivot.position, hitPoint);
                maxMultiplier = Mathf.Min(distance / (waterScale.x * scaler.transform.localScale.y), 1f);
            }
            else
            {
                // ヒットしていない場合は最大まで伸ばす
                maxMultiplier = 1f;
            }

            if (!parameter.IsLooping)
            {
                scaleMultiplier += parameter.WaterSpeed * Time.fixedDeltaTime;
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0, maxMultiplier);
            }

            Vector3 scale = waterScale;
            scale.x *= scaleMultiplier;
            waterPivot.localScale = scale;

            if (scaler.CurrentStep == scaler.MinStep)
            {
                // スケーラーが最小の場合は水を出さない
                waterPivot.localScale = Vector3.zero;
            }
            else
            {
                // それ以外の場合は水流の力を加える
                isObjectHit = HoseWater.TryAddWaterForce(scale.y, out hitPoint);
            }
        }

        private Vector3 CalculateWaterScale(float lengthScale)
        {
            return waterDefaultScale + Vector3.right * (lengthScale * parameter.LengthMultiplier);
        }

        private async UniTaskVoid DoLoopWater()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(parameter.FirstDelay), cancellationToken: destroyCancellationToken);

            while (!destroyCancellationToken.IsCancellationRequested)
            {
                float timer = 0f;

                void AddWaterSpeed(int direction)
                {
                    scaleMultiplier += parameter.WaterSpeed * Time.fixedDeltaTime * direction;
                    scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0, maxMultiplier);

                    timer += Time.fixedDeltaTime;
                }

                // 水の柱をだんだん長くする
                await UniTask.WaitUntil(() =>
                {
                    AddWaterSpeed(1);

                    return timer >= parameter.OnTime;
                }, PlayerLoopTiming.FixedUpdate);

                timer = 0f;

                // 水の柱をだんだん短くする
                await UniTask.WaitUntil(() =>
                {
                    AddWaterSpeed(-1);

                    return timer >= parameter.OffTime;
                }, PlayerLoopTiming.FixedUpdate);

                if (parameter.LookAtPlayer)
                {
                    await LookAt(playerTransform.position, 0.5f);
                }
            }
        }

        private Tween LookAt(Vector3 targetPos, float time)
        {
            Vector2 dir = targetPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Z軸だけ回転させる
            return rotatePivot.DORotate(new Vector3(0, 0, angle - 90f), time, RotateMode.Fast);
        }
    }
}