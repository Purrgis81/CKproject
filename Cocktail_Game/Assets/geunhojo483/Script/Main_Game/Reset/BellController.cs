using UnityEngine;

// 종(벨) 오브젝트에 붙이세요. 클릭하면 바 전체를 처음 상태로 리셋합니다.
//   - 재료 양/구성(%) 초기화
//   - 쉐이커 본체 + 뚜껑 처음 상태로
//   - 재료(병)들 제자리로 (ResettableTransform 붙은 것들)
//   - 잔은 "지금 선택된 종류 그대로" 깨끗하게 다시 생성 (1번으로 안 돌아감!)
[RequireComponent(typeof(Collider2D))]
public class BellController : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("🔔 종! 전체 리셋!");

        // 1. 재료 양/구성(%) 초기화
        if (ShakerManager.Instance != null)
        {
            ShakerManager.Instance.ResetAll();
        }

        // 2. 쉐이커 본체 + 뚜껑 처음 상태로 (뚜껑도 이 안에서 제자리로 감)
        if (ShakerStateMachine.Instance != null)
        {
            ShakerStateMachine.Instance.ResetAndReappear();
        }

        // 3. 잔 리셋 — 현재 선택된 종류 유지!
        if (GlassController.Instance != null)
        {
            GlassController.Instance.ResetGlass();
        }

        // 4. 재료(병)들 제자리로
        ResettableTransform[] resettables =
            Object.FindObjectsByType<ResettableTransform>(FindObjectsSortMode.None);

        foreach (var item in resettables)
        {
            item.ResetToStart();
        }

        Debug.Log($"   → 재료 {resettables.Length}개 제자리로 복귀");
    }
}