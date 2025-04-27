using UnityEngine;

/// <summary>
/// 중간 난이도 AI 전략 구현
/// 기본적인 공격/방어 전략을 사용합니다
/// </summary>
public class MediumAIStrategy : IAIStrategy
{
    public Vector2Int MakeMove(BoardManager boardManager, int computerStoneType)
    {
        // 플레이어 돌 타입 계산 (컴퓨터가 흑(1)이면 플레이어는 백(2), 반대도 성립)
        int playerStoneType = (computerStoneType == 1) ? 2 : 1;

        // 1. 자신이 4개 연속되었는지 확인 (승리 가능)
        Vector2Int winPosition = boardManager.FindWinningMove(computerStoneType);
        if (winPosition.x != -1)
        {
            Debug.Log("AI(중간): 승리 위치 발견 (" + winPosition.x + ", " + winPosition.y + ")");
            return winPosition;
        }

        // 2. 상대가 4개 연속되었는지 확인 (방어 필요)
        Vector2Int blockPosition = boardManager.FindWinningMove(playerStoneType);
        if (blockPosition.x != -1)
        {
            Debug.Log("AI(중간): 방어 위치 발견 (" + blockPosition.x + ", " + blockPosition.y + ")");
            return blockPosition;
        }

        // 3. 자신의 돌이 많은 위치 근처에 돌 놓기
        Vector2Int expandPosition = boardManager.FindGoodMove(computerStoneType);
        if (expandPosition.x != -1)
        {
            Debug.Log("AI(중간): 좋은 위치 발견 (" + expandPosition.x + ", " + expandPosition.y + ")");
            return expandPosition;
        }

        // 4. 전략적 위치를 찾지 못한 경우 랜덤 위치 선택
        Debug.Log("AI(중간): 전략적 위치를 찾지 못해 랜덤 위치 선택");
        return MakeRandomMove(boardManager);
    }

    // 랜덤 위치 찾기 (보조 메서드)
    private Vector2Int MakeRandomMove(BoardManager boardManager)
    {
        int x, y;
        int attempts = 0;
        int boardSize = boardManager.boardSize;

        // 빈 위치 찾을 때까지 랜덤 시도
        while (attempts < 100)
        {
            x = Random.Range(0, boardSize);
            y = Random.Range(0, boardSize);

            if (boardManager.IsCellEmpty(x, y))
            {
                Debug.Log("AI(중간)가 랜덤 위치 선택: (" + x + ", " + y + ")");
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
                    Debug.Log("AI(중간)의 최종 위치 탐색: (" + i + ", " + j + ")");
                    return new Vector2Int(i, j);
                }
            }
        }

        // 빈 위치를 찾지 못한 경우 (꽉 찬 보드)
        Debug.LogWarning("AI(중간): 놓을 수 있는 위치가 없습니다.");
        return new Vector2Int(-1, -1);
    }
}