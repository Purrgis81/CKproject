using UnityEngine;

// 쉐이커 입구에 두는 "받이". 재료에서 떨어진 액체 방울이 여기로 들어오면
// 그 재료의 양을 ShakerManager에 더해줍니다.
//   - 조준을 빗나가 흘린 액체는 안 세집니다 (실제로 들어온 것만 인식).
//   - 만드는 법: 빈 오브젝트를 쉐이커 입구에 두고,
//       · Layer = "Catcher"
//       · Collider2D 추가 + Is Trigger 체크
//       · 이 스크립트 추가
[RequireComponent(typeof(Collider2D))]
public class ShakerCatcher : MonoBehaviour
{
    [Tooltip("방울 하나가 들어올 때 더해지는 양")]
    public float amountPerDrop = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 게 액체 방울인지 확인
        LiquidParticleCollision drop = other.GetComponent<LiquidParticleCollision>();
        if (drop == null) return;

        // 재료 정보가 없는 방울은 무시 (예: 쉐이커 자신이 따른 완성 액체)
        if (drop.sourceIngredient == null) return;

        // 이 재료 +amountPerDrop
        if (ShakerManager.Instance != null)
        {
            ShakerManager.Instance.AddAmount(drop.sourceIngredient, amountPerDrop);
        }
    }
}