using Constants;
using UnityEngine;

namespace Module.Player.Weapon
{
    public class Overlap : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tag.Handle.Ball))
                renderer.enabled = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Tag.Handle.Ball))
                renderer.enabled = true;
        }
    }
}