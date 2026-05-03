using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class bottle_Select : MonoBehaviour
{
    [Header("호버 효과 설정")]
    [Tooltip("호버 시 위로 떠오르는 거리 (월드 단위)")]
    public float hoverLift = 0.2f;

    [Tooltip("호버 시 확대 배율")]
    public float hoverScale = 1.08f;

    [Tooltip("호버 시 적용할 sorting order")]
    public int hoverSortingOrder = 100;

    [Tooltip("애니메이션 속도 (값이 클수록 빠름)")]
    public float animationSpeed = 12f;

    // 시작 상태 기억용
    private SpriteRenderer sr;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private int originalSortingOrder;

    // 매 프레임 부드럽게 다가갈 목표값
    private Vector3 targetPos;
    private Vector3 targetScale;



    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        originalPos = transform.position;
        originalScale = transform.localScale;
        originalSortingOrder = sr.sortingOrder;

        targetPos = originalPos;
        targetScale = originalScale;
    }

    void OnMouseEnter()
    {
        // 즉시 변경: sorting order
        sr.sortingOrder = hoverSortingOrder;

        // 부드럽게 변경 예약: 위치, 크기
        targetPos = originalPos + Vector3.up * hoverLift;
        targetScale = originalScale * hoverScale;
        
    }

    void OnMouseExit()
    {
        // 역순으로 원복
        sr.sortingOrder = originalSortingOrder;
        targetPos = originalPos;
        targetScale = originalScale;
    }

    void Update()
    {
        // 매 프레임 목표값 쪽으로 보간 → 부드러운 애니메이션
        float t = animationSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPos, t);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
    }
}