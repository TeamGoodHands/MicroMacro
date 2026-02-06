using UnityEngine;

namespace Module.Scaling
{
    public class InCameraScaleActivator : MonoBehaviour
    {
        [SerializeField] private Renderer capRenderer;
        [SerializeField] private Scaler capScaler;
        private Camera mainCamera;
        private Plane[] frustumPlanes = new Plane[6];
        private bool isCameraInView;

        private void Start()
        {
            mainCamera = Camera.main;
            capScaler.enabled = false;
        }

        private void Update()
        {
            bool isCameraInViewThisFrame = IsCameraInView();
            if (isCameraInView != isCameraInViewThisFrame)
            {
                capScaler.enabled = isCameraInViewThisFrame;
                isCameraInView = isCameraInViewThisFrame;
            }
        }

        private bool IsCameraInView()
        {
            GeometryUtility.CalculateFrustumPlanes(mainCamera, frustumPlanes);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, capRenderer.bounds);
        }
    }
}