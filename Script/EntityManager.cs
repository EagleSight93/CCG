using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
//using Unity.Collections;
//using UnityEditor.EditorTools;

using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.XR;

//Singleton class
public class EntityManager : MonoBehaviour
{
    [HideInInspector]
    public static EntityManager manager;

    //Prefabs
    public GameObject mHandAreaPrefab;
    public GameObject mHandCardPrefab;
    public GameObject mUnitPrefab;
    public GameObject mCellPrefab;

    //References
    public GameObject mAllUnitsObject;
    public GameObject mUnitCardsArea;
    public GameObject mPlayerDeckArea, mOpponentDeckArea;
    public GameObject mPlayerHandArea, mOpponentHandArea;
    public GameObject mPlayerArea;
    public GameObject mPlayerEnergyPanel;

    //Player Data
    private List<BaseCard> mPlayerDeckData = new List<BaseCard>();
    private List<BaseCard> mOpponentDeckData = new List<BaseCard>();

    //Player Game Data
    private Stack<GameObject> mPlayerDeck = new Stack<GameObject>();
    public List<BaseUnit> mPlayerUnits = new List<BaseUnit>();
    public List<GameObject> mHandList = new List<GameObject>();

    //Opponent Game Data
    private Stack<GameObject> mOpponentDeck = new Stack<GameObject>();
    public List<BaseUnit> mOpponentUnits = new List<BaseUnit>();
    public List<GameObject> mOpponentHandList = new List<GameObject>();

    //General Game Data
    public Color bluePlayerColor;
    public Color redPlayerColor;
    public Color depletedUnitColor;
    public int startingEnergy;
    public int startingHandSize;

    List<EnergyCell> eCells = new List<EnergyCell>();
    public int currentEnergy = 0;
    public int maxPlayerEnergy = 0;
    public bool ableToBuild;
    public bool movingUnit;
    public bool targetingEnemy;
    
    public HandCard selectedCard;
    public BaseUnit selectedUnit;

    public BaseUnit targetUnit;
    public Cell targetCell;
    
    public void Awake()
    {
        //If this is not only instace
        if (manager != null)
        {
            //destroy this instance
            Destroy(gameObject);
            return;
        }

        //Singleton reference
        manager = this;
        //disable building
        ableToBuild = false;
        //Set moving unit 
        movingUnit = false;

    }


    public void GameSetup()
    {
        //Get player deck data
        mPlayerDeckData = PlayerData.playerData.GetPlayerDeck();
        mOpponentDeckData = PlayerData.playerData.GetPlayerDeck();

        //Build deck
        BuildDeck(mPlayerDeckData ,"Player");
        BuildDeck(mOpponentDeckData, "Opponent");

        //Deal Starting hand
        DrawCards(startingHandSize,"Player");
        DrawCards(startingHandSize, "Opponent");


        //Random select player Zones


        //


        //Set up Player Leader
        foreach (BaseCard deckCard in mPlayerDeckData)
        {
            //If it is a leader card
            if (deckCard.mType.Equals("Leader"))
            {
                //Build on player starting position
                BaseUnit leader = SpawnUnit(deckCard, "Player",0, 2);
                leader.gameObject.GetComponent<UnitInteractable>().Refresh();

            }

        }

        //set player energy
        SetPlayerEnergy(startingEnergy);



        /*
        //TEST TEST TEST TEST TEST
        BaseUnit leader2 = SpawnUnit(deckCard, "Opponent", 2, 2);
        leader2.gameObject.GetComponent<UnitInteractable>().Refresh();
        //TEST TEST TEST TEST TEST
        */

    }



    private void BuildDeck(List<BaseCard> deckList, string player)
    {
        List<BaseCard> tmpShuffledDeckList = deckList;

        //Shuffle list
        System.Random random = new System.Random();
        int n = tmpShuffledDeckList.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            BaseCard value = tmpShuffledDeckList[k];
            tmpShuffledDeckList[k] = tmpShuffledDeckList[n];
            tmpShuffledDeckList[n] = value;
        }

        //Build Deck
        foreach (BaseCard deckCard in tmpShuffledDeckList)
        {
            //If not a leader card
            if (!deckCard.mType.Equals("Leader"))
            {
                //set at player or opponent side
                if (player.Equals("Player"))
                {
                    //Create new card, set name object to cardID
                    GameObject newCardObject = Instantiate(mHandCardPrefab, mPlayerDeckArea.transform);
                    newCardObject.name = newCardObject.transform.GetSiblingIndex() + 1 + "_" + deckCard.mCardId;

                    //Set Position
                    newCardObject.GetComponent<RectTransform>().anchoredPosition = new Vector2Int(-320, 0);

                    //Push to PlayerStack
                    mPlayerDeck.Push(newCardObject);

                    //Get HandCard Component
                    HandCard newCard = newCardObject.GetComponent<HandCard>();

                    //Setup Collection Card data
                    newCard.Setup(deckCard);
                }
                else
                {
                    //Create new card, set name object to cardID
                    GameObject newCardObject = Instantiate(mHandCardPrefab, mOpponentDeckArea.transform);
                    newCardObject.name = newCardObject.transform.GetSiblingIndex() + 1 + "_" + deckCard.mCardId;

                    //Set Position
                    newCardObject.GetComponent<RectTransform>().anchoredPosition = new Vector2Int(-320, 0);
                    
                    //Push to PlayerStack
                    mOpponentDeck.Push(newCardObject);

                    //Get HandCard Component
                    HandCard newCard = newCardObject.GetComponent<HandCard>();

                    //Setup Collection Card data
                    newCard.Setup(deckCard);
                }

            }

        }

    }

    private void SetPlayerEnergy(int energy)
    {
        //clear Player energy list
        eCells.Clear();

        //Drain energy surplus
        currentEnergy = 0;

        //disable all energy cells and add them to player energy cell list
        foreach (Transform eCell in mPlayerEnergyPanel.transform.GetChild(1).transform)
        {
            eCell.gameObject.SetActive(false);
            eCells.Add(eCell.GetComponent<EnergyCell>());
        }

        //set active as much as energy is abaible
        for (int e = 0; e < energy; e++)
        {
            eCells[e].gameObject.SetActive(true);
            eCells[e].Charge();
        }

        //Save max energy
        maxPlayerEnergy = energy;

        //set Current and max enery text
        mPlayerEnergyPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = currentEnergy.ToString();
        mPlayerEnergyPanel.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = maxPlayerEnergy.ToString();


    }

    public void EndTurn()
    {
        //Draw a card if able, then fix hand
        DrawCards(1,"Player");

        //Refresh energycells and upgrade capacity by 1 if not maxed out (number of Ecells GO)
        if (maxPlayerEnergy != mPlayerEnergyPanel.transform.GetChild(1).childCount)
        {
            SetPlayerEnergy(maxPlayerEnergy + 1);
        }
        else
        {
            //set active as much as energy is abaible
            for (int e = 0; e < maxPlayerEnergy; e++)
            {
                eCells[e].gameObject.SetActive(true);
                eCells[e].Charge();
            }
        }

        //Refresh Units actions
        foreach (BaseUnit unit in mPlayerUnits)
        {
            unit.GetComponent<UnitInteractable>().Refresh();
        }


        //lock all player actions





    }

    private void DrawCards(int quantity, string player)
    {
        if (player.Equals("Player"))
        {
            //if hand is not maxed out
            if (mHandList.Count < 6)
            {
                int goalSize = mHandList.Count() + quantity;

                while (goalSize > mHandList.Count())
                {
                    //Get a card from stack
                    GameObject card = mPlayerDeck.Pop();

                    //Add to hand array
                    mHandList.Add(card);

                }

                FixHand("Player");
            }
        }
        else
        {
            print(mOpponentDeck.Peek().name);

            //if hand is not maxed out
            if (mOpponentHandList.Count < 6)
            {
                int goalSize = mOpponentHandList.Count() + quantity;

                while (goalSize > mOpponentHandList.Count())
                {
                    //Get a card from stack
                    GameObject card = mOpponentDeck.Pop();

                    //Add to hand array
                    mOpponentHandList.Add(card);

                }

                FixHand("Opponent");
            }
        }



        
    }

    public void FixHand(string player)
    {

        if (player.Equals("Player"))
        {
            //duplicate hand panel object
            GameObject newHandArea = Instantiate(mHandAreaPrefab, mPlayerHandArea.transform.parent);
            newHandArea.transform.position = mPlayerHandArea.transform.position;

            //Set i position multiplier
            int i = 0;
            //Fix cards position
            foreach (GameObject card in mHandList)
            {
                //move to player hand
                card.transform.SetParent(newHandArea.transform);
                //X = (Card counter * revealed part of card) + (half of revealed card * (max hand size - current hand size))
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2Int((i * 182) + (91 * (6 - mHandList.Count())), -260);
                i++;
            }

            //Switch old hand with new one
            GameObject oldHand = mPlayerHandArea;
            Destroy(oldHand);
            mPlayerHandArea = newHandArea;
        }
        else
        {
            //duplicate hand panel object
            GameObject newHandArea = Instantiate(mHandAreaPrefab, mOpponentHandArea.transform.parent);
            newHandArea.transform.position = mOpponentHandArea.transform.position;

            //Set i position multiplier
            int i = 0;
            //Fix cards position
            foreach (GameObject card in mOpponentHandList)
            {
                //move to player hand
                card.transform.SetParent(newHandArea.transform);
                //X = (Card counter * revealed part of card) + (half of revealed card * (max hand size - current hand size))
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2Int((i * 182) + (91 * (6 - mHandList.Count())), -260);
                i++;
            }

            //Switch old hand with new one
            GameObject oldHand = mOpponentHandArea;
            Destroy(oldHand);
            mOpponentHandArea = newHandArea;
        }

    }


    //spawn with no card reference (get's built here)
    public BaseUnit SpawnUnit(BaseCard card, string team, int row, int column)
    {
        //CREATE CARD
        //Create new card, set name object to cardID
        GameObject newCardObject = Instantiate(mHandCardPrefab, mUnitCardsArea.transform);
        newCardObject.name = card.mCardId;

        //Set parent and reset postion
        newCardObject.transform.SetParent(mUnitCardsArea.transform);
        newCardObject.GetComponent<RectTransform>().anchoredPosition = Vector2Int.zero;
        
        //Get CollectionCard Component
        HandCard newCard = newCardObject.GetComponent<HandCard>();

        //Setup Collection Card data
        newCard.Setup(card);


        //CREATE UNIT
        //Create new unit, set name object to cardID
        GameObject newUnitObject = Instantiate(mUnitPrefab, mAllUnitsObject.transform);
        newUnitObject.name = card.mCardId;

        //Set unit Type
        BaseUnit newUnit = (BaseUnit)newUnitObject.AddComponent(Type.GetType(card.mCardId));

        //Setup BaseUnit data
        newUnit.Setup(newCardObject, team);

        //Spawn on Board
        Cell destinyCell = Board.gameBoard.mBoardCells[row, column];
        newUnit.Place(destinyCell);

        return newUnit;

    }

    //If Building a card reference is needed (only a player can do this)
    public BaseUnit BuildUnit()
    {
        //Create a new card, set object name to cardID
        GameObject newUnitObject = Instantiate(mUnitPrefab, mAllUnitsObject.transform);
        newUnitObject.name = newUnitObject.transform.GetSiblingIndex()+ "_" + selectedCard.mData.mCardId;

        //Set unit Type
        BaseUnit newUnit = (BaseUnit)newUnitObject.AddComponent(Type.GetType(selectedCard.mData.mCardId));

        //Setup BaseUnit data
        newUnit.Setup(selectedCard.gameObject, "Player");

        //Spawn on Board
        Cell destinyCell = Board.gameBoard.mBoardCells[targetCell.mPositionInArray.x, targetCell.mPositionInArray.y];
        newUnit.Place(destinyCell);

        return newUnit;

    }

    //Coroutines
    public IEnumerator Summon(BaseCard cardData)
    {
        //create list of highlighted cells
        List<Cell> toHighlight = new List<Cell>();

        //List of energycells to pay
        List<EnergyCell> tmpPayment = new List<EnergyCell>();

        //only if can be paid
        if (cardData.mCost <= currentEnergy)
        {
            //Mark energy cells to spent
            for (int e = eCells.Count() - 1; e >= 0; e--)
            {
                //if position in between current energy and diference with unit cost
                if (eCells[e].charged && ((currentEnergy - 1 >= e) && (e >= currentEnergy - cardData.mCost)))
                {
                    //make cells blink and add them to payment list
                    eCells[e].StartCoroutine("Blink");
                    tmpPayment.Add(eCells[e]);
                }
                    
            }


            //for each friendly unit
            foreach (BaseUnit unit in mPlayerUnits)
            {

                //get unit script to get current cell data (last 5 chars)
                string componentName = unit.name.Substring(unit.name.Length - 5);
                BaseUnit unitData = (BaseUnit)unit.GetComponent(componentName);

                //Get position          
                int xPostion = unitData.mCurrentCell.mPositionInArray.x;
                int yPostion = unitData.mCurrentCell.mPositionInArray.y;

                //check surrounding cells

                //up
                if (Board.gameBoard.ValidateCell(xPostion, yPostion + 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list, then paint
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion, yPostion + 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion + 1]);
                        Board.gameBoard.mBoardCells[xPostion, yPostion + 1].SpawnPaint();
                    }
                    
                }

                //down 
                if (Board.gameBoard.ValidateCell(xPostion, yPostion - 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion, yPostion - 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion - 1]);
                        Board.gameBoard.mBoardCells[xPostion, yPostion - 1].SpawnPaint();
                    }
                        
                }

                //right
                if (Board.gameBoard.ValidateCell(xPostion + 1, yPostion, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion + 1, yPostion]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + 1, yPostion]);
                        Board.gameBoard.mBoardCells[xPostion + 1, yPostion].SpawnPaint();
                    }
                        
                }

                //left
                if (Board.gameBoard.ValidateCell(xPostion - 1, yPostion, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion - 1, yPostion]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - 1, yPostion]);
                        Board.gameBoard.mBoardCells[xPostion - 1, yPostion].SpawnPaint();
                    }
                        
                }

                //upper right
                if (Board.gameBoard.ValidateCell(xPostion + 1, yPostion + 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion + 1, yPostion + 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + 1, yPostion + 1]);
                        Board.gameBoard.mBoardCells[xPostion + 1, yPostion + 1].SpawnPaint();
                    }
                        
                }

                //upper left
                if (Board.gameBoard.ValidateCell(xPostion - 1, yPostion + 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion - 1, yPostion + 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - 1, yPostion + 1]);
                        Board.gameBoard.mBoardCells[xPostion - 1, yPostion + 1].SpawnPaint();
                    }
                        
                }

                //lower right
                if (Board.gameBoard.ValidateCell(xPostion + 1, yPostion - 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion + 1, yPostion - 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + 1, yPostion - 1]);
                        Board.gameBoard.mBoardCells[xPostion + 1, yPostion - 1].SpawnPaint();
                    }
                        
                }

                //lower left
                if (Board.gameBoard.ValidateCell(xPostion - 1, yPostion - 1, unitData) == CellState.Free)
                {
                    // if not in array add to occupied cells list
                    if (!toHighlight.Contains(Board.gameBoard.mBoardCells[xPostion - 1, yPostion - 1]))
                    {
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - 1, yPostion - 1]);
                        Board.gameBoard.mBoardCells[xPostion - 1, yPostion - 1].SpawnPaint();
                    }
                        
                }

            }

            //While able to build
            while (ableToBuild)
            {
                //if oressed left click
                if (Input.GetMouseButtonDown(1))
                {
                    print("Cancelled");

                    foreach (EnergyCell e in tmpPayment)
                    {
                        e.StopCoroutine("Blink");
                        e.gameObject.GetComponent<Image>().color = e.avaibleColor;
                    }

                    ableToBuild = false;
                }

                //If a cell was selected
                if (targetCell != null)
                {
                    //if pressed cell was highlighted    
                    if (toHighlight.Contains(targetCell))
                    {
                        //Build unit in target cell
                        BuildUnit();

                        //Remove from Hand list
                        mHandList.Remove(selectedCard.gameObject);

                        //Fix hand again
                        FixHand("Player");

                        //Parent this card to collection Area and reset position
                        selectedCard.gameObject.transform.SetParent(mUnitCardsArea.transform);
                        selectedCard.GetComponent<RectTransform>().anchoredPosition = Vector2Int.zero;
                        selectedCard.GetComponent<Canvas>().enabled = true;


                        //Pay for Build
                        foreach (EnergyCell e in tmpPayment)
                        {
                            e.StopCoroutine("Blink");
                            e.Use();
                        }

                        //refresh current energy text
                        mPlayerEnergyPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = currentEnergy.ToString();

                        //disable building to exit loop
                        ableToBuild = false;
                    }
                    else
                    {
                        print("Cannot build there");

                        foreach (EnergyCell e in tmpPayment)
                        {
                            e.StopCoroutine("Blink");
                            e.gameObject.GetComponent<Image>().color = e.avaibleColor;
                        }

                        //disable building to exit loop
                        ableToBuild = false;
                    }

                }

                yield return null;
            }

            //Unlit each lit cell 
            foreach (Cell cell in toHighlight)
            {
                cell.StartingColor();
            }

        }
        else
        {
            //if can't pay
            print("Can't Pay for that");

            //disable building
            ableToBuild = false;

            //retun energy cells to lit color
            foreach (EnergyCell e in tmpPayment)
            {
                e.StopCoroutine("Blink");
                e.gameObject.GetComponent<Image>().color = e.avaibleColor;
            }
        }

        //Hide selected card border if active
        selectedCard.transform.GetChild(0).gameObject.SetActive(false);

        //Reset parameters
        selectedCard = null;
        targetCell = null;

    }

    public IEnumerator MoveOrFight()
    {
        //create list of highlighted cells
        List<Cell> toHighlight = new List<Cell>();

        //Get unit positon position          
        int xPostion = selectedUnit.mCurrentCell.mPositionInArray.x;
        int yPostion = selectedUnit.mCurrentCell.mPositionInArray.y;

        //get list of possible destiny cells 

        //Horizontal Movement
        for (int i = 1; i <= selectedUnit.mMovement.x; i++)
        {
            CellState cellState = Board.gameBoard.ValidateCell(xPostion + i, yPostion, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion]);

            }

        }

        for (int i = 1; i <= selectedUnit.mMovement.x; i++)
        {
            CellState cellState = Board.gameBoard.ValidateCell(xPostion - i, yPostion, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion]);

            }

        }

        //Vertical Movement
        for (int i = 1; i <= selectedUnit.mMovement.y; i++)
        {
            CellState cellState = Board.gameBoard.ValidateCell(xPostion, yPostion + i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion, yPostion + i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion + i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion, yPostion + i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion + i]);

            }

        }

        for (int i = 1; i <= selectedUnit.mMovement.y; i++)
        {
            CellState cellState = Board.gameBoard.ValidateCell(xPostion, yPostion - i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion, yPostion - i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion - i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion, yPostion - i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion - i]);

            }

        }

        //Diagonal Movement
        //Upper Left
        for (int i = 1; i <= selectedUnit.mMovement.z; i++)
        {

            //if corner is surrounded by enemies 
            if (i == 1)
            {
                CellState cellStateBack = Board.gameBoard.ValidateCell(xPostion - i, yPostion, selectedUnit);
                CellState cellStateUp = Board.gameBoard.ValidateCell(xPostion, yPostion + i, selectedUnit);

                if (cellStateBack == CellState.Enemy && cellStateUp == CellState.Enemy)
                {
                    Board.gameBoard.mBoardCells[xPostion - i, yPostion].EnemyPaint();
                    Board.gameBoard.mBoardCells[xPostion, yPostion + i].EnemyPaint();

                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion]);
                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion + i]);

                    CellState cornerCellState = Board.gameBoard.ValidateCell(xPostion - i, yPostion + i, selectedUnit);

                    if (cornerCellState == CellState.Enemy)
                    {
                        Board.gameBoard.mBoardCells[xPostion - i, yPostion + i].EnemyPaint();
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion + i]);

                    }

                    break;

                }

            }

            CellState cellState = Board.gameBoard.ValidateCell(xPostion - i, yPostion + i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion + i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion + i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion + i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion + i]);

            }

        }

        //Upper Right
        for (int i = 1; i <= selectedUnit.mMovement.z; i++)
        {

            //if corner is surrounded by enemies 
            if (i == 1)
            {
                CellState cellStateBack = Board.gameBoard.ValidateCell(xPostion + i, yPostion, selectedUnit);
                CellState cellStateUp = Board.gameBoard.ValidateCell(xPostion, yPostion + i, selectedUnit);

                if (cellStateBack == CellState.Enemy && cellStateUp == CellState.Enemy)
                {
                    Board.gameBoard.mBoardCells[xPostion + i, yPostion].EnemyPaint();
                    Board.gameBoard.mBoardCells[xPostion, yPostion + i].EnemyPaint();

                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion]);
                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion + i]);

                    CellState cornerCellState = Board.gameBoard.ValidateCell(xPostion + i, yPostion + i, selectedUnit);

                    if (cornerCellState == CellState.Enemy)
                    {
                        Board.gameBoard.mBoardCells[xPostion + i, yPostion + i].EnemyPaint();
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion + i]);

                    }

                    break;

                }

            }

            CellState cellState = Board.gameBoard.ValidateCell(xPostion + i, yPostion + i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion + i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion + i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion + i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion + i]);

            }

        }

        //Lower Left
        for (int i = 1; i <= selectedUnit.mMovement.z; i++)
        {

            //if corner is surrounded by enemies 
            if (i == 1)
            {
                CellState cellStateBack = Board.gameBoard.ValidateCell(xPostion - i, yPostion, selectedUnit);
                CellState cellStateUp = Board.gameBoard.ValidateCell(xPostion, yPostion - i, selectedUnit);

                if (cellStateBack == CellState.Enemy && cellStateUp == CellState.Enemy)
                {
                    Board.gameBoard.mBoardCells[xPostion - i, yPostion].EnemyPaint();
                    Board.gameBoard.mBoardCells[xPostion, yPostion - i].EnemyPaint();

                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion]);
                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion - i]);

                    CellState cornerCellState = Board.gameBoard.ValidateCell(xPostion - i, yPostion - i, selectedUnit);

                    if (cornerCellState == CellState.Enemy)
                    {
                        Board.gameBoard.mBoardCells[xPostion - i, yPostion - i].EnemyPaint();
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion - i]);

                    }

                    break;

                }

            }

            CellState cellState = Board.gameBoard.ValidateCell(xPostion - i, yPostion - i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion - i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion - i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion - i, yPostion - i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion - i, yPostion - i]);

            }

        }

        //Lower Right
        for (int i = 1; i <= selectedUnit.mMovement.z; i++)
        {

            //if corner is surrounded by enemies 
            if (i == 1)
            {
                CellState cellStateBack = Board.gameBoard.ValidateCell(xPostion + i, yPostion, selectedUnit);
                CellState cellStateUp = Board.gameBoard.ValidateCell(xPostion, yPostion - i, selectedUnit);

                if (cellStateBack == CellState.Enemy && cellStateUp == CellState.Enemy)
                {
                    Board.gameBoard.mBoardCells[xPostion + i, yPostion].EnemyPaint();
                    Board.gameBoard.mBoardCells[xPostion, yPostion - i].EnemyPaint();

                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion]);
                    toHighlight.Add(Board.gameBoard.mBoardCells[xPostion, yPostion - i]);

                    CellState cornerCellState = Board.gameBoard.ValidateCell(xPostion + i, yPostion - i, selectedUnit);

                    if (cornerCellState == CellState.Enemy)
                    {
                        Board.gameBoard.mBoardCells[xPostion + i, yPostion - i].EnemyPaint();
                        toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion - i]);

                    }

                    break;

                }

            }

            CellState cellState = Board.gameBoard.ValidateCell(xPostion + i, yPostion - i, selectedUnit);

            //paint cell according to cellstate and add to cell list
            if (cellState == CellState.Enemy)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion - i].EnemyPaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion - i]);
                break;

            }

            if (cellState == CellState.Free)
            {
                Board.gameBoard.mBoardCells[xPostion + i, yPostion - i].MovePaint();
                toHighlight.Add(Board.gameBoard.mBoardCells[xPostion + i, yPostion - i]);

            }

        }

        //Check player input
        while (movingUnit)
        {
            UnitInteractable interactable = selectedUnit.GetComponent<UnitInteractable>();

            //if pressed left click
            if (Input.GetMouseButtonDown(1))
            {
                print("Cancelled");
                interactable.StopCoroutine("Blink");
                selectedUnit.gameObject.GetComponentInChildren<Image>().color = selectedUnit.avaibleColor;
                movingUnit = false;
                continue;
            }

            //if selected an unit
            if (targetUnit != null)
            {
                //if that unit was on range
                if (Vector3.Distance(selectedUnit.transform.position, targetUnit.transform.position) < 185)
                {
                    //Stop Unit Blinking
                    interactable.StopCoroutine("Blink");

                    //combat
                    targetUnit.damage(selectedUnit.mPower);
                    selectedUnit.damage(targetUnit.mPower);

                    //use movement
                    interactable.Fight();

                    print("touched me " + targetUnit.name);

                    //disable Moving to exit loop
                    movingUnit = false;
                }

                targetUnit = null;

                continue;
            }

            //If a cell was selected
            if (targetCell != null)
            {
                //if pressed cell was highlighted    
                if (toHighlight.Contains(targetCell))
                {
                    //move to target cell
                    selectedUnit.Place(targetCell);

                    //Stop Unit Blinking
                    interactable.StopCoroutine("Blink");

                    //use movement
                    interactable.Move();

                    //disable Moving to exit loop
                    movingUnit = false;
                }

                targetCell = null; 
            }

            yield return null;
        }

        //Unlit each lit cell 
        foreach (Cell cell in toHighlight)
        {
            cell.StartingColor();
        }
        
    }

    public IEnumerator Fight()
    {

        //Check player input
        while (movingUnit)
        {
            UnitInteractable interactable = selectedUnit.GetComponent<UnitInteractable>();

            //if pressed left click
            if (Input.GetMouseButtonDown(1))
            {
                print("Cancelled");
                interactable.StopCoroutine("Blink");
                selectedUnit.gameObject.GetComponentInChildren<Image>().color = selectedUnit.depletedColor;
                movingUnit = false;
                continue;
            }

            //if selected an unit
            if (targetUnit != null)
            {
                //if that unit was on range
                if (Vector3.Distance(selectedUnit.transform.position, targetUnit.transform.position) < 185)
                {
                    //Stop Unit Blinking
                    interactable.StopCoroutine("Blink");

                    //combat
                    targetUnit.damage(selectedUnit.mPower);
                    selectedUnit.damage(targetUnit.mPower);

                    //Fight interaction
                    interactable.Fight();

                    //disable Moving to exit loop
                    movingUnit = false;
                }

                //if not in range cancell targetUnit
                targetUnit = null;

                continue;
            }

            //If a cell was selected, return to null state
            if (targetCell != null )
            {
                targetCell = null;
            }

            yield return null;
        }

        //Reset parameters
        selectedUnit = null;
       
    }

}