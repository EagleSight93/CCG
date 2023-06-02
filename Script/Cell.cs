using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour , IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public Board mBoard;

    [HideInInspector]
    public Vector2Int mPositionInArray;

    [HideInInspector]
    public RectTransform mRectTransform;

    [HideInInspector]
    public BaseUnit mCurrentUnit;


    private Image cellImage;
    private Color startCellColor;

    public void Setup(Vector2Int cellPosition, Board board)
    {
        //save board and position (ID) in cell array
        mBoard = board;
        mPositionInArray = cellPosition;
        mRectTransform = this.GetComponent<RectTransform>();

        //get image and color references
        cellImage = gameObject.GetComponent<Image>();
        startCellColor = cellImage.color;
    }


    //make a function to get a cell from mouse click
    
    public void OnPointerClick(PointerEventData eventData )
    {
        if ((EntityManager.manager.ableToBuild || EntityManager.manager.movingUnit) && eventData.button == PointerEventData.InputButton.Left)
        {
            EntityManager.manager.targetCell = this;
        }
        
    }


    //Lit when mouse over
    public void OnPointerEnter(PointerEventData eventData)
    {
        Lit();
    }

    //unlit when mouse exits
    public void OnPointerExit(PointerEventData eventData)
    {
        Unlit();
    }


    public void SpawnPaint()
    {
        Color cellColor = Color.green;
        cellImage.color = new Color(cellColor.r, cellColor.g, cellColor.b, startCellColor.a);
    }

    public void MovePaint()
    {
        cellImage.color = new Color(startCellColor.r, startCellColor.g, startCellColor.b, startCellColor.a * 4);
    }

    public void EnemyPaint()
    {
        Color cellColor = Color.red;
        cellImage.color = new Color(cellColor.r, cellColor.g, cellColor.b, startCellColor.a);
    }

    public void StartingColor()
    {
        cellImage.color = startCellColor;
    }


    public void Lit()
    {
        Color cellColor = cellImage.color;
        cellImage.color = new Color(cellColor.r, cellColor.g, cellColor.b, cellColor.a * 2);
    }
    
    public void Unlit()
    {
        Color cellColor = cellImage.color;
        cellImage.color = new Color(cellColor.r, cellColor.g, cellColor.b, cellColor.a * 0.5f);
    }
    


    /*
    
    try to use a coroutine in phase 2

    
    IEnumerator Lit()
    {
        Image cellImage = gameObject.GetComponent<Image>();

        Color cellColor = cellImage.color;

        // loop over 1 second
        for (float i = 0; i <= 0.35f; i += Time.deltaTime)
        {
            print(i);
            // set color with i as alpha
            cellImage.color = new Color(cellColor.r, cellColor.g, cellColor.b, cellColor.a + 0.15f);
            yield return null;
        }
        yield return null;
    }
    */

}
