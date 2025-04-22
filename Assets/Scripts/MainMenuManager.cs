using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 기존 패널들
    public GameObject gameModePanel;
    public GameObject mainMenuButtonsPanel;
    
    // 새 패널: 컴퓨터 설정
    public GameObject computerSettingsPanel;
    
    // 선택된 설정을 저장할 변수들
    private bool isPlayerFirst = true; // true: 플레이어 선공, false: 플레이어 후공
    private int computerDifficulty = 1; // 1, 2, 3 중 하나

    private void Start()
    {
        // 게임 시작 시 게임 모드 패널과 컴퓨터 설정 패널은 비활성화
        if (gameModePanel != null)
            gameModePanel.SetActive(false);
            
        if (computerSettingsPanel != null)
            computerSettingsPanel.SetActive(false);
    }

    public void StartGame()
    {
        // 메인 메뉴에서 시작 버튼 - 게임 모드 패널 활성화
        if (mainMenuButtonsPanel != null)
            mainMenuButtonsPanel.SetActive(false);
        
        if (gameModePanel != null)
            gameModePanel.SetActive(true);
    }

    public void PlayVsComputer()
    {
        // 컴퓨터랑 하기 선택 - 컴퓨터 설정 패널 활성화
        if (gameModePanel != null)
            gameModePanel.SetActive(false);
            
        if (computerSettingsPanel != null)
            computerSettingsPanel.SetActive(true);
        
        // 기본값으로 설정
        isPlayerFirst = true;
        computerDifficulty = 1;
    }

    // 선공/후공 설정
    public void SetPlayerFirst(bool isFirst)
    {
        isPlayerFirst = isFirst;
        Debug.Log("플레이어 " + (isFirst ? "선공" : "후공") + " 선택됨");
    }
    
    // 난이도 설정
    public void SetComputerDifficulty(int difficulty)
    {
        if (difficulty >= 1 && difficulty <= 3)
        {
            computerDifficulty = difficulty;
            Debug.Log("컴퓨터 난이도 " + difficulty + " 선택됨");
        }
    }
    
    // 컴퓨터 상대로 게임 시작
    public void StartComputerGame()
    {
        // 선택된 설정을 PlayerPrefs에 저장
        PlayerPrefs.SetString("GameMode", "Computer");
        PlayerPrefs.SetInt("IsPlayerFirst", isPlayerFirst ? 1 : 0);
        PlayerPrefs.SetInt("ComputerDifficulty", computerDifficulty);
        
        // 게임 씬 로드
        SceneManager.LoadScene("GameScene");
    }

    public void PlayVsOnline()
    {
        // 사용자(온라인)와 하기 모드 - 네트워크 관련 설정 및 게임 씬 로드
        PlayerPrefs.SetString("GameMode", "Online");
        SceneManager.LoadScene("GameScene");
    }

    public void PlayLocalMultiplayer()
    {
        // 로컬 대결 모드 - 로컬 멀티플레이어 설정 및 게임 씬 로드
        PlayerPrefs.SetString("GameMode", "Local");
        SceneManager.LoadScene("GameScene");
    }

    public void BackToMainMenu()
    {
        // 게임 모드 선택 패널에서 메인 메뉴로 돌아가기
        if (gameModePanel != null)
            gameModePanel.SetActive(false);
            
        if (computerSettingsPanel != null)
            computerSettingsPanel.SetActive(false);
        
        if (mainMenuButtonsPanel != null)
            mainMenuButtonsPanel.SetActive(true);
    }
    
    public void BackToGameModeSelect()
    {
        // 컴퓨터 설정 패널에서 게임 모드 선택으로 돌아가기
        if (computerSettingsPanel != null)
            computerSettingsPanel.SetActive(false);
            
        if (gameModePanel != null)
            gameModePanel.SetActive(true);
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