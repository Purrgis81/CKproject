using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ShakerLidController : MonoBehaviour
{
    [Header("뚜껑 위치 고정")]
    public Transform shakerBody;
    public Vector3 worldOffset = new Vector3(0, 1f, 0);

    [Header("뚜껑 원래 위치")]
    [Tooltip("따르기 모드일 때 뚜껑이 있을 고정 위치")]
    public Vector3 fixedWorldPosition;  // 자동 저장됨
    private bool fixedPositionSet = false;

    void Start()
    {
        // 시작 시 고정 위치 자동 저장
        if (shakerBody != null && !fixedPositionSet)
        {
            fixedWorldPosition = shakerBody.position + worldOffset;
            fixedPositionSet = true;
        }
    }

    void LateUpdate()
    {
        if (ShakerStateMachine.Instance == null) return;

        var mode = ShakerStateMachine.Instance.currentMode;

        // ★ 분리 상태일 때만 본체 따라가기
        if (mode == ShakerMode.Separated)
        {
            if (shakerBody != null)
            {
                transform.position = shakerBody.position + worldOffset;
                transform.rotation = Quaternion.identity;

                // 고정 위치 갱신
                fixedWorldPosition = transform.position;
            }
        }
        // ★ 따르기 모드일 때는 고정 위치에 머물기!
        else if (mode == ShakerMode.Pouring || mode == ShakerMode.ReadyToThrow)
        {
            transform.position = fixedWorldPosition;
            transform.rotation = Quaternion.identity;
        }
    }

    void OnMouseDown()
    {
        if (ShakerStateMachine.Instance.currentMode != ShakerMode.Separated)
            return;

        Debug.Log("🎩 뚜껑 클릭! 닫힘 상태로!");
        ShakerStateMachine.Instance.SetMode(ShakerMode.Closed);
    }
}