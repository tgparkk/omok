// InputManager.cs
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private BoardManager boardManager;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }
    
    void Update()
    {
        // 마우스 클릭 처리 (PC)
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
        
        // 터치 입력 처리 (모바일)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleInput(Input.GetTouch(0).position);
        }
    }
    
    void HandleInput(Vector3 inputPosition)
    {
        // 스크린 좌표를 월드 좌표로 변환
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(inputPosition);
        
        // 가장 가까운 교차점 찾기 - 반올림으로 교차점 위치 결정
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.y);
        
        // 보드 범위 체크 및 돌 놓기
        boardManager.PlaceStone(x, y);
    }
}