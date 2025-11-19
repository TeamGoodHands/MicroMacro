using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
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

        private Transform playerTransform;
        private Vector3 waterDefaultScale;
        private Vector3 waterScale;
        private Vector3 scalerDefaultScale;
        private RaycastHit hitInfo;
        private float scaleMultiplier = 1f;
        private bool isObjectHit;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;

            waterDefaultScale = waterPivot.localScale;
            waterScale = waterDefaultScale;
            scalerDefaultScale = scaler.transform.localScale;

            if (parameter.IsLooping)
            {
                DoLoopWater().Forget();
            }
        }

        float maxMultiplier = 1f;

        private void FixedUpdate()
        {
            float lengthScale = scaler.transform.localScale.x - scalerDefaultScale.x;
            waterScale = waterDefaultScale + Vector3.right * (lengthScale * parameter.LengthMultiplier);

            if (!parameter.IsLooping)
            {
                if (isObjectHit)
                {
                    float distance = Vector3.Distance(waterPivot.position, hitInfo.point);
                    maxMultiplier = distance / (waterScale.x * 3f);
                }
                else
                {
                    maxMultiplier = 1f;
                }

                scaleMultiplier += parameter.WaterSpeed * Time.fixedDeltaTime;
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0, maxMultiplier);
            }


            Vector3 scale = waterScale;
            scale.x *= scaleMultiplier;
            waterPivot.localScale = scale;

            if (scaler.CurrentStep == scaler.MinStep)
            {
                waterPivot.localScale = Vector3.zero;
            }
            else
            {
                WaterCast();
            }
        }

        private void WaterCast()
        {
            Vector3 position = rotatePivot.position;
            float radius = waterScale.y;
            float maxDistance = scaler.transform.localScale.x * waterPivot.localScale.x - radius;

            int layerMask = ~(Layer.Mask.PlayerOnly | Layer.Mask.Enemy | Layer.Mask.Bullet);

            isObjectHit = Physics.SphereCast(position, radius, rotatePivot.up, out hitInfo, maxDistance, layerMask);

            UGizmos.DrawSphereCast(position, radius, rotatePivot.up, maxDistance, isObjectHit, hitInfo);
        }

        private async UniTaskVoid DoLoopWater()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(parameter.FirstDelay), cancellationToken: destroyCancellationToken);
            
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                float timer = 0f;

                await UniTask.WaitUntil(() =>
                {
                    if (!isObjectHit)
                    {
                        scaleMultiplier += parameter.WaterSpeed * Time.fixedDeltaTime;
                    }

                    scaleMultiplier = Mathf.Clamp01(scaleMultiplier);

                    timer += Time.fixedDeltaTime;

                    return timer >= parameter.OnTime;
                }, PlayerLoopTiming.FixedUpdate);

                timer = 0f;

                await UniTask.WaitUntil(() =>
                {
                    scaleMultiplier -= parameter.WaterSpeed * Time.fixedDeltaTime;

                    scaleMultiplier = Mathf.Clamp01(scaleMultiplier);

                    timer += Time.fixedDeltaTime;

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