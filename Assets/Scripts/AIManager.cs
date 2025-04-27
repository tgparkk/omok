using UnityEngine;

/// <summary>
/// AI 전략을 관리하고 난이도에 따라 적절한 전략을 선택하는 클래스
/// </summary>
public class AIManager : MonoBehaviour
{
    private IAIStrategy currentStrategy;
    private int computerDifficulty;
    private BoardManager boardManager;

    /// <summary>
    /// AIManager 초기화
    /// </summary>
    /// <param name="boardManager">게임 보드 참조</param>
    /// <param name="difficulty">AI 난이도 (1: 쉬움, 2: 중간, 3: 어려움)</param>
    public void Initialize(BoardManager boardManager, int difficulty)
    {
        this.boardManager = boardManager;
        SetDifficulty(difficulty);
    }

    /// <summary>
    /// AI 난이도 설정
    /// </summary>
    /// <param name="difficulty">AI 난이도 (1: 쉬움, 2: 중간, 3: 어려움)</param>
    public void SetDifficulty(int difficulty)
    {
        computerDifficulty = difficulty;

        // 난이도에 따라 전략 결정
        switch (difficulty)
        {
            case 1: // 쉬움
                currentStrategy = new EasyAIStrategy();
                break;
            case 2: // 중간
                // 중간 난이도 전략 사용
                currentStrategy = new MediumAIStrategy();
                break;
            case 3: // 어려움
                // 어려운 난이도 전략 사용
                currentStrategy = new HardAIStrategy();
                break;
            default:
                currentStrategy = new EasyAIStrategy();
                Debug.LogWarning("알 수 없는 난이도입니다. 기본 난이도(쉬움)으로 설정합니다.");
                break;
        }

        Debug.Log($"AI 난이도가 {difficulty}로 설정되었습니다.");
    }

    /// <summary>
    /// AI의 다음 수를 계산
    /// </summary>
    /// <param name="computerStoneType">컴퓨터 돌 타입 (1: 흑돌, 2: 백돌)</param>
    /// <returns>다음 돌을 놓을 위치</returns>
    public Vector2Int GetNextMove(int computerStoneType)
    {
        if (currentStrategy == null)
        {
            Debug.LogError("AI 전략이 설정되지 않았습니다!");
            return new Vector2Int(-1, -1);
        }

        // 선택된 전략으로 다음 수 결정
        return currentStrategy.MakeMove(boardManager, computerStoneType);
    }

    /// <summary>
    /// 현재 설정된 난이도 반환
    /// </summary>
    /// <returns>AI 난이도 (1: 쉬움, 2: 중간, 3: 어려움)</returns>
    public int GetDifficulty()
    {
        return computerDifficulty;
    }
}