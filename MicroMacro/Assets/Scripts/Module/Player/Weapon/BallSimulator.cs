using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Module.Player.Weapon
{
    public class BallSimulator : MonoBehaviour
    {
        [SerializeField] private GameObject ballSimPrefab; // 何でもOK。予測位置を表示するオブジェクト

        [Header("表示する点の数")]
        [SerializeField] private int simulateCount = 13;
        [SerializeField] private GameObject startPosition; // 発射開始位置
        
        private const float　GRAVITY_SCALE = 3f; // Bullet.csの加わる重力

        private List<GameObject> simulatePointList;
        private List<Renderer> rendererList;

        void Start()
        {
            Init();
        }

        void Update()
        {
            // デバッグ用に線を出してみる。必要無いなら無くても問題なし。
            if (simulatePointList != null && simulatePointList.Count > 0)
            {
                for (int i = 0; i < simulateCount; i++)
                {
                    if (i == 0)
                    {
                        Debug.DrawLine(startPosition.transform.position, simulatePointList[i].transform.position);
                    }
                    else if (i < simulateCount)
                    {
                        Debug.DrawLine(simulatePointList[i - 1].transform.position,
                            simulatePointList[i].transform.position);
                    }

                    if (rendererList[i].enabled == false)
                    {
                        RendererSwitch(false, i);
                    }
                }
            }
        }

        private void Init()
        {
            IsSimulate = false;
            if (simulatePointList != null && simulatePointList.Count > 0)
            {
                foreach (var obj in simulatePointList)
                {
                    Destroy(obj.gameObject);
                }
            }

            // 位置を表示するオブジェクトを予め作っておく
            if (ballSimPrefab != null)
            {
                simulatePointList = new List<GameObject>();
                rendererList = new List<Renderer>();
                for (int i = 0; i < simulateCount; i++)
                {
                    var obj = Instantiate(ballSimPrefab);
                    obj.transform.SetParent(transform);
                    obj.transform.position = Vector3.zero;
                    simulatePointList.Add(obj);
                    rendererList.Add(obj.GetComponent<Renderer>());
                }
            }
        }

        /*
          弾道を予測計算する。オブジェクトを再生成せず、位置だけ動かす。
          targetにはRigidbodyが必須
         */
        public void Simulate(Vector2 velocity)
        {
            if (!IsSimulate)
                return;

            if (simulatePointList != null && simulatePointList.Count > 0)
            {
                Vector2 force = velocity;

                //弾道予測の位置に点を移動
                for (int i = 0; i < simulateCount; i++)
                {
                    var time = (i * 0.05f); // 〇秒ごとの位置を予測。
                    var x = time * force.x;
                    var y = (force.y * time) - 0.5f * (-Physics.gravity.y * GRAVITY_SCALE) * Mathf.Pow(time, 2.0f);

                    simulatePointList[i].transform.position = startPosition.transform.position + new Vector3(x, y, 0f);
                }
            }
        }

        private void RendererSwitch(bool value)
        {
            if (simulatePointList != null && simulatePointList.Count > 0)
            {
                foreach (var obj in rendererList)
                {
                    obj.enabled = value;
                }
            }
        }

        private void RendererSwitch(bool value, int num)
        {
            if (simulatePointList != null && simulatePointList.Count > 0)
            {
                for (int i = num; i < simulateCount; i++)
                {
                    rendererList[i].enabled = value;
                }
            }
        }

        private bool isSimulate;
        public bool IsSimulate
        {
            set
            {
                isSimulate = value;
                if (value == false)
                {
                    RendererSwitch(false);
                }
                else
                {
                    RendererSwitch(true);
                }
            }
            get { return isSimulate; }
        }
    }
}