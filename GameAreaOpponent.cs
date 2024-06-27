using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAreaOpponent : MonoBehaviour
{
    public int[,] field;
    private int fieldHeight;
    private int fieldWidth;
    private readonly int stageHeight = 20;
    private readonly int stageWidth = 10;
    private readonly int edgeTop = 4;
    private readonly int edgeSide = 2;
    private readonly int wallTickness = 1;
    void Init()
    {
        fieldHeight = stageHeight + wallTickness + edgeTop + edgeSide;
        fieldWidth = stageWidth + wallTickness * 2 + edgeSide * 2;
        field = new int[fieldHeight, fieldWidth];
    }
    void Start()
    {
        Init();
        InitField();
        DrawStage();
    }
    void Update()
    {
        DrawStage();
    }
    private void InitField()
    {
        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                field[y, x] = 0;
            }
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
}
