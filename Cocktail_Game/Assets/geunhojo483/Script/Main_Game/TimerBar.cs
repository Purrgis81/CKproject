using UnityEngine;

// 남은 시간만큼 차 있는 타이머 바. 시간이 줄면 '오른쪽 끝부터' 깎여서 왼쪽으로 줄어듦.
//   - GameManager.TimeRatio(1=가득, 0=끝)를 읽어서 마스크 가로 폭을 조절.
//   - 잔 채우기와 같은 SpriteMask 기법인데, 세로가 아니라 '가로'로 줄이고 오른쪽을 고정.
//
// ★ 셋업:
//   1. 바 그림(SpriteRenderer) 하나 만들고 '가득 찬 길이'로 배치
//   2. 그 위에 SpriteMask 오브젝트(barMask)를 바 전체를 덮게 배치 + Sprite 칸 채우기
//   3. 바 그림의 Mask Interaction = Visible Inside Mask
//   4. 이 스크립트를 아무 데나(또는 바에) 붙이고 barMask 연결
public class TimerBar : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("줄어들 SpriteMask 의 Transform")]
    public Transform barMask;

    [Tooltip("비우면 GameManager.Instance 를 자동으로 사용")]
    public GameManager gameManager;

    // '가득' 기준 원본 크기/위치 (오른쪽 끝 고정용)
    private Vector3 baseScale;
    private float spriteWidth = 1f;
    private float spriteMaxX = 0.5f;     // 피벗 기준 오른쪽 끝
    private float maskRightLocalX;       // 오른쪽 끝 고정 좌표

    void Start()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        CaptureBase();
        UpdateBar(1f);
    }

    void Update()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        float ratio = (gameManager != null) ? gameManager.TimeRatio : 1f;
        UpdateBar(ratio);
    }

    // 에디터에서 맞춰둔 '가득' 상태 기준으로 오른쪽 끝 위치 기록 (피벗 어디든 OK)
    void CaptureBase()
    {
        if (barMask == null) return;

        baseScale = barMask.localScale;

        Sprite spr = null;
        SpriteMask sm = barMask.GetComponent<SpriteMask>();
        if (sm != null) spr = sm.sprite;
        if (spr == null)
        {
            SpriteRenderer sr = barMask.GetComponent<SpriteRenderer>();
            if (sr != null) spr = sr.sprite;
        }
        spriteWidth = (spr != null) ? spr.bounds.size.x : 1f;
        spriteMaxX = (spr != null) ? spr.bounds.max.x : 0.5f;

        // 오른쪽 끝 = 위치 + (피벗에서 오른쪽 끝까지 거리 × 스케일)
        maskRightLocalX = barMask.localPosition.x + spriteMaxX * baseScale.x;
    }

    void UpdateBar(float ratio)
    {
        if (barMask == null) return;
        ratio = Mathf.Clamp01(ratio);

        // 가로 폭만 비율만큼 줄임
        float newScaleX = baseScale.x * ratio;

        Vector3 sc = barMask.localScale;
        sc.x = newScaleX;
        barMask.localScale = sc;

        // 오른쪽 끝을 고정 → 왼쪽으로 줄어드는 것처럼 보임
        Vector3 pos = barMask.localPosition;
        pos.x = maskRightLocalX - spriteMaxX * newScaleX;
        barMask.localPosition = pos;
    }
}