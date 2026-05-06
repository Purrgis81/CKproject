using UnityEngine;

// 이 줄 덕분에 우클릭 메뉴에서 재료 데이터를 만들 수 있어요!
[CreateAssetMenu(fileName = "NewIngredient", menuName = "CoinForge/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("기본 정보")]
    public int ingredientID;           // 고유 번호 (1~23)
    public string ingredientName;      // 재료 이름 (예: "럼")

    [Header("시각 정보")]
    public Sprite ingredientSprite;    // 재료 스프라이트
    public Color liquidColor = Color.white;  // 액체 색 (나중에 사용)
}