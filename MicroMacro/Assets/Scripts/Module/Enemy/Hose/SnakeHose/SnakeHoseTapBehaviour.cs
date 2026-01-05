using System;
using System.Threading;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using NaughtyAttributes;
using PropertyGenerator.Generated;
using UGizmo;
using UnityEditor;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class SnakeHoseTapBehaviour : MonoBehaviour, IWaterFlow
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

        private Transform playerTransform;
        private Vector3 waterDefaultScale;
        private Vector3 scalerDefaultScale;
        private Quaternion defaultRotation;
        private float scaleMultiplier = 1f;

        private static readonly int WaterThresholdId = Shader.PropertyToID("_WaterThreshold");
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");

        private void Start()
        {
            // プレイヤーのTransformを取得する
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;

            WaterFlow = new WaterFlow(scaler, waterPivot, rotatePivot, parameter);

            scaler.OnScaleStarted += OnScaleStarted;

            // 初期情報の取得
            waterDefaultScale = waterPivot.localScale;
            scalerDefaultScale = scaler.transform.localScale;
            defaultRotation = rotatePivot.localRotation;
            scaleMultiplier = 0f;

            SetRapidsMode(isRapids);
        }

        public void SetRapidsMode(bool isRapids)
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

            if (args.CurrentStep > 0)
            {
                SetRapidsMode(true);
            }
            else
            {
                SetRapidsMode(false);
            }
        }

        [SerializeField, ReadOnly] private float maxMultiplier = 1f;

        private void FixedUpdate()
        {
            // 1. 本来伸びるべき長さ（アニメーション・スケーラー考慮）を計算
            float lengthScale = scaler.transform.localScale.x - scalerDefaultScale.x;

            // ターゲットとなるローカルスケール（壁がない場合の最大サイズ）
            Vector3 targetLocalScale = CalculateWaterScale(lengthScale);
            targetLocalScale.x *= scaleMultiplier;

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

        public async UniTask OnWater()
        {
            float timer = 0f;

            // 水の柱をだんだん長くする
            await UniTask.WaitUntil(() =>
            {
                scaleMultiplier += parameter.WaterSpeed * Time.fixedDeltaTime;
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0, maxMultiplier);

                timer += Time.fixedDeltaTime;

                return timer >= parameter.OnTime;
            }, PlayerLoopTiming.FixedUpdate, cancellationToken: destroyCancellationToken);
        }

        public async UniTask OffWater()
        {
            float timer = 0f;

            // 水の柱をだんだん長くする
            await UniTask.WaitUntil(() =>
            {
                scaleMultiplier -= parameter.WaterSpeed * Time.fixedDeltaTime;
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0, maxMultiplier);

                timer += Time.fixedDeltaTime;

                return timer >= parameter.OffTime;
            }, PlayerLoopTiming.FixedUpdate, cancellationToken: destroyCancellationToken);
        }

        public Tween ShakeBody(float duration)
        {
            return rotatePivot.DOShakePosition(duration, 0.002f, 30, 90, false, true);
        }

        /// <summary>
        /// 指定時間、プレイヤーの方をゆっくり向き続けます
        /// </summary>
        /// <param name="duration">追従する合計時間（秒）</param>
        /// <param name="smoothSpeed">振り向く速さ（値が大きいほど速い。目安：5.0f〜10.0f）</param>
        public async UniTask LookAtPlayerSmoothAsync(float duration, float smoothSpeed)
        {
            float timeElapsed = 0f;

            // MonoBehaviourに標準搭載された destroyCancellationToken を取得
            // これにより、このコンポーネントやGameObjectが破棄されると自動でキャンセルされます
            var token = this.destroyCancellationToken;

            while (timeElapsed < duration)
            {
                // 1. ターゲット（ゴール）の角度を計算
                Vector2 dir = (Vector2)playerTransform.position - (Vector2)rotatePivot.position;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

                // 2. 現在の見た目の角度を計算
                Vector3 currentUp = rotatePivot.up;
                float currentAngle = Mathf.Atan2(currentUp.y, currentUp.x) * Mathf.Rad2Deg - 90f;

                // 3. 【新機能】補間処理
                // 現在の角度からターゲット角度へ、時間をかけて少しずつ近づけます
                // Mathf.LerpAngle は 360度の境目も適切に処理してくれる賢いメソッドです
                float nextAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);

                // 4. 「今回動くべき量（差分）」を算出
                // ゴールとの差ではなく、「次のフレームの理想角度」との差を使います
                float deltaAngle = Mathf.DeltaAngle(currentAngle, nextAngle);

                // 5. 回転を適用
                Quaternion rotationDiff = Quaternion.AngleAxis(deltaAngle, Vector3.forward);
                rotatePivot.rotation = rotationDiff * rotatePivot.rotation;

                // 時間経過を加算
                timeElapsed += Time.deltaTime;

                // 次のフレームまで待機（ここでトークンを渡して安全性を担保）
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }


        public Tween ResetAngle(float time)
        {
            return rotatePivot.DOLocalRotateQuaternion(defaultRotation, time).OnComplete(() => scaler.SetScale(0, true));
        }

        private void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(waterPivot.position, 0.5f);
                Gizmos.DrawSphere(playerTransform.position, 0.5f);
            }
        }
    }
}