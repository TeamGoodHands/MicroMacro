using System;
using Module.Application.Respawn;
using UnityEngine;

namespace Module.UI
{
    public class IrisOutPresenter : MonoBehaviour
    {
        [SerializeField] private IrisOutEffector irisOutEffector;
        private RespawnSystem respawnSystem;

        public void Start()
        {
            respawnSystem = FindAnyObjectByType<RespawnSystem>();

            if (respawnSystem == null)
            {
                throw new Exception("RespawnSystem not found");
            }

            respawnSystem.OnPlayerDespawn += irisOutEffector.DoIrisOut;
            respawnSystem.OnPlayerRespawn += irisOutEffector.DoIrisIn;
        }
    }
}