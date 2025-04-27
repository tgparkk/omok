using UnityEngine;

/// <summary>
/// AI 전략을 위한 인터페이스
/// 다양한 난이도의 AI 구현을 위한 기본 구조 제공
/// </summary>
public interface IAIStrategy
{
    /// <summary>
    /// AI의 다음 수를 결정합니다.
    /// </summary>
    /// <param name="boardManager">게임 보드 정보에 접근하기 위한 BoardManager</param>
    /// <param name="computerStoneType">컴퓨터 돌 타입 (1: 흑돌, 2: 백돌)</param>
    /// <returns>다음 돌을 놓을 위치의 좌표. 유효하지 않은 경우 (-1, -1) 반환</returns>
    Vector2Int MakeMove(BoardManager boardManager, int computerStoneType);
}