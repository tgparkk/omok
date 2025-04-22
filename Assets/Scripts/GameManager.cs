using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button restartButton;
    public Button mainMenuButton;
    
    private BoardManager boardManager;
    private string gameMode;
    private bool isPlayerFirst;
    private int computerDifficulty;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        
        // 패널 초기 상태 설정
        resultPanel.SetActive(false);
        
        // 버튼 이벤트 연결
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        // 설정 로드
        LoadGameSettings();
        
        // 게임 모드에 따라 설정
        SetupGameMode();
    }
    
    void LoadGameSettings()
    {
        gameMode = PlayerPrefs.GetString("GameMode", "Local");
        isPlayerFirst = PlayerPrefs.GetInt("IsPlayerFirst", 1) == 1;
        computerDifficulty = PlayerPrefs.GetInt("ComputerDifficulty", 1);
        
        Debug.Log("게임 모드: " + gameMode);
        if (gameMode == "Computer")
        {
            Debug.Log("플레이어 " + (isPlayerFirst ? "선공" : "후공"));
            Debug.Log("컴퓨터 난이도: " + computerDifficulty);
        }
    }
    
    void SetupGameMode()
    {
        switch(gameMode)
        {
            case "Computer":
                // AI 모드 설정
                Debug.Log("컴퓨터랑 하기 모드 활성화");
                SetupComputerGame();
                break;
                
            case "Online":
                // 온라인 모드 설정
                Debug.Log("사용자와 하기 모드 활성화");
                break;
                
            case "Local":
                // 로컬 멀티플레이어 설정
                Debug.Log("로컬 대결 모드 활성화");
                break;
                
            default:
                // 기본값: 로컬 모드
                Debug.Log("기본 로컬 모드 활성화");
                break;
        }
    }
    
    void SetupComputerGame()
    {
        // 컴퓨터 모드 설정
        // 여기서 선공/후공 및 난이도에 따른 설정
        
        if (!isPlayerFirst)
        {
            // 컴퓨터가 첫 수를 두도록 설정
            Debug.Log("컴퓨터가 선공합니다.");
            // (아직 구현되지 않은) AI 로직 호출
            // MakeComputerMove();
        }
        
        // 난이도 설정 - 현재는 로그만 출력
        Debug.Log("컴퓨터 난이도 " + computerDifficulty + "로 설정됨");
        // 나중에 실제 AI 난이도 조정 로직 추가
    }
    
    // (아직 구현되지 않은) AI의 수를 두는 함수
    void MakeComputerMove()
    {
        // TODO: 컴퓨터 AI 로직 구현
        // 예: boardManager.PlaceStone(x, y);
    }
    
    // BoardManager에서 호출할 승리 함수
    public void ShowVictory(bool isBlackWin)
    {
        string winner = isBlackWin ? "흑돌" : "백돌";
        resultText.text = winner + " 승리!";
        
        // 결과 패널 표시
        resultPanel.SetActive(true);
    }
    
    // 게임 재시작
    public void RestartGame()
    {
        boardManager.ResetBoard();
        resultPanel.SetActive(false);
        
        // 컴퓨터 모드이고 컴퓨터가 선공이면 컴퓨터의 첫 수 두기
        if (gameMode == "Computer" && !isPlayerFirst)
        {
            // MakeComputerMove();
        }
    }
    
    // 메인 메뉴로 돌아가기
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}