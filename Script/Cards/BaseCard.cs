using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BaseCard 
{
    public string mCardId;
    public string mImgFileName;
    public string mCardName;
    public string mType;
    public string mSubType;
    public string mDescription;
    public int mCost;
    public int mPower;
    public int mHealth;

    public BaseCard()
    {

    }

    public BaseCard(string id, string imgFileName, string name, string type, string subType, string description, int cost, int power, int hp)
    {
        mCardId = id;
        mImgFileName = imgFileName;
        mCardName = name;
        mType = type;
        mSubType = subType;
        mDescription = description;
        mCost = cost;
        mPower = power;
        mHealth = hp;

    }
}
