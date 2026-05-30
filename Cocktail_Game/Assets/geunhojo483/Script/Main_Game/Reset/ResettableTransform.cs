using UnityEngine;

// 종을 누르면 제자리로 돌아와야 하는 물건에 붙이세요 (재료 병 등).
//   - 시작 시점의 위치/회전을 기억해뒀다가 ResetToStart()로 복원합니다.
//   - Rigidbody2D가 있으면 속도도 0으로 멈춰요.
public class ResettableTransform : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody2D rb;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
    }

    public void ResetToStart()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}