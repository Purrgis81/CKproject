using UnityEngine;
using System;
using System.Collections.Generic;

// 홀드해서 옮기고, 세게 휘둘러 놓으면 관성으로 날아가는 공용 컴포넌트.
//   - straightenOnGrab: 잡는 순간 똑바로 세움(회전 0).
//   - InteractionLocked(static): 켜져 있으면 아무것도 못 잡음 (재료 집는 동안 쉐이커 잠금용).
//   - onTap: 드래그 없이 톡 누르면(탭) 호출되는 콜백 (뚜껑 분리 트리거로 사용).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DraggableObject : MonoBehaviour
{
    // 어떤 것이든 잡는 것을 전역으로 막는 잠금 (재료를 집는 동안 true)
    public static bool InteractionLocked = false;

    [Header("던지기")]
    public float throwVelocityMultiplier = 1f;
    public float maxThrowSpeed = 30f;
    [Tooltip("이 속도 미만으로 놓으면 던지지 않음 (살살 = 안 던짐, 그냥 떨어짐)")]
    public float minThrowSpeed = 5f;

    [Header("옵션")]
    public bool freezePhysicsWhileHeld = true;
    [Tooltip("안 던졌을 때 잡았던 자리로 되돌리기 (물리 낙하 쓰면 끄기)")]
    public bool returnIfNotThrown = false;
    [Tooltip("잡는 순간 똑바로 세우기 (회전 0)")]
    public bool straightenOnGrab = false;

    [Header("우선순위 양보")]
    [Tooltip("이 콜라이더들 위를 클릭하면 이 오브젝트는 잡히지 않음 (예: 뚜껑/클릭영역에 양보)")]
    public Collider2D[] yieldToColliders;

    [Header("탭(톡 누르기) 판정")]
    [Tooltip("이 픽셀 이하로 움직이고 놓으면 '탭'으로 간주 (던지기/이동 아님)")]
    public float tapPixelThreshold = 12f;

    // 드래그 없이 톡 눌렀을 때 호출 (예: 닫힌 쉐이커 → 뚜껑 분리)
    public Action onTap;

    public bool IsHeld { get; private set; }

    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 grabOffset;
    private Vector3 grabStartPos;
    private RigidbodyType2D originalBodyType;

    private Vector3 pressMouseScreen;
    private bool movedWhileHeld = false;

    private readonly List<Vector3> velPositions = new List<Vector3>();
    private readonly List<float> velTimes = new List<float>();
    private const float velWindow = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        originalBodyType = rb.bodyType;
    }

    void OnMouseDown()
    {
        if (InteractionLocked) return;   // 재료 집는 중엔 못 잡음
        if (IsPointerOverYieldCollider()) return;   // 뚜껑 등 위를 클릭하면 양보

        IsHeld = true;
        grabStartPos = transform.position;
        pressMouseScreen = Input.mousePosition;
        movedWhileHeld = false;

        if (freezePhysicsWhileHeld)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (straightenOnGrab)
        {
            transform.rotation = Quaternion.identity;
            rb.angularVelocity = 0f;   // 남은 회전 관성 제거
        }

        grabOffset = transform.position - GetMouseWorld();
        velPositions.Clear();
        velTimes.Clear();
    }

    void OnMouseDrag()
    {
        if (!IsHeld) return;

        // 충분히 움직였으면 '탭'이 아니라 드래그
        if (Vector3.Distance(Input.mousePosition, pressMouseScreen) > tapPixelThreshold)
            movedWhileHeld = true;

        Vector3 mouseWorld = GetMouseWorld();
        transform.position = mouseWorld + grabOffset;
        RecordVelocitySample(transform.position);
    }

    void OnMouseUp()
    {
        if (!IsHeld) return;
        IsHeld = false;

        // [탭] 드래그 거의 없이 놓음 → 던지지 않고 콜백만
        if (!movedWhileHeld)
        {
            transform.position = grabStartPos;
            rb.bodyType = originalBodyType;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            if (onTap != null) onTap.Invoke();
            return;
        }

        Vector3 v = ComputeThrowVelocity() * throwVelocityMultiplier;
        if (v.magnitude > maxThrowSpeed) v = v.normalized * maxThrowSpeed;

        if (v.magnitude >= minThrowSpeed)
        {
            // 세게 → 던지기
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = v;
        }
        else
        {
            // 살살 → 안 던짐
            if (returnIfNotThrown) transform.position = grabStartPos;
            rb.bodyType = originalBodyType;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void RecordVelocitySample(Vector3 pos)
    {
        velPositions.Add(pos);
        velTimes.Add(Time.time);
        while (velTimes.Count > 0 && Time.time - velTimes[0] > velWindow)
        {
            velPositions.RemoveAt(0);
            velTimes.RemoveAt(0);
        }
    }

    Vector3 ComputeThrowVelocity()
    {
        if (velPositions.Count < 2) return Vector3.zero;
        Vector3 oldest = velPositions[0];
        Vector3 newest = velPositions[velPositions.Count - 1];
        float dt = velTimes[velTimes.Count - 1] - velTimes[0];
        if (dt <= 0f) return Vector3.zero;
        return (newest - oldest) / dt;
    }

    Vector3 GetMouseWorld()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return mouseWorld;
    }

    // 마우스가 '양보 콜라이더'(켜져 있는) 위에 있나? → 있으면 이 오브젝트는 안 잡힘
    bool IsPointerOverYieldCollider()
    {
        if (yieldToColliders == null || cam == null) return false;
        Vector2 mw = cam.ScreenToWorldPoint(Input.mousePosition);
        foreach (Collider2D c in yieldToColliders)
        {
            if (c != null && c.enabled && c.gameObject.activeInHierarchy && c.OverlapPoint(mw))
                return true;
        }
        return false;
    }
}