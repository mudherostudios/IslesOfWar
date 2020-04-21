using System;
using System.Collections.Generic;
using UnityEngine;
using MudHero.WebSocketCommunication;
using MudHero.XayaCommunication;
using BitcoinLib.Responses;

public class TransactionResolver : MonoBehaviour
{
    public XayaCommander Commander;
    public Dictionary<Guid, string> completedTransactions = new Dictionary<Guid, string>();
    private Dictionary<Guid, string> unsignedProposals = new Dictionary<Guid, string>();
    
    //Telemetry should only pass valid orderIds of online users.
    public TradePayload HandleTransactionData(TradePayload tradePayload, OrderPayload order = null, string command = null)
    {
        switch (tradePayload.Data.Phase)
        {
            case TransactionPhase.ADDRESS_REQUEST:
                return GetNameAndPaymentAddresses(tradePayload.PlayerName, tradePayload.Data.Id);
            case TransactionPhase.ADDRESS_RESPONSE:
                return GetProposal(tradePayload.PlayerName, tradePayload.Data.InputAddresses, order, command);
            case TransactionPhase.PROPOSAL:
                return GetSellerSignedProposal(tradePayload.PlayerName, tradePayload.Data.Contract, tradePayload.Data.Id);
            case TransactionPhase.SELLER_REJECT:
                Debug.LogWarning("SELLER_REJECT:\n" + tradePayload.Data.Reason);
                return null;
            case TransactionPhase.SELLER_SIGN:
                return GetFinalizedResponse(tradePayload);
            case TransactionPhase.BIDDER_REJECT_SIGN:
                Debug.LogWarning("BIDDER_REJECT_SIGN:\n" + tradePayload.Data.Reason);
                return null;
            case TransactionPhase.BIDDER_CONFIRMED_SIGN:
                completedTransactions.Add(tradePayload.Data.Id, tradePayload.Data.Contract);
                return null;
            case TransactionPhase.REJECT_PERMANENT:
                Debug.LogWarning("REJECT_PERMANENT:\n" + tradePayload.Data.Reason);
                return null;
            default:
                return null;
        }
    }
//-----------------------------------------------------------------------------------------------------------------
//-------------------------------------------Response Checking-----------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------
    public TradePayload GetFinalizedResponse(TradePayload tradePayload)
    {
        FinalizePsbtResponse finalized = GetFinalizedProposal(unsignedProposals[tradePayload.Data.Id], tradePayload.Data.Contract);
        if (finalized.Complete)
        {
            completedTransactions.Add(tradePayload.Data.Id, finalized.Hex);
            unsignedProposals.Remove(tradePayload.Data.Id);
            return GetConfirmationPayload(tradePayload.PlayerName, finalized.Hex, tradePayload.Data.Id);
        }
        else
        {
            unsignedProposals.Remove(tradePayload.Data.Id);
            return GetRejectProposal(tradePayload.PlayerName, "Finalizaiton Failed.", 2, tradePayload.Data.Id);
        }
    }
//-----------------------------------------------------------------------------------------------------------------
//--------------------------------------------Payload Formation----------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------
    //Message for seller to send two of their addresses that can be used for the contract.
    public TradePayload GetSellerAddresses(string recipient, Guid orderId)
    {
        TransactionData requestData = new TransactionData(TransactionPhase.ADDRESS_REQUEST, 0, null, null, orderId);
        return new TradePayload(recipient, Commander.GetPlayer(), requestData);
    }

    //Message containting two generated or selected addresses for the buyer.
    private TradePayload GetNameAndPaymentAddresses(string recipient, Guid orderId)
    {
        Commander.UnlockWallet();
        TransactionData sellerNameInputs = new TransactionData(TransactionPhase.ADDRESS_RESPONSE, 0, null, null, orderId);
        sellerNameInputs.InputAddresses.Add(Commander.xayaService.GetNewAddress(null));
        sellerNameInputs.InputAddresses.Add(Commander.xayaService.GetNewAddress(null));
        return new TradePayload(recipient, Commander.GetPlayer(), sellerNameInputs);
    }

    //Message containing the original proposal.
    private TradePayload GetProposal(string recipient, List<string> inputAddresses, OrderPayload order, string command)
    {
        Commander.UnlockWallet();
        unsignedProposals.Add(order.OrderId,Commander.GetUnsignedProposal(Commander.GetPlayer(), inputAddresses[0], inputAddresses[1], command, order.PriceInChi));
        TransactionData unsignedProposalData = new TransactionData(TransactionPhase.PROPOSAL, TransactionType.PSBT, unsignedProposals[order.OrderId], null, order.OrderId);
        return new TradePayload(recipient, Commander.GetPlayer(), unsignedProposalData);
    }

    //Message containing signed proposal by the seller for the buyer.
    private TradePayload GetSellerSignedProposal(string recipient, string psbt, Guid orderId)
    {
        Commander.UnlockWallet();
        string signedPsbt = Commander.SignPsbt(psbt);
        TransactionData signedData = new TransactionData(TransactionPhase.SELLER_SIGN, TransactionType.PSBT, signedPsbt, null, orderId);
        return new TradePayload(recipient, Commander.GetPlayer(), signedData);
    }

    //Combine both signatures and finalize the proposal.
    private FinalizePsbtResponse GetFinalizedProposal(string unsignedProposal, string sellerSignedProposal)
    {
        List<string> signatures = new List<string>() { sellerSignedProposal, Commander.SignPsbt(unsignedProposal) };
        return Commander.FinalizePsbt(signatures);
    }

    //Message containing the finalized psbt in hex format and sent to seller as receipt.
    private TradePayload GetConfirmationPayload(string recipient, string finalizedHex, Guid orderId)
    {
        TransactionData confirmationData = new TransactionData(TransactionPhase.BIDDER_CONFIRMED_SIGN, TransactionType.HEX, finalizedHex, null, orderId);
        return new TradePayload(recipient, Commander.GetPlayer(), confirmationData);
    }

    //Message rejecting proposal because of bad or incorrect data.
    public TradePayload GetRejectProposal(string recipient, string reason, int rejectionType, Guid orderId)
    {
        TransactionPhase rejectionPhase;

        switch (rejectionType)
        {
            case 0:
                rejectionPhase = TransactionPhase.SELLER_REJECT;
                break;
            case 1:
                rejectionPhase = TransactionPhase.BIDDER_REJECT_SIGN;
                break;
            case 2:
                rejectionPhase = TransactionPhase.REJECT_PERMANENT;
                break;
            default:
                rejectionPhase = TransactionPhase.REJECT_PERMANENT;
                break;
        }

        TransactionData rejectionData = new TransactionData(rejectionPhase, 0, null, reason, orderId);
        return new TradePayload(recipient, Commander.GetPlayer(), rejectionData);
    }
}
