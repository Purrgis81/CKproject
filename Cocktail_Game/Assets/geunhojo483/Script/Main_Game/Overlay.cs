using UnityEngine;

public class HoverZone : MonoBehaviour
{
    [Header("이 영역에 마우스가 들어오면 켜질 오버레이")]
    public GameObject overlay;

    [Header("영역 설정 (월드 좌표 기준)")]
    [Tooltip("영역의 좌측 하단 모서리")]
    public Vector2 areaMin = new Vector2(-7, -3);

    [Tooltip("영역의 우측 상단 모서리")]
    public Vector2 areaMax = new Vector2(7, 1);

    private bool isInside = false;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // 시작 시 오버레이는 꺼두기
        if (overlay != null)
        {
            overlay.SetActive(false);
        }
    }

    void Update()
    {
        if (cam == null) return;

        // 마우스의 화면 좌표를 월드 좌표로 변환
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // 영역 안에 있는지 검사 (사각형 안에 점이 있는지)
        bool currentlyInside =
            mouseWorld.x >= areaMin.x &&
            mouseWorld.x <= areaMax.x &&
            mouseWorld.y >= areaMin.y &&
            mouseWorld.y <= areaMax.y;

        // 상태가 변할 때만 오버레이 토글 (효율적)
        if (currentlyInside && !isInside)
        {
            // 영역 진입
            isInside = true;
            if (overlay != null) overlay.SetActive(true);
        }
        else if (!currentlyInside && isInside)
        {
            // 영역 이탈
            isInside = false;
            if (overlay != null) overlay.SetActive(false);
        }
    }

    // Scene 뷰에서 영역을 노란 사각형으로 시각화 (게임 실행 중에는 안 보임)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (areaMin.x + areaMax.x) / 2,
            (areaMin.y + areaMax.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            areaMax.x - areaMin.x,
            areaMax.y - areaMin.y,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);

        // 모서리 점도 표시
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(areaMin.x, areaMin.y, 0), 0.1f);
        Gizmos.DrawSphere(new Vector3(areaMax.x, areaMax.y, 0), 0.1f);
    }
}