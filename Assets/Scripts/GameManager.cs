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
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        
        // 패널 초기 상태 설정
        resultPanel.SetActive(false);
        
        // 버튼 이벤트 연결
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
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
    }
    
    // 메인 메뉴로 돌아가기
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}