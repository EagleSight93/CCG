using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyCell : MonoBehaviour
{
    // Start is called before the first frame update
    public Color depletedColor;
    public Color avaibleColor;
    public bool charged;

    public void Charge()
    {
        charged = true;
        transform.GetComponent<Image>().color = avaibleColor;
        EntityManager.manager.currentEnergy++;
    }

    public void Use()
    {
        charged = false;
        transform.GetComponent<Image>().color = depletedColor;
        EntityManager.manager.currentEnergy--;
    }

    public IEnumerator Blink()
    {
        //counter c
        float c = 0;

        //Always
        while (true)
        {
            //lerp between avaible and depleted w/Mathf PingPong
            Color lerpedColor;
            lerpedColor = Color.Lerp(depletedColor, avaibleColor, Mathf.PingPong(c, 1));
            //how fast
            c += 0.020f;
            //Set color
            transform.GetComponent<Image>().color = lerpedColor;
            //each frame
            yield return null;
        }

    }


}
