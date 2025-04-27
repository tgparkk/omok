using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 어려움 난이도 AI 전략 구현
/// 더 복잡한 공격/방어 전략과 고급 패턴을 사용합니다
/// </summary>
public class HardAIStrategy : IAIStrategy
{
    // 방향 벡터 (가로, 세로, 대각선 ↘, 대각선 ↗)
    private readonly int[] dx = { 1, 0, 1, 1 };
    private readonly int[] dy = { 0, 1, 1, -1 };

    public Vector2Int MakeMove(BoardManager boardManager, int computerStoneType)
    {
        int boardSize = boardManager.boardSize;
        int playerStoneType = (computerStoneType == 1) ? 2 : 1;

        // 1. 자신의 승리 가능 위치 검사 (가장 높은 우선순위)
        Vector2Int winPosition = boardManager.FindWinningMove(computerStoneType);
        if (winPosition.x != -1)
        {
            Debug.Log("AI(어려움): 승리 위치 발견 (" + winPosition.x + ", " + winPosition.y + ")");
            return winPosition;
        }

        // 2. 상대방의 승리 방지 (두 번째 우선순위)
        Vector2Int blockPosition = boardManager.FindWinningMove(playerStoneType);
        if (blockPosition.x != -1)
        {
            Debug.Log("AI(어려움): 방어 위치 발견 (" + blockPosition.x + ", " + blockPosition.y + ")");
            return blockPosition;
        }

        // 3. 포크 공격 시도 (여러 방향으로 동시에 위협 만들기)
        Vector2Int forkPosition = FindForkMove(boardManager, computerStoneType);
        if (forkPosition.x != -1)
        {
            Debug.Log("AI(어려움): 포크 공격 위치 발견 (" + forkPosition.x + ", " + forkPosition.y + ")");
            return forkPosition;
        }

        // 4. 상대방의 포크 방지
        Vector2Int blockForkPosition = FindForkMove(boardManager, playerStoneType);
        if (blockForkPosition.x != -1)
        {
            Debug.Log("AI(어려움): 포크 방어 위치 발견 (" + blockForkPosition.x + ", " + blockForkPosition.y + ")");
            return blockForkPosition;
        }

        // 5. 열린 3 만들기 (강력한 공격 패턴)
        Vector2Int openThreePosition = FindOpenThreeMove(boardManager, computerStoneType);
        if (openThreePosition.x != -1)
        {
            Debug.Log("AI(어려움): 열린 3 위치 발견 (" + openThreePosition.x + ", " + openThreePosition.y + ")");
            return openThreePosition;
        }

        // 6. 상대방의 열린 3 방해
        Vector2Int blockOpenThreePosition = FindOpenThreeMove(boardManager, playerStoneType);
        if (blockOpenThreePosition.x != -1)
        {
            Debug.Log("AI(어려움): 상대 열린 3 방해 위치 발견 (" + blockOpenThreePosition.x + ", " + blockOpenThreePosition.y + ")");
            return blockOpenThreePosition;
        }

        // 7. 자신의 돌이 많은 위치 근처에 돌 놓기 (전략적 확장)
        Vector2Int expandPosition = boardManager.FindGoodMove(computerStoneType);
        if (expandPosition.x != -1)
        {
            Debug.Log("AI(어려움): 전략적 확장 위치 발견 (" + expandPosition.x + ", " + expandPosition.y + ")");
            return expandPosition;
        }

        // 8. 중앙 또는 중앙 근처에 돌 놓기 (보드 초반에 유리)
        int center = boardSize / 2;
        if (boardManager.IsCellEmpty(center, center))
        {
            Debug.Log("AI(어려움): 중앙에 돌 놓기 (" + center + ", " + center + ")");
            return new Vector2Int(center, center);
        }

        // 중앙 부근 위치 시도
        for (int offset = 1; offset <= 3; offset++)
        {
            // 중앙 주변 위치들을 우선순위에 따라 시도
            Vector2Int[] positions = {
                new Vector2Int(center, center + offset),
                new Vector2Int(center + offset, center),
                new Vector2Int(center, center - offset),
                new Vector2Int(center - offset, center),
                new Vector2Int(center + offset, center + offset),
                new Vector2Int(center - offset, center - offset),
                new Vector2Int(center + offset, center - offset),
                new Vector2Int(center - offset, center + offset)
            };

            foreach (Vector2Int pos in positions)
            {
                if (pos.x >= 0 && pos.x < boardSize &&
                    pos.y >= 0 && pos.y < boardSize &&
                    boardManager.IsCellEmpty(pos.x, pos.y))
                {
                    Debug.Log("AI(어려움): 중앙 근처에 돌 놓기 (" + pos.x + ", " + pos.y + ")");
                    return pos;
                }
            }
        }

        // 최후의 수단: 전략적 위치를 찾지 못한 경우 인공 지능적으로 좋은 위치 선택
        Vector2Int bestPosition = FindBestMove(boardManager, computerStoneType);
        if (bestPosition.x != -1)
        {
            Debug.Log("AI(어려움): 계산된 최적 위치 선택 (" + bestPosition.x + ", " + bestPosition.y + ")");
            return bestPosition;
        }

        // 그래도 못 찾으면 랜덤 위치 선택
        Debug.Log("AI(어려움): 전략적 위치를 찾지 못해 랜덤 위치 선택");
        return MakeRandomMove(boardManager);
    }

    // 포크 공격 위치 찾기 (여러 방향에서 동시에 위협이 되는 위치)
    private Vector2Int FindForkMove(BoardManager boardManager, int stoneType)
    {
        int boardSize = boardManager.boardSize;
        Vector2Int bestMove = new Vector2Int(-1, -1);
        int bestScore = 0;

        // 보드의 모든 빈 위치 검사
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!boardManager.IsCellEmpty(x, y)) continue;

                int threatCount = 0;

                // 이 위치에 돌을 놓았을 때 생성되는 위협 수 계산
                for (int dir = 0; dir < 4; dir++)
                {
                    int[] sequence = GetLineSequence(boardManager, x, y, dir, stoneType);

                    // 이 방향에 3개 연속 돌이 있으면 위협으로 간주
                    if (CountConsecutiveStones(sequence, stoneType) >= 3)
                    {
                        threatCount++;
                    }
                }

                // 두 개 이상의 위협이 있으면 포크 공격 위치로 간주
                if (threatCount >= 2 && threatCount > bestScore)
                {
                    bestScore = threatCount;
                    bestMove = new Vector2Int(x, y);
                }
            }
        }

        return bestMove;
    }

    // 열린 3을 만들 수 있는 위치 찾기
    private Vector2Int FindOpenThreeMove(BoardManager boardManager, int stoneType)
    {
        int boardSize = boardManager.boardSize;
        Vector2Int bestMove = new Vector2Int(-1, -1);
        int bestScore = 0;

        // 보드의 모든 빈 위치 검사
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!boardManager.IsCellEmpty(x, y)) continue;

                int openThreeCount = 0;

                // 이 위치에 돌을 놓았을 때 생성되는 열린 3의 수 계산
                for (int dir = 0; dir < 4; dir++)
                {
                    int[] sequence = GetLineSequence(boardManager, x, y, dir, stoneType);

                    // 열린 3 패턴 확인 (양쪽이 열린 3개 연속 돌)
                    if (IsOpenThree(sequence, stoneType))
                    {
                        openThreeCount++;
                    }
                }

                // 열린 3의 수가 많을수록 좋은 위치
                if (openThreeCount > bestScore)
                {
                    bestScore = openThreeCount;
                    bestMove = new Vector2Int(x, y);
                }
            }
        }

        return bestMove;
    }

    // 특정 방향의 돌 배열 얻기
    private int[] GetLineSequence(BoardManager boardManager, int x, int y, int dirIndex, int stoneType)
    {
        int boardSize = boardManager.boardSize;
        List<int> sequence = new List<int>();

        // 양방향으로 6칸씩 확인 (총 13칸)
        for (int offset = -6; offset <= 6; offset++)
        {
            int nx = x + offset * dx[dirIndex];
            int ny = y + offset * dy[dirIndex];

            // 보드 범위 내인지 확인
            if (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize)
            {
                if (offset == 0)
                {
                    // 현재 위치는 돌을 놓는다고 가정
                    sequence.Add(stoneType);
                }
                else
                {
                    // 이미 놓여진 돌의 상태 가져오기 (BoardManager.GetCellState 사용)
                    int cellState = boardManager.GetCellState(nx, ny);
                    sequence.Add(cellState);
                }
            }
            else
            {
                // 보드 밖은 -1로 표시
                sequence.Add(-1);
            }
        }

        return sequence.ToArray();
    }

    // 연속된 돌의 개수 세기
    private int CountConsecutiveStones(int[] sequence, int stoneType)
    {
        int maxCount = 0;
        int currentCount = 0;

        foreach (int cell in sequence)
        {
            if (cell == stoneType)
            {
                currentCount++;
                if (currentCount > maxCount)
                {
                    maxCount = currentCount;
                }
            }
            else
            {
                currentCount = 0;
            }
        }

        return maxCount;
    }

    // 열린 3인지 확인 (양쪽이 열린 연속 3개 돌)
    private bool IsOpenThree(int[] sequence, int stoneType)
    {
        // 가운데 위치 (sequence.Length는 항상 13이어야 함)
        int center = sequence.Length / 2;

        // 패턴 1: 0[SSS]0 (중앙에 연속 3개 돌, 양쪽 빈칸)
        if (sequence[center - 1] == stoneType && sequence[center] == stoneType && sequence[center + 1] == stoneType &&
            sequence[center - 2] == 0 && sequence[center + 2] == 0)
        {
            return true;
        }

        // 패턴 2: 0[SS]0S0 (왼쪽에 연속 2개 돌, 오른쪽에 1개 돌, 모두 빈칸으로 분리)
        if (sequence[center - 2] == 0 &&
            sequence[center - 1] == stoneType && sequence[center] == stoneType &&
            sequence[center + 1] == 0 && sequence[center + 2] == stoneType && sequence[center + 3] == 0)
        {
            return true;
        }

        // 패턴 3: 0S0[SS]0 (오른쪽에 연속 2개 돌, 왼쪽에 1개 돌, 모두 빈칸으로 분리)
        if (sequence[center - 3] == 0 && sequence[center - 2] == stoneType && sequence[center - 1] == 0 &&
            sequence[center] == stoneType && sequence[center + 1] == stoneType && sequence[center + 2] == 0)
        {
            return true;
        }

        return false;
    }

    // 최적의 위치 찾기 (영향력 기반 평가)
    private Vector2Int FindBestMove(BoardManager boardManager, int computerStoneType)
    {
        int boardSize = boardManager.boardSize;
        Vector2Int bestMove = new Vector2Int(-1, -1);
        int bestScore = int.MinValue;
        int playerStoneType = (computerStoneType == 1) ? 2 : 1;

        // 가중치 정의
        int myStoneWeight = 10;    // 자신의 돌 가중치
        int enemyStoneWeight = 8;  // 상대 돌 가중치
        int emptyWeight = 1;       // 빈 칸 가중치

        // 보드의 모든 빈 위치 검사
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!boardManager.IsCellEmpty(x, y)) continue;

                int score = 0;

                // 주변 9x9 영역의 돌 상태 확인하여 점수 계산
                for (int i = -4; i <= 4; i++)
                {
                    for (int j = -4; j <= 4; j++)
                    {
                        int nx = x + i;
                        int ny = y + j;

                        // 거리에 따른 가중치 (가까울수록 영향력 큼)
                        int distance = Mathf.Max(Mathf.Abs(i), Mathf.Abs(j));
                        int distanceWeight = 5 - distance; // 5에서 거리를 뺌

                        if (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize)
                        {
                            int cellState = boardManager.GetCellState(nx, ny);

                            if (cellState == computerStoneType)
                            {
                                // 자신의 돌 근처에 놓는 것 선호
                                score += myStoneWeight * distanceWeight;
                            }
                            else if (cellState == playerStoneType)
                            {
                                // 상대방의 돌 근처에 놓는 것도 고려
                                score += enemyStoneWeight * distanceWeight;
                            }
                            else if (cellState == 0)
                            {
                                // 빈 공간도 약간의 가중치
                                score += emptyWeight;
                            }
                        }
                    }
                }

                // 중앙에 가까울수록 추가 점수
                int centerDistance = Mathf.Max(Mathf.Abs(x - boardSize / 2), Mathf.Abs(y - boardSize / 2));
                int centerBonus = (boardSize / 2 - centerDistance) * 2;
                score += centerBonus;

                // 최고 점수 갱신
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Vector2Int(x, y);
                }
            }
        }

        return bestMove;
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
                Debug.Log("AI(어려움)가 랜덤 위치 선택: (" + x + ", " + y + ")");
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
                    Debug.Log("AI(어려움)의 최종 위치 탐색: (" + i + ", " + j + ")");
                    return new Vector2Int(i, j);
                }
            }
        }

        // 빈 위치를 찾지 못한 경우 (꽉 찬 보드)
        Debug.LogWarning("AI(어려움): 놓을 수 있는 위치가 없습니다.");
        return new Vector2Int(-1, -1);
    }
}