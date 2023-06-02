using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitInteractable : EventTrigger
{
    public GameObject zoomCard;
    public Vector3 collectionPosition;


    #region Events

    public override void OnPointerClick(PointerEventData eventData)
    {
        //if not building or moving other, left click on this unit
        if (eventData.button == PointerEventData.InputButton.Left && !EntityManager.manager.ableToBuild && !EntityManager.manager.movingUnit)
        {
            //Get this unit baseUnit class with current data
            string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
            BaseUnit unitLiveData = (BaseUnit)gameObject.GetComponent(componentName);

            //if this unit can move and it's mine
            if ((unitLiveData.canMove || unitLiveData.canFight) && unitLiveData.mCurrentTeam.Equals("Player"))
            {
                //start blinking
                StartCoroutine("Blink");

                //Lock Movement
                EntityManager.manager.movingUnit = true;

                //set this as selected Unit
                EntityManager.manager.selectedUnit = unitLiveData;

                //if can't move then it could fight
                if (!unitLiveData.canMove)
                {
                    EntityManager.manager.StartCoroutine("Fight");
                    return;
                }

                //Start move of fight coroutine
                EntityManager.manager.StartCoroutine("MoveOrFight");

                return;

            }

        }


        //if not building and moving other unit, left click on this unit
        if (eventData.button == PointerEventData.InputButton.Left && !EntityManager.manager.ableToBuild && EntityManager.manager.movingUnit)
        {
            string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
            BaseUnit unitLiveData = (BaseUnit)gameObject.GetComponent(componentName);

            if (EntityManager.manager.selectedUnit.canFight && !unitLiveData.mCurrentTeam.Equals("Player"))
            {
                //set this as selected target Unit
                EntityManager.manager.targetUnit = unitLiveData;

            }
        }

    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        
        //Get this unit baseUnit class
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Save collection card start position
        collectionPosition = unitData.CardGO.transform.position;

        //Show card on side of table
        unitData.CardGO.GetComponent<RectTransform>().anchoredPosition = new Vector2Int(-950, 1050);


        //if moving flag is active then spawn damage icon
        if (EntityManager.manager.movingUnit)
        { 
            //If not friendly unit and in range
            if (!unitData.mCurrentTeam.Equals("Player") && Vector3.Distance(EntityManager.manager.selectedUnit.transform.position, transform.position) < 185)
            {
                //Show Attack Icon
                transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
            }
            
        }    
            

    }

    public override void OnPointerExit(PointerEventData eventData)
    {

        //Get this unit baseUnit class
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Reset collection card to start position
        unitData.CardGO.transform.position = collectionPosition;

        //Hide Attack Icon
        transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
    }


    public void Refresh()
    {
        //Get this unit baseUnit class
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Set border color 
        transform.GetComponentInChildren<Image>().color = unitData.avaibleColor;
        //Set image color 
        transform.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.white;

        unitData.canMove = true;
        unitData.canFight = true;
    }

    
    public void Move()
    {
        //Get this unit baseUnit class
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Set border color 
        transform.GetComponentInChildren<Image>().color = unitData.depletedColor;
        unitData.canMove = false;
    }

    //Fight ends movement
    public void Fight()
    {
        //Get this unit baseUnit class
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Set image color 
        transform.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.grey;

        //Set border color 
        transform.GetComponentInChildren<Image>().color = unitData.depletedColor;

        unitData.canFight = false;
        unitData.canMove = false;
    }

    public IEnumerator Blink()
    {
        //counter c
        float c = 0;

        //get unit script to get current cell data (last 5 chars)
        string componentName = gameObject.name.Substring(gameObject.name.Length - 5);
        BaseUnit unitData = (BaseUnit)gameObject.GetComponent(componentName);

        //Always
        while (true)
        {
            //lerp between avaible and depleted w/Mathf PingPong
            Color lerpedColor;
            lerpedColor = Color.Lerp(unitData.depletedColor, unitData.avaibleColor, Mathf.PingPong(c, 1));
            //how fast
            c += 0.025f;
            //Set color
            transform.GetComponentInChildren<Image>().color = lerpedColor;
            //each frame
            yield return null;
        }

    }

    public void StopBlinking()
    {
        StopCoroutine("Blinking");
    }

    #endregion

}
