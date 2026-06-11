using UnityEngine;

// 잔에 붙임. 잔의 받이(= Catcher 레이어 트리거)에 닿은 액체 방울을 세어서 차오르고,
// 가득 차야만 손님에게 넘길(서빙) 수 있게 함.
//
//  ★ 받이 만들기: 잔(또는 자식)에 트리거 콜라이더를 두고, 그 오브젝트의
//     레이어를 "Catcher" 로 설정. 방울이 그 받이에 닿으면 AddDrop 을 불러줌.
//     (Catcher 레이어가 아닌 콜라이더에는 방울이 튕기기만 하고 안 채워짐)
[RequireComponent(typeof(Collider2D))]
public class GlassFillController : MonoBehaviour
{
    [Header("채우기")]
    [Tooltip("이만큼 차면 '가득참' = 넘길 수 있음")]
    public float fillToServe = 30f;
    [Tooltip("방울 하나당 차는 양")]
    public float amountPerDrop = 1f;
    [Tooltip("켜면 쉐이커가 따른 완성 액체(재료 꼬리표 없는 것)만 받음. 일단 끄고 테스트 권장")]
    public bool onlyFinishedCocktail = false;

    [Header("액체 마스크 (바닥 고정하고 차오름)")]
    [Tooltip("차오르는 SpriteMask의 Transform")]
    public Transform liquidMask;

    [Header("상태 (읽기용)")]
    public float currentFill = 0f;
    public bool IsFull { get { return currentFill >= fillToServe; } }

    private Vector3 maskBaseScale;
    private float maskSpriteHeight = 1f;
    private float maskSpriteMinY = -0.5f;
    private float maskBottomLocalY;

    void Start()
    {
        CaptureMaskBase();
        UpdateLiquid();
    }

    // 방울이 잔의 Catcher 받이에 닿으면 불러주는 함수 (LiquidParticleCollision 에서 호출)
    public void AddDrop(LiquidParticleCollision drop)
    {
        if (IsFull) return;
        if (drop == null) return;
        if (onlyFinishedCocktail && drop.sourceIngredient != null) return;

        currentFill += amountPerDrop;
        if (currentFill > fillToServe) currentFill = fillToServe;
        UpdateLiquid();

        Debug.Log($"🥛 잔 채움 +{amountPerDrop} → {currentFill:F0}/{fillToServe:F0}");
    }

    // 에디터에서 맞춰둔 '가득' 마스크 상태 기준으로 바닥 위치 기록
    // (스프라이트 피벗이 Center/Bottom/Top 어디든 정확히 동작하게 bounds로 계산)
    void CaptureMaskBase()
    {
        if (liquidMask == null) return;

        maskBaseScale = liquidMask.localScale;

        Sprite spr = null;
        SpriteMask sm = liquidMask.GetComponent<SpriteMask>();
        if (sm != null) spr = sm.sprite;
        if (spr == null)
        {
            SpriteRenderer sr = liquidMask.GetComponent<SpriteRenderer>();
            if (sr != null) spr = sr.sprite;
        }
        maskSpriteHeight = (spr != null) ? spr.bounds.size.y : 1f;
        maskSpriteMinY = (spr != null) ? spr.bounds.min.y : -0.5f; // 피벗 기준 아래쪽 끝

        // 실제 바닥 = 위치 + (피벗에서 아래끝까지 거리 × 스케일)
        maskBottomLocalY = liquidMask.localPosition.y + maskSpriteMinY * maskBaseScale.y;
    }

    void UpdateLiquid()
    {
        if (liquidMask == null) return;
        float ratio = (fillToServe <= 0f) ? 0f : Mathf.Clamp01(currentFill / fillToServe);

        // 바닥 고정한 채 위로 키움
        float newScaleY = maskBaseScale.y * ratio;

        Vector3 sc = liquidMask.localScale;
        sc.y = newScaleY;
        liquidMask.localScale = sc;

        // 바닥이 maskBottomLocalY에 고정되도록 피벗 위치를 역산
        Vector3 pos = liquidMask.localPosition;
        pos.y = maskBottomLocalY - maskSpriteMinY * newScaleY;
        liquidMask.localPosition = pos;
    }

    // 잔을 누르면 = 넘기기 (가득 찼을 때만)
    void OnMouseDown()
    {
        if (!IsFull)
        {
            Debug.Log($"🥛 아직 덜 찼어요! ({currentFill:F0}/{fillToServe:F0}) 더 따른 뒤 넘기세요.");
            return;
        }
        if (OrderManager.Instance != null) OrderManager.Instance.Serve();
    }

    public void ResetFill()
    {
        currentFill = 0f;
        UpdateLiquid();
    }
}