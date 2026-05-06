using UnityEngine;
using System.Collections.Generic;

public class LiquidPourEffect : MonoBehaviour
{
    [Header("액체 색")]
    public Color liquidColor = new Color(0.83f, 0.63f, 0.09f, 1f);
    public Color highlightColor = new Color(1f, 0.9f, 0.59f, 0.7f);

    [Header("주둥이 위치 (재료 기준 로컬 좌표)")]
    [Tooltip("재료의 어디가 주둥이인지 (보통 위쪽)")]
    public Vector2 spoutLocalOffset = new Vector2(0f, 0.5f);

    [Header("흐름 설정")]
    [Tooltip("이 각도 이상 기울어야 흐르기 시작 (도)")]
    public float flowThreshold = 50f;
    [Tooltip("최대 흐름 강도가 되는 각도 (도)")]
    public float maxFlowAngle = 100f;

    [Header("입자 크기 설정")]
    [Tooltip("전체 크기 배율 (1=기본, 2=2배 크게, 0.5=절반)")]
    [Range(0.1f, 5f)]
    public float sizeMultiplier = 2.5f;
    [Tooltip("입자의 최소 크기")]
    public float minDropSize = 0.05f;
    [Tooltip("입자의 최대 크기")]
    public float maxDropSize = 0.15f;

    [Header("입자 생성 설정")]
    [Tooltip("강하게 흐를 때 입자 사이 시간 (작을수록 콸콸)")]
    public float minSpawnInterval = 0.015f;
    [Tooltip("약하게 흐를 때 입자 사이 시간")]
    public float maxSpawnInterval = 0.06f;

    [Header("입자 움직임")]
    public float particleSpeed = 3f;
    public float particleLifetime = 1.5f;
    public float gravity = 8f;

    [Header("잔상")]
    [Tooltip("잔상 길이 (0이면 잔상 없음)")]
    public int trailLength = 8;
    [Tooltip("잔상 크기 비율 (입자 대비)")]
    [Range(0.1f, 1f)]
    public float trailSizeRatio = 0.7f;

    // 내부 상태
    private List<LiquidParticle> particles = new List<LiquidParticle>();
    private float lastSpawnTime = 0f;
    private bool isPouring = false;

    private class LiquidParticle
    {
        public GameObject obj;
        public SpriteRenderer renderer;
        public Vector3 velocity;
        public float life;
        public float maxLife;
        public float radius;
        public List<GameObject> trailObjects = new List<GameObject>();
    }

    public void StartPouring()
    {
        isPouring = true;
    }

    public void StopPouring()
    {
        isPouring = false;
    }

    void Update()
    {
        if (isPouring)
        {
            TrySpawnParticle();
        }
        UpdateParticles();
    }

    float CalculateFlowAmount()
    {
        float currentAngle = Mathf.Abs(NormalizeAngle(transform.eulerAngles.z));
        if (currentAngle < flowThreshold) return 0f;
        float t = (currentAngle - flowThreshold) / (maxFlowAngle - flowThreshold);
        return Mathf.Clamp01(t);
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    Vector3 GetSpoutWorldPosition()
    {
        return transform.TransformPoint(spoutLocalOffset);
    }

    Vector3 GetSpoutDirection()
    {
        Vector3 worldUp = transform.TransformDirection(Vector3.up);
        Vector3 finalDir = worldUp + Vector3.down * 0.3f;
        return finalDir.normalized;
    }

    void TrySpawnParticle()
    {
        float flow = CalculateFlowAmount();
        if (flow <= 0f) return;

        float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, flow);
        if (Time.time - lastSpawnTime < interval) return;
        lastSpawnTime = Time.time;

        SpawnParticle(flow);
    }

    void SpawnParticle(float flow)
    {
        GameObject particleObj = new GameObject("LiquidParticle");
        particleObj.transform.position = GetSpoutWorldPosition();

        SpriteRenderer sr = particleObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = liquidColor;

        // 부모(재료)와 같은 Sorting Layer + 앞에 보이게
        SpriteRenderer parentRenderer = GetComponent<SpriteRenderer>();
        if (parentRenderer != null)
        {
            sr.sortingLayerID = parentRenderer.sortingLayerID;
            sr.sortingOrder = parentRenderer.sortingOrder - 1;
        }
        else
        {
            sr.sortingOrder = 1000;
        }

        LiquidParticle p = new LiquidParticle();
        p.obj = particleObj;
        p.renderer = sr;
        p.life = particleLifetime;
        p.maxLife = particleLifetime;

        // 크기 = 기본크기 × 흐름 영향 × 전체 배율
        float baseSize = Random.Range(minDropSize, maxDropSize);
        float flowEffect = 0.7f + flow * 0.3f;
        p.radius = baseSize * flowEffect * sizeMultiplier;

        Vector3 dir = GetSpoutDirection();
        float spread = 0.2f;
        Vector3 randomOffset = new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0
        );
        p.velocity = (dir + randomOffset).normalized * particleSpeed;

        particleObj.transform.localScale = Vector3.one * p.radius;

        particles.Add(p);
    }

    void UpdateParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            LiquidParticle p = particles[i];

            // 잔상 추가
            if (trailLength > 0)
            {
                if (p.trailObjects.Count >= trailLength)
                {
                    Destroy(p.trailObjects[0]);
                    p.trailObjects.RemoveAt(0);
                }

                GameObject trail = new GameObject("Trail");
                trail.transform.position = p.obj.transform.position;
                SpriteRenderer trailSr = trail.AddComponent<SpriteRenderer>();
                trailSr.sprite = p.renderer.sprite;
                trailSr.color = new Color(liquidColor.r, liquidColor.g, liquidColor.b, 0.4f);
                trailSr.sortingLayerID = p.renderer.sortingLayerID;
                trailSr.sortingOrder = p.renderer.sortingOrder - 1;
                trail.transform.localScale = Vector3.one * p.radius * trailSizeRatio;
                p.trailObjects.Add(trail);

                // 잔상 페이드
                for (int j = 0; j < p.trailObjects.Count; j++)
                {
                    if (p.trailObjects[j] == null) continue;
                    float ratio = (float)j / p.trailObjects.Count;
                    SpriteRenderer ts = p.trailObjects[j].GetComponent<SpriteRenderer>();
                    if (ts != null)
                    {
                        Color c = liquidColor;
                        c.a = ratio * 0.5f;
                        ts.color = c;
                        p.trailObjects[j].transform.localScale =
                            Vector3.one * p.radius * ratio * trailSizeRatio;
                    }
                }
            }

            // 중력
            p.velocity.y -= gravity * Time.deltaTime;
            p.obj.transform.position += p.velocity * Time.deltaTime;
            p.life -= Time.deltaTime;

            // 죽으면 제거
            if (p.life <= 0f || p.obj.transform.position.y < -20f)
            {
                foreach (var trailObj in p.trailObjects)
                {
                    if (trailObj != null) Destroy(trailObj);
                }
                Destroy(p.obj);
                particles.RemoveAt(i);
            }
        }
    }

    // 동적으로 원형 스프라이트 생성
    static Sprite cachedCircleSprite;
    Sprite CreateCircleSprite()
    {
        if (cachedCircleSprite != null) return cachedCircleSprite;

        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    float alpha = 1f - (distance / radius) * 0.3f;
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cachedCircleSprite;
    }

    void OnDisable()
    {
        foreach (var p in particles)
        {
            foreach (var trailObj in p.trailObjects)
            {
                if (trailObj != null) Destroy(trailObj);
            }
            if (p.obj != null) Destroy(p.obj);
        }
        particles.Clear();
    }
}