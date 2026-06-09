using UnityEngine;

// 마우스가 재료 위에 올라가면, 그 재료의 IngredientData.ingredientName을
// 마우스 옆에 OnGUI로 표시하는 툴팁 매니저.
//   - 씬에 빈 오브젝트 하나 만들고 이 스크립트만 붙이면 됨 (재료마다 X).
//   - 재료에 Collider2D + IngredientPourController(ingredientData 연결)가 있어야 인식됨.
public class IngredientTooltipManager : MonoBehaviour
{
    [Header("스타일")]
    public int fontSize = 18;
    public Color textColor = Color.white;
    [Tooltip("글씨 뒤 어두운 배경 (가독성)")]
    public bool showBackground = true;
    [Tooltip("마우스로부터 글씨 간격 (픽셀)")]
    public Vector2 offset = new Vector2(16f, 8f);

    private string hoveredName = null;
    private Camera cam;
    private Texture2D bgTex;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        hoveredName = null;
        if (cam == null) { cam = Camera.main; if (cam == null) return; }

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 밑에 겹친 콜라이더들 중에서 '재료'를 찾음
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
        foreach (Collider2D h in hits)
        {
            IngredientPourController ipc = h.GetComponentInParent<IngredientPourController>();
            if (ipc != null && ipc.ingredientData != null)
            {
                hoveredName = ipc.ingredientData.ingredientName;
                break;
            }
        }
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(hoveredName)) return;

        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = fontSize;
        s.normal.textColor = textColor;

        // OnGUI는 y가 위→아래, Input.mousePosition은 아래→위라 뒤집어줌
        float mx = Input.mousePosition.x;
        float my = Screen.height - Input.mousePosition.y;

        Vector2 size = s.CalcSize(new GUIContent(hoveredName));
        float padX = 6f, padY = 3f;

        Rect box = new Rect(mx + offset.x, my + offset.y,
                            size.x + padX * 2f, size.y + padY * 2f);

        // 화면 밖으로 안 나가게 가두기 (잘림 방지)
        box.x = Mathf.Clamp(box.x, 0f, Mathf.Max(0f, Screen.width - box.width));
        box.y = Mathf.Clamp(box.y, 0f, Mathf.Max(0f, Screen.height - box.height));

        if (showBackground)
        {
            if (bgTex == null) bgTex = Texture2D.whiteTexture;
            Color prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(box, bgTex);
            GUI.color = prev;
        }

        GUI.Label(new Rect(box.x + padX, box.y + padY, size.x, size.y), hoveredName, s);
    }
}