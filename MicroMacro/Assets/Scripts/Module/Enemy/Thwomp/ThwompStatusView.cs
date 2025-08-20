using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Thwomp
{
    public class ThwompStatusView : MonoBehaviour
    {
        [SerializeField] private EnemyStatus status;
        [SerializeField] private Texture[] crackTextures;
        [SerializeField] private Renderer bodyRenderer;

        private static readonly int textureProperty = Shader.PropertyToID("_Texture");

        private void Start()
        {
            status.OnDamage += hp =>
            {
                if (hp < status.MaxHealth)
                {
                    int index = status.MaxHealth - hp - 1;
                    bodyRenderer.material.SetTexture(textureProperty, crackTextures[index]);
                }
            };
        }
    }
}