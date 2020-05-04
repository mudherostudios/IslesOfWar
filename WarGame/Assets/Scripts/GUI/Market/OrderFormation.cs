using UnityEngine;
using UnityEngine.UI;

public class OrderFormation : MonoBehaviour
{
    public MarketClient Client;
    public OpenOrderPrompt PromptWindow;
    public InputField WarbuxField, OilField, MetalField, ConcreteField;
    public Button NextButton, CancelButton;
    public Text Phase;

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

        if (stage == 1) Phase.text = "Buying";
        if (stage == 2)
        {
            PromptWindow.Prompt(Client.Sells, Client.Buys, Client.Fees);
            Phase.text = "Confirm";
            NextButton.transform.GetChild(0).GetComponent<Text>().text = "Confirm";
            InteractableInputs(false);
            NextButton.interactable = false;
            CancelButton.interactable = false;
        }
    }

    public void CheckMax(int field) { if (stage == 0) MaxInput(field); }

    private void MaxInput(int field)
    {
        double converted = 0;

        if (field == 0) converted = double.Parse(WarbuxField.text);
        else if (field == 1) converted = double.Parse(OilField.text);
        else if (field == 2) converted = double.Parse(MetalField.text);
        else if (field == 3) converted = double.Parse(ConcreteField.text);

        if (converted > Client.PlayerResources[field])
        {
            if (field == 0) WarbuxField.text = Client.PlayerResources[field].ToString();
            if (field == 1) OilField.text = Client.PlayerResources[field].ToString();
            if (field == 2) MetalField.text = Client.PlayerResources[field].ToString();
            if (field == 3) ConcreteField.text = Client.PlayerResources[field].ToString();
        }
    }

    public void CreateMarketOrder()
    {
        bool success = Client.PlaceMarketOrder();
        Debug.Log($"Placed Order:{success}");
        CancelOffer();
    }

    private void CommitValues()
    {
        double[] values = new double[4];

        successful &= double.TryParse(WarbuxField.text, out values[0]);
        successful &= double.TryParse(OilField.text, out values[1]);
        successful &= double.TryParse(MetalField.text, out values[2]);
        successful &= double.TryParse(ConcreteField.text, out values[3]);

        if (stage == 0) Client.AddSells(values);
        else if (stage == 1) Client.AddBuys(values);

        stage++;
    }

    void SetInputs(string value, int initiator)
    {
        if (initiator != 0) WarbuxField.text = value;
        if (initiator != 1) OilField.text = value;
        if (initiator != 2) MetalField.text = value;
        if (initiator != 3) ConcreteField.text = value;
    }

    void InteractableInputs(bool interactable)
    {
        WarbuxField.interactable = interactable;
        OilField.interactable = interactable;
        MetalField.interactable = interactable;
        ConcreteField.interactable = interactable;
    }

    public void StartOffer(int initiator)
    {
        if (stage == -1)
        {
            stage++;
            Phase.text = "Selling";
            NextButton.interactable = true;
            CancelButton.interactable = true;
            SetInputs("0", initiator);
        }
    }

    public void CancelOffer()
    {
        SetInputs(null, -1);
        
        stage = -1;
        Phase.text = "Order Formation";

        Client.ClearOpenOrderBuffer();

        NextButton.transform.GetChild(0).GetComponent<Text>().text = "Next";
        NextButton.interactable = false;
        CancelButton.interactable = false;
        InteractableInputs(true);
    }
}
