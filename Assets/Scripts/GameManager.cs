using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button restartButton;
    public Button mainMenuButton;

    private BoardManager boardManager;
    private AIManager aiManager;
    private string gameMode;
    private bool isPlayerFirst;
    private int computerDifficulty;

    private bool isComputerTurn = false;
    private bool isGameActive = true;
    private bool isProcessingComputerMove = false;
    private bool isFirstMove = true; // 첫 번째 수인지 확인하기 위한 변수

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

        // AIManager
        InitializeAIManager();

        // 게임 모드에 따라 설정
        SetupGameMode();

        // 시작 시 지연 후 컴퓨터가 선공인 경우 첫 수 두기
        if (gameMode == "Computer" && !isPlayerFirst)
        {
            Debug.Log("컴퓨터가 선공입니다. 첫 수를 둡니다.");
            // 약간의 지연 후 컴퓨터 턴 실행 (UI가 완전히 로드된 후)
            StartCoroutine(DelayedComputerFirstMove());
        }
    }

    // 컴퓨터 첫 수를 위한 지연 코루틴
    IEnumerator DelayedComputerFirstMove()
    {
        yield return new WaitForSeconds(0.5f);
        isComputerTurn = true;
    }

    // AIManager 초기화
    void InitializeAIManager()
    {
        if (gameMode == "Computer")
        {
            aiManager = gameObject.AddComponent<AIManager>();
            aiManager.Initialize(boardManager, computerDifficulty);
            Debug.Log("AIManager 초기화됨 - 난이도: " + computerDifficulty);
        }
    }

    // Update 메서드 수정
    void Update()
    {
        // 컴퓨터 턴 처리 - 이미 실행 중이면 중복 실행 방지
        if (gameMode == "Computer" && isComputerTurn && !resultPanel.activeSelf && !isProcessingComputerMove && isGameActive)
        {
            Debug.Log("컴퓨터 턴 감지!");
            StartCoroutine(MakeComputerMoveWithDelay());
            isComputerTurn = false; // 컴퓨터 턴 종료
        }
    }

    public bool IsComputerTurn()
    {
        // 컴퓨터 모드이고 현재 컴퓨터 턴인 경우 true 반환
        return gameMode == "Computer" && (isComputerTurn || isProcessingComputerMove);
    }

    // 컴퓨터의 수를 두는 코루틴
    IEnumerator MakeComputerMoveWithDelay()
    {
        isProcessingComputerMove = true;
        Debug.Log("컴퓨터가 수를 두기 위해 생각 중...");

        // 사람처럼 생각하는 시간을 주기 위한 지연
        yield return new WaitForSeconds(1.0f);

        if (resultPanel.activeSelf || !isGameActive)
        {
            isProcessingComputerMove = false;
            yield break;
        }

        Vector2Int movePosition;
        // 컴퓨터 돌 타입 결정 (플레이어가 흑(1)이면 컴퓨터는 백(2))
        int computerStoneType = isPlayerFirst ? 2 : 1;

        // 첫 번째 수일 경우 중앙 또는 중앙 근처에 놓기
        if (isFirstMove)
        {
            movePosition = GetStrategicFirstMove();
            isFirstMove = false;
        }
        else
        {
            // AIManager를 통해 다음 수 계산
            movePosition = aiManager.GetNextMove(computerStoneType);
        }

        // 유효한 위치라면 돌 놓기
        if (movePosition.x >= 0 && movePosition.y >= 0)
        {
            // 다른 메서드가 실행되지 않도록 직접 BoardManager의 돌 놓기 메서드 호출
            bool success = boardManager.PlaceStoneAI(movePosition.x, movePosition.y, !isPlayerFirst);
            if (!success)
            {
                Debug.LogError("컴퓨터가 돌을 놓는데 실패했습니다: (" + movePosition.x + ", " + movePosition.y + ")");
            }
        }

        Debug.Log("컴퓨터가 돌을 놓았습니다!");
        isProcessingComputerMove = false;
    }

    // 전략적인 첫 수 선택
    private Vector2Int GetStrategicFirstMove()
    {
        int boardSize = boardManager.boardSize;
        int center = boardSize / 2;

        // 중앙에 돌 놓기
        if (boardManager.IsCellEmpty(center, center))
        {
            Debug.Log("컴퓨터의 첫 수: 중앙 (" + center + ", " + center + ")");
            return new Vector2Int(center, center);
        }

        // 중앙이 이미 차있으면 중앙 주변에 놓기
        int[,] offsets = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int x = center + offsets[i, 0];
            int y = center + offsets[i, 1];

            if (x >= 0 && x < boardSize && y >= 0 && y < boardSize && boardManager.IsCellEmpty(x, y))
            {
                Debug.Log("컴퓨터의 첫 수: 중앙 근처 (" + x + ", " + y + ")");
                return new Vector2Int(x, y);
            }
        }

        // 중앙과 그 주변이 모두 차있으면 기본 AI 전략 사용
        Debug.Log("컴퓨터의 첫 수: 기본 AI 전략 사용");

        // 컴퓨터 돌 타입 결정
        int computerStoneType = isPlayerFirst ? 2 : 1;
        return aiManager.GetNextMove(computerStoneType);
    }

    // 플레이어가 돌을 놓았을 때 호출되는 함수
    public void PlayerMoved()
    {
        if (gameMode == "Computer" && !resultPanel.activeSelf && !isComputerTurn && !isProcessingComputerMove && isGameActive)
        {
            Debug.Log("플레이어가 돌을 놓았습니다. 컴퓨터 턴으로 전환합니다.");
            isComputerTurn = true;

            // 첫 수가 아님을 표시
            isFirstMove = false;
        }
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
        switch (gameMode)
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
        // 선공/후공 정보는 BoardManager에 전달
        boardManager.SetTurnOrder(isPlayerFirst);

        // 난이도 설정
        Debug.Log("컴퓨터 난이도 " + computerDifficulty + "로 설정됨");

        // 첫 수 여부 초기화
        isFirstMove = true;
    }

    // 난이도 변경 함수 (필요시 실시간으로 난이도 변경 가능)
    public void SetComputerDifficulty(int difficulty)
    {
        computerDifficulty = difficulty;
        if (aiManager != null)
        {
            aiManager.SetDifficulty(difficulty);
            Debug.Log("컴퓨터 난이도가 " + difficulty + "로 변경되었습니다.");
        }
    }

    // BoardManager에서 호출할 승리 함수
    public void ShowVictory(bool isBlackWin)
    {
        isGameActive = false; // 게임 종료

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
        isGameActive = true;
        isFirstMove = true; // 첫 수 여부 재설정

        // 컴퓨터 모드이고 컴퓨터가 선공이면 컴퓨터의 첫 수 두기
        if (gameMode == "Computer")
        {
            boardManager.SetTurnOrder(isPlayerFirst); // 턴 순서 다시 설정

            if (!isPlayerFirst)
            {
                // 약간의 지연 후 컴퓨터 턴 실행
                StartCoroutine(DelayedComputerFirstMove());
            }
        }
    }

    // 메인 메뉴로 돌아가기
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 플레이어가 돌을 두면 컴퓨터 턴으로 변경하는 함수
    public void OnPlayerPlacedStone()
    {
        if (gameMode == "Computer" && isGameActive)
        {
            isComputerTurn = true;
            isFirstMove = false; // 첫 수가 아님을 표시
        }
    }
}