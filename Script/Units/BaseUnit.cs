using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BaseUnit : MonoBehaviour
{

    [HideInInspector]
    public string mCurrentTeam;
    public Cell mCurrentCell = null;

    //Card Attributes
    public GameObject CardGO;
    protected BaseCard cardData;

    public int mCost;
    public int mPower;
    public int mHealth;



    //Unit Attributes
    public Color avaibleColor;
    public Color depletedColor;

    public Vector3 mMovement;
    public bool canMove;
    public bool canFight;
    public int direction;

    //Visual setup
    public virtual void Setup(GameObject card, string mTeam)
    {

        //Set team
        mCurrentTeam = mTeam;

        //Save card GO
        CardGO = card;

        //Save card data
        cardData = card.GetComponent<HandCard>().mData;

        //set depleted unit color
        depletedColor = EntityManager.manager.depletedUnitColor;

        //Set Border and add to correspondent unit list
        switch (mTeam)
        {
            case "Player":
                //set frame color
                avaibleColor = EntityManager.manager.bluePlayerColor;
                gameObject.GetComponent<UnitInteractable>().Fight();
                //add to player unit list
                EntityManager.manager.mPlayerUnits.Add(this);
                break;

            default:
                //set frame color
                avaibleColor = EntityManager.manager.redPlayerColor;
                gameObject.GetComponent<UnitInteractable>().Fight();
                //add to opponent unit list
                EntityManager.manager.mOpponentUnits.Add(this);
                break;
        }

        //Set Card art
        transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("CardArt/" + cardData.mCardId.Substring(0, 1) + "/" + cardData.mImgFileName);

        //Set Movement
        mMovement = new Vector3(2, 2, 1);
   

        //Set Cost
        mCost = cardData.mCost;

        //Set Power
        SetPower(cardData.mPower);

        //Set Health
        SetHealth(cardData.mHealth);
    }


    public void Place(Cell newCell)
    {
        //if unit was placed, cut reference of cell to this unit
        if (mCurrentCell != null)
            mCurrentCell.mCurrentUnit = null;

        //Connect Unit with Cell
        mCurrentCell = newCell;
        newCell.mCurrentUnit = this;

        //place transform
        transform.position = newCell.transform.position;
        gameObject.SetActive(true);

    }

    public virtual void damage(int damage)
    {
        mHealth -= damage;
        transform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = mHealth.ToString();
    }


    void SetPower(int power)
    {
        mPower = power;
        transform.GetChild(0).GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = power.ToString();

    }
    

    void SetHealth(int hp)
    {
        mHealth = hp;
        transform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = hp.ToString();
    }


    #region Movement
    #endregion

}
