// BoardManager.cs
using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 15; // ������ ũ�� (15x15)
    public GameObject cellPrefab; // ĭ ������
    public GameObject blackStonePrefab; // �浹 ������
    public GameObject whiteStonePrefab; // �鵹 ������

    private GameObject[,] cells; // ĭ ��ü �迭
    private int[,] boardState; // 0: ��ĭ, 1: �浹, 2: �鵹
    private bool isBlackTurn = true; // ���� �� (true: �浹, false: �鵹)

    void Start()
    {
        InitializeBoard();
    }

    // ������ �ʱ�ȭ
    void InitializeBoard()
    {
        cells = new GameObject[boardSize, boardSize];
        boardState = new int[boardSize, boardSize];

        // ���� ĭ ����
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                cell.transform.SetParent(transform);
                cell.name = $"Cell_{x}_{y}";

                // ĭ�� ��ġ ���� ����
                CellInfo cellInfo = cell.AddComponent<CellInfo>();
                cellInfo.x = x;
                cellInfo.y = y;

                cells[x, y] = cell;
                boardState[x, y] = 0; // ��ĭ���� �ʱ�ȭ
            }
        }

        // ī�޶� ��ġ ����
        Camera.main.transform.position = new Vector3((boardSize - 1) / 2f, (boardSize - 1) / 2f, -10);
        Camera.main.orthographicSize = boardSize / 2f + 1;
    }

    // �� ����
    public bool PlaceStone(int x, int y)
    {
        // �̹� ���� �ִ��� Ȯ��
        if (boardState[x, y] != 0)
            return false;

        // �� ����
        int stoneType = isBlackTurn ? 1 : 2;
        boardState[x, y] = stoneType;

        // �� ����
        GameObject stonePrefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, -0.1f); // �ణ �տ� ��ġ
        GameObject stone = Instantiate(stonePrefab, position, Quaternion.identity);
        stone.transform.SetParent(transform);

        // �¸� üũ
        if (CheckWin(x, y, stoneType))
        {
            Debug.Log((isBlackTurn ? "�浹" : "�鵹") + " �¸�!");
            // ���⿡ �¸� ó�� ���� �߰�
        }

        // �� ��ȯ
        isBlackTurn = !isBlackTurn;
        return true;
    }

    // �¸� üũ (����, ����, �밢�� �������� ���� 5�� Ȯ��)
    bool CheckWin(int x, int y, int stoneType)
    {
        // ���� ���� üũ
        if (CountStonesInDirection(x, y, 1, 0, stoneType) + CountStonesInDirection(x, y, -1, 0, stoneType) - 1 >= 5)
            return true;

        // ���� ���� üũ
        if (CountStonesInDirection(x, y, 0, 1, stoneType) + CountStonesInDirection(x, y, 0, -1, stoneType) - 1 >= 5)
            return true;

        // �밢�� ���� üũ (/)
        if (CountStonesInDirection(x, y, 1, 1, stoneType) + CountStonesInDirection(x, y, -1, -1, stoneType) - 1 >= 5)
            return true;

        // �밢�� ���� üũ (\)
        if (CountStonesInDirection(x, y, 1, -1, stoneType) + CountStonesInDirection(x, y, -1, 1, stoneType) - 1 >= 5)
            return true;

        return false;
    }

    // Ư�� �������� ���ӵ� ���� �� ���� ����
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

    // ���� �� ���� ���
    public bool IsBlackTurn()
    {
        return isBlackTurn;
    }

    // ���� ���� �ʱ�ȭ (����ۿ�)
    public void ResetBoard()
    {
        // ��� �� ����
        foreach (Transform child in transform)
        {
            if (child.GetComponent<CellInfo>() == null) // ���� �ƴ� ���� ����
                Destroy(child.gameObject);
        }

        // ���� ���� �ʱ�ȭ
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

// �� ĭ�� ��ġ ������ �����ϴ� ��ũ��Ʈ
public class CellInfo : MonoBehaviour
{
    public int x;
    public int y;
}