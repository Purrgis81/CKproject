using UnityEngine;
using TMPro;

// 점수 관리 + 화면 표시.
//   - AddScore(점수)로 점수를 더하고, 연결된 TextMeshPro에 표시.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("표시")]
    [Tooltip("점수를 보여줄 TextMeshPro (예: SCORE 글씨)")]
    public TMP_Text scoreText;
    [Tooltip("앞에 붙일 글자 (예: 'SCORE:')")]
    public string prefix = "SCORE:";
    [Tooltip("0을 채워 몇 자리로 보일지")]
    public int padDigits = 10;

    [Header("상태 (읽기용)")]
    public int score = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateText();
    }

    // 점수 더하기 (음수면 깎임, 0 밑으로는 안 내려감)
    public void AddScore(int amount)
    {
        score += amount;
        if (score < 0) score = 0;
        UpdateText();
        Debug.Log($"🏆 점수 {(amount >= 0 ? "+" : "")}{amount} → 총 {score}");
    }

    public void ResetScore()
    {
        score = 0;
        UpdateText();
    }

    void UpdateText()
    {
        if (scoreText != null)
            scoreText.text = prefix + score.ToString().PadLeft(padDigits, '0');
    }
}