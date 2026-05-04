using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("발사 설정")]
    public float forceMultiplier = 10f;   // 힘 배율 (클수록 더 멀리 날아감)
    public float maxDragDistance = 3f;    // 최대로 당길 수 있는 거리

    [Header("잡기 조건")]
    public float maxGrabSpeed = 0.05f;      // 이 속도보다 느려야 잡을 수 있음
    public float maxGrabAngle = 5f;         // 이 각도(도) 이내여야 잡을 수 있음

    private Rigidbody2D rb;
    private Camera cam;

    private Vector3 startPos;       // 오브젝트의 원래 위치 (고무줄의 중심)
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        startPos = transform.position;       // 시작 위치 기억
        rb.bodyType = RigidbodyType2D.Kinematic; // 드래그 중엔 물리 영향 X
    }

    void OnMouseDown()
    {
        // 1) 아직 움직이고 있으면 무시
        if (rb.linearVelocity.magnitude > maxGrabSpeed) return;
        if (Mathf.Abs(rb.angularVelocity) > 0.1f) return;

        // 2) 기울어져 있으면 무시
        //    eulerAngles.z는 0~360으로 나오므로 -180~180 범위로 변환
        float angle = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
        if (Mathf.Abs(angle) > maxGrabAngle) return;

        // 통과! 잡기 시작
        isDragging = true;
        startPos = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        // 1) 마우스 월드 좌표 구하기
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        // 2) 시작 위치에서 마우스까지의 벡터
        Vector3 offset = mouseWorld - startPos;

        // 3) X축은 "왼쪽"으로만 (offset.x가 양수면 0으로 잘라냄)
        if (offset.x < 0) offset.x = 0;

        // 4) Y축은 고정 (항상 0)
        offset.y = 0;

        // 5) 최대 거리 제한
        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        // 6) 오브젝트 위치 업데이트
        transform.position = startPos + offset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // 당겨진 벡터 = 현재 위치 - 시작 위치  (음수 X 방향)
        Vector3 dragVector = transform.position - startPos;

        // 물리 켜기
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 발사! "당긴 반대 방향"으로 힘을 줌 → -dragVector
        // 당긴 거리에 비례해서 힘이 세짐
        Vector2 force = -dragVector * forceMultiplier;

        rb.AddForce(force, ForceMode2D.Impulse);
    }
}