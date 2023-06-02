using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class DeckCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public string mCardId;
    public int mQuantity;
    private CollectionManager mCollectionManager;
    
    private GameObject zoomedCard;

    public void Setup(BaseCard card, CollectionManager collectionManager)
    {
        //Set reference to manager and Card ID
        mCardId = card.mCardId;
        mCollectionManager = collectionManager;

        //Set Card art
        transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("CardArt/" + card.mCardId.Substring(0, 1) + "/" + card.mImgFileName);

        //Set Card name Text
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.mCardName;

        //If card is leader 
        if (card.mType.Equals("Leader"))
        {
            //Disable Card Cost
            transform.GetChild(2).gameObject.SetActive(false);

            //Enable Crest
            transform.GetChild(4).gameObject.SetActive(true);

            //Get Image
            Image crest = transform.GetChild(4).GetComponent<Image>();

            //Get Team
            string team = card.mCardId.Substring(0, 1);
            
            //Build Crest Route
            string route = "Logos/" + team + "_Crest";
            
            //Set Crest
            crest.sprite = Resources.Load<Sprite>(route);

            //Set Color     NEEDS IMPROVEMENT LATER!!!!!!!!!
            if (team.Equals("A"))
            {
                crest.color = new Color32(255, 195, 10, 255);
            }
            else
            {
                crest.color = new Color32(255, 1, 69, 255);
            }

        }
        else
        {
            //Set Card Cost
            transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = card.mCost.ToString();
        }

        //Set Quantity
        mQuantity = 1;

    }


    //Set quantity and update card in deck panel
    public void SetQuantity(int q)
    {
        //Set Card Quantity
        mQuantity = q;

        //If more than one card, enable Quantity Text  "Make SWITCH LATER?"
        if (q > 1)
        {
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X " + mQuantity;
        }

        if (q == 1)
        {
            transform.GetChild(3).gameObject.SetActive(false);
        }

        if (q == 0)
        {
            if (zoomedCard)
            {
                Destroy(zoomedCard);
            }
            Destroy(this.gameObject);
        }
    }


    #region Events
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && mQuantity < 3)
        {
            mCollectionManager.AddToDeck(mCardId);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            mCollectionManager.RemoveFromDeck(mCardId);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Find card in collectionPanel by transform (to be able to fin disabled GO)
        foreach(Transform zoom in mCollectionManager.mCollectionPanel.transform)
        {
            if (zoom.gameObject.name.Equals(mCardId))
            {
                //Copy GO
                zoomedCard = Instantiate(zoom.gameObject);
            }
        }

        //Set GO active
        zoomedCard.gameObject.SetActive(true);

        //Set Parent and scale
        zoomedCard.transform.SetParent(mCollectionManager.mCollectionPanel.transform.parent);
        

        //Set width and height of rect
        RectTransform cardRect = zoomedCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(290,400);
        zoomedCard.transform.localScale = Vector3.one * 1.3f;

        //disable quanity text and disabled frame
        zoomedCard.transform.GetChild(12).gameObject.SetActive(false);
        zoomedCard.transform.GetChild(13).gameObject.SetActive(false);

        //Set Position
        //cardRect.anchoredPosition = new Vector2Int(1300,650);
        cardRect.anchoredPosition = new Vector2Int(1300,(int)Mathf.Clamp(transform.position.y, 263, 600) );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Destroy zoom card
        Destroy(zoomedCard);
    }


    #endregion
}
