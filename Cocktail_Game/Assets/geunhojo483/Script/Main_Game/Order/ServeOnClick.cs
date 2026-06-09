using UnityEngine;

// 손님 NPC에 붙여서, 클릭하면 "칵테일 건네주기"가 되게 함.
//   - 클릭 → OrderManager.Serve() → 레시피 판정 후 점수 + 손님 퇴장.
//   - NPC에 Collider2D가 있어야 클릭이 먹힘.
[RequireComponent(typeof(Collider2D))]
public class ServeOnClick : MonoBehaviour
{
    void OnMouseDown()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.Serve();
        }
    }
}