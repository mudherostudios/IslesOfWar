using UnityEngine;
using UnityEngine.UI;

public class OrderFormation : MonoBehaviour
{
    public MarketClient client;
    public OpenOrderPrompt promptWindow;
    public InputField warbuxField, oilField, metalField, concreteField;
    public Button nextButton, cancelButton;
    public Text phase;

    private int stage = -2;
    private bool successful = true;

    private void Start() { CancelOffer(); }
    
    public void Next()
    {
        if (stage == 0 || stage == 1)
        {
            CommitValues();
            SetInputs("0", -1);
        }

        if (stage == 1) phase.text = "Buying";
        if (stage == 2)
        {
            promptWindow.Prompt(client.Sells, client.Buys, client.Fees);
            phase.text = "Confirm";
            nextButton.transform.GetChild(0).GetComponent<Text>().text = "Confirm";
            InteractableInputs(false);
            nextButton.interactable = false;
            cancelButton.interactable = false;
        }
    }

    public void CheckMax(int field) { if (stage == 0) MaxInput(field); }

    private void MaxInput(int field)
    {
        double converted = 0;

        if (field == 0 && warbuxField.text != null) converted = double.Parse(warbuxField.text);
        else if (field == 1 && oilField.text != null) converted = double.Parse(oilField.text);
        else if (field == 2 && metalField.text != null) converted = double.Parse(metalField.text);
        else if (field == 3 && concreteField.text != null) converted = double.Parse(concreteField.text);

        if (converted > client.PlayerResources[field])
        {
            if (field == 0) warbuxField.text = client.PlayerResources[field].ToString();
            else if (field == 1) oilField.text = client.PlayerResources[field].ToString();
            else if (field == 2) metalField.text = client.PlayerResources[field].ToString();
            else if (field == 3) concreteField.text = client.PlayerResources[field].ToString();
        }
    }

    public void CreateMarketOrder()
    {
        bool success = client.PlaceMarketOrder();
        Debug.Log($"Placed Order:{success}");
        CancelOffer();
    }

    private void CommitValues()
    {
        double[] values = new double[4];

        successful &= double.TryParse(warbuxField.text, out values[0]);
        successful &= double.TryParse(oilField.text, out values[1]);
        successful &= double.TryParse(metalField.text, out values[2]);
        successful &= double.TryParse(concreteField.text, out values[3]);

        if (stage == 0) client.AddSells(values);
        else if (stage == 1) client.AddBuys(values);

        stage++;
    }

    void SetInputs(string value, int initiator)
    {
        if (initiator != 0) warbuxField.text = value;
        if (initiator != 1) oilField.text = value;
        if (initiator != 2) metalField.text = value;
        if (initiator != 3) concreteField.text = value;
    }

    void InteractableInputs(bool interactable)
    {
        warbuxField.interactable = interactable;
        oilField.interactable = interactable;
        metalField.interactable = interactable;
        concreteField.interactable = interactable;
    }

    public void StartOffer(int initiator)
    {
        if (stage == -1)
        {
            stage++;
            phase.text = "Selling";
            nextButton.interactable = true;
            cancelButton.interactable = true;
            SetInputs("0", initiator);
        }
    }

    public void CancelOffer()
    {
        SetInputs(null, -1);
        
        stage = -1;
        phase.text = "Order Formation";

        client.ClearOpenOrderBuffer();

        nextButton.transform.GetChild(0).GetComponent<Text>().text = "Next";
        nextButton.interactable = false;
        cancelButton.interactable = false;
        InteractableInputs(true);
    }
}
