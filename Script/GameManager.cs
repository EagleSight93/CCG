using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2Int boardSize;

    void Start()
    {
        //Setup board
        Board.gameBoard.Build(boardSize.x, boardSize.y);
        //Setup Pieces
        EntityManager.manager.GameSetup();
    }

 
}
