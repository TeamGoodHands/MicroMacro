using System;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public class FallSequencerSwitcher : MonoBehaviour
    {
        [SerializeField] private FallObjectsSequencer sequencer1;
        [SerializeField] private FallObjectsSequencer sequencer2;

        public FallObjectsSequencer Current;

        public void Switch()
        {
            Current?.Collect();

            if (Current == null)
            {
                Current = sequencer1;
                return;
            }

            Current = Current == sequencer1 ? sequencer2 : sequencer1;
        }
    }
}