using UnityEngine;
using System.Collections;

// 액체 방울 한 개의 충돌 처리.
//   ① Catcher 레이어에 닿음 → (잔 받이면) 잔 채우기 알림 → 퍽 터지며 사라짐
//   ② 일반 콜라이더에 닿음 → 통~ 튕긴 다음 서서히 사라짐
//   ※ 쉐이커 받이는 ShakerCatcher 가 자기 트리거에서 따로 셈 (여기선 안 건드림)
public class LiquidParticleCollision : MonoBehaviour
{
    [Header("튕김 설정")]
    [Tooltip("튕길 때 속도가 얼마나 남나 (0.4 = 40%)")]
    public float bounceDamping = 0.4f;
    [Tooltip("튕긴 뒤 사라지는 데 걸리는 시간(초)")]
    public float fadeAfterBounce = 0.4f;

    private bool hasCollided = false;   // Catcher 에 잡혀 터지는 중
    private bool bounced = false;       // 이미 한 번 튕겨서 사라지는 중

    // ★ 이 방울이 어떤 재료에서 나왔는지 (쉐이커/잔 받이가 읽어서 양을 셈)
    public IngredientData sourceIngredient;

    // ── ① Catcher 레이어 (트리거 받이) ──────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Catcher")) return;

        // 잔의 받이면 잔 채우기 알림 (쉐이커 받이는 ShakerCatcher 가 알아서)
        GlassFillController glass = other.GetComponentInParent<GlassFillController>();
        if (glass != null) glass.AddDrop(this);

        hasCollided = true;
        StartCoroutine(SplashEffect());
    }

    // ── ② 일반 (솔리드) 콜라이더 → 튕기고 사라짐 ───────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided) return;

        // 혹시 Catcher 가 솔리드 콜라이더여도 잡히게
        if (collision.gameObject.layer == LayerMask.NameToLayer("Catcher"))
        {
            GlassFillController glass = collision.collider.GetComponentInParent<GlassFillController>();
            if (glass != null) glass.AddDrop(this);

            hasCollided = true;
            StartCoroutine(SplashEffect());
            return;
        }

        if (bounced) return;
        bounced = true;

        // 부딪힌 면의 법선(normal) 방향으로 통~ 튕겨 보냄
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && collision.contactCount > 0)
        {
            Vector2 n = collision.GetContact(0).normal;
            float speed = collision.relativeVelocity.magnitude;
            Vector2 dir = (n + new Vector2(Random.Range(-0.3f, 0.3f), 0f)).normalized;
            rb.linearVelocity = dir * speed * bounceDamping;
        }

        StartCoroutine(FadeAndDestroy());
    }

    // 튕긴 뒤 날아가면서 서서히 투명해지다 사라짐
    IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < fadeAfterBounce)
        {
            // 사라지는 중에 Catcher 에 들어가면 SplashEffect 쪽이 이어받음
            if (hasCollided) yield break;

            elapsed += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - (elapsed / fadeAfterBounce);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    // Catcher 에 잡혔을 때: 퍽 부풀며 사라짐
    IEnumerator SplashEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 originalScale = transform.localScale;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
            transform.localScale = originalScale * scale * (1f - t);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}