using UnityEngine;

// 주문 흐름 지휘:
//   - 라운드 시작 → 손님(아래에서↑) + 영수증(위에서↓) 등장
//   - 칵테일을 내주면 Serve() → 손님 만족하고 퇴장 → 잠시 뒤 다음 손님
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [Header("연결")]
    [Tooltip("손님 NPC (SlideInOut: enterFrom = Down)")]
    public SlideInOut customer;
    [Tooltip("영수증 (SlideInOut: enterFrom = Up)")]
    public SlideInOut receipt;

    [Header("타이밍")]
    [Tooltip("손님이 떠난 뒤 다음 손님까지 대기(초)")]
    public float nextOrderDelay = 1.5f;

    [Header("상태 (읽기용)")]
    public bool orderActive = false;   // 지금 손님이 와 있는지

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        NextOrder();
    }

    // 새 손님 등장
    public void NextOrder()
    {
        if (customer != null) customer.Arrive();
        if (receipt != null) receipt.Arrive();
        orderActive = true;
        Debug.Log("🧑 새 손님 등장! 주문(영수증) 도착.");
        // TODO: 여기서 새 레시피를 뽑아 영수증에 표시하고 RecipeChecker 목표로 설정
    }

    // 칵테일을 손님에게 내줬을 때 호출 → 만족하고 퇴장
    public void Serve()
    {
        if (!orderActive) return;       // 손님 없을 때 중복 호출 방지
        orderActive = false;

        Debug.Log("😋 손님 만족! 퇴장합니다.");
        if (customer != null) customer.Leave();
        if (receipt != null) receipt.Leave();

        Invoke(nameof(NextOrder), nextOrderDelay);
    }
}
