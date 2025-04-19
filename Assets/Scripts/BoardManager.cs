// BoardManager.cs
using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 15; // 오목판 크기 (15x15)
    public GameObject cellPrefab; // 칸 프리팹
    public GameObject blackStonePrefab; // 흑돌 프리팹
    public GameObject whiteStonePrefab; // 백돌 프리팹
    
    private GameObject[,] cells; // 칸 객체 배열
    private int[,] boardState; // 0: 빈칸, 1: 흑돌, 2: 백돌
    private bool isBlackTurn = true; // 현재 턴 (true: 흑돌, false: 백돌)
    
    void Start()
    {
        InitializeBoard();
    }
    
    // 오목판 초기화
    void InitializeBoard()
    {
        cells = new GameObject[boardSize, boardSize];
        boardState = new int[boardSize, boardSize];
        
        // 보드 칸 생성
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                cell.transform.SetParent(transform);
                cell.name = $"Cell_{x}_{y}";
                
                // 칸에 위치 정보 저장
                CellInfo cellInfo = cell.AddComponent<CellInfo>();
                cellInfo.x = x;
                cellInfo.y = y;
                
                cells[x, y] = cell;
                boardState[x, y] = 0; // 빈칸으로 초기화
            }
        }
        
        // 카메라 위치 조정
        Camera.main.transform.position = new Vector3((boardSize - 1) / 2f, (boardSize - 1) / 2f, -10);
        Camera.main.orthographicSize = boardSize / 2f + 1;
    }
    
    // 돌 놓기
    public bool PlaceStone(int x, int y)
    {
        // 이미 돌이 있는지 확인
        if (boardState[x, y] != 0)
            return false;
        
        // 돌 놓기
        int stoneType = isBlackTurn ? 1 : 2;
        boardState[x, y] = stoneType;
        
        // 돌 생성
        GameObject stonePrefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, -0.1f); // 약간 앞에 위치
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);
        stone.transform.SetParent(transform);
        
        // 승리 체크
        if (CheckWin(x, y, stoneType))
        {
            Debug.Log((isBlackTurn ? "흑돌" : "백돌") + " 승리!");
            // 여기에 승리 처리 로직 추가
        }
        
        // 턴 전환
        isBlackTurn = !isBlackTurn;
        return true;
    }
    
    // 승리 체크 (가로, 세로, 대각선 방향으로 연속 5개 확인)
    bool CheckWin(int x, int y, int stoneType)
    {
        // 가로 방향 체크
        if (CountStonesInDirection(x, y, 1, 0, stoneType) + CountStonesInDirection(x, y, -1, 0, stoneType) - 1 >= 5)
            return true;
        
        // 세로 방향 체크
        if (CountStonesInDirection(x, y, 0, 1, stoneType) + CountStonesInDirection(x, y, 0, -1, stoneType) - 1 >= 5)
            return true;
        
        // 대각선 방향 체크 (/)
        if (CountStonesInDirection(x, y, 1, 1, stoneType) + CountStonesInDirection(x, y, -1, -1, stoneType) - 1 >= 5)
            return true;
        
        // 대각선 방향 체크 (\)
        if (CountStonesInDirection(x, y, 1, -1, stoneType) + CountStonesInDirection(x, y, -1, 1, stoneType) - 1 >= 5)
            return true;
        
        return false;
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
            if (child.GetComponent<CellInfo>() == null) // 셀이 아닌 돌만 제거
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
    }
}

// 각 칸의 위치 정보를 저장하는 스크립트
public class CellInfo : MonoBehaviour
{
    public int x;
    public int y;
}