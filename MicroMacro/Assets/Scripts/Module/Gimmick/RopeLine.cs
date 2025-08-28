using UnityEngine;

namespace Module.Gimmick
{
    public class RopeLine : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] Transform startPoint;
        [SerializeField] Transform endPoint;

        void Update()
        {
            Vector3[] positions = new Vector3[] { startPoint.position, endPoint.position, };
            lineRenderer?.SetPositions(positions);
        }
    }
}