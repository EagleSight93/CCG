using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public string mCardId;
    RectTransform mRectTransform;

    public BaseCard mData;
    public int mIndex;

    private bool raised = false;


    public void Setup(BaseCard card)
    {
        //Get RectTransform Component
        mRectTransform = gameObject.GetComponent<RectTransform>();

        //Set reference to manager and Card ID
        mCardId = card.mCardId;
        mData = card;


        //Set Card art
        transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>("CardArt/" + card.mCardId.Substring(0, 1) + "/" + card.mImgFileName);

        //Set Card name Text
        transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = card.mCardName;

        //Set Card Type
        transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = card.mType;

        //Set Card SubType
        transform.GetChild(8).GetComponent<TextMeshProUGUI>().text = card.mSubType;

        //Set Card Description
        transform.GetChild(9).GetComponent<TextMeshProUGUI>().text = card.mDescription;

        //Set Card Cost
        transform.GetChild(10).GetComponentInChildren<TextMeshProUGUI>().text = card.mCost.ToString();

        //Set Card Power
        transform.GetChild(11).GetComponentInChildren<TextMeshProUGUI>().text = card.mPower.ToString();

        //Set Card Health
        transform.GetChild(12).GetComponentInChildren<TextMeshProUGUI>().text = card.mHealth.ToString();

    }




    #region Events

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !EntityManager.manager.ableToBuild && !EntityManager.manager.movingUnit)
        {
            //Enable Build
            EntityManager.manager.ableToBuild = true;

            //Set this as selectedCard
            EntityManager.manager.selectedCard = this;

            //Show border
            transform.GetChild(0).gameObject.SetActive(true);

            //Start summon coroutine
            EntityManager.manager.StartCoroutine("Summon", mData);

        }

        //drop the card 
        DropCard();

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!raised)
            RaiseCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (raised)
            DropCard();
    }

    private void DropCard()
    {
        mRectTransform.anchoredPosition = new Vector2Int((int)mRectTransform.anchoredPosition.x, -260);
        transform.SetSiblingIndex(mIndex);

        raised = !raised;
    }

    private void RaiseCard()
    {
        //Save handParent & Index
        mIndex = transform.GetSiblingIndex();
        mRectTransform.anchoredPosition = new Vector2Int((int)mRectTransform.anchoredPosition.x, 0);
        transform.SetAsLastSibling();
        raised = !raised;
    }


    #endregion
}
