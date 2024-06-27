using UnityEngine;

public class GameArea : MonoBehaviour
{
    public int[,] field;
    public int[,] bufferField;
    private int fieldHeight;
    private int fieldWidth;
    private readonly int stageHeight = 20;
    private readonly int stageWidth = 10;
    private readonly int edgeTop = 4;
    private readonly int edgeSide = 2;
    private readonly int wallTickness = 1;

    private GameMino gameMino;
    private GameMino nextGameMino;
    private float timer;
    private float maxTimer = 1.0f;

    void Init()
    {
        fieldHeight = stageHeight + wallTickness + edgeTop + edgeSide;
        fieldWidth = stageWidth + wallTickness * 2 + edgeSide * 2;
        field = new int[fieldHeight, fieldWidth];
        bufferField = new int[fieldHeight, fieldWidth];
        gameMino = new GameMino();
        nextGameMino = new GameMino();
    }
    void Start()
    {
        Init();
        InitBufferField();
        DrawStage();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > maxTimer)
        {
            if (!IsCollison(0, 1, 0))
            {
                gameMino.y ++;
                timer = 0;
            }
            else if (IsStack())
            {
                BufferFieldAddMino();
                MinoChanger();
                DeleteLines();
                timer = 0;
            }
        }
        MinoController();
        InitField();
        FieldAddMino();
        DrawStage();
    }
    private void InitField()
    {
        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                field[y, x] = bufferField[y, x];
            }
        }
    }
    private void InitBufferField()
    {
        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                bufferField[y, x] = 0;
            }
        }
        for (int y = 0; y < edgeTop + stageHeight; y++)
        {
            bufferField[y, edgeSide] = bufferField[y, edgeSide + stageWidth + 1] = 1;
        }
        for (int x = edgeSide; x < edgeSide + stageWidth + wallTickness * 2; x++)
        {
            bufferField[edgeTop + stageHeight, x] = 1;
        }
    }
    private void DrawStage()
    {
        for (int row = 0; row < stageHeight; row++)
        {
            for (int col = 0; col < stageWidth; col++)
            {
                GameObject element = transform.GetChild(row).GetChild(col).gameObject;
                switch (field[edgeTop + row, edgeSide + wallTickness + col])
                {
                    case 1: element.GetComponent<Renderer>().material.color = Color.magenta; break;
                    case 2: element.GetComponent<Renderer>().material.color = Color.cyan; break;
                    case 3: element.GetComponent<Renderer>().material.color = Color.yellow; break;
                    case 4: element.GetComponent<Renderer>().material.color = Color.green; break;
                    case 5: element.GetComponent<Renderer>().material.color = Color.red; break;
                    case 6: element.GetComponent<Renderer>().material.color = Color.blue; break;
                    case 7: element.GetComponent<Renderer>().material.color = new Color(1.0f, 0.5f, 0.0f, 1.0f); break;
                    default: element.GetComponent<Renderer>().material.color = Color.clear; break;
                }
            }
        }
    }
    private void FieldAddMino()
    {
        for (int y = 0; y < gameMino.size; y++)
        {
            for (int x = 0; x < gameMino.size; x++)
            {
                field[gameMino.y + y, gameMino.x + x] |= gameMino.types[gameMino.angle, y, x];
            }
        }
    }
    private void BufferFieldAddMino()
    {
        for (int y = 0; y < gameMino.size; y++)
        {
            for (int x = 0; x < gameMino.size; x++)
            {
                bufferField[gameMino.y + y, gameMino.x + x] |= gameMino.types[gameMino.angle, y, x];
            }
        }
    }
    private void MinoController()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && !IsCollison(0, 1, 0)) gameMino.y++;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && !IsCollison(1, 0, 0)) gameMino.x++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !IsCollison(-1, 0, 0)) gameMino.x--;
        else if (Input.GetKeyDown(KeyCode.UpArrow) && !IsCollison(0, 0, 1)) gameMino.angle = (gameMino.angle + 1) % gameMino.size;
    }
    private void MinoChanger()
    {
        gameMino = nextGameMino;
        nextGameMino = new GameMino();
    }
    private bool IsCollison(int x, int y, int angle)
    {
        for (int r = 0; r < gameMino.size; r++)
        {
            for (int c = 0; c < gameMino.size; c++)
            {
                int nextAngle = (gameMino.angle + angle) % gameMino.size;
                if (bufferField[gameMino.y + y + r, gameMino.x + x + c] >= 1 && gameMino.types[nextAngle, r, c] >= 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool IsStack()
    {
        for (int r = 0; r < gameMino.size; r++)
        {
            for (int c = 0; c < gameMino.size; c++)
            {
                if (bufferField[gameMino.y + r + 1, gameMino.x + c] >= 1 && gameMino.types[gameMino.angle, r, c] >= 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
    bool IsDeleteLine(int y)
    {
        for (int x = edgeSide + wallTickness; x < edgeSide + wallTickness * 2 + stageWidth; x++)
        {
            if (bufferField[y, x] == 0) return false;
        }
        return true;
    }
    void DeleteLine(int y)
    {
        for (int x = edgeSide + wallTickness; x < edgeSide + wallTickness * 2 + stageWidth; x++)
        {
            bufferField[y, x] = 0;
        }
    }
    void FallLine(int startY)
    {
        for (int y = startY; y >= edgeTop; y--)
        {
            for (int x = edgeSide + wallTickness; x < edgeSide + wallTickness * 2 + stageWidth; x++)
            {
                bufferField[y, x] = bufferField[y - 1, x];
                bufferField[y - 1, x] = 0;
            }
        }
    }
    void DeleteLines()
    {
        for (int y = edgeTop; y < edgeTop + stageHeight; y++)
        {
            if (!IsDeleteLine(y)) continue;
            DeleteLine(y);
            FallLine(y);
        }
    }
}
