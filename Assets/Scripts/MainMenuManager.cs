using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // 게임 씬으로 전환 (GameScene은 게임 씬의 이름입니다)
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        // 설정 패널 열기 기능 (나중에 구현)
        Debug.Log("설정 메뉴 열기");
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}