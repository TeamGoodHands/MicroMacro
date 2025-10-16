using System;
using System.Collections.Generic;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallSequencerSwitcher : MonoBehaviour
    {
        [SerializeField] private FallObjectsSequencer sequencer1;
        [SerializeField] private FallObjectsSequencer sequencer2;

        [SerializeField] private List<FallPattern> fallPatterns;

        public FallObjectsSequencer Current;
        private int currentFallPointPatternIndex = 0;

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
    }
}