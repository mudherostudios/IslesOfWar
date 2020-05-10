using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateChiOrderPrompt : ChiPrompt
{
    public ChiOrderFormation former;

    public void Ok()
    {
        former.CreateChiOrder();
        Close();
    }

    public void Cancel()
    {
        former.CancelChiOffer();
        Close();
    }
}
