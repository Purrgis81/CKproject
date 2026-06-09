using System.Collections.Generic;

// 레시피 한 줄의 재료 (이름 + 목표 양 ml). 이름은 IngredientData.ingredientName 과 일치해야 함.
[System.Serializable]
public class RecipeIngredient
{
    public string ingredientName;
    public float amountMl;     // 이 재료를 정확히 이만큼(ml) 따라야 통과
    public RecipeIngredient(string n, float ml) { ingredientName = n; amountMl = ml; }
}

[System.Serializable]
public class Recipe
{
    public string name;
    public GlassType glass;
    public bool requiresIce;
    public List<RecipeIngredient> ingredients;
    public Recipe(string n, GlassType g, bool ice, List<RecipeIngredient> ing)
    { name = n; glass = g; requiresIce = ice; ingredients = ing; }
}

// 칵테일 50종 레시피 테이블 (각 재료를 정해진 ml만큼 따라야 함. 합이 100이라 한 잔 ≈ 100ml)
// 재료 이름 = 실제 IngredientData 이름(_S)
public static class RecipeDatabase
{
    public static List<Recipe> All = new List<Recipe>()
    {
        new Recipe("민들레주", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Dandelion_S", 46), new RecipeIngredient("Champagne_S", 54) }),
        new Recipe("나의 라임레몬나무", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Lime_S", 27), new RecipeIngredient("Lemon_S", 27), new RecipeIngredient("liqueur_S", 46) }),
        new Recipe("별이 그리는 밤", GlassType.Hurricane, false, new List<RecipeIngredient> { new RecipeIngredient("Starfruit_S", 30), new RecipeIngredient("Honey_S", 10), new RecipeIngredient("Blueberry_S", 30), new RecipeIngredient("Rum_S", 30) }),
        new Recipe("첫눈 오는 날", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Apple_S", 40), new RecipeIngredient("Champagne_S", 47), new RecipeIngredient("Mint_S", 13) }),
        new Recipe("황금빛 오후", GlassType.OnTheRocks, false, new List<RecipeIngredient> { new RecipeIngredient("Pineapple_S", 43), new RecipeIngredient("Maple_S", 14), new RecipeIngredient("Brandy_S", 43) }),
        new Recipe("붉은 낙조", GlassType.OnTheRocks, false, new List<RecipeIngredient> { new RecipeIngredient("Grapefruit_S", 50), new RecipeIngredient("Tequila_S", 50) }),
        new Recipe("복숭아 편지", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Peach_S", 38), new RecipeIngredient("Honey_S", 12), new RecipeIngredient("SparklingWater_S", 50) }),
        new Recipe("딸기밭에서", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Strawberry_S", 40), new RecipeIngredient("Lime_S", 20), new RecipeIngredient("Vodka_S", 40) }),
        new Recipe("초록 바람", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Mint_S", 15), new RecipeIngredient("Lime_S", 23), new RecipeIngredient("SparklingWater_S", 62) }),
        new Recipe("사라진 여름", GlassType.Hurricane, true, new List<RecipeIngredient> { new RecipeIngredient("Pineapple_S", 40), new RecipeIngredient("Rum_S", 40), new RecipeIngredient("Lemon_S", 20) }),
        new Recipe("블루문 소나타", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Blueberry_S", 30), new RecipeIngredient("Gin_S", 30), new RecipeIngredient("TonicWater_S", 40) }),
        new Recipe("레몬 서약", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Lemon_S", 27), new RecipeIngredient("Honey_S", 18), new RecipeIngredient("Vodka_S", 55) }),
        new Recipe("한낮의 안개", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Absinthe_S", 29), new RecipeIngredient("Mint_S", 14), new RecipeIngredient("SparklingWater_S", 57) }),
        new Recipe("스타더스트", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Starfruit_S", 37), new RecipeIngredient("Champagne_S", 44), new RecipeIngredient("Lemon_S", 19) }),
        new Recipe("달콤한 작별", GlassType.OnTheRocks, true, new List<RecipeIngredient> { new RecipeIngredient("Peach_S", 43), new RecipeIngredient("Maple_S", 14), new RecipeIngredient("Rum_S", 43) }),
        new Recipe("자몽 천국", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Grapefruit_S", 32), new RecipeIngredient("liqueur_S", 26), new RecipeIngredient("SparklingWater_S", 42) }),
        new Recipe("브랜디의 밤", GlassType.OnTheRocks, false, new List<RecipeIngredient> { new RecipeIngredient("Brandy_S", 55), new RecipeIngredient("Honey_S", 18), new RecipeIngredient("Lemon_S", 27) }),
        new Recipe("딸기 고백", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Strawberry_S", 46), new RecipeIngredient("Champagne_S", 54) }),
        new Recipe("파인애플 특급", GlassType.Margarita, false, new List<RecipeIngredient> { new RecipeIngredient("Pineapple_S", 40), new RecipeIngredient("Tequila_S", 40), new RecipeIngredient("Lime_S", 20) }),
        new Recipe("민들레 꿈", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Dandelion_S", 32), new RecipeIngredient("Honey_S", 10), new RecipeIngredient("Lemon_S", 16), new RecipeIngredient("SparklingWater_S", 42) }),
        new Recipe("사과 왈츠", GlassType.OnTheRocks, true, new List<RecipeIngredient> { new RecipeIngredient("Apple_S", 43), new RecipeIngredient("Brandy_S", 43), new RecipeIngredient("Maple_S", 14) }),
        new Recipe("여름밤 블루", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Blueberry_S", 26), new RecipeIngredient("Vodka_S", 26), new RecipeIngredient("Lime_S", 13), new RecipeIngredient("SparklingWater_S", 35) }),
        new Recipe("레몬 행진곡", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Lemon_S", 18), new RecipeIngredient("Gin_S", 35), new RecipeIngredient("TonicWater_S", 47) }),
        new Recipe("복숭아빛 노을", GlassType.Hurricane, false, new List<RecipeIngredient> { new RecipeIngredient("Peach_S", 33), new RecipeIngredient("Champagne_S", 39), new RecipeIngredient("Lime_S", 17), new RecipeIngredient("Honey_S", 11) }),
        new Recipe("럼의 전설", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Rum_S", 55), new RecipeIngredient("Lime_S", 27), new RecipeIngredient("Mint_S", 18) }),
        new Recipe("초록 미로", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Absinthe_S", 31), new RecipeIngredient("Starfruit_S", 46), new RecipeIngredient("Lime_S", 23) }),
        new Recipe("딸기 소풍", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Strawberry_S", 32), new RecipeIngredient("Mint_S", 10), new RecipeIngredient("Lemon_S", 16), new RecipeIngredient("SparklingWater_S", 42) }),
        new Recipe("황혼의 데킬라", GlassType.Margarita, false, new List<RecipeIngredient> { new RecipeIngredient("Tequila_S", 43), new RecipeIngredient("Grapefruit_S", 43), new RecipeIngredient("Maple_S", 14) }),
        new Recipe("파인애플 낙원", GlassType.Hurricane, false, new List<RecipeIngredient> { new RecipeIngredient("Pineapple_S", 29), new RecipeIngredient("liqueur_S", 24), new RecipeIngredient("Mint_S", 9), new RecipeIngredient("SparklingWater_S", 38) }),
        new Recipe("토닉 교향곡", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("TonicWater_S", 42), new RecipeIngredient("Gin_S", 32), new RecipeIngredient("Lemon_S", 16), new RecipeIngredient("Honey_S", 10) }),
        new Recipe("사과 모험", GlassType.OnTheRocks, false, new List<RecipeIngredient> { new RecipeIngredient("Apple_S", 40), new RecipeIngredient("Lime_S", 20), new RecipeIngredient("Rum_S", 40) }),
        new Recipe("블루베리 왕국", GlassType.Hurricane, true, new List<RecipeIngredient> { new RecipeIngredient("Blueberry_S", 40), new RecipeIngredient("Maple_S", 13), new RecipeIngredient("Champagne_S", 47) }),
        new Recipe("민들레 산책", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Dandelion_S", 38), new RecipeIngredient("Mint_S", 12), new RecipeIngredient("SparklingWater_S", 50) }),
        new Recipe("복숭아 탐정", GlassType.OnTheRocks, true, new List<RecipeIngredient> { new RecipeIngredient("Peach_S", 40), new RecipeIngredient("Brandy_S", 40), new RecipeIngredient("Lemon_S", 20) }),
        new Recipe("스타후르츠 전설", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Starfruit_S", 27), new RecipeIngredient("Vodka_S", 27), new RecipeIngredient("Honey_S", 9), new RecipeIngredient("SparklingWater_S", 37) }),
        new Recipe("자몽의 고백", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Grapefruit_S", 43), new RecipeIngredient("Vodka_S", 43), new RecipeIngredient("Mint_S", 14) }),
        new Recipe("레몬 소네트", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Lemon_S", 23), new RecipeIngredient("Maple_S", 15), new RecipeIngredient("SparklingWater_S", 62) }),
        new Recipe("딸기 혁명", GlassType.Margarita, true, new List<RecipeIngredient> { new RecipeIngredient("Strawberry_S", 40), new RecipeIngredient("Tequila_S", 40), new RecipeIngredient("Lime_S", 20) }),
        new Recipe("압생트의 속삭임", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Absinthe_S", 45), new RecipeIngredient("Honey_S", 22), new RecipeIngredient("Lemon_S", 33) }),
        new Recipe("파인애플 세레나데", GlassType.Hurricane, true, new List<RecipeIngredient> { new RecipeIngredient("Pineapple_S", 40), new RecipeIngredient("Champagne_S", 47), new RecipeIngredient("Mint_S", 13) }),
        new Recipe("블루베리 항해", GlassType.Hurricane, true, new List<RecipeIngredient> { new RecipeIngredient("Blueberry_S", 35), new RecipeIngredient("Rum_S", 35), new RecipeIngredient("Lime_S", 18), new RecipeIngredient("Maple_S", 12) }),
        new Recipe("민트 왕관", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Mint_S", 17), new RecipeIngredient("Champagne_S", 58), new RecipeIngredient("Lemon_S", 25) }),
        new Recipe("사과 비밀", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Apple_S", 30), new RecipeIngredient("Gin_S", 30), new RecipeIngredient("TonicWater_S", 40) }),
        new Recipe("복숭아 왈츠", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Peach_S", 46), new RecipeIngredient("liqueur_S", 39), new RecipeIngredient("Honey_S", 15) }),
        new Recipe("자몽 폭풍", GlassType.Highball, false, new List<RecipeIngredient> { new RecipeIngredient("Grapefruit_S", 26), new RecipeIngredient("Rum_S", 26), new RecipeIngredient("Lime_S", 13), new RecipeIngredient("SparklingWater_S", 35) }),
        new Recipe("스타후르츠 여명", GlassType.OnTheRocks, true, new List<RecipeIngredient> { new RecipeIngredient("Starfruit_S", 43), new RecipeIngredient("Maple_S", 14), new RecipeIngredient("Gin_S", 43) }),
        new Recipe("딸기 은하", GlassType.Highball, true, new List<RecipeIngredient> { new RecipeIngredient("Strawberry_S", 35), new RecipeIngredient("Vodka_S", 35), new RecipeIngredient("Honey_S", 12), new RecipeIngredient("Lemon_S", 18) }),
        new Recipe("민들레 기적", GlassType.Cocktail, false, new List<RecipeIngredient> { new RecipeIngredient("Dandelion_S", 43), new RecipeIngredient("Lime_S", 21), new RecipeIngredient("liqueur_S", 36) }),
        new Recipe("브랜디 협주곡", GlassType.OnTheRocks, false, new List<RecipeIngredient> { new RecipeIngredient("Brandy_S", 27), new RecipeIngredient("Apple_S", 27), new RecipeIngredient("Maple_S", 9), new RecipeIngredient("SparklingWater_S", 37) }),
        new Recipe("레몬 무법자", GlassType.Margarita, false, new List<RecipeIngredient> { new RecipeIngredient("Lemon_S", 27), new RecipeIngredient("Tequila_S", 55), new RecipeIngredient("Honey_S", 18) }),
    };
}