using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CollectionCard : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public string mCardId;
    public int mQuantity;
    private CollectionManager mCollectionManager;

    public void Setup(BaseCard card,int quantity, CollectionManager collectionManager)
    {
        //Set reference to manager and Card ID
        mCardId = card.mCardId;
        mCollectionManager = collectionManager;

        //Set Card art
        transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("CardArt/" + card.mCardId.Substring(0,1) + "/" +  card.mImgFileName);

        //Set Card name Text
        transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = card.mCardName;

        //Set Card Type
        transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = card.mType;

        //Set Card SubType
        transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = card.mSubType;

        //Set Card Description
        transform.GetChild(8).GetComponent<TextMeshProUGUI>().text = card.mDescription;

        //Set Card Cost
        transform.GetChild(9).GetComponentInChildren<TextMeshProUGUI>().text = card.mCost.ToString();

        //Set Card Power
        transform.GetChild(10).GetComponentInChildren<TextMeshProUGUI>().text = card.mPower.ToString();

        //Set Card Health
        transform.GetChild(11).GetComponentInChildren<TextMeshProUGUI>().text = card.mHealth.ToString();

        //Set Quantity
        SetQuantity(quantity);

    }


    //Set quantity and update card in collection panel
    public void SetQuantity(int q)
    {
        //Set Card Quantity
        mQuantity = q;

        //If more than one card, enable Quantity Text  "Make SWITCH LATER?"
        if (q > 1)
        {
            transform.GetChild(12).gameObject.SetActive(true);
            transform.GetChild(12).GetComponent<TextMeshProUGUI>().text = "X " + mQuantity;
            transform.GetChild(13).gameObject.SetActive(false);
        }

        if (q == 1)
        {
            transform.GetChild(12).gameObject.SetActive(false);
            transform.GetChild(13).gameObject.SetActive(false);
        }

        if (q == 0)
        {
            transform.GetChild(13).gameObject.SetActive(true);
        }
    }


    #region Events
    public void OnPointerClick(PointerEventData eventData)
    {
        //If pressed left click and there are cards left
        if (eventData.button == PointerEventData.InputButton.Left && mQuantity > 0)
        {
            mCollectionManager.AddToDeck(mCardId);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            mCollectionManager.RemoveFromDeck(mCardId);
        }

        #endregion
    }
}
