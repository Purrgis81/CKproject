using UnityEngine;

// 주문 흐름 지휘:
//   - 라운드 시작 → 손님(아래에서↑) 먼저 등장 → "다 올라온 뒤" 영수증(위에서↓) 등장
//   - 칵테일을 내주면 Serve() → 손님 + 영수증 "동시에" 퇴장 → 잠시 뒤 다음 손님
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [Header("연결")]
    [Tooltip("손님 NPC (SlideInOut: enterFrom = Down)")]
    public SlideInOut customer;
    [Tooltip("영수증 (SlideInOut: enterFrom = Up)")]
    public SlideInOut receipt;

    [Header("타이밍")]
    [Tooltip("손님이 다 올라온 뒤 영수증을 내리기까지 약간의 텀(초)")]
    public float receiptDelay = 0.15f;
    [Tooltip("손님이 떠난 뒤 다음 손님까지 대기(초)")]
    public float nextOrderDelay = 1.5f;

    [Header("점수 판정")]
    [Tooltip("레시피 일치 판정용")]
    public RecipeChecker checker;
    [Tooltip("손님마다 무작위 레시피를 주문 (끄면 RecipeChecker.targetIndex 고정)")]
    public bool randomOrder = true;
    [Tooltip("같은 레시피가 연속으로 나오지 않게")]
    public bool avoidRepeat = true;
    [Tooltip("레시피가 맞았을 때 점수")]
    public int goodScore = 100;
    [Tooltip("틀렸을 때 점수 (0 또는 음수)")]
    public int missScore = 0;

    [Header("상태 (읽기용)")]
    public bool orderActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 손님이 "다 올라오면" 영수증을 내리도록 연결
        if (customer != null) customer.onArrived = OnCustomerArrived;

        NextOrder();
    }

    // 새 손님 등장 (손님 먼저)
    public void NextOrder()
    {
        orderActive = true;

        // 새 주문 레시피 뽑기 (무작위)
        PickOrderRecipe();

        Debug.Log("🧑 새 손님 등장 중...");

        if (customer != null)
        {
            customer.Arrive();   // 손님 먼저 올라옴 → 다 올라오면 OnCustomerArrived 호출됨
        }
        else if (receipt != null)
        {
            receipt.Arrive();    // 손님이 없으면 영수증만 바로
        }
    }

    // 이번 손님이 주문할 레시피를 정함
    void PickOrderRecipe()
    {
        if (!randomOrder || checker == null) return;
        if (RecipeDatabase.All == null || RecipeDatabase.All.Count == 0) return;

        int count = RecipeDatabase.All.Count;          // 50개면 0~49
        int idx = Random.Range(0, count);               // 0 이상 count 미만

        // 직전과 같은 게 또 나오면 한 칸 옆으로 (연속 방지)
        if (avoidRepeat && count > 1 && idx == checker.targetIndex)
            idx = (idx + 1) % count;

        checker.targetIndex = idx;
        Debug.Log($"📋 새 주문: [{idx}] {RecipeDatabase.All[idx].name}");
    }

    // 손님이 다 올라온 뒤 호출됨 → 영수증 내림
    void OnCustomerArrived()
    {
        Debug.Log("🧑 손님 도착 완료 → 영수증 내려옴");
        if (receipt != null) Invoke(nameof(DropReceipt), receiptDelay);
    }

    void DropReceipt()
    {
        if (receipt != null) receipt.Arrive();
    }

    // 칵테일을 손님에게 건네줬을 때 호출 → 잔 가득참 확인 → 레시피 판정 → 점수 → 퇴장
    public void Serve()
    {
        if (!orderActive) return;

        // 0. 잔이 가득 차야만 넘길 수 있음
        if (!IsGlassFull())
        {
            Debug.Log("🥛 잔이 덜 찼어요! 다 채운 뒤에 넘기세요.");
            return;
        }

        orderActive = false;

        // 1. 레시피가 주문과 맞는지 판정
        bool ok = (checker != null && checker.IsSatisfied());
        int gained = ok ? goodScore : missScore;

        // 불만족이면 정확한 이유를 콘솔에 출력
        if (!ok && checker != null)
            Debug.Log("😕 불만족 이유: " + checker.GetUnsatisfiedReason());

        // 2. 점수 반영
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(gained);
        Debug.Log(ok ? $"😋 손님 만족! +{gained}점" : $"😕 손님 실망... +{gained}점");

        // 3. 손님 + 영수증 동시 퇴장
        if (customer != null) customer.Leave();
        if (receipt != null) receipt.Leave();

        // 4. 다음 손님을 위해 쉐이커 비우고, 잔 새로 (빈 잔으로)
        if (ShakerManager.Instance != null) ShakerManager.Instance.ResetAll();
        if (GlassController.Instance != null) GlassController.Instance.ResetGlass();

        Invoke(nameof(NextOrder), nextOrderDelay);
    }

    // 현재 잔이 가득 찼는지 (채우기 시스템이 없으면 통과)
    bool IsGlassFull()
    {
        if (GlassController.Instance == null) return true;
        GameObject g = GlassController.Instance.GetCurrentGlass();
        if (g == null) return false;
        GlassFillController fill = g.GetComponent<GlassFillController>();
        if (fill == null) return true;   // 채우기 컨트롤러 안 붙였으면 그냥 통과
        return fill.IsFull;
    }
}