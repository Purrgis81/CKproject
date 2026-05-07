using UnityEngine;
using System.Collections;

public class LiquidParticleCollision : MonoBehaviour
{
    private bool hasCollided = false;

    void Start()
    {
        Debug.Log($"💧 입자 생성됨! 내 Layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ★ 모든 충돌 출력 (어떤 거든 다)
        Debug.Log($"🔍 충돌 감지! 상대: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (hasCollided) return;

        // Catcher 레이어 체크
        if (other.gameObject.layer != LayerMask.NameToLayer("Catcher"))
        {
            Debug.Log("   ❌ Catcher 아님 → 무시");
            return;
        }

        Debug.Log("   ✅ Catcher 맞음 → 사라짐!");
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