using UnityEngine;
using System.Collections.Generic;

// 게임 진행 상태
public enum ShakerState
{
    AddingIngredients,
    LidClosed,
    Shaking,
    Pouring,
    Done
}

public class ShakerManager : MonoBehaviour
{
    public static ShakerManager Instance;

    [Header("현재 상태")]
    public ShakerState currentState = ShakerState.AddingIngredients;

    [Header("게임 상태")]
    public List<IngredientData> addedIngredients = new List<IngredientData>();
    public bool hasIce = false;                          // ★ 추가
    public GlassType currentGlassType = GlassType.Cocktail;  // ★ 추가

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ===== 재료 관리 =====
    public void AddIngredient(IngredientData data)
    {
        addedIngredients.Add(data);
        Debug.Log($"재료 추가됨: {data.ingredientName} (현재 {addedIngredients.Count}개)");
    }

    // ===== 얼음 관리 (★ 추가) =====
    public void SetIce(bool ice)
    {
        hasIce = ice;
        Debug.Log($"🧊 얼음 상태: {(ice ? "추가됨" : "없음")}");
    }

    // ===== 잔 관리 (★ 추가) =====
    public void SetGlass(GlassType type)
    {
        currentGlassType = type;
        Debug.Log($"🥃 잔 종류: {type}");
    }

    // ===== 상태 변경 =====
    public void ChangeState(ShakerState newState)
    {
        currentState = newState;
        Debug.Log($"상태 변경: {newState}");
    }

    // ===== 전체 상태 리셋 (잔 던진 후 등) =====
    public void ResetAll()
    {
        addedIngredients.Clear();
        hasIce = false;
        currentState = ShakerState.AddingIngredients;
        Debug.Log("🔄 ShakerManager 전체 리셋");
    }
}