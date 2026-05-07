using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ShakerBodyController : MonoBehaviour
{
    void OnMouseDown()
    {
        var currentMode = ShakerStateMachine.Instance.currentMode;

        // [경우 1] 닫힘 상태에서 클릭 → 흔들기 시작!
        if (currentMode == ShakerMode.Closed)
        {
            Debug.Log("🍹 본체 클릭! 흔들기 시작!");
            ShakerStateMachine.Instance.SetMode(ShakerMode.Shaking);
            return;
        }

        // [경우 2] 따르기 대기 상태에서 클릭 → 따르기!
        if (currentMode == ShakerMode.ReadyToPour)
        {
            Debug.Log("🥤 본체 클릭! 따르기 시작!");
            ShakerStateMachine.Instance.SetMode(ShakerMode.Pouring);
            return;
        }

        // 그 외 상태에서는 무시
        Debug.Log($"⚠️ 현재 모드({currentMode})에서는 본체 클릭 무시");
    }
}