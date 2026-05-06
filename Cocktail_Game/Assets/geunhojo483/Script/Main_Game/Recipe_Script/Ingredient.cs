using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Ingredient : MonoBehaviour
{
    [Header("재료 데이터")]
    public IngredientData data;

    [Header("이동 애니메이션 설정")]
    public Transform shakerTopPosition;
    public float moveSpeed = 100f;
    public float tiltAngle = 100f;
    public float tiltDuration = 0.3f;
    public float pourDuration = 0.5f;

    [Header("호버 효과 설정")]
    [Tooltip("호버 시 위로 떠오르는 거리")]
    public float hoverLift = 0.2f;
    [Tooltip("호버 시 확대 배율")]
    public float hoverScale = 1.08f;
    [Tooltip("호버 시 sorting order")]
    public int hoverSortingOrder = 100;
    [Tooltip("호버 애니메이션 속도")]
    public float hoverSpeed = 12f;

    // ===== 내부 상태 =====
    private bool isUsed = false;          // 이미 사용했는지
    private bool isPouring = false;        // 현재 따르는 중인지 (호버 비활성화용)
    private Vector3 targetPosition;        // 이동 목표 위치 (고정)

    // ===== 호버 관련 =====
    private SpriteRenderer sr;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private int originalSortingOrder;
    private Vector3 hoverTargetPos;
    private Vector3 hoverTargetScale;

    void Start()
    {
        // 호버용 초기 상태 저장
        sr = GetComponent<SpriteRenderer>();
        originalPos = transform.position;
        originalScale = transform.localScale;
        originalSortingOrder = sr.sortingOrder;
        hoverTargetPos = originalPos;
        hoverTargetScale = originalScale;
    }

    void Update()
    {
        // 따르는 중이면 호버 애니메이션 안 함! (충돌 방지)
        if (isPouring) return;

        // 호버 애니메이션: 부드럽게 목표값으로 보간
        float t = hoverSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, hoverTargetPos, t);
        transform.localScale = Vector3.Lerp(transform.localScale, hoverTargetScale, t);
    }

    // ===== 마우스 호버 진입 =====
    void OnMouseEnter()
    {
        if (isUsed || isPouring) return;  // 사용했거나 따르는 중이면 무시

        sr.sortingOrder = hoverSortingOrder;
        hoverTargetPos = originalPos + Vector3.up * hoverLift;
        hoverTargetScale = originalScale * hoverScale;
    }

    // ===== 마우스 호버 이탈 =====
    void OnMouseExit()
    {
        if (isUsed || isPouring) return;

        sr.sortingOrder = originalSortingOrder;
        hoverTargetPos = originalPos;
        hoverTargetScale = originalScale;
    }

    // ===== 마우스 클릭 =====
    void OnMouseDown()
    {
        if (isUsed) return;
        if (ShakerManager.Instance.currentState != ShakerState.AddingIngredients) return;

        isUsed = true;
        isPouring = true;  // ★ 호버 비활성화

        // 호버 상태 초기화 (혹시 떠 있었으면 원래대로)
        sr.sortingOrder = originalSortingOrder;
        transform.localScale = originalScale;

        // 목표 위치 고정 (Z축은 자기 자신 그대로 사용)
        Vector3 target = shakerTopPosition.position;
        target.z = transform.position.z;
        targetPosition = target;

        StartCoroutine(PourSequence());
    }

    // ===== 따르기 전체 시퀀스 =====
    IEnumerator PourSequence()
    {
        yield return StartCoroutine(MoveToShaker());

        // 기울이기
        yield return StartCoroutine(Tilt(tiltAngle));

        // ★ 액체 효과 시작!
        LiquidPourEffect liquidEffect = GetComponent<LiquidPourEffect>();
        if (liquidEffect != null)
        {
            // 재료 데이터의 색상 적용
            if (data != null) liquidEffect.liquidColor = data.liquidColor;
            liquidEffect.StartPouring();
        }

        yield return new WaitForSeconds(pourDuration);

        // ★ 액체 효과 멈춤!
        if (liquidEffect != null)
        {
            liquidEffect.StopPouring();
        }

        ShakerManager.Instance.AddIngredient(data);

        yield return StartCoroutine(Tilt(0f));

        // 잠시 대기 후 (남은 입자 다 떨어지게)
        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false);
    }

    // ===== 쉐이커 위로 이동 =====
    IEnumerator MoveToShaker()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f && elapsed < timeout)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    // ===== 기울이기 =====
    IEnumerator Tilt(float targetAngle)
    {
        float startAngle = transform.eulerAngles.z;
        if (startAngle > 180f) startAngle -= 360f;

        float elapsed = 0f;

        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiltDuration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }
}