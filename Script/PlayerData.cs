using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerData : MonoBehaviour
{
    public static PlayerData playerData;

    public static Dictionary<string, BaseCard> mCardDictionary = new Dictionary<string, BaseCard>();

    private List<BaseCard> mPlayerCollection = new List<BaseCard>();
    private Dictionary<string, int> mCollectionCardQuantity = new Dictionary<string, int>();

    private List<BaseCard> mPlayerDeck = new List<BaseCard>();

    private string dataDirectory;
    private string deckFileName;



    void Awake()
    {
        //Make singleton
        //If there is already a reference
        if (playerData != null)
        {
            //destroy this instance
            Destroy(gameObject);
            return;
        }

        dataDirectory = Application.dataPath + "/Data";
        deckFileName = "PlayerDeck.csv"; 

        //Set reference for other classes
        playerData = this;

        //Don't destroy between scenes
        DontDestroyOnLoad(this);

        //Check if data directoy exists
        if (!Directory.Exists(dataDirectory))
        {
            //If there isn't one create a directory
            Directory.CreateDirectory(dataDirectory);
        }

        //Load Game Data
        LoadLibrary();

        //Load Player Data
        LoadCollection();//PEDING, SHOULD WORK LIKE LOAD DECK (LOAD A BASIC COLLECTION IF NEW PLAYER)
        LoadDeck();

    }


    #region PlayerDataManagement
    private void LoadLibrary()
    {
        List<BaseCard> mCardLibrary = new List<BaseCard>();

        //Get full collection Data
        TextAsset collectionDataCSV = Resources.Load<TextAsset>("Data/FullCollection");

        //Make an array of strings for each line
        string[] line = collectionDataCSV.text.Split(new char[] { '\n' });

        //Build list of Basecard
        for (int i = 1; i < line.Length; i++)
        {
            //make an array of parts in line (divided by ;)
            string[] part = line[i].Split(new char[] { ';' });

            //Add to full collection list
            mCardLibrary.Add(new BaseCard(part[0].Trim(), part[1].Trim(), part[2].Trim(), part[3].Trim(), part[4].Trim(), part[5].Trim(), Convert.ToInt32(part[06].Trim()), Convert.ToInt32(part[07].Trim()), Convert.ToInt32(part[08].Trim())));
        }

        //Sort By Cost (remove later?)
        mCardLibrary = mCardLibrary.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();

        //Build public dictionary of Basecards
        foreach (BaseCard card in mCardLibrary)
        {
            mCardDictionary.Add(card.mCardId, card);
        }
    }

    private void LoadCollection()
    {
        //Empty current collection
        mPlayerCollection.Clear();

        //Get full collection Data
        TextAsset playerCollectionDataCSV = Resources.Load<TextAsset>("Data/PlayerCollection");

        //Make an array of strings for each line
        string[] line = playerCollectionDataCSV.text.Split(new char[] { '\n' });

        //Build list of Basecard
        for (int i = 1; i < line.Length; i++)
        {
            //make an array of parts in line (divided by ;)
            string[] part = line[i].Split(new char[] { ';' });

            //Add an instance to player collection list from dictionary
            mPlayerCollection.Add(mCardDictionary[part[0].Trim()]);

            //Add to quantity dictionary
            mCollectionCardQuantity.Add(part[0].Trim(), Convert.ToInt32(part[1].Trim()));

        }

        //Sort By Cost and name
        mPlayerCollection = mPlayerCollection.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();
    }

    public void LoadDeck()
    {
        //Empty current deck
        mPlayerDeck.Clear();

        List<string> line = new List<string>();
        string deckFilePath = dataDirectory + "/" + deckFileName;

        if (!File.Exists(deckFilePath))
        {
            print("Loading base deck");
            //Get base deck Data from resources
            TextAsset playerDeckDataCSV = Resources.Load<TextAsset>("Data/BasePlayerDeck");
            //Make a list of strings for each line
            line = playerDeckDataCSV.text.Split(new char[] { '\n' }).ToList();

            //Build list of Basecard
            for (int i = 1; i < line.Count() - 1; i++)
            {
                //make an array of parts in line (divided by ;)
                string[] part = line[i].Split(new char[] { ';' });

                //add as many as in quantity row
                for (int x = 0; x < Convert.ToInt32(part[1].Trim()); x++)
                {
                    //Add an instance to player collection list from dictionary
                    mPlayerDeck.Add(mCardDictionary[part[0].Trim()]);
                }

            }

            //Sort By Cost and name
            mPlayerDeck = mPlayerDeck.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();

            //SAVE BASE AS PLAYER DECK NAME WITH DECK FILE NAME
            using (StreamWriter streamWriter = File.CreateText(deckFilePath))
            {   
                //Write headers
                streamWriter.WriteLine("ID_CARD;QUANTITY");

                for (int i = 1; i < line.Count() - 1; i++)
                {
                    //Write each line read in base deck
                    streamWriter.Write(line[i]);
                }
                streamWriter.Close();
            }

            #if UNITY_EDITOR
            //Re-import the file to update the reference in the editor
            AssetDatabase.ImportAsset("Assets/Data/PlayerDeck.csv");
            #endif
        }
        else
        {
            print("Loading Player deck");
            //Make a list of strings for each line
            line = File.ReadAllLines(deckFilePath).ToList();

            //Build list of Basecard
            for (int i = 1; i < line.Count() ; i++)
            {
                //make an array of parts in line (divided by ;)
                string[] part = line[i].Split(new char[] { ';' });

                //add as many as in quantity row
                for (int x = 0; x < Convert.ToInt32(part[1].Trim()); x++)
                {
                    //Add an instance to player collection list from dictionary
                    mPlayerDeck.Add(mCardDictionary[part[0].Trim()]);
                }

            }

            //Sort By Cost and name
            mPlayerDeck = mPlayerDeck.OrderBy(c => c.mCost).ThenBy(n => n.mCardName).ToList();
        }


    }
    public void SaveDeck(List<BaseCard> ToSave)
    {
        //List Count deck cards in decklist to save
        var newDeck = ToSave.GroupBy(info => info.mCardId)
                            .Select(group => new { Metric = group.Key, Count = group.Count() })
                            .OrderBy(x => x.Metric).ToList();

        //Set File path
        string deckFilePath = dataDirectory + "/" + deckFileName;

        //Overwite or create decklist file
        using (StreamWriter streamWriter = File.CreateText(deckFilePath))
        {
            //Write headers
            streamWriter.WriteLine("ID_CARD;QUANTITY");
            
            //Write each line in list
            foreach (var card in newDeck)
            {
                streamWriter.WriteLine(card.Metric + ";" + card.Count);
            }
            streamWriter.Close();
        }

        #if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset("Assets/Data/PlayerDeck.csv");
        #endif

        //Load player deck again 
        LoadDeck();

    }
    #endregion

    #region GetSet

    //GET SECCTION
    public List<BaseCard> GetPlayerCollection()
    {
        return mPlayerCollection;
    }
    public Dictionary<string, int> GetCollectionQuantity()
    {
        return mCollectionCardQuantity;
    }
    public List<BaseCard> GetPlayerDeck()
    {
        return mPlayerDeck;
    }



    //SET SECCTION
    public void SetPlayerDeck(List<BaseCard> deckList)
    {
        mPlayerDeck = deckList;
    }

    //TO JUSE LATER WHEN BUYING CARDS
    public void SetPlayerCollection(List<BaseCard> collectionList)
    {
        mPlayerCollection = collectionList;
    }
    #endregion
}