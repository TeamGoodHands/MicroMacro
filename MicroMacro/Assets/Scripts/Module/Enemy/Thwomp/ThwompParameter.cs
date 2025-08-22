using System;
using CoreModule.Helper;
using UnityEngine;

namespace Module.Enemy
{
    [Serializable]
    public class ThwompParameter
    {
        [SerializeField, Header("上に動く高さ")] private float moveHeight = 2f;
        [SerializeField, Header("左右に動くスピード")] private float moveSpeed = 1f;
        [SerializeField, Header("上にに動くスピード")] private float upSpeed = 1f;
        [SerializeField, Header("落下待機時間")] private MinMaxValue fallDelay ;
        [SerializeField, Header("プレイヤーの地点に動くまでの待機時間")] private float moveDelay = 1f;
        [SerializeField, Header("落下スピード")] private float fallSpeed = 1f;
        [SerializeField, Header("プレイヤーを感知する距離")] private float detectionRange = 3f;
        [SerializeField, Header("攻撃間隔時間")] private float attackIntervalTime = 1f;
        [SerializeField, Header("攻撃時の色")] private Color attackColor = Color.red;
        [SerializeField, Header("通常時の色")] private Color defaultColor = Color.aliceBlue;
        
        public float MoveHeight => moveHeight;
        public float MoveSpeed => moveSpeed;
        public float UpSpeed => upSpeed;
        public float FallDelay => fallDelay.GetRandom();
        public float MoveDelay => moveDelay;
        public float FallSpeed => fallSpeed;
        public float DetectionRange => detectionRange;
        public float AttackIntervalTime => attackIntervalTime;

        public Color AttackColor => attackColor;
        public Color DefaultColor => defaultColor;
    }
}