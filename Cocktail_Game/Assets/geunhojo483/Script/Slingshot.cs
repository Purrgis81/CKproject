using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("발사 설정")]
    public float forceMultiplier = 10f;   // 힘 배율 (클수록 더 멀리 날아감)
    public float maxDragDistance = 3f;    // 최대로 당길 수 있는 거리
    public float minDragDistance = 0.5f;  // 당겨짐이 발생할 수 있는 최소 단위

    [Header("잡기 조건")]
    public float maxGrabSpeed = 0.05f;    // 이 속도보다 느려야 잡을 수 있음
    public float maxGrabAngle = 5f;       // 이 각도(도) 이내여야 잡을 수 있음

    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 startPos;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        startPos = transform.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnMouseDown()
    {
        // 🚫 움직이는 중이면 무시
        if (rb.linearVelocity.magnitude > maxGrabSpeed) return;
        if (Mathf.Abs(rb.angularVelocity) > 0.1f) return;

        // 🚫 기울어져 있으면 무시
        float angle = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
        if (Mathf.Abs(angle) > maxGrabAngle) return;

        // ✅ 잡기 시작
        isDragging = true;
        startPos = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 offset = mouseWorld - startPos;

        if (offset.x < 0) offset.x = 0;   // 왼쪽으로만
        offset.y = 0;                      // Y축 고정

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        transform.position = startPos + offset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector3 dragVector = transform.position - startPos;

        // 🚫 최소 거리 미만이면 발사 취소
        if (dragVector.magnitude < minDragDistance)   // 👈 여기를 변수로 변경!
        {
            transform.position = startPos;  // 원래 자리로
            return;
        }

        // ✅ 발사
        rb.bodyType = RigidbodyType2D.Dynamic;
        Vector2 force = -dragVector * forceMultiplier;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}