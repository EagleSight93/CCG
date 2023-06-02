using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum CellState
{
    None,
    Friendly,
    Enemy,
    Free,
    OutOfBounds
}

//Singleton class
public class Board : MonoBehaviour
{
    public static Board gameBoard;

    public GameObject mCellPrefab;

    [HideInInspector]
    public Cell[,] mBoardCells;


    public void Awake()
    {
        if (gameBoard != null)
        {
            //destroy this instance
            Destroy(gameObject);
            return;
        }

        gameBoard = this;
    }

    public void Build(int columns, int rows)
    {
        mBoardCells = new Cell[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                //Create new cell and parent to board
                GameObject newCell = Instantiate(mCellPrefab, transform);

                //Get rectTrans and move the cell
                RectTransform rectNewCell = newCell.GetComponent<RectTransform>();
                //                                                 (cell + x padding) + x external border
                rectNewCell.anchoredPosition = new Vector2Int((x * (150 + 5)) + 10, (y * (150 + 5)) + 10);

                //Add the Cell to array of cells, then Setup cell
                mBoardCells[x, y] = newCell.GetComponent<Cell>(); newCell.GetComponent<Cell>();
                mBoardCells[x, y].Setup(new Vector2Int(x, y), this);
            }
        }

    }

    public CellState ValidateCell(int xPosInArray, int yPosInArray, BaseUnit consultingUnit)
    {
        if (InBoard(xPosInArray, yPosInArray))
        {
            //Get Cell
            Cell targetCell = mBoardCells[xPosInArray, yPosInArray];

            //If cell has a unit, check team
            if (targetCell.mCurrentUnit != null)
            {
                //Get unit team color
                if (consultingUnit.mCurrentTeam == targetCell.mCurrentUnit.mCurrentTeam)
                {
                    return CellState.Friendly;
                }
                else
                {
                    return CellState.Enemy;
                }
            }
            return CellState.Free;
        }
        else
        {
            return CellState.OutOfBounds;
        }

        
    }


    public bool InBoard (int xPosInArray, int yPosInArray)
    {
        //Bounds Check
        if (xPosInArray < 0 || xPosInArray > (mBoardCells.GetLength(0) - 1))
            return false;

        if (yPosInArray < 0 || yPosInArray > mBoardCells.GetLength(1) - 1)
            return false;

        return true;

    }

}
