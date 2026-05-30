using UnityEngine;

// 재료(병 등)를 잡고 "위아래로 드래그"해서 기울여 따르는 컨트롤러.
//   - 클릭 즉시 따르기 X, 순간이동 X. 있는 그 자리에서 기울인 만큼만 흐름.
//   - 양 카운트는 쉐이커 받이(ShakerCatcher)가 함 (실제로 들어간 방울만).
//   - 마우스 올리면 sortingOrder를 올려 앞으로 가져오는 호버 효과.
//   - LiquidPourEffect 와 같은 오브젝트에 붙여야 합니다.
[RequireComponent(typeof(LiquidPourEffect))]
public class IngredientPourController : MonoBehaviour
{
    [Header("재료 정보")]
    [Tooltip("이 병이 어떤 재료인지 (방울 꼬리표 + 양 추적용) — 꼭 연결!")]
    public IngredientData ingredientData;

    [Header("따르기 기울기 설정")]
    [Tooltip("최대로 기울어지는 각도")]
    public float maxPourAngle = 120f;
    [Tooltip("이 거리만큼 위아래로 드래그하면 최대 각도까지 (월드 단위)")]
    public float pourDragRange = 3f;
    [Tooltip("드래그 방향이 반대로 느껴지면 체크")]
    public bool invertDrag = false;

    [Header("마우스 호버 (앞으로 가져오기)")]
    [Tooltip("마우스 올렸을 때 적용할 sortingOrder")]
    public int hoverSortingOrder = 51;

    private Camera cam;
    private LiquidPourEffect liquidEffect;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;      // 호버 끝나면 되돌릴 원래 값

    // 지금 재료를 집고 있는 개수 (쉐이커가 참고: >0이면 쉐이커 고정)
    public static int HeldCount = 0;
    public static bool AnyHeld => HeldCount > 0;

    private bool isPouring = false;
    private Vector3 startMouseWorld;       // 잡은 순간의 마우스 위치 (기준점)
    private Quaternion uprightRotation;    // 따르기 전 똑바로 선 회전

    void Start()
    {
        cam = Camera.main;
        liquidEffect = GetComponent<LiquidPourEffect>();
        uprightRotation = transform.rotation;

        // 호버용: 스프라이트와 원래 sortingOrder 기억
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    void OnMouseDown()
    {
        isPouring = true;

        // 재료 집는 중 → 쉐이커 등 다른 것 못 잡게 잠금
        HeldCount++;
        DraggableObject.InteractionLocked = true;

        // 기준 마우스 위치 기록 (여기서부터 얼마나 끌었는지로 각도 결정)
        startMouseWorld = GetMouseWorld();

        // 액체 효과 시작 (각도가 임계값 넘으면 자동으로 흐름)
        if (liquidEffect != null)
        {
            liquidEffect.StartPouring();
        }
    }

    void Update()
    {
        if (!isPouring) return;

        // 세로(위아래) 드래그량으로 기울기 결정 — 아래로 끌수록 많이 기울임
        Vector3 mouseWorld = GetMouseWorld();
        float drag = startMouseWorld.y - mouseWorld.y;
        if (invertDrag) drag = -drag;

        float ratio = Mathf.Clamp01(drag / pourDragRange);
        float targetAngle = ratio * maxPourAngle;

        // 회전 적용 → LiquidPourEffect가 이 각도를 읽어 흐름량을 정함
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

        // ※ 양 카운트는 여기서 안 함! 받이(ShakerCatcher)가 실제로 들어온 방울만 셈.
    }

    void OnMouseUp()
    {
        if (!isPouring) return;
        isPouring = false;

        // 잠금 해제 (마지막 재료를 놓을 때만 완전 해제)
        HeldCount = Mathf.Max(0, HeldCount - 1);
        if (HeldCount == 0) DraggableObject.InteractionLocked = false;

        // 똑바로 세우고 흐름 멈춤
        transform.rotation = uprightRotation;

        if (liquidEffect != null)
        {
            liquidEffect.StopPouring();
        }
    }

    // 마우스 올리면 앞으로 (원래 OnMouseOver였지만, 한 번만 적용하면 충분해서 Enter로)
    void OnMouseEnter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = hoverSortingOrder;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
    }

    Vector3 GetMouseWorld()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return mouseWorld;
    }
}