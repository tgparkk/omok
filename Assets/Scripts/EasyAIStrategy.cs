using UnityEngine;

public class EasyAIStrategy : IAIStrategy
{
    public Vector2Int MakeMove(BoardManager boardManager, int computerStoneType)
    {
        int x, y;
        bool placed = false;
        int attempts = 0;
        int boardSize = boardManager.boardSize;

        // 빈 위치를 찾을 때까지 랜덤 시도
        while (!placed && attempts < 100)
        {
            x = Random.Range(0, boardSize);
            y = Random.Range(0, boardSize);

            // boardManager의 IsCellEmpty 메서드 사용
            if (boardManager.IsCellEmpty(x, y))
            {
                Debug.Log("AI(쉬움)가 돌을 놓을 위치: (" + x + ", " + y + ")");
                return new Vector2Int(x, y);
            }

            attempts++;
        }

        // 최대 시도 횟수 초과하면 보드 전체 탐색
        Debug.Log("랜덤 시도 실패, 보드 전체 탐색 중...");

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (boardManager.IsCellEmpty(i, j))
                {
                    Debug.Log("AI(쉬움)의 최종 위치 탐색: (" + i + ", " + j + ")");
                    return new Vector2Int(i, j);
                }
            }
        }

        // 빈 위치를 찾지 못한 경우 (꽉 찬 보드)
        Debug.LogWarning("AI(쉬움): 놓을 수 있는 위치가 없습니다.");
        return new Vector2Int(-1, -1);
    }
}