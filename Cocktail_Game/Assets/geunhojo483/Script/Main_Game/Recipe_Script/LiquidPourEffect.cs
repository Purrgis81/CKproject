using UnityEngine;
using System.Collections.Generic;

public class LiquidPourEffect : MonoBehaviour
{
    [Header("액체 색")]
    public Color liquidColor = new Color(0.83f, 0.63f, 0.09f, 1f);
    public Color highlightColor = new Color(1f, 0.9f, 0.59f, 0.7f);

    [Header("재료 식별 (보통 자동으로 채워짐)")]
    [Tooltip("이 액체가 어떤 재료인지. 같은 오브젝트에 IngredientPourController가 있으면 자동으로 가져옴")]
    public IngredientData sourceIngredient;

    [Header("주둥이 위치 (재료 기준 로컬 좌표)")]
    [Tooltip("재료의 어디가 주둥이인지 (보통 위쪽)")]
    public Vector2 spoutLocalOffset = new Vector2(0f, 0.5f);

    [Header("흐름 설정")]
    [Tooltip("이 각도 이상 기울어야 흐르기 시작 (도)")]
    public float flowThreshold = 50f;
    [Tooltip("최대 흐름 강도가 되는 각도 (도)")]
    public float maxFlowAngle = 100f;

    [Header("입자 크기 설정")]
    [Range(0.1f, 5f)]
    public float sizeMultiplier = 2.5f;
    public float minDropSize = 0.05f;
    public float maxDropSize = 0.15f;

    [Header("입자 생성 설정")]
    public float minSpawnInterval = 0.015f;
    public float maxSpawnInterval = 0.06f;

    [Header("입자 움직임")]
    public float particleSpeed = 3f;
    public float particleLifetime = 1.5f;
    public float gravity = 8f;

    [Header("잔상")]
    public int trailLength = 8;
    [Range(0.1f, 1f)]
    public float trailSizeRatio = 0.7f;

    // 내부 상태
    private List<LiquidParticle> particles = new List<LiquidParticle>();
    private float lastSpawnTime = 0f;
    private bool isPouring = false;
    private bool isOnShaker = false;

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

    void Start()
    {
        isOnShaker = GetComponent<ShakerStateMachine>() != null;

        // ★ 같은 오브젝트에 재료 컨트롤러가 있으면, 그 재료 정보를 가져옴
        IngredientPourController ipc = GetComponent<IngredientPourController>();
        if (ipc != null)
        {
            sourceIngredient = ipc.ingredientData;
        }
    }

    public void StartPouring()
    {
        isPouring = true;
    }

    public void StopPouring()
    {
        isPouring = false;
    }

    // ★ 현재 흐름량(0~1)을 밖에서 읽기 위한 공개 함수
    public float GetCurrentFlow()
    {
        return CalculateFlowAmount();
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

        particleObj.layer = LayerMask.NameToLayer("Liquid");

        SpriteRenderer sr = particleObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = liquidColor;

        if (isOnShaker)
        {
            sr.sortingLayerName = "Body";
            sr.sortingOrder = -1;
        }
        else
        {
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
        }

        CircleCollider2D col = particleObj.AddComponent<CircleCollider2D>();
        col.isTrigger = false;   // 솔리드: 일반 콜라이더에 부딪히면 튕길 수 있게
        col.radius = 0.5f;

        // 방울끼리는 서로 충돌하지 않게 (물줄기가 흩어지는 것 방지)
        int liquidLayer = LayerMask.NameToLayer("Liquid");
        if (liquidLayer >= 0) Physics2D.IgnoreLayerCollision(liquidLayer, liquidLayer, true);

        // 자기를 만든 병(쉐이커)의 콜라이더와는 충돌 무시 (입구에서 막히는 것 방지)
        foreach (Collider2D myCol in GetComponentsInChildren<Collider2D>())
        {
            if (myCol != null) Physics2D.IgnoreCollision(col, myCol, true);
        }
        // ※ 특정 레이어 전체를 통과시키고 싶으면: Project Settings > Physics 2D >
        //    Layer Collision Matrix 에서 Liquid × 해당 레이어 체크 해제 (코드 불필요)

        Rigidbody2D particleRb = particleObj.AddComponent<Rigidbody2D>();
        particleRb.bodyType = RigidbodyType2D.Dynamic;
        particleRb.gravityScale = 0;
        particleRb.linearDamping = 0;
        particleRb.angularDamping = 0;

        // ★ 충돌 감지 + 이 방울이 어떤 재료인지 표시
        LiquidParticleCollision collision = particleObj.AddComponent<LiquidParticleCollision>();
        collision.sourceIngredient = sourceIngredient;

        LiquidParticle p = new LiquidParticle();
        p.obj = particleObj;
        p.renderer = sr;
        p.life = particleLifetime;
        p.maxLife = particleLifetime;

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

        particleRb.linearVelocity = p.velocity;

        particleObj.transform.localScale = Vector3.one * p.radius;

        particles.Add(p);
    }

    void UpdateParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            LiquidParticle p = particles[i];

            if (p.obj == null)
            {
                foreach (var trailObj in p.trailObjects)
                {
                    if (trailObj != null) Destroy(trailObj);
                }
                particles.RemoveAt(i);
                continue;
            }

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

                if (isOnShaker)
                {
                    trailSr.sortingLayerName = "Body";
                    trailSr.sortingOrder = -2;
                }
                else
                {
                    trailSr.sortingLayerID = p.renderer.sortingLayerID;
                    trailSr.sortingOrder = p.renderer.sortingOrder - 1;
                }

                trail.transform.localScale = Vector3.one * p.radius * trailSizeRatio;
                p.trailObjects.Add(trail);

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

            Rigidbody2D rb = p.obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity += new Vector2(0, -gravity * Time.deltaTime);
            }

            p.life -= Time.deltaTime;

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