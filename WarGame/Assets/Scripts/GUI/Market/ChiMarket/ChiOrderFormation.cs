using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChiOrderFormation : MonoBehaviour
{
    public MarketClient client;
    public InputField chiInput, warbuxInput;
    public Button submitButton, cancelButton;
    public CreateChiOrderPrompt orderPrompt;

    private bool offering;

    private void Start() { offering = true;  CancelChiOffer(); }

    public void PromptForConfirmation()
    {
        if (chiInput.text == null || warbuxInput.text == null) { CancelChiOffer(); return; }

        decimal convertedPrice = decimal.Parse(chiInput.text);
        int convertedAmount = int.Parse(warbuxInput.text);

        if (convertedAmount == 0 || convertedPrice == 0) { CancelChiOffer(); return; }

        client.AddChiOrder(convertedPrice, convertedAmount);
        orderPrompt.Prompt(client.ChiPrice, client.WarbuxAmount);
        InteractableButtons(false);
        InteractableInputs(false);
    }

    public void CreateChiOrder()
    {
        CancelChiOffer();
    }

    public void CancelChiOffer()
    {
        client.ClearChiOrder();
        InteractableButtons(false);
        InteractableInputs(true);
        SetInputs(null, -1);
        offering = false;
    }

    public void CheckMax()
    {
        if (warbuxInput.text == null || warbuxInput.text == "") return;
        double converted = double.Parse(warbuxInput.text);
        if (converted > client.PlayerResources[0]) warbuxInput.text = client.PlayerResources[0].ToString();
    }

    public void StartOffer(int field)
    {
        if (!offering)
        {
            offering = true;
            SetInputs("0", field);
            submitButton.interactable = true;
        }
    }

    private void SetInputs(string value, int field)
    {
        if (field != 0) chiInput.text = value;
        if (field != 1) warbuxInput.text = value;
    }

    private void InteractableInputs(bool interactable)
    {
        chiInput.interactable = interactable;
        warbuxInput.interactable = interactable;
    }

    private void InteractableButtons(bool interactable)
    {
        submitButton.interactable = interactable;
        cancelButton.interactable = interactable;
    }
}
