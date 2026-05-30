using UnityEngine;

// 뚜껑:
//   - 분리 상태: 드래그해서 본체에 올리면 닫힘.
//   - 닫힘 상태: 뚜껑을 "누르면(클릭)" 분리 → 흔들기가 끝났으면 따르기 단계로.
[RequireComponent(typeof(BoxCollider2D))]
public class ShakerLidController : MonoBehaviour
{
    [Header("본체 참조")]
    public Transform shakerBody;

    [Header("닫힘 판정")]
    [Tooltip("뚜껑을 놓았을 때 본체와 이 거리 이내면 닫힘")]
    public float closeDistance = 1.5f;

    private Camera cam;
    private Vector3 startPosition;
    private bool isHeld = false;
    private Vector3 grabOffset;
    private bool dragMovedEnough = false;   // 클릭과 드래그 구분용

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
    }

    void OnMouseDown()
    {
        if (ShakerStateMachine.Instance == null) return;

        ShakerMode mode = ShakerStateMachine.Instance.currentMode;

        // [닫힘] 뚜껑을 누르면 분리
        if (mode == ShakerMode.Closed)
        {
            ShakerStateMachine.Instance.OnLidRemoved();
            return;
        }

        // [분리] 드래그해서 닫기
        if (mode == ShakerMode.Separated)
        {
            isHeld = true;
            dragMovedEnough = false;
            transform.rotation = Quaternion.identity;   // 잡으면 똑바로
            DraggableObject.InteractionLocked = true;    // 뚜껑 잡는 동안 본체 못 잡게
            Vector3 mouseWorld = GetMouseWorld();
            grabOffset = transform.position - mouseWorld;
        }
    }

    void OnMouseDrag()
    {
        if (!isHeld) return;
        Vector3 mouseWorld = GetMouseWorld();
        transform.position = mouseWorld + grabOffset;
        dragMovedEnough = true;
    }

    void OnMouseUp()
    {
        if (!isHeld) return;
        isHeld = false;
        DraggableObject.InteractionLocked = false;   // 본체 잠금 해제

        if (shakerBody == null)
        {
            Debug.LogWarning("⚠️ 뚜껑에 shakerBody 참조가 비어있어요!");
            return;
        }

        float distance = Vector3.Distance(transform.position, shakerBody.position);

        if (distance <= closeDistance)
        {
            Debug.Log("🎩 뚜껑을 본체에 올림 → 닫힘!");
            ShakerStateMachine.Instance.SetMode(ShakerMode.Closed);
        }
        else
        {
            Debug.Log($"🎩 뚜껑 내려놓음 (본체와 거리 {distance:F1}, {closeDistance} 이내여야 닫힘)");
        }
    }

    // 분리/리셋 시 뚜껑을 시작 위치로 (ShakerStateMachine이 호출)
    public void ResetToStart()
    {
        transform.position = startPosition;
    }

    Vector3 GetMouseWorld()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return mouseWorld;
    }
}