using UnityEngine;

public enum ShakerMode
{
    Separated,      // 분리 (재료 투입) - 카운터 위에 놓임
    Closed,         // 닫힘 (위아래로 흔드는 중)
    ReadyToPour,    // 따르기 대기 (뚜껑 분리 완료, 위로 들어올리면 따라짐)
    Pouring         // 따르는 중
}

// ★ 물리 기반:
//   - 쉐이커 Rigidbody2D는 Dynamic + 중력 → 안 잡으면 카운터 위에 떨어져 놓임.
//   - 입력(잡기/이동/던지기)은 DraggableObject가 전담 (잡는 동안만 Kinematic).
//   - 이 스크립트는 잡혀서 움직이는 동안의 "움직임"만 관찰해 흔들기/따르기를 판단.
//   - 단계 전환: 흔들기 끝 → (뚜껑을 눌러 분리) → 따르기.  ※ 본체 이동은 던지기 전용.
[RequireComponent(typeof(Rigidbody2D))]
public class ShakerStateMachine : MonoBehaviour
{
    public static ShakerStateMachine Instance;

    [Header("스프라이트")]
    public SpriteRenderer bodyRenderer;
    public Sprite separatedSprite;
    public Sprite combinedSprite;

    [Header("뚜껑")]
    public GameObject lidObject;
    [Tooltip("쉐이커 자식으로 둔 '뚜껑 콜라이더'. 닫혔을 때만 켜지고, 클릭하면 열림 (머신이 직접 관리)")]
    public Collider2D lidCollider;

    [Header("흔들기 (위아래로 움직여 흔들기)")]
    [Tooltip("위아래로 움직인 거리가 이만큼 쌓이면 흔들기 완료")]
    public float shakeDistanceNeeded = 8f;

    [Header("따르기 (위로 들어올려 기울이기)")]
    [Tooltip("기준 높이보다 이만큼 위로 올리면 최대 각도까지 기울어짐 (월드 단위)")]
    public float pourLiftRange = 2f;
    public float maxPourAngle = 120f;
    [Tooltip("실제로 따른 시간이 이만큼 쌓이면 따르기 완료")]
    public float pourTimeToComplete = 2f;

    [Header("현재 상태")]
    public ShakerMode currentMode = ShakerMode.Separated;
    public bool shakeComplete = false;   // 다른 스크립트(뚜껑)가 읽음

    private Vector3 originalPosition;
    private Rigidbody2D rb;
    private Camera cam;
    private LiquidPourEffect liquidEffect;

    private Vector3 lastPos;
    private float shakeProgress = 0f;
    private float pourReferenceY = 0f;
    private float pourProgress = 0f;
    private bool pourFinished = false;

    private DraggableObject draggable;
    private bool frozenForIngredient = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Start()
    {
        originalPosition = transform.position;
        liquidEffect = GetComponent<LiquidPourEffect>();

        draggable = GetComponent<DraggableObject>();

        lastPos = transform.position;
        SetMode(ShakerMode.Separated);
    }

    void Update()
    {
        // 닫힘 상태에서 뚜껑 콜라이더를 클릭하면 → 열기
        if (currentMode == ShakerMode.Closed && Input.GetMouseButtonDown(0)
            && lidCollider != null && lidCollider.enabled && cam != null)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            if (lidCollider.OverlapPoint(mouseWorld))
            {
                OnLidRemoved();
                return;
            }
        }

        // 재료를 집고 있는 동안엔 쉐이커 완전 고정 (안 움직이게)
        if (IngredientPourController.AnyHeld)
        {
            if (!frozenForIngredient) frozenForIngredient = true;
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            lastPos = transform.position;
            return;
        }
        else if (frozenForIngredient)
        {
            // 재료를 막 놓음 → 물리 복구 (카운터 위에 다시 놓이게)
            frozenForIngredient = false;
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        }

        bool isHeld = (rb != null && rb.bodyType == RigidbodyType2D.Kinematic);

        if (isHeld)
        {
            // 잡혀서 움직이는 동안만 흔들기/따르기 관찰
            ObserveMotion();
        }
        else
        {
            // 물리로 굴러다니는 중(놓임/던져짐) → 화면 밖이면 리스폰
            CheckIfOutOfScreen();
        }

        lastPos = transform.position;
    }

    // ===== 움직임 관찰 =====
    void ObserveMotion()
    {
        if (currentMode == ShakerMode.Closed)
        {
            if (!shakeComplete)
            {
                float dy = Mathf.Abs(transform.position.y - lastPos.y);
                shakeProgress += dy;

                if (shakeProgress >= shakeDistanceNeeded)
                {
                    shakeComplete = true;
                    Debug.Log("🍸 흔들기 완료! 뚜껑을 눌러 분리하세요.");
                }
            }
        }
        else if (currentMode == ShakerMode.ReadyToPour || currentMode == ShakerMode.Pouring)
        {
            UpdatePourByLift();
        }
    }

    void UpdatePourByLift()
    {
        if (pourFinished) return;

        float lift = transform.position.y - pourReferenceY;
        float ratio = Mathf.Clamp01(lift / pourLiftRange);
        float angle = ratio * maxPourAngle;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        bool flowing = angle >= GetPourThreshold();

        if (flowing)
        {
            if (currentMode != ShakerMode.Pouring) currentMode = ShakerMode.Pouring;
            if (liquidEffect != null) liquidEffect.StartPouring();

            pourProgress += Time.deltaTime;
            if (pourProgress >= pourTimeToComplete) FinishPour();
        }
        else
        {
            if (liquidEffect != null) liquidEffect.StopPouring();
        }
    }

    void FinishPour()
    {
        pourFinished = true;
        if (liquidEffect != null) liquidEffect.StopPouring();
        Debug.Log("✅ 따르기 완료! 쉐이커를 휘둘러 치우세요. (종을 눌러 새로 시작)");
    }

    float GetPourThreshold()
    {
        if (liquidEffect != null) return liquidEffect.flowThreshold;
        return 50f;
    }

    // 뚜껑 콜라이더(자식)를 켜고 끔
    void SetLidClickArea(bool on)
    {
        if (lidCollider != null)
        {
            lidCollider.enabled = on;
            Debug.Log($"🎩 뚜껑 콜라이더 {(on ? "켬" : "끔")} → {lidCollider.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Lid Collider 슬롯이 비어있어요! ShakerStateMachine 인스펙터에 연결하세요.");
        }
    }

    // ===== 뚜껑이 눌려 분리되면 호출 (ShakerLidController가 호출) =====
    public void OnLidRemoved()
    {
        if (currentMode != ShakerMode.Closed) return;

        if (shakeComplete)
        {
            Debug.Log("🎩 뚜껑 분리 인식 → 따르기 단계!");
            SetMode(ShakerMode.ReadyToPour);
        }
        else
        {
            Debug.Log("🎩 아직 덜 흔들렸어요 → 다시 분리 상태로 (뚜껑 닫고 더 흔드세요)");
            SetMode(ShakerMode.Separated);
        }
    }

    // ===== 상태 변경 =====
    public void SetMode(ShakerMode newMode)
    {
        currentMode = newMode;
        Debug.Log($"🍹 쉐이커 모드: {newMode}");

        switch (newMode)
        {
            case ShakerMode.Separated: ApplySeparatedMode(); break;
            case ShakerMode.Closed: ApplyClosedMode(); break;
            case ShakerMode.ReadyToPour: ApplyReadyToPourMode(); break;
            case ShakerMode.Pouring: break;
        }
    }

    void ApplySeparatedMode()
    {
        if (bodyRenderer != null && separatedSprite != null)
            bodyRenderer.sprite = separatedSprite;

        if (lidObject != null)
        {
            lidObject.SetActive(true);
            ShakerLidController lidCtrl = lidObject.GetComponent<ShakerLidController>();
            if (lidCtrl != null) lidCtrl.ResetToStart();
        }
        SetLidClickArea(false);

        // 카운터 위 제자리로 되돌리고, 물리(Dynamic) 켜서 안정적으로 놓임
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        lastPos = transform.position;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        shakeComplete = false;
        shakeProgress = 0f;
        pourFinished = false;
        pourProgress = 0f;
    }

    void ApplyClosedMode()
    {
        Debug.Log("🔒 뚜껑 닫힘! 위아래로 흔든 뒤, 뚜껑을 클릭해 여세요.");

        if (bodyRenderer != null && combinedSprite != null)
            bodyRenderer.sprite = combinedSprite;

        // 뚜껑(드래그용)은 숨기고, 대신 자식 '클릭 영역'을 켜서 클릭으로 열 수 있게
        if (lidObject != null) lidObject.SetActive(false);
        SetLidClickArea(true);

        shakeComplete = false;
        shakeProgress = 0f;
        lastPos = transform.position;
    }

    void ApplyReadyToPourMode()
    {
        // 분리 사인: 본체는 분리 스프라이트, 뚜껑은 다시 나타남(벗긴 모습)
        if (bodyRenderer != null && separatedSprite != null)
            bodyRenderer.sprite = separatedSprite;
        if (lidObject != null)
        {
            lidObject.SetActive(true);
            ShakerLidController lidCtrl = lidObject.GetComponent<ShakerLidController>();
            if (lidCtrl != null) lidCtrl.ResetToStart();
        }
        SetLidClickArea(false);

        pourReferenceY = transform.position.y;  // 지금 높이 기준, 위로 올리면 따라짐
        pourProgress = 0f;
        pourFinished = false;
        transform.rotation = Quaternion.identity;
        Debug.Log("🥤 따르기 대기! 위로 들어올리면 따라집니다.");
    }

    // ===== 화면 밖 체크 (던져진 뒤) =====
    void CheckIfOutOfScreen()
    {
        if (cam == null) return;

        Vector3 v = cam.WorldToViewportPoint(transform.position);
        if (v.x < -0.2f || v.x > 1.2f || v.y < -0.2f || v.y > 1.2f)
        {
            Debug.Log("🚀 쉐이커 화면 밖으로 나감!");
            gameObject.SetActive(false);
            if (ShakerSpawner.Instance != null)
                ShakerSpawner.Instance.OnShakerFinished();
        }
    }

    // ===== 외부에서 재활성화 =====
    public void ResetAndReappear()
    {
        gameObject.SetActive(true);

        if (bodyRenderer != null)
        {
            Color c = bodyRenderer.color; c.a = 1f; bodyRenderer.color = c;
        }
        if (lidObject != null)
        {
            SpriteRenderer lidSr = lidObject.GetComponent<SpriteRenderer>();
            if (lidSr != null) { Color c = lidSr.color; c.a = 1f; lidSr.color = c; }
        }

        SetMode(ShakerMode.Separated);
        Debug.Log("🍹 쉐이커 재등장!");
    }
}