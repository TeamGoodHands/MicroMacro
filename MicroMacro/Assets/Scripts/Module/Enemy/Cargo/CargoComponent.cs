using System;
using Module.Application.Dialogue;
using Module.UI;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public class CargoComponent
    {
        [SerializeField] private CargoParameter parameter;
        [SerializeField] private CargoCondition condition;
        [SerializeField] private Transform transform;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Renderer renderer;
        [SerializeField] private HealthStatus status;
        [SerializeField] private Rigidbody moveParent;
        [SerializeField] private CargoControllerWrapper animatorWrapper;
        [SerializeField] private CinemachineBasicMultiChannelPerlin cineMachinePerlin;
        [SerializeField] private CinemachineCamera nearInEnemyCamera;
        [SerializeField] private CinemachineCamera bossCamera;
        [SerializeField] private CinemachineCamera deathCamera;
        [SerializeField] private CanvasGroup hpBarCanvasGroup;
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;
        [SerializeField] private NoiseSettings backAttackNoise;
        [SerializeField] private NoiseSettings moveNoise;
        [SerializeField] private DialogueManager dialogueManager;

        public AudioSource BGMSource { get; set; }

        public CargoParameter Parameter => parameter;
        public CargoCondition Condition => condition;
        public Transform Transform => transform;
        public Renderer Renderer => renderer;
        public HealthStatus Status => status;
        public Rigidbody MoveParent => moveParent;
        public CargoControllerWrapper AnimatorWrapper => animatorWrapper;
        public CinemachineBasicMultiChannelPerlin CineMachinePerlin => cineMachinePerlin;
        public CinemachineCamera NearInEnemyCamera => nearInEnemyCamera;
        public CinemachineCamera DeathCamera => deathCamera;
        public CinemachineCamera BossCamera => bossCamera;
        public CanvasGroup HpBarCanvasGroup => hpBarCanvasGroup;
        public Transform Start => start;
        public Transform Goal => goal;
        public DialogueManager DialogueManager => dialogueManager;
        public NoiseSettings BackAttackNoise => backAttackNoise;
        public NoiseSettings MoveNoise => moveNoise;
    }
}