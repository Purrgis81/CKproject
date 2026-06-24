using UnityEngine;
using UnityEngine.SceneManagement;
using System;

// 게임 전체 흐름 관리: 60초 타이머 → 0이 되면 게임 종료(엔드).
//   - 재시작 / 나가기 기능 제공.
//   - 남은 시간 비율(0~1)을 TimerBar 같은 비주얼이 읽어감.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("타이머")]
    [Tooltip("제한 시간(초)")]
    public float totalTime = 60f;

    [Header("상태 (읽기용)")]
    public float timeLeft;
    public bool isPlaying = false;

    // 남은 시간 비율 0~1 (1=가득, 0=끝). 타이머 바가 이걸 읽음.
    public float TimeRatio
    {
        get { return (totalTime <= 0f) ? 0f : Mathf.Clamp01(timeLeft / totalTime); }
    }

    // 게임이 끝났을 때 불릴 이벤트 (UI 패널 등이 구독)
    public event Action OnGameEnd;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!isPlaying) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame();
        }
    }

    public void StartGame()
    {
        timeLeft = totalTime;
        isPlaying = true;
        Debug.Log($"▶ 게임 시작! 제한 {totalTime:F0}초");
    }

    void EndGame()
    {
        isPlaying = false;
        Debug.Log("⏱ 시간 종료 — 게임 엔드!");
        OnGameEnd?.Invoke();   // 구독자(엔드 화면 등)에게 알림
    }

    // 같은 씬을 처음부터 다시 로드 = 재시작
    public void Restart()
    {
        Time.timeScale = 1f;   // 혹시 멈춰뒀으면 풀기
        Scene cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }

    // 게임 나가기 (빌드에서는 종료, 에디터에서는 플레이 중지)
    public void QuitGame()
    {
        Debug.Log("게임 종료(나가기)");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 메뉴 등 다른 씬으로 나가고 싶을 때 (씬 이름으로)
    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}