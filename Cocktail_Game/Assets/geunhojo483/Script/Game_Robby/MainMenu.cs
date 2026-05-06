using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환을 위해 꼭 필요!

public class MainMenu : MonoBehaviour
{
    // 게임 시작 버튼이 호출할 함수
    public void OnClickStartGame()
    {
        // "Main_Game" 씬으로 이동
        SceneManager.LoadScene("Main_Game");
    }

    // 게임 종료 버튼이 호출할 함수
    public void OnClickQuitGame()
    {
        // 에디터에서 실행 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 빌드된 실제 게임에서 실행될 때
            Application.Quit();
#endif
    }
}