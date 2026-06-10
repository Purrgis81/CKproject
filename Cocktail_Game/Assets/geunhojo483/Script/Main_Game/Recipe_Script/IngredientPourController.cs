using UnityEngine;

// 재료(병)를 좌클릭으로 잡고, 우클릭으로 기울여 따르는 컨트롤러.
//   - 좌클릭 누르고 있는 동안 = 잡은 상태
//   - 잡은 채 우클릭 누르면 → pourAngle 까지 기울어짐 / 우클릭 떼면 → 다시 똑바로
//   - 좌클릭 떼면 → 손 놓고 똑바로 섬
//   - 액체는 pourStartAngle(기본 90도)부터 흐름 (LiquidPourEffect.flowThreshold 에 연동)
//   - 실제 양 카운트는 쉐이커 받이(ShakerCatcher)가 함.
//   - 병의 '똑바로 선' 상태 = 회전 z 0도 기준.
[RequireComponent(typeof(LiquidPourEffect))]
public class IngredientPourController : MonoBehaviour
{
    [Header("재료 정보")]
    [Tooltip("이 병이 어떤 재료인지 (방울 꼬리표 + 양 추적용) — 꼭 연결!")]
    public IngredientData ingredientData;

    [Header("따르기 기울기 설정")]
    [Tooltip("우클릭 시 기울어지는 각도. 반대로 기울면 부호(-)로 바꾸세요")]
    public float pourAngle = 120f;
    [Tooltip("이 각도부터 액체가 나오기 시작 (도)")]
    public float pourStartAngle = 90f;
    [Tooltip("기울고/펴지는 속도 (도/초). 클수록 빠릿하게 움직임")]
    public float tiltSpeed = 540f;

    [Header("마우스 호버 (앞으로 가져오기)")]
    [Tooltip("마우스 올렸을 때 적용할 sortingOrder")]
    public int hoverSortingOrder = 51;

    private LiquidPourEffect liquidEffect;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    // 지금 재료를 잡고 있는 개수 (쉐이커가 참고: >0이면 쉐이커 고정)
    public static int HeldCount = 0;
    public static bool AnyHeld => HeldCount > 0;

    private bool grabbed = false;
    private float currentTilt = 0f;     // 현재 기울기(도). 0 = 똑바로

    void Start()
    {
        liquidEffect = GetComponent<LiquidPourEffect>();

        // 액체가 나오기 시작하는 각도를 90도(pourStartAngle)에 맞춤
        if (liquidEffect != null)
        {
            liquidEffect.flowThreshold = pourStartAngle;
            liquidEffect.maxFlowAngle = Mathf.Max(Mathf.Abs(pourAngle), pourStartAngle + 1f);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalSortingOrder = spriteRenderer.sortingOrder;
    }

    // 좌클릭 누름 = 잡기
    void OnMouseDown()
    {
        grabbed = true;

        // 재료 잡는 중 → 쉐이커 등 다른 것 못 잡게 잠금
        HeldCount++;
        DraggableObject.InteractionLocked = true;

        // 효과 켜둠 (실제 흐름은 각도가 90도 넘을 때만 나옴)
        if (liquidEffect != null) liquidEffect.StartPouring();
    }

    void Update()
    {
        if (!grabbed) return;

        // 잡은 채 우클릭 누르고 있으면 pourAngle 까지, 떼면 0(똑바로)으로
        bool rightHeld = Input.GetMouseButton(1);
        float target = rightHeld ? pourAngle : 0f;

        currentTilt = Mathf.MoveTowardsAngle(currentTilt, target, tiltSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);

        // ※ 양 카운트는 여기서 안 함! 받이(ShakerCatcher)가 실제로 들어온 방울만 셈.
    }

    // 좌클릭 뗌 = 손 놓고 그냥 똑바로
    void OnMouseUp()
    {
        if (!grabbed) return;
        grabbed = false;

        HeldCount = Mathf.Max(0, HeldCount - 1);
        if (HeldCount == 0) DraggableObject.InteractionLocked = false;

        currentTilt = 0f;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (liquidEffect != null) liquidEffect.StopPouring();
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null) spriteRenderer.sortingOrder = hoverSortingOrder;
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null) spriteRenderer.sortingOrder = originalSortingOrder;
    }
}