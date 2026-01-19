using System;
using System.Collections.Generic;
using CoreModule.ObjectPool;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallSequencerSwitcher : MonoBehaviour
    {
        [SerializeField] private FallObjectsSequencer sequencer1;
        [SerializeField] private FallObjectsSequencer sequencer2;
        [SerializeField] private HealthStatus bossStatus;

        [SerializeField] private List<FallPattern> fallPatterns;
        [SerializeField] private GameObject projectileObjectPrefab;

        public FallObjectsSequencer Current;
        private ObjectPool<ProjectileObjectCache> fallObjectPool;
        private int currentFallPointPatternIndex = 0;
        private event Action OnDeath;

        private void Awake()
        {
            bossStatus.OnDeath += HandleDeath;

            fallObjectPool = new ObjectPool<ProjectileObjectCache>(() =>
            {
                GameObject obj = Instantiate(projectileObjectPrefab, ObjectPool.Root, true);
                obj.SetActive(false);

                ProjectileObject projectileObject = obj.GetComponent<ProjectileObject>();
                OnDeath += projectileObject.Disable;
                return new ProjectileObjectCache(projectileObject);
            }, item =>
            {
                item.Obj.transform.SetPositionAndRotation(item.DefaultPos, Quaternion.identity);

                item.Rb.isKinematic = true;
                item.Col.enabled = true;

                item.Attacker.Reset();
                item.Effector.enabled = false;
                item.Scaler.ResetScale();

                item.Obj.gameObject.SetActive(false);
            }, item => { item.Effector.enabled = true; }, 16);

            sequencer1.SetPool(fallObjectPool);
            sequencer2.SetPool(fallObjectPool);
        }


        private void HandleDeath()
        {
            Current?.Cancel();
            OnDeath?.Invoke();
        }

        public void Switch()
        {
            Current?.Collect();

            if (Current == null)
            {
                Current = sequencer1;
                Current.SetPattern(fallPatterns[currentFallPointPatternIndex++]);
                Repeat();
                return;
            }

            Current = Current == sequencer1 ? sequencer2 : sequencer1;
            Current.SetPattern(fallPatterns[currentFallPointPatternIndex++]);
            Repeat();
        }

        private void Repeat()
        {
            currentFallPointPatternIndex %= fallPatterns.Count;
        }

        private void OnDestroy()
        {
            if (bossStatus != null)
            {
                bossStatus.OnDeath -= HandleDeath;
            }
        }
    }
}