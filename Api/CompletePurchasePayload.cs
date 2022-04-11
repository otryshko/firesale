public class CompletePurchasePayload {
    public CompletePurchasePayload(Purchase purchase){
        Purchase = purchase;
    }
    public Purchase? Purchase { get; }
}