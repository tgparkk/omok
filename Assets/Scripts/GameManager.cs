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
    private string gameMode;
    private bool isPlayerFirst;
    private int computerDifficulty;

    private bool isComputerTurn = false;
    private bool isGameActive = true;

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

    // Update 메서드 수정
    void Update()
    {
        // 컴퓨터 턴 처리 - 이미 실행 중이면 중복 실행 방지
        if (gameMode == "Computer" && isComputerTurn && !resultPanel.activeSelf && !isProcessingComputerMove)
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

    // 클래스 변수로 추가
    private bool isProcessingComputerMove = false;

    // MakeComputerMoveWithDelay 수정
    IEnumerator MakeComputerMoveWithDelay()
    {
        isProcessingComputerMove = true;
        Debug.Log("컴퓨터가 수를 두기 위해 생각 중...");

        // 사람처럼 생각하는 시간을 주기 위한 지연
        yield return new WaitForSeconds(1.0f);

        if (resultPanel.activeSelf)
        {
            isProcessingComputerMove = false;
            yield break;
        }

        // 간단하게 랜덤으로 돌 놓기
        MakeRandomMove();

        Debug.Log("컴퓨터가 돌을 놓았습니다!");
        isProcessingComputerMove = false;

        // 이 부분이 중요: 플레이어 턴임을 명시적으로 표시
        // isComputerTurn = false; 이미 Update에서 false로 설정했으므로 중복 설정 불필요
    }

    // PlayerMoved 메서드 수정
    public void PlayerMoved()
    {
        if (gameMode == "Computer" && !resultPanel.activeSelf && !isComputerTurn && !isProcessingComputerMove)
        {
            Debug.Log("플레이어가 돌을 놓았습니다. 컴퓨터 턴으로 전환합니다.");
            isComputerTurn = true;
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
        // 선공/후공 및 난이도에 따른 설정

        if (!isPlayerFirst)
        {
            // 컴퓨터가 첫 수를 두도록 설정
            Debug.Log("컴퓨터가 선공합니다.");
            isComputerTurn = true;
        }

        // 난이도 설정
        Debug.Log("컴퓨터 난이도 " + computerDifficulty + "로 설정됨");
    }

    void MakeRandomMove()
    {
        int x, y;
        bool placed = false;
        int attempts = 0;

        // 빈 위치 찾을 때까지 랜덤 시도
        while (!placed && attempts < 100)
        {
            x = Random.Range(0, boardManager.boardSize);
            y = Random.Range(0, boardManager.boardSize);

            // boardManager의 IsCellEmpty 메서드 사용
            if (boardManager.IsCellEmpty(x, y))
            {
                Debug.Log("컴퓨터가 돌을 놓을 위치: (" + x + ", " + y + ")");
                placed = boardManager.PlaceStone(x, y);
            }

            attempts++;
        }

        // 최대 시도 횟수 초과하면 보드 전체 탐색
        if (!placed)
        {
            Debug.Log("랜덤 시도 실패, 보드 전체 탐색 중...");

            for (int i = 0; i < boardManager.boardSize; i++)
            {
                for (int j = 0; j < boardManager.boardSize; j++)
                {
                    if (boardManager.IsCellEmpty(i, j))
                    {
                        Debug.Log("최종 위치 탐색: (" + i + ", " + j + ")");
                        boardManager.PlaceStone(i, j);
                        return;
                    }
                }
            }
        }
    }

    // AI의 수를 두는 함수
    void MakeComputerMove()
    {
        // AI 난이도에 따라 다른 로직 적용
        switch (computerDifficulty)
        {
            case 1: // 쉬움: 랜덤 위치에 돌 놓기
                MakeRandomMove();
                break;
            case 2: // 중간: 약간의 전략 사용
                MakeMediumMove();
                break;
            case 3: // 어려움: 더 복잡한 전략 사용
                MakeHardMove();
                break;
            default:
                MakeRandomMove();
                break;
        }
    }

    // 중간 난이도 AI (공격/수비 기본 로직)
    void MakeMediumMove()
    {
        // 1. 자신이 4개 연속되었는지 확인 (승리 가능)
        if (TryToWin()) return;

        // 2. 상대가 4개 연속되었는지 확인 (방어 필요)
        if (TryToBlock()) return;

        // 3. 그 외 자신의 돌이 많은 위치 근처에 돌 놓기
        if (TryToExpandOwn()) return;

        // 4. 위 전략들이 실패하면 랜덤으로 놓기
        MakeRandomMove();
    }

    // 어려운 난이도 AI (더 복잡한 전략)
    void MakeHardMove()
    {
        // 기본적으로 중간 난이도와 동일한 전략에 더 복잡한 패턴 인식 추가
        if (TryToWin()) return;
        if (TryToBlock()) return;

        // 3-3 공격 패턴 시도 (열린 3이 두 개 만들어지는 위치)
        if (TryToCreateThreeThree()) return;

        // 중간 난이도 전략으로 대체
        if (TryToExpandOwn()) return;

        // 최후의 수단으로 랜덤 돌 놓기
        MakeRandomMove();
    }

    // 4개 연속된 돌을 발견하여 승리 가능한 위치에 돌 놓기
    bool TryToWin()
    {
        int computerStoneType = isPlayerFirst ? 2 : 1; // 플레이어가 흑(1)이면 컴퓨터는 백(2)

        // 승리 가능한 위치 찾기
        Vector2Int winPosition = boardManager.FindWinningMove(computerStoneType);

        if (winPosition.x != -1) // 유효한 위치를 찾았다면
        {
            boardManager.PlaceStone(winPosition.x, winPosition.y);
            return true;
        }

        return false;
    }

    // 상대방의 4개 연속된 돌을 막기
    bool TryToBlock()
    {
        int playerStoneType = isPlayerFirst ? 1 : 2; // 플레이어가 흑(1)이면 플레이어는 흑

        // 상대의 승리 가능한 위치 찾기
        Vector2Int blockPosition = boardManager.FindWinningMove(playerStoneType);

        if (blockPosition.x != -1) // 유효한 위치를 찾았다면
        {
            boardManager.PlaceStone(blockPosition.x, blockPosition.y);
            return true;
        }

        return false;
    }

    // 자신의 돌이 많은 위치 근처에 돌 놓기
    bool TryToExpandOwn()
    {
        int computerStoneType = isPlayerFirst ? 2 : 1; // 플레이어가 흑(1)이면 컴퓨터는 백(2)

        // 자신의 돌이 2~3개 연속된 위치 찾기
        Vector2Int expandPosition = boardManager.FindGoodMove(computerStoneType);

        if (expandPosition.x != -1) // 유효한 위치를 찾았다면
        {
            boardManager.PlaceStone(expandPosition.x, expandPosition.y);
            return true;
        }

        return false;
    }

    // 3-3 공격 패턴 만들기 (고급 전략)
    bool TryToCreateThreeThree()
    {
        // 난이도 3에서만 사용되는 고급 전략
        // 실제 구현은 복잡할 수 있으므로 여기서는 기본 구조만 제공
        // 두 군데 이상에서 열린 3을 만들 수 있는 위치 찾기

        // 구현을 간단하게 하기 위해 중간 난이도 전략 재사용
        return false;
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

        // 컴퓨터 모드이고 컴퓨터가 선공이면 컴퓨터의 첫 수 두기
        if (gameMode == "Computer" && !isPlayerFirst)
        {
            isComputerTurn = true;
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
        }
    }
}