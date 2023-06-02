using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public GameObject mCardPrefab;
    public GameObject mDeckCardPrefab;
    public GameObject mCollectionPanelPrefab;
    public GameObject mDeckPanelPrefab;


    public GameObject mCollectionPanel;
    public GameObject mDeckPanel;

    List<BaseCard> mPlayerCollection = new List<BaseCard>();
    Dictionary<string, int> mCollectionCardQuantity = new Dictionary<string, int>();

    List<BaseCard> mPlayerDeck = new List<BaseCard>();

    private void Start()
    {
        print("starting collection");
        //Get player Collection
        mPlayerCollection = PlayerData.playerData.GetPlayerCollection();
        //Get card Collection quantity
        mCollectionCardQuantity = PlayerData.playerData.GetCollectionQuantity();
        //Set collection panel
        SetCollectionPanel();

        //Get player Deck
        if (PlayerData.playerData.GetPlayerDeck().Count() > 0)
        {
            //Read deck list
            foreach (BaseCard card in PlayerData.playerData.GetPlayerDeck())
            {
                //Add the cards to deck
                AddToDeck(card.mCardId);
            }
        }
 
    }

    #region CardManagement
    public void AddToDeck(string cardId)
    {
        //Find card in dictionary
        BaseCard toAdd = PlayerData.mCardDictionary[cardId];

        //If adding leader
        if (toAdd.mType.Equals("Leader"))
        {
            //Hide leader cards
            HideLeaders();

            //Hide opposite team cards
            HideOppositeTeam(cardId);

            //Find in collectionPanel
            Transform inCollectionObject = mCollectionPanel.transform.Find(cardId);

            //Get Component
            CollectionCard inCollection = inCollectionObject.GetComponent<CollectionCard>();

            //If there are cards avaible in collection
            if (inCollection.mQuantity > 0)
            {
                //substact 1 from quantity
                inCollection.SetQuantity(inCollection.mQuantity - 1);

                //Update deck list
                UpdateDeck(cardId);

            }

            return;
        }


        //If deck has a card
        if (mPlayerDeck.Count > 0)
        {

            //Find in collectionPanel
            Transform inCollectionObject = mCollectionPanel.transform.Find(cardId);

            //Get Component
            CollectionCard inCollection = inCollectionObject.GetComponent<CollectionCard>();

            //If there are cards avaible in collection
            if (inCollection.mQuantity > 0)
            {
                //substact 1 from quantity
                inCollection.SetQuantity(inCollection.mQuantity - 1);

                //Update deck list
                UpdateDeck(cardId);

            }

        }

    }

    public void RemoveFromDeck(string cardId)
    {
        //Find card in dictionary
        BaseCard toRemove = PlayerData.mCardDictionary[cardId];

        //If deck is not empty, and removing a leader
        if (mPlayerDeck.Count > 0 && toRemove.mType.Equals("Leader"))
        {
            //find in deckPanel
            Transform inDeckObject = mDeckPanel.transform.Find(cardId);

            //Get component 
            DeckCard inDeck = inDeckObject.GetComponent<DeckCard>();

            //Set quantity to 0 to destroy zoom cards if there was one showing up
            inDeck.SetQuantity(0);

            //Set Empty deck list 
            mPlayerDeck.Clear();

            //Reload Panels
            ReloadCollectionPanel();
            ReloadDeckPanel();

            return;
        }

        //Find card in deck list
        toRemove = mPlayerDeck.Find(c => c.mCardId == cardId);

        if (toRemove != null)
        {
            //Remove card from list
            mPlayerDeck.Remove(toRemove);

            //Search for card in Deck Panel 
            Transform inDeckObject = mDeckPanel.transform.Find(cardId);

            //Get component 
            DeckCard inDeck = inDeckObject.GetComponent<DeckCard>();

            //Remove 1 cards , SetQuantity manages card graphic behaviour
            inDeck.SetQuantity(inDeck.mQuantity - 1);

            //search for card in Collection Panel
            Transform inCollectionObject = mCollectionPanel.transform.Find(cardId);

            //Get component 
            CollectionCard inCollection = inCollectionObject.GetComponent<CollectionCard>();

            //Add 1 card , SetQuantity manages card graphic behaviour
            inCollection.SetQuantity(inCollection.mQuantity + 1);

        }
    }

    public void UpdateDeck(string cardId)
    {
        //Find card in dictionary
        BaseCard toAdd = PlayerData.mCardDictionary[cardId];

        //Try to find in deckPanel
        Transform inDeckObject = mDeckPanel.transform.Find(cardId);

        //If it already exist
        if (inDeckObject)
        {
            //Get Component
            DeckCard inDeck = inDeckObject.GetComponent<DeckCard>();

            if (inDeck.mQuantity < 3)
            {
                //Add 1 to quantity
                inDeck.SetQuantity(inDeck.mQuantity + 1);

                //Add to decklist
                mPlayerDeck.Add(toAdd);

                //Sort By Cost
                mPlayerDeck = mPlayerDeck.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();

            }

            return;

        }

        //Add card to decklist
        mPlayerDeck.Add(toAdd);

        //Sort By Cost and name
        mPlayerDeck = mPlayerDeck.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();

        //Reload the deck panel with changes
        ReloadDeckPanel();

    }
    #endregion

    #region UiManagement
    void ReloadDeckPanel()
    {
        //duplicate deck panel object
        GameObject newDeckPanel = Instantiate(mDeckPanelPrefab, mDeckPanel.transform.parent);
        newDeckPanel.transform.position = mDeckPanel.transform.position;

        //Set parent panel Scroll Component
        mDeckPanel.transform.parent.gameObject.GetComponent<ScrollRect>().content = newDeckPanel.GetComponent<RectTransform>();

        //Switch old panel with new one
        GameObject oldDeckPanel = mDeckPanel;
        Destroy(oldDeckPanel);
        mDeckPanel = newDeckPanel;

        //Setup panel again
        SetDeckPanel();

    }

    void ReloadCollectionPanel()
    {
        //duplicate deck panel object
        GameObject newCollectionPanel = Instantiate(mCollectionPanelPrefab, mCollectionPanel.transform.parent);
        newCollectionPanel.transform.position = mCollectionPanel.transform.position;

        //Set parent panel Scroll Component
        mCollectionPanel.transform.parent.gameObject.GetComponent<ScrollRect>().content = newCollectionPanel.GetComponent<RectTransform>();

        //Switch old panel with new one
        GameObject oldCollectionPanel = mCollectionPanel;
        Destroy(oldCollectionPanel);
        mCollectionPanel = newCollectionPanel;

        //Setup panel again
        SetCollectionPanel();
    }

    private void SetCollectionPanel()
    {
        foreach (BaseCard collectionCard in mPlayerCollection)
        {
            //Search if this is not the only card in the collection panel
            Transform inCollectionObject = mCollectionPanel.transform.Find(collectionCard.mCardId);

            //If this card is not unique
            if (inCollectionObject)
            {
                //Get CollectionCard Component
                CollectionCard inCollection = inCollectionObject.GetComponent<CollectionCard>();

                //Add a new copy to the collection card object
                inCollection.SetQuantity(inCollection.mQuantity + 1);

                //Next iteration
                continue;
            }

            //Create new card, set name object to cardID
            GameObject newCardObject = Instantiate(mCardPrefab, mCollectionPanel.transform);
            newCardObject.name = collectionCard.mCardId;

            //Get CollectionCard Component
            CollectionCard newCard = newCardObject.GetComponent<CollectionCard>();

            //Setup Collection Card data
            newCard.Setup(collectionCard, mCollectionCardQuantity[collectionCard.mCardId], this);

        }
    }

    public void SetDeckPanel()
    {
        //If there are cards in deck list
        if(mPlayerDeck.Count() > 0)
        {
            //Read deck list and add to panel
            foreach (BaseCard deckCard in mPlayerDeck)
            {
                //Try to find in deckPanel
                Transform inDeckObject = mDeckPanel.transform.Find(deckCard.mCardId);

                //If it already exist
                if (inDeckObject)
                {
                    //Get Component
                    DeckCard inDeck = inDeckObject.GetComponent<DeckCard>();

                    //Add 1 to quantity
                    inDeck.SetQuantity(inDeck.mQuantity + 1);

                    continue;

                }

                //Create new deck card
                GameObject newCardObject = Instantiate(mDeckCardPrefab, mDeckPanel.transform);
                newCardObject.name = deckCard.mCardId;

                //Get Component
                DeckCard newCard = newCardObject.GetComponent<DeckCard>();

                //Setup Deck Card data
                newCard.Setup(deckCard, this);

            }
        }


    }

    void HideLeaders()
    {
        //Make a list of all other leaders in collection
        List<string> leadersList = new List<string>();

        foreach (BaseCard collectionCard in mPlayerCollection)
        {
            //If cards is of leader type and is not the selected leader
            if (collectionCard.mType.Equals("Leader"))
            {
                //leadersList.Add(collectionCard.mCardId);
                mCollectionPanel.transform.Find(collectionCard.mCardId).gameObject.SetActive(false);

            }
        }
    }

    void HideOppositeTeam(string cardId)
    {
        foreach (Transform card in mCollectionPanel.transform)
        {
            //Change the index dependence for type "Leader" depence
            if (card.GetSiblingIndex() > 1 &&
                !card.gameObject.GetComponent<CollectionCard>().mCardId.Substring(0, 1).Equals(cardId.Substring(0, 1)))
                card.gameObject.SetActive(false);
        }
    }

    public void SaveDeckButton()
    {
        PlayerData.playerData.SaveDeck(mPlayerDeck);
    }
    #endregion


}



