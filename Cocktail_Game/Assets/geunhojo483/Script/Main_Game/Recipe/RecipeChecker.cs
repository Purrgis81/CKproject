using UnityEngine;
using System.Collections.Generic;
using System.Text;

// 지금 만든 칵테일이 목표 레시피와 맞는지 판정하는 체커.
//   - 레시피 테이블(RecipeDatabase)에서 목표 하나를 골라 비교.
//   - 각 재료를 목표 ml ±오차 안에 따르면 통과. (옵션: 잔 종류, 얼음도 확인)
public class RecipeChecker : MonoBehaviour
{
    [Header("현재 목표 레시피")]
    [Tooltip("RecipeDatabase.All 의 몇 번째 (0부터)")]
    public int targetIndex = 0;

    [Header("판정 설정 (ml 기준)")]
    [Tooltip("각 재료 목표 ml의 ±몇 % 안이면 통과 (예: 0.2 = ±20%)")]
    public float toleranceRatio = 0.2f;
    [Tooltip("위 비율로 계산한 오차가 이 ml보다 작으면, 최소 이만큼은 봐줌 (적은 양 재료 보정)")]
    public float minToleranceMl = 3f;
    [Tooltip("이 양(ml) 이상 따라야 판정 시작")]
    public float minTotalAmount = 5f;
    [Tooltip("레시피에 없는 재료가 이 ml 이상 들어가면 실패 (그 미만은 실수로 보고 무시)")]
    public float ignoreExtraBelowMl = 4f;
    [Tooltip("잔 종류도 맞춰야 통과")]
    public bool checkGlass = true;
    [Tooltip("얼음 유무도 맞춰야 통과")]
    public bool checkIce = true;

    [Header("디버그")]
    public bool showUI = true;

    [Header("영수증에 표시")]
    [Tooltip("글씨를 띄울 영수증 Transform (비우면 화면 중앙)")]
    public Transform receiptTransform;
    [Tooltip("미세 위치 보정 (픽셀, +y는 아래로)")]
    public Vector2 screenOffset = Vector2.zero;
    [Tooltip("글씨 박스 너비 (픽셀)")]
    public float boxWidth = 320f;
    [Tooltip("글씨 크기")]
    public int fontSize = 22;

    [Header("글씨 색 (노란 종이에 잘 보이게)")]
    [Tooltip("기본 잉크색 (제목/일반 글씨)")]
    public Color inkColor = new Color(0.18f, 0.12f, 0.06f);   // 진한 갈색
    [Tooltip("비율이 맞은 재료 색")]
    public Color okColor = new Color(0.12f, 0.45f, 0.18f);    // 진한 초록
    [Tooltip("비율이 안 맞은 재료 색")]
    public Color badColor = new Color(0.65f, 0.16f, 0.13f);   // 진한 빨강

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

        // 1. 레시피 재료들이 각자 목표 ml ±오차 안에 있어야 함
        foreach (var ri in r.ingredients)
        {
            float cur = ShakerManager.Instance.GetAmountByName(ri.ingredientName);
            float tol = Mathf.Max(ri.amountMl * toleranceRatio, minToleranceMl);
            if (Mathf.Abs(cur - ri.amountMl) > tol) return false;
        }

        // 2. 레시피에 없는 재료가 (의미있는 ml) 들어가 있으면 실패
        foreach (var nm in ShakerManager.Instance.GetCurrentIngredientNames())
        {
            if (RecipeHas(r, nm)) continue;
            float cur = ShakerManager.Instance.GetAmountByName(nm);
            if (cur >= ignoreExtraBelowMl) return false;
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

    // 왜 불만족인지 첫 번째 이유를 글로 돌려줌 (디버그용)
    public string GetUnsatisfiedReason()
    {
        if (ShakerManager.Instance == null) return "ShakerManager 없음";
        Recipe r = Current;
        if (r == null) return "목표 레시피 없음";

        if (ShakerManager.Instance.GetTotalAmount() < minTotalAmount)
            return $"총량 부족 ({ShakerManager.Instance.GetTotalAmount():F0} < {minTotalAmount})";

        foreach (var ri in r.ingredients)
        {
            float cur = ShakerManager.Instance.GetAmountByName(ri.ingredientName);
            float tol = Mathf.Max(ri.amountMl * toleranceRatio, minToleranceMl);
            if (Mathf.Abs(cur - ri.amountMl) > tol)
                return $"양 안 맞음 → {ri.ingredientName}: {cur:F0}ml (목표 {ri.amountMl:F0}±{tol:F0}ml)";
        }

        foreach (var nm in ShakerManager.Instance.GetCurrentIngredientNames())
        {
            if (RecipeHas(r, nm)) continue;
            float cur = ShakerManager.Instance.GetAmountByName(nm);
            if (cur >= ignoreExtraBelowMl)
                return $"불필요한 재료 들어감 → {nm} {cur:F0}ml";
        }

        if (checkGlass && GlassController.Instance != null && GlassController.Instance.currentGlassType != r.glass)
            return $"잔 종류 다름 (현재 {GlassController.Instance.currentGlassType} / 목표 {r.glass})";

        if (checkIce && ShakerManager.Instance.hasIce != r.requiresIce)
            return $"얼음 다름 (현재 {ShakerManager.Instance.hasIce} / 목표 {r.requiresIce})";

        return "만족함";
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

        // 1. 표시할 내용을 리치텍스트 한 덩어리로
        string okHex = "#" + ColorUtility.ToHtmlStringRGB(okColor);
        string badHex = "#" + ColorUtility.ToHtmlStringRGB(badColor);

        StringBuilder sb = new StringBuilder();
        string iceText = r.requiresIce ? " +얼음" : "";
        sb.AppendLine($"<b>목표: {r.name}</b>");
        sb.AppendLine($"<size=80%>[{r.glass}{iceText}]</size>");

        foreach (var ri in r.ingredients)
        {
            float cur = ShakerManager.Instance.GetAmountByName(ri.ingredientName);
            float tol = Mathf.Max(ri.amountMl * toleranceRatio, minToleranceMl);
            bool ok = Mathf.Abs(cur - ri.amountMl) <= tol;
            string color = ok ? okHex : badHex;
            sb.AppendLine($"<color={color}>{ri.ingredientName}: {cur:F0}/{ri.amountMl:F0}ml</color>");
        }

        bool done = IsSatisfied();
        sb.Append(done ? $"<color={okHex}>✔ 완성!</color>" : "… 조합 중");

        string content = sb.ToString();

        // 2. 가운데 정렬 스타일
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = fontSize;
        s.richText = true;
        s.alignment = TextAnchor.UpperCenter;
        s.normal.textColor = inkColor;   // 기본 잉크색 (제목/조합중 등)
        s.wordWrap = true;

        float h = s.CalcHeight(new GUIContent(content), boxWidth);

        // 3. 영수증의 화면 좌표 구하기 (월드 → 화면, OnGUI는 y가 위→아래라 뒤집음)
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (receiptTransform != null && Camera.main != null)
        {
            Vector3 sp = Camera.main.WorldToScreenPoint(receiptTransform.position);
            center = new Vector2(sp.x, Screen.height - sp.y);
        }
        center += screenOffset;

        // 4. 그 중앙에 글씨 박스 배치
        Rect rect = new Rect(center.x - boxWidth / 2f, center.y - h / 2f, boxWidth, h);
        GUI.Label(rect, content, s);
    }
}