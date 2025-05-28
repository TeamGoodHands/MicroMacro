using UnityEngine;

namespace CoreModule.Input
{
    /// <summary>
    ///     MonoBehaviourをシングルトン化するクラス
    /// </summary>
    /// <typeparam name="T">派生型</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                    if (instance == null)
                    {
                        Debug.LogError(typeof(T) + " をアタッチしているGameObjectはありません");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (this != Instance)
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}