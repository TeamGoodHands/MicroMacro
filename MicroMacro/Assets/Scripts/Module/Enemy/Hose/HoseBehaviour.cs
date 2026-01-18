using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using NaughtyAttributes;
using PropertyGenerator.Generated;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class HoseBehaviour : MonoBehaviour, IWaterFlow
    {
        [SerializeField] private WaterFlowParameter parameter;
        [SerializeField] private Scaler scaler;
        [SerializeField] private Transform waterPivot;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private Transform waterHead;
        [SerializeField] private HoseControllerWrapper hoseControllerWrapper;
        [SerializeField] private bool isRapids;
        [SerializeField] private Renderer screwRenderer;
        [SerializeField] private Renderer waterRenderer;

        public WaterFlow WaterFlow { get; private set; }
        public event Action<WaterState> OnWaterStateChanged;

        private Transform playerTransform;
        private Vector3 waterDefaultScale;
        private Vector3 scalerDefaultScale;

        public float CurrentIntensity { get; private set; } = 1f;
        private WaterState waterState;

        private static readonly int WaterThresholdId = Shader.PropertyToID("_WaterThreshold");
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");

        private void Awake()
        {
            // プレイヤーのTransformを取得する
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;

            WaterFlow = new WaterFlow(scaler, waterPivot, rotatePivot, parameter);

            scaler.OnScaleStarted += OnScaleStarted;

            // 初期情報の取得
            waterDefaultScale = waterPivot.localScale;
            scalerDefaultScale = scaler.transform.localScale;

            // 水のオンオフを切り替える場合は実行
            if (parameter.IsLooping)
            {
                DoLoopWater().Forget();
            }

            SetRapidsMode(isRapids);
        }

        private void SetRapidsMode(bool isRapids)
        {
            this.isRapids = isRapids;
            if (isRapids)
            {
                screwRenderer.material.DOFloat(parameter.ScrewWidth, WaterThresholdId, 1f);
                waterRenderer.material.SetColor(MainColor, parameter.RapidsWaterColor);
            }
            else
            {
                screwRenderer.material.DOFloat(1f, WaterThresholdId, 1f);
                waterRenderer.material.SetColor(MainColor, parameter.DefaultWaterColor);
            }
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            int direction = (args.CurrentStep - args.PreviousStep) > 0 ? 1 : -1;
            if (direction > 0)
            {
                hoseControllerWrapper.SetRotateRTrigger();
            }
            else
            {
                hoseControllerWrapper.SetRotateLTrigger();
            }
        }

        [SerializeField, ReadOnly] private float maxMultiplier = 1f;

        private void FixedUpdate()
        {
            // 1. 本来伸びるべき長さ（アニメーション・スケーラー考慮）を計算
            float lengthScale = scaler.transform.localScale.x - scalerDefaultScale.x;

            // アニメーションの進行（scaleMultiplier）
            if (!parameter.IsLooping)
            {
                // BoxCastの結果を待たずに、単純に伸びようとする（壁に当たれば視覚的に縮むだけ）
                CurrentIntensity += parameter.WaterSpeed * Time.fixedDeltaTime;
                CurrentIntensity = Mathf.Clamp(CurrentIntensity, 0, 1f); // maxMultiplierによる制限はBoxCast後に行うためここでは1f上限
            }

            // ターゲットとなるローカルスケール（壁がない場合の最大サイズ）
            Vector3 targetLocalScale = CalculateWaterScale(lengthScale);
            targetLocalScale.x *= CurrentIntensity;

            if (scaler.CurrentStep == scaler.MinStep)
            {
                waterPivot.localScale = Vector3.zero;
                return;
            }

            // 2. ローカルスケールを「ワールド空間での距離」に変換してBoxCast用の距離を算出
            // waterPivotの親のスケールが影響するため、lossyScale比率を利用して変換係数を求める
            // 簡易的に親のXスケールを使用（回転などが複雑でない前提）
            float parentScaleX = waterPivot.parent != null ? waterPivot.parent.lossyScale.x : 1f;

            // BoxCastすべき距離
            float castMaxDistance = targetLocalScale.x * parentScaleX;

            // 3. BoxCastを実行し、実際に水が到達した距離を取得
            // HoseWater側の引数変更に対応
            bool isHit = WaterFlow.TryAddWaterForce(targetLocalScale.y, castMaxDistance, out float actualDistance);

            // 4. 実際の距離をローカルスケールに戻して適用
            // 「実際の距離」を「親のスケール」で割れば、設定すべきローカルスケールになる
            Vector3 finalScale = targetLocalScale;
            finalScale.x = actualDistance / parentScaleX;

            // スケール適用
            waterPivot.localScale = finalScale;
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
                    CurrentIntensity += parameter.WaterSpeed * Time.fixedDeltaTime * direction;
                    CurrentIntensity = Mathf.Clamp(CurrentIntensity, 0, maxMultiplier);

                    timer += Time.fixedDeltaTime;
                }

                SetWaterState(WaterState.Pushing);
                // 水の柱をだんだん長くする
                await UniTask.WaitUntil(() =>
                {
                    AddWaterSpeed(1);

                    if (CurrentIntensity == 1f)
                    {
                        SetWaterState(WaterState.Pushed);
                    }

                    return timer >= parameter.OnTime;
                }, PlayerLoopTiming.FixedUpdate, cancellationToken: destroyCancellationToken);

                SetWaterState(WaterState.Pushed);

                timer = 0f;

                SetWaterState(WaterState.Ending);
                // 水の柱をだんだん短くする
                await UniTask.WaitUntil(() =>
                {
                    AddWaterSpeed(-1);

                    if (CurrentIntensity == 0f)
                    {
                        SetWaterState(WaterState.End);
                    }

                    return timer >= parameter.OffTime;
                }, PlayerLoopTiming.FixedUpdate, cancellationToken: destroyCancellationToken);

                SetWaterState(WaterState.End);

                if (parameter.LookAtPlayer)
                {
                    await LookAt(playerTransform.position, 0.5f);
                }
            }
        }

        private void SetWaterState(WaterState state)
        {
            if (waterState != state)
            {
                waterState = state;
                OnWaterStateChanged?.Invoke(state);
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