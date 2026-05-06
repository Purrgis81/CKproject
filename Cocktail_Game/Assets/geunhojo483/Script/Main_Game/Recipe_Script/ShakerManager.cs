using UnityEngine;
using System.Collections.Generic;

// 게임 진행 상태를 표현하는 enum
public enum ShakerState
{
    AddingIngredients,  // 재료 투입 단계
    LidClosed,          // 뚜껑 덮인 상태
    Shaking,            // 흔드는 중
    Pouring,            // 잔에 따르는 중
    Done                // 완성
}

public class ShakerManager : MonoBehaviour
{
    // 싱글톤 패턴: 다른 스크립트에서 ShakerManager.Instance로 쉽게 접근 가능
    public static ShakerManager Instance;

    [Header("현재 상태")]
    public ShakerState currentState = ShakerState.AddingIngredients;

    // 들어간 재료 목록
    public List<IngredientData> addedIngredients = new List<IngredientData>();

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 재료가 들어왔을 때 호출됨 (Ingredient.cs에서 부름)
    public void AddIngredient(IngredientData data)
    {
        addedIngredients.Add(data);
        Debug.Log($"재료 추가됨: {data.ingredientName} (현재 {addedIngredients.Count}개)");
    }

    // 상태 변경용 함수
    public void ChangeState(ShakerState newState)
    {
        currentState = newState;
        Debug.Log($"상태 변경: {newState}");
    }
}