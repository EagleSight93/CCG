using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class A_L01 : BaseUnit
{
    public override void Setup(GameObject card, string mTeam)
    {
        base.Setup(card, mTeam);

        if (mCurrentTeam.Equals("Player"))
        {
            EntityManager.manager.mPlayerArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = mHealth.ToString();
        }
        else
        {

        }


    }



    public override void damage(int damage)
    {
        base.damage(damage);

        if (mCurrentTeam.Equals("Player"))
        {
            EntityManager.manager.mPlayerArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = mHealth.ToString();
        }
        else
        {

        }

    }


}
