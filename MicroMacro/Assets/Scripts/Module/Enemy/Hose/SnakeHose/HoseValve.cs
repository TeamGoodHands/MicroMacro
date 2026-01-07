using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class HoseValve : MonoBehaviour
{
    [SerializeField] private float stepAngle = 90f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Renderer valveRender;

    private float currentAngle = 0f;
    private bool isLocked = false;
    private bool isRotating = false;

    /// <summary>
    /// DOTweenを用いてバルブを回転させます
    /// </summary>
    public async UniTask RotateValveAsync()
    {
        if (isLocked)
            return;

        if (isRotating)
            return;

        isRotating = true;

        float nextAngle = currentAngle + stepAngle;
        Vector3 target = new Vector3(0f, nextAngle, 0f);

        // DOTweenで回転処理。360度を超えても正しく回るモードを指定
        await transform.DOLocalRotate(target, duration, RotateMode.FastBeyond360)
            .ToUniTask();

        currentAngle = nextAngle;

        if (currentAngle >= 360f)
        {
            isLocked = true;

            if (valveRender != null)
            {
                valveRender.material.color = lockedColor;
            }
        }

        isRotating = false;
    }
}