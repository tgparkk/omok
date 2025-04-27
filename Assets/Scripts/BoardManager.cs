// BoardManager.cs
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 15; // 오목판 크기 (15x15)
    public GameObject blackStonePrefab; // 흑돌 프리팹
    public GameObject whiteStonePrefab; // 백돌 프리팹

    private int[,] boardState; // 0: 빈칸, 1: 흑돌, 2: 백돌
    private bool isBlackTurn = true; // 현재 턴 (true: 흑돌, false: 백돌)

    private bool isGameOver = false;
    private bool isPlayerFirst = true; // 플레이어가 선공(흑돌)인지 여부
    private bool isComputerGame = false; // 컴퓨터 대전 모드인지 여부

    void Start()
    {
        InitializeBoard();
    }

    // 오목판 초기화
    void InitializeBoard()
    {
        // 바둑판 색상을 좀 더 갈색계열로 변경
        Color boardColor = new Color(0.87f, 0.72f, 0.53f); // 좀 더 따뜻한 갈색 계열

        // 배경 색상 변경 (카메라 배경색 변경)
        Camera.main.backgroundColor = boardColor;

        // 보드 크기 초기화 (교차점 개수)
        boardState = new int[boardSize, boardSize];

        // 배경 판 생성 (하나의 큰 스프라이트로)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(transform);
        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();

        // 단색 스프라이트 생성
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, boardColor); // 바둑판 색상
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        bgRenderer.sprite = sprite;

        // 배경 크기 설정 (보드보다 약간 크게)
        background.transform.localScale = new Vector3(boardSize + 1, boardSize + 1, 1);
        background.transform.position = new Vector3((boardSize - 1) / 2f, (boardSize - 1) / 2f, 0.1f);

        // 격자선 생성 (가로)
        for (int y = 0; y < boardSize; y++)
        {
            // 가운데 가로선은 더 굵게
            float lineWidth = (y == (boardSize - 1) / 2) ? 0.06f : 0.03f;
            DrawLine(new Vector3(0, y, -0.05f), new Vector3(boardSize - 1, y, -0.05f), lineWidth);
        }

        // 격자선 생성 (세로)
        for (int x = 0; x < boardSize; x++)
        {
            // 가운데 세로선은 더 굵게
            float lineWidth = (x == (boardSize - 1) / 2) ? 0.06f : 0.03f;
            DrawLine(new Vector3(x, 0, -0.05f), new Vector3(x, boardSize - 1, -0.05f), lineWidth);
        }

        // 카메라 위치 조정
        Camera.main.transform.position = new Vector3((boardSize - 1) / 2f, (boardSize - 1) / 2f, -10);
        Camera.main.orthographicSize = boardSize / 2f + 1;

        // 화점(花点) 표시 - 오목 기준점 (일반적으로 3-3, 3-11, 11-3, 11-11 위치와 중앙에 점을 표시)
        if (boardSize >= 13)
        {
            DrawDot(3, 3);
            DrawDot(3, boardSize - 4);
            DrawDot(boardSize - 4, 3);
            DrawDot(boardSize - 4, boardSize - 4);
            DrawDot((boardSize - 1) / 2, (boardSize - 1) / 2); // 중앙점
        }

        // 기본적으로 항상 흑돌(선공)부터 시작
        isBlackTurn = true;
    }

    // 컴퓨터 게임에서 턴 순서 설정
    public void SetTurnOrder(bool playerFirst)
    {
        isPlayerFirst = playerFirst;
        isComputerGame = true;
        Debug.Log("플레이어 " + (isPlayerFirst ? "선공(흑돌)" : "후공(백돌)") + " 설정됨");
    }

    // 선 그리기 함수 - 선 굵기 매개변수 추가
    void DrawLine(Vector3 start, Vector3 end, float width = 0.06f)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(transform);
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        line.material = lineMaterial;

        // 격자선 색상을 검은색으로 설정
        line.startColor = line.endColor = new Color(0f, 0f, 0f, 0.8f);
        line.startWidth = line.endWidth = width; // 선 굵기 적용
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.sortingOrder = -1; // 돌 아래에 그리기
    }

    // 화점(기준점) 그리기 함수
    void DrawDot(int x, int y)
    {
        GameObject dotObj = new GameObject("Dot");
        dotObj.transform.SetParent(transform);
        dotObj.transform.position = new Vector3(x, y, -0.06f);

        SpriteRenderer renderer = dotObj.AddComponent<SpriteRenderer>();

        // 작은 원형 스프라이트 생성
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];

        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                float dx = i - 16;
                float dy = j - 16;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                // 작은 검은 점 생성
                if (distance < 4)
                {
                    colors[i + j * 32] = Color.black;
                }
                else
                {
                    colors[i + j * 32] = new Color(0, 0, 0, 0);
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite dotSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        renderer.sprite = dotSprite;
        renderer.sortingOrder = -1;

        // 크기 조정
        dotObj.transform.localScale = new Vector3(0.1f, 0.1f, 1);
    }

    // 특정 위치가 비어있는지 확인하는 함수
    public bool IsCellEmpty(int x, int y)
    {
        // 보드 범위 체크
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return false;

        // 돌이 있는지 확인 (boardState 배열 사용)
        return boardState[x, y] == 0;
    }

    // 특정 위치의 셀 상태 반환 (0: 빈칸, 1: 흑돌, 2: 백돌, -1: 범위 밖)
    public int GetCellState(int x, int y)
    {
        // 보드 범위 체크
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return -1;

        // 셀 상태 반환
        return boardState[x, y];
    }

    // 특정 돌 타입의 연속 4개를 찾아 승리 가능한 위치 반환
    public Vector2Int FindWinningMove(int stoneType)
    {
        // 모든 방향 (가로, 세로, 대각선)
        int[] dx = { 1, 0, 1, 1 }; // 가로, 세로, 대각선(↘), 대각선(↗)
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // 빈 위치만 검사
                if (boardState[x, y] != 0) continue;

                // 모든 방향 검사
                for (int dir = 0; dir < 4; dir++)
                {
                    int count = 0;

                    // 양방향 검사 (앞뒤로 확인)
                    for (int sign = -1; sign <= 1; sign += 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            int nx = x + sign * i * dx[dir];
                            int ny = y + sign * i * dy[dir];

                            // 보드 범위 체크
                            if (nx < 0 || nx >= boardSize || ny < 0 || ny >= boardSize)
                                break;

                            // 같은 돌이 아니면 중단
                            if (boardState[nx, ny] != stoneType)
                                break;

                            count++;
                        }
                    }

                    // 연속 4개 발견 (5목을 만들 수 있는 위치)
                    if (count >= 4)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
        }

        // 승리 가능한 위치 없음
        return new Vector2Int(-1, -1);
    }

    // 연속된 2~3개 돌이 있는 좋은 위치 찾기
    public Vector2Int FindGoodMove(int stoneType)
    {
        Vector2Int bestMove = new Vector2Int(-1, -1);
        int bestScore = -1;

        // 모든 방향 (가로, 세로, 대각선)
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // 빈 위치만 검사
                if (boardState[x, y] != 0) continue;

                int totalScore = 0;

                // 모든 방향 검사
                for (int dir = 0; dir < 4; dir++)
                {
                    int score = 0;

                    // 양방향 검사 (앞뒤로 확인)
                    for (int sign = -1; sign <= 1; sign += 2)
                    {
                        for (int i = 1; i <= 3; i++) // 최대 3칸까지 검사
                        {
                            int nx = x + sign * i * dx[dir];
                            int ny = y + sign * i * dy[dir];

                            // 보드 범위 체크
                            if (nx < 0 || nx >= boardSize || ny < 0 || ny >= boardSize)
                                break;

                            // 같은 돌이면 점수 추가
                            if (boardState[nx, ny] == stoneType)
                                score++;
                            else
                                break;
                        }
                    }

                    // 이 방향의 점수 합산
                    totalScore += score * score; // 제곱해서 연속된 돌에 가중치 부여
                }

                // 최고 점수 갱신
                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestMove = new Vector2Int(x, y);
                }
            }
        }

        // 좋은 위치가 없으면 (-1, -1) 반환
        return bestMove;
    }

    // 플레이어용 돌 놓기 함수
    public bool PlaceStone(int x, int y)
    {
        if (isGameOver)
            return false;

        // 컴퓨터 턴이면 플레이어가 돌을 놓을 수 없음
        if (isComputerGame)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.IsComputerTurn())
            {
                Debug.Log("컴퓨터 턴입니다. 플레이어는 돌을 놓을 수 없습니다.");
                return false;
            }
        }

        // 보드 범위 체크
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return false;

        // 이미 돌이 있는지 확인
        if (!IsCellEmpty(x, y))
            return false;

        // 돌 놓기
        int stoneType = isBlackTurn ? 1 : 2;
        boardState[x, y] = stoneType;

        // 돌 생성
        GameObject stonePrefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, -0.1f);
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);
        stone.transform.SetParent(transform);

        // 승리 체크
        if (CheckWin(x, y, stoneType))
        {
            isGameOver = true;

            // GameManager에 승리 알림
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ShowVictory(isBlackTurn);
            }

            return true;
        }

        // 턴 전환
        isBlackTurn = !isBlackTurn;

        // GameManager에 플레이어 턴 종료 알림
        GameManager gameManager1 = FindObjectOfType<GameManager>();
        if (gameManager1 != null)
        {
            gameManager1.PlayerMoved();
        }

        return true;
    }

    // AI 전용 돌 놓기 함수 - 턴 관리를 명확하게 하기 위한 별도 메서드
    public bool PlaceStoneAI(int x, int y, bool isAIBlack)
    {
        if (isGameOver)
            return false;

        // 보드 범위 체크
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return false;

        // 이미 돌이 있는지 확인
        if (!IsCellEmpty(x, y))
            return false;

        // AI가 흑돌이면 현재 턴도 흑돌 차례여야 함
        if (isAIBlack != isBlackTurn)
        {
            Debug.LogError("AI 턴과 현재 보드 턴이 일치하지 않습니다!");
            return false;
        }

        // 돌 놓기
        int stoneType = isBlackTurn ? 1 : 2;
        boardState[x, y] = stoneType;

        // 돌 생성
        GameObject stonePrefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, -0.1f);
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);
        stone.transform.SetParent(transform);

        // 승리 체크
        if (CheckWin(x, y, stoneType))
        {
            isGameOver = true;

            // GameManager에 승리 알림
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ShowVictory(isBlackTurn);
            }

            return true;
        }

        // 턴 전환
        isBlackTurn = !isBlackTurn;

        return true;
    }

    // 승리 체크 (가로, 세로, 대각선 방향으로 연속 5개 확인)
    bool CheckWin(int x, int y, int stoneType)
    {
        bool isWin = false;

        // 가로 방향 체크
        if (CountStonesInDirection(x, y, 1, 0, stoneType) + CountStonesInDirection(x, y, -1, 0, stoneType) - 1 >= 5)
            isWin = true;

        // 세로 방향 체크
        if (CountStonesInDirection(x, y, 0, 1, stoneType) + CountStonesInDirection(x, y, 0, -1, stoneType) - 1 >= 5)
            isWin = true;

        // 대각선 방향 체크 (/)
        if (CountStonesInDirection(x, y, 1, 1, stoneType) + CountStonesInDirection(x, y, -1, -1, stoneType) - 1 >= 5)
            isWin = true;

        // 대각선 방향 체크 (\)
        if (CountStonesInDirection(x, y, 1, -1, stoneType) + CountStonesInDirection(x, y, -1, 1, stoneType) - 1 >= 5)
            isWin = true;

        return isWin;
    }

    // 특정 방향으로 연속된 같은 돌 개수 세기
    int CountStonesInDirection(int x, int y, int dx, int dy, int stoneType)
    {
        int count = 0;
        int nx = x;
        int ny = y;

        while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && boardState[nx, ny] == stoneType)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }

    // 현재 턴 정보 얻기
    public bool IsBlackTurn()
    {
        return isBlackTurn;
    }

    // 보드 상태 초기화 (재시작용)
    public void ResetBoard()
    {
        // 모든 돌 제거
        foreach (Transform child in transform)
        {
            if (!child.name.Contains("GridLine") && !child.name.Contains("Background") && !child.name.Contains("Dot"))
                Destroy(child.gameObject);
        }

        // 보드 상태 초기화
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                boardState[x, y] = 0;
            }
        }

        isBlackTurn = true; // 항상 흑돌(선공)부터 시작
        isGameOver = false; // 게임 종료 상태 초기화
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}