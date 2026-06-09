using UnityEngine;

// 얼음 오브젝트에 붙임. 클릭하면 "쉐이커에 얼음 넣었다"고 체크(ShakerManager.hasIce = true).
//   - 서빙하면 ShakerManager.ResetAll()에서 hasIce가 다시 false로 꺼져요.
//   - ★ 이 오브젝트엔 액체 따르기/받이 스크립트(IngredientPourController 등)를 붙이지 마세요.
//     얼음은 ml로 안 세고 '깃발(있다/없다)'로만 처리하니까, 액체로 들어가면 '불필요한 재료'로 실패해요.
//   - 클릭 받으려면 이 오브젝트에 Collider2D 가 있어야 해요.
[RequireComponent(typeof(Collider2D))]
public class IceClickToAdd : MonoBehaviour
{
    [Header("옵션")]
    [Tooltip("얼음이 들어가면 이 오브젝트 보이기를 끔 (다음 손님 때 자동으로 다시 켜짐)")]
    public bool hideWhenInShaker = false;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (ShakerManager.Instance == null)
        {
            Debug.LogWarning("ShakerManager.Instance 가 없어요. 씬에 ShakerManager가 있는지 확인!");
            return;
        }

        ShakerManager.Instance.hasIce = true;
        Debug.Log("🧊 얼음 투입! (hasIce = true)");
    }

    // hasIce 상태에 맞춰 보이기/숨기기 (켜고 끄는 동작을 따로 안 해도 리셋되면 다시 보임)
    void Update()
    {
        if (!hideWhenInShaker || sr == null || ShakerManager.Instance == null) return;
        sr.enabled = !ShakerManager.Instance.hasIce;
    }
}