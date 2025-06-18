using UnityEngine;

public class MoveFloor : MonoBehaviour
{
    [SerializeField] private Rigidbody rigbody;

    private float rad = 0f;

    void FixedUpdate()
    {
        rad += Time.fixedDeltaTime * Mathf.PI * 2f / 2f; // 3秒で1周
        rigbody.position = new Vector3(Mathf.Cos(rad) * 3f, Mathf.Sin(rad) * 3f);
    }
}