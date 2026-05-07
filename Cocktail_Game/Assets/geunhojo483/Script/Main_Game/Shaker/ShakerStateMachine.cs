using UnityEngine;
using System.Collections;

public enum ShakerMode
{
    Separated,      // 1: 분리 상태 (재료 투입)
    Closed,         // 1.5: 닫힘 상태 (흔들기 대기)
    Shaking,        // 2: 흔들기 중
    ReadyToPour,    // 3: 따르기 대기
    Pouring,        // 4: 따르는 중
    ReadyToThrow    // 5: 던지기 대기 (따르기 완료)
}

public class ShakerStateMachine : MonoBehaviour
{
    public static ShakerStateMachine Instance;

    [Header("스프라이트 변환")]
    public SpriteRenderer bodyRenderer;
    public Sprite separatedSprite;
    public Sprite combinedSprite;

    [Header("뚜껑 제어")]
    public GameObject lidObject;

    [Header("흔들기 설정")]
    public float shakeDuration = 1f;
    public float shakeAmount = 0.2f;
    public float shakeFrequency = 30f;

    [Header("따르기 설정")]
    public Transform glassPosition;
    public float pourTiltAngle = 100f;
    public float pourDuration = 1.5f;

    [Header("Slingshot 설정")]
    public float forceMultiplier = 10f;
    public float maxDragDistance = 3f;
    public float minDragDistance = 0.5f;

    [Header("현재 상태")]
    public ShakerMode currentMode = ShakerMode.Separated;

    // 내부 변수
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody2D rb;
    private Camera cam;
    private bool wasDraggedFar = false;  // 드래그 중 멀리 갔는지 추적

    // Slingshot 관련
    private Vector3 dragStartPos;
    private bool isDragging = false;
    private bool hasBeenThrown = false;

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
        originalRotation = transform.rotation;

        // Rigidbody2D 초기화
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        SetMode(ShakerMode.Separated);
    }

    void Update()
    {
        // ★ 던진 후 화면 밖으로 나가면 처리
        if (hasBeenThrown && currentMode == ShakerMode.ReadyToThrow)
        {
            CheckIfOutOfScreen();
        }
    }

    // ===== 상태 변경 =====
    public void SetMode(ShakerMode newMode)
    {
        currentMode = newMode;
        Debug.Log($"🍹 쉐이커 모드: {newMode}");

        switch (newMode)
        {
            case ShakerMode.Separated:
                ApplySeparatedMode();
                break;
            case ShakerMode.Closed:
                ApplyClosedMode();
                break;
            case ShakerMode.Shaking:
                ApplyShakingMode();
                break;
            case ShakerMode.ReadyToPour:
                ApplyReadyToPourMode();
                break;
            case ShakerMode.Pouring:
                ApplyPouringMode();
                break;
            case ShakerMode.ReadyToThrow:
                ApplyReadyToThrowMode();
                break;
        }
    }

    // ===== 모드별 동작 =====

    void ApplySeparatedMode()
    {
        if (bodyRenderer != null && separatedSprite != null)
            bodyRenderer.sprite = separatedSprite;

        if (lidObject != null)
            lidObject.SetActive(true);

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Rigidbody2D는 Kinematic 유지
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void ApplyClosedMode()
    {
        Debug.Log("🔒 뚜껑 닫힘! 흔들기 대기 중...");

        if (bodyRenderer != null && combinedSprite != null)
            bodyRenderer.sprite = combinedSprite;

        if (lidObject != null)
            lidObject.SetActive(false);
    }

    void ApplyShakingMode()
    {
        Debug.Log("💪 흔들기 시작!");
        StartCoroutine(ShakeRoutine());
    }

    void ApplyReadyToPourMode()
    {
        transform.position = originalPosition;
    }

    void ApplyPouringMode()
    {
        // ★ 안전장치: 드래그 강제 종료
        isDragging = false;

        StartCoroutine(PourRoutine());
    }

    void ApplyReadyToThrowMode()
    {
        Debug.Log("🎯 던지기 가능! 드래그해서 발사하세요!");
        // 던지기 모드로 진입했을 때 시작 위치 저장
        dragStartPos = transform.position;
    }

    // ===== 흔들기 =====
    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // ★ Y축만 흔들기! X는 0으로 고정
            float offsetY = Mathf.Sin(elapsed * shakeFrequency) * shakeAmount;

            transform.position = originalPosition + new Vector3(0, offsetY, 0);

            yield return null;
        }

        transform.position = originalPosition;
        SetMode(ShakerMode.ReadyToPour);
    }

    // ===== 따르기 =====
    IEnumerator PourRoutine()
    {
        Debug.Log("🥤 따르기 시작!");

        // 1. 잔 위치로 순간이동
        if (glassPosition != null)
        {
            transform.position = glassPosition.position;
        }

        // 2. 분리 스프라이트로 변경
        if (bodyRenderer != null && separatedSprite != null)
        {
            bodyRenderer.sprite = separatedSprite;
            Debug.Log("📦 분리 스프라이트로 변경");
        }

        // 3. 뚜껑 다시 표시
        if (lidObject != null)
        {
            lidObject.SetActive(true);
            Debug.Log("🎩 뚜껑 다시 표시");
        }

        yield return new WaitForSeconds(0.1f);

        // 4. 액체 효과 시작!
        LiquidPourEffect liquidEffect = GetComponent<LiquidPourEffect>();
        if (liquidEffect != null)
        {
            liquidEffect.StartPouring();
        }

        // 5. 기울이기 (+100도)
        float tiltDuration = 0.3f;
        float elapsed = 0f;
        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiltDuration;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0f, pourTiltAngle, t));
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, pourTiltAngle);

        // 6. 따르는 시간
        Debug.Log("🍹 따르는 중...");
        yield return new WaitForSeconds(pourDuration);

        // 7. 0도로 복귀
        elapsed = 0f;
        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiltDuration;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(pourTiltAngle, 0f, t));
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        // 8. 액체 효과 종료
        if (liquidEffect != null)
        {
            liquidEffect.StopPouring();
        }

        // 9. 따르기 완료!
        Debug.Log("✅ 따르기 완료!");

        yield return new WaitForSeconds(0.3f);

        // 페이드 아웃 (이 안에서 ShakerSpawner도 호출함)
        yield return StartCoroutine(FadeOutAndDisappear());

        // ★ 여기서 추가 호출은 안 함! (FadeOutAndDisappear 안에서 이미 호출됨)
    }

    // ===== Slingshot 마우스 입력 =====

    void OnMouseDown()
    {
        // 따르기 중이면 안 됨 (애니메이션 방해)
        if (currentMode == ShakerMode.Pouring) return;
        if (hasBeenThrown) return;

        isDragging = true;
        dragStartPos = transform.position;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        wasDraggedFar = false;

        Debug.Log($"✋ 쉐이커 잡음! (모드: {currentMode})");
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 offset = mouseWorld - dragStartPos;
        if (offset.x < 0) offset.x = 0;  // 왼쪽으로만
        offset.y = 0;                      // Y축 고정

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        transform.position = dragStartPos + offset;

        // 한 번이라도 minDragDistance 이상 갔으면 기록
        Vector3 currentDragVector = transform.position - dragStartPos;
        if (currentDragVector.magnitude >= minDragDistance)
        {
            wasDraggedFar = true;
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector3 dragVector = transform.position - dragStartPos;

        // 드래그 중 멀리 안 갔으면 = 진짜 클릭
        if (!wasDraggedFar)
        {
            transform.position = dragStartPos;
            HandleClick();
            return;
        }

        // 드래그 중 멀리 갔지만 끝에 원위치 = 취소
        if (dragVector.magnitude < minDragDistance)
        {
            transform.position = dragStartPos;
            Debug.Log("🚫 드래그 취소!");
            return;
        }

        // ★ 드래그 거리 충분 = 던지기! (이 부분이 빠져있었음!)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 force = -dragVector * forceMultiplier;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        hasBeenThrown = true;
        Debug.Log($"🚀 쉐이커 발사! (모드: {currentMode})");

        SetMode(ShakerMode.ReadyToThrow);
    }

    // ★ 클릭 처리 함수
    void HandleClick()
    {
        Debug.Log($"👆 클릭됨! 현재 모드: {currentMode}");

        switch (currentMode)
        {
            case ShakerMode.Closed:
                // 닫힘 → 흔들기 시작
                SetMode(ShakerMode.Shaking);
                break;

            case ShakerMode.ReadyToPour:
                // ★ 따르기 대기 → 따르기 시작!
                SetMode(ShakerMode.Pouring);
                break;

            // 다른 모드는 클릭 무시
            default:
                Debug.Log($"   → {currentMode}에서 클릭 무시");
                break;
        }
    }

    // ===== 화면 밖 체크 =====
    void CheckIfOutOfScreen()
    {
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -0.2f || viewportPos.x > 1.2f ||
            viewportPos.y < -0.2f || viewportPos.y > 1.2f)
        {
            Debug.Log("🚀 쉐이커 화면 밖으로 나감!");

            gameObject.SetActive(false);

            if (ShakerSpawner.Instance != null)
            {
                ShakerSpawner.Instance.OnShakerFinished();
            }
        }
    }

    // ===== 외부에서 재활성화 =====
    public void ResetAndReappear()
    {
        // 위치/회전 복원
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // 본체 활성화
        gameObject.SetActive(true);

        // 알파 복원
        if (bodyRenderer != null)
        {
            Color c = bodyRenderer.color;
            c.a = 1f;
            bodyRenderer.color = c;
        }

        // 뚜껑 활성화
        if (lidObject != null)
        {
            lidObject.SetActive(true);
            SpriteRenderer lidSr = lidObject.GetComponent<SpriteRenderer>();
            if (lidSr != null)
            {
                Color c = lidSr.color;
                c.a = 1f;
                lidSr.color = c;
            }
        }

        // Rigidbody2D 초기화
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 던지기 상태 초기화
        hasBeenThrown = false;
        isDragging = false;

        // 모드 1로 시작
        SetMode(ShakerMode.Separated);

        Debug.Log("🍹 쉐이커 재등장!");
    }
    IEnumerator FadeOutAndDisappear()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        SpriteRenderer lidSr = lidObject != null ? lidObject.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);

            // 본체 페이드
            if (bodyRenderer != null)
            {
                Color c = bodyRenderer.color;
                c.a = alpha;
                bodyRenderer.color = c;
            }

            // 뚜껑 페이드
            if (lidSr != null)
            {
                Color c = lidSr.color;
                c.a = alpha;
                lidSr.color = c;
            }

            // 모든 자식 페이드
            foreach (var sr in childRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            yield return null;
        }

        // ★ 사라지기 전에 ShakerSpawner에 알림! (코루틴 살아있을 때!)
        if (ShakerSpawner.Instance != null)
        {
            Debug.Log("📞 ShakerSpawner.OnShakerFinished() 호출!");
            ShakerSpawner.Instance.OnShakerFinished();
        }
        else
        {
            Debug.LogError("❌ ShakerSpawner.Instance가 null!");
        }

        // 모든 자식 비활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 뚜껑 비활성화
        if (lidObject != null)
        {
            lidObject.SetActive(false);
        }

        // 완전히 사라짐
        gameObject.SetActive(false);
    }
}