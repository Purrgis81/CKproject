using UnityEngine;
using System.Collections.Generic;

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
    public bool hasIce = false;
    public GlassType currentGlassType = GlassType.Cocktail;

    [Header("디버그")]
    [Tooltip("화면에 현재 재료 구성(%)을 표시")]
    public bool showCompositionUI = true;

    // 재료별 따라진 양 누적 (재료 → 양)
    private Dictionary<IngredientData, float> ingredientAmounts = new Dictionary<IngredientData, float>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ===== 재료 양 추가 =====
    public void AddAmount(IngredientData data, float amount)
    {
        if (data == null) return;

        if (!ingredientAmounts.ContainsKey(data))
        {
            ingredientAmounts[data] = 0f;
            if (!addedIngredients.Contains(data))
            {
                addedIngredients.Add(data);
                Debug.Log($"재료 추가됨: {data.ingredientName}");
            }
        }
        ingredientAmounts[data] += amount;
    }

    public float GetAmount(IngredientData data)
    {
        if (data == null) return 0f;
        if (!ingredientAmounts.ContainsKey(data)) return 0f;
        return ingredientAmounts[data];
    }

    public float GetTotalAmount()
    {
        float total = 0f;
        foreach (var pair in ingredientAmounts) total += pair.Value;
        return total;
    }

    public float GetPercentage(IngredientData data)
    {
        float total = GetTotalAmount();
        if (total <= 0f) return 0f;
        return GetAmount(data) / total * 100f;
    }

    // ★ 이름으로 비율 조회 (레시피 판정용 - 같은 이름끼리 합산)
    public float GetPercentageByName(string ingredientName)
    {
        float total = GetTotalAmount();
        if (total <= 0f) return 0f;

        float sum = 0f;
        foreach (var pair in ingredientAmounts)
        {
            if (pair.Key != null && pair.Key.ingredientName == ingredientName)
                sum += pair.Value;
        }
        return sum / total * 100f;
    }

    // ★ 이름으로 절대량(ml) 조회 (레시피 ml 판정용 - 같은 이름끼리 합산)
    public float GetAmountByName(string ingredientName)
    {
        float sum = 0f;
        foreach (var pair in ingredientAmounts)
        {
            if (pair.Key != null && pair.Key.ingredientName == ingredientName)
                sum += pair.Value;
        }
        return sum;
    }

    // ★ 지금 들어가 있는 재료 이름 목록 (양이 0보다 큰 것)
    public List<string> GetCurrentIngredientNames()
    {
        List<string> names = new List<string>();
        foreach (var pair in ingredientAmounts)
        {
            if (pair.Value > 0f && pair.Key != null && !names.Contains(pair.Key.ingredientName))
                names.Add(pair.Key.ingredientName);
        }
        return names;
    }

    public void LogComposition()
    {
        float total = GetTotalAmount();
        Debug.Log($"=== 현재 조합 (총량 {total:F0}) ===");
        foreach (var pair in ingredientAmounts)
        {
            float pct = 0f;
            if (total > 0f) pct = pair.Value / total * 100f;
            string name = "?";
            if (pair.Key != null) name = pair.Key.ingredientName;
            Debug.Log($"   {name}: {pct:F0}% (양 {pair.Value:F0})");
        }
    }

    // 호환용
    public void AddIngredient(IngredientData data) { AddAmount(data, 0f); }

    public void SetIce(bool ice)
    {
        hasIce = ice;
        Debug.Log($"🧊 얼음 상태: {(ice ? "추가됨" : "없음")}");
    }

    public void SetGlass(GlassType type)
    {
        currentGlassType = type;
        Debug.Log($"🥃 잔 종류: {type}");
    }

    public void ChangeState(ShakerState newState)
    {
        currentState = newState;
        Debug.Log($"상태 변경: {newState}");
    }

    public void ResetAll()
    {
        addedIngredients.Clear();
        ingredientAmounts.Clear();
        hasIce = false;
        currentState = ShakerState.AddingIngredients;
        Debug.Log("🔄 ShakerManager 전체 리셋");
    }

    void OnGUI()
    {
        if (!showCompositionUI) return;

        float total = GetTotalAmount();
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        int y = 10;
        GUI.Label(new Rect(10, y, 500, 30), $"총량: {total:F0}", style);
        y += 28;

        foreach (var pair in ingredientAmounts)
        {
            float pct = 0f;
            if (total > 0f) pct = pair.Value / total * 100f;
            string name = "?";
            if (pair.Key != null) name = pair.Key.ingredientName;
            GUI.Label(new Rect(10, y, 500, 30), $"{name}: {pct:F0}%  (양 {pair.Value:F0})", style);
            y += 28;
        }
    }
}