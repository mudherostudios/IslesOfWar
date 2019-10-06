using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostSlider : MonoBehaviour
{
    public Vector2 onPosition;
    public Vector2 offPosition;
    public float speed;

    public Text[] costTexts;
    public Text title;
    public double[] cost;
  
    //Not working right now but would be nice to have a slide effect.
    public bool TurnOn(float startTime, bool turnOn)
    {
        bool isStopped = false;
        Vector2 startPosition = offPosition;
        Vector2 endPosition = onPosition;

        if (!turnOn)
        {
            startPosition = onPosition;
            endPosition = offPosition;
        }

        float movedAmount = (Time.time - startTime) * speed;
        float totalDistance = Vector2.Distance(onPosition, offPosition);
        float fractionMoved = movedAmount / totalDistance;

        Debug.Log(fractionMoved);
        isStopped = fractionMoved >= 1.0 - 0.01;

        if (isStopped)
            transform.position = endPosition;
        else
            transform.position = Vector2.Lerp(startPosition, endPosition, fractionMoved);

        return isStopped;
    }

    public void TurnOn(bool on)
    {
        if (on)
            transform.position = onPosition;
        else
            transform.position = offPosition;
    }

    public void SetCost(string name, double[] _cost)
    {
        title.text = name;
        cost = _cost;
        costTexts[0].text = string.Format("-{0}", cost[0]);
        costTexts[1].text = string.Format("-{0}", cost[1]);
        costTexts[2].text = string.Format("-{0}", cost[2]);
        costTexts[3].text = string.Format("-{0}", cost[3]);
    }
}
