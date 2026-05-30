using UnityEngine;
using System.Collections;

public class LiquidParticleCollision : MonoBehaviour
{
    private bool hasCollided = false;

    // ★ 이 방울이 어떤 재료에서 나왔는지 (쉐이커 받이가 읽어서 양을 셈)
    public IngredientData sourceIngredient;

    void Start()
    {
        // (디버그 로그가 필요하면 여기서)
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return;

        // Catcher 레이어가 아니면 무시
        if (other.gameObject.layer != LayerMask.NameToLayer("Catcher"))
        {
            return;
        }

        hasCollided = true;
        StartCoroutine(SplashEffect());
    }

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