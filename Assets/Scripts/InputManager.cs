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
        // ���콺 Ŭ�� ó�� (PC)
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }

        // ��ġ �Է� ó�� (�����)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleInput(Input.GetTouch(0).position);
        }
    }

    void HandleInput(Vector3 inputPosition)
    {
        // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(inputPosition);
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.y);

        // ���� ���� üũ
        if (x >= 0 && x < boardManager.boardSize && y >= 0 && y < boardManager.boardSize)
        {
            boardManager.PlaceStone(x, y);
        }
    }
}