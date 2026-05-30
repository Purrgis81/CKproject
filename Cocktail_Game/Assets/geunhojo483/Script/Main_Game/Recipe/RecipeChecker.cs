using UnityEngine;
using System.Collections.Generic;

// 지금 만든 칵테일이 목표 레시피와 맞는지 판정하는 체커.
//   - 레시피 테이블(RecipeDatabase)에서 목표 하나를 골라 비교.
//   - 비율(%)을 허용 오차 안에서 맞추면 통과. (옵션: 잔 종류, 얼음도 확인)
public class RecipeChecker : MonoBehaviour
{
    [Header("현재 목표 레시피")]
    [Tooltip("RecipeDatabase.All 의 몇 번째 (0부터)")]
    public int targetIndex = 0;

    [Header("판정 설정")]
    [Tooltip("목표 ±이 값(%) 안이면 그 재료는 통과")]
    public float tolerancePercent = 8f;
    [Tooltip("이 양 이상 따라야 판정 시작")]
    public float minTotalAmount = 5f;
    [Tooltip("이 비율 미만으로 들어간 '레시피에 없는 재료'는 실수로 보고 무시")]
    public float ignoreExtraBelowPercent = 3f;
    [Tooltip("잔 종류도 맞춰야 통과")]
    public bool checkGlass = true;
    [Tooltip("얼음 유무도 맞춰야 통과")]
    public bool checkIce = true;

    [Header("디버그")]
    public bool showUI = true;

    public Recipe Current
    {
        get
        {
            if (RecipeDatabase.All == null || RecipeDatabase.All.Count == 0) return null;
            int idx = Mathf.Clamp(targetIndex, 0, RecipeDatabase.All.Count - 1);
            return RecipeDatabase.All[idx];
        }
    }

    // 목표 레시피를 만족하는가?
    public bool IsSatisfied()
    {
        if (ShakerManager.Instance == null) return false;
        Recipe r = Current;
        if (r == null) return false;

        if (ShakerManager.Instance.GetTotalAmount() < minTotalAmount) return false;

        // 1. 레시피 재료들이 목표 비율 ±오차 안에 있어야 함
        foreach (var ri in r.ingredients)
        {
            float cur = ShakerManager.Instance.GetPercentageByName(ri.ingredientName);
            if (Mathf.Abs(cur - ri.percent) > tolerancePercent) return false;
        }

        // 2. 레시피에 없는 재료가 (의미있는 양) 들어가 있으면 실패
        foreach (var nm in ShakerManager.Instance.GetCurrentIngredientNames())
        {
            if (RecipeHas(r, nm)) continue;
            float cur = ShakerManager.Instance.GetPercentageByName(nm);
            if (cur >= ignoreExtraBelowPercent) return false;
        }

        // 3. 잔 종류 (옵션)
        if (checkGlass && GlassController.Instance != null)
        {
            if (GlassController.Instance.currentGlassType != r.glass) return false;
        }

        // 4. 얼음 (옵션)
        if (checkIce && ShakerManager.Instance.hasIce != r.requiresIce) return false;

        return true;
    }

    bool RecipeHas(Recipe r, string name)
    {
        foreach (var ri in r.ingredients)
        {
            if (ri.ingredientName == name) return true;
        }
        return false;
    }

    void OnGUI()
    {
        if (!showUI) return;
        if (ShakerManager.Instance == null) return;
        Recipe r = Current;
        if (r == null) return;

        float x = 320f;
        int y = 10;

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = 22; title.normal.textColor = Color.white;

        string iceText = r.requiresIce ? " + 얼음" : "";
        GUI.Label(new Rect(x, y, 420, 30), $"목표: {r.name}  [{r.glass}{iceText}]", title);
        y += 30;

        foreach (var ri in r.ingredients)
        {
            float cur = ShakerManager.Instance.GetPercentageByName(ri.ingredientName);
            bool ok = Mathf.Abs(cur - ri.percent) <= tolerancePercent;

            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 20;
            s.normal.textColor = ok ? Color.green : new Color(1f, 0.5f, 0.5f);

            GUI.Label(new Rect(x, y, 420, 28),
                $"{ri.ingredientName}: {cur:F0}% / 목표 {ri.percent:F0}%", s);
            y += 26;
        }

        GUIStyle res = new GUIStyle(GUI.skin.label);
        res.fontSize = 24;
        bool done = IsSatisfied();
        res.normal.textColor = done ? Color.green : Color.yellow;
        GUI.Label(new Rect(x, y + 6, 420, 36), done ? "✔ 레시피 완성!" : "… 조합 중", res);
    }
}