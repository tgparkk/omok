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
        if (boardSize >= 13) {
            DrawDot(3, 3);
            DrawDot(3, boardSize - 4);
            DrawDot(boardSize - 4, 3);
            DrawDot(boardSize - 4, boardSize - 4);
            DrawDot((boardSize - 1) / 2, (boardSize - 1) / 2); // 중앙점
        }
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
        
        for (int i = 0; i < 32; i++) {
            for (int j = 0; j < 32; j++) {
                float dx = i - 16;
                float dy = j - 16;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                // 작은 검은 점 생성
                if (distance < 4) {
                    colors[i + j * 32] = Color.black;
                } else {
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
    
    // 돌 놓기 (교차점에 놓도록 수정)
    public bool PlaceStone(int x, int y)
    {
        if (isGameOver)
            return false;
        
        // 보드 범위 체크
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return false;
            
        // 이미 돌이 있는지 확인
        if (boardState[x, y] != 0)
            return false;
        
        // 돌 놓기
        int stoneType = isBlackTurn ? 1 : 2;
        boardState[x, y] = stoneType;
        
        // 돌 생성 - 정확히 교차점에 위치하도록
        GameObject stonePrefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, -0.1f); // 교차점 위치
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);
        stone.transform.SetParent(transform);
        
        // 승리 체크
        if (CheckWin(x, y, stoneType))
        {
            Debug.Log((isBlackTurn ? "흑돌" : "백돌") + " 승리!");
            isGameOver = true; // 게임 종료 설정
        
            // GameManager에 승리 알림
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ShowVictory(isBlackTurn);
            }
        }
        else
        {
            // 턴 전환 (게임이 끝나지 않았을 때만)
            isBlackTurn = !isBlackTurn;
        }
        
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
        
        // 승리 시 GameManager 호출
        if (isWin)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // 흑돌(1) 또는 백돌(2) 승리 여부 전달
                gameManager.ShowVictory(stoneType == 1);
            }
        }
    
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
        
        isBlackTurn = true;
        isGameOver = false; // 게임 종료 상태 초기화
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}