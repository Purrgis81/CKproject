using UnityEngine;

// 잔에 붙임. 받이(트리거 콜라이더)에 들어온 액체 방울을 '세어서' 차오르고,
// 가득 차야만 손님에게 넘길(서빙) 수 있게 함.
//
//  ★ 받이 트리거가 이 스크립트로 신호를 보내려면 둘 중 하나:
//     (A) 트리거 콜라이더를 이 오브젝트(잔)에 직접 붙이기  ← 제일 간단
//     (B) 자식 'Catcher'에 트리거를 두고, 잔(루트)에 Rigidbody2D 붙이기
//        (자식 콜라이더의 트리거는 Rigidbody가 있는 부모로 올라옴)
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
    private float maskBottomLocalY;

    void Start()
    {
        CaptureMaskBase();
        UpdateLiquid();
    }

    // 받이에 액체 방울이 들어오면 1개 세고 그만큼 채움
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsFull) return;

        LiquidParticleCollision drop = other.GetComponent<LiquidParticleCollision>();
        if (drop == null)
        {
            // 디버그: 들어왔는데 액체가 아님 (다른 콜라이더)
            // Debug.Log($"(잔) 트리거 들어옴, 액체 아님: {other.name}");
            return;
        }

        if (onlyFinishedCocktail && drop.sourceIngredient != null) return;

        currentFill += amountPerDrop;
        if (currentFill > fillToServe) currentFill = fillToServe;
        UpdateLiquid();

        Debug.Log($"🥛 잔 채움 +{amountPerDrop} → {currentFill:F0}/{fillToServe:F0}");
    }

    // 에디터에서 맞춰둔 '가득' 마스크 상태 기준으로 바닥 위치 기록
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

        float fullHeight = maskSpriteHeight * maskBaseScale.y;
        maskBottomLocalY = liquidMask.localPosition.y - fullHeight * 0.5f;
    }

    void UpdateLiquid()
    {
        if (liquidMask == null) return;
        float ratio = (fillToServe <= 0f) ? 0f : Mathf.Clamp01(currentFill / fillToServe);

        // 바닥 고정한 채 위로 키움
        float newScaleY = maskBaseScale.y * ratio;
        float newHeight = maskSpriteHeight * newScaleY;

        Vector3 sc = liquidMask.localScale;
        sc.y = newScaleY;
        liquidMask.localScale = sc;

        Vector3 pos = liquidMask.localPosition;
        pos.y = maskBottomLocalY + newHeight * 0.5f;
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