public class PurchaseFireSaleItemPayload {
    public PurchaseFireSaleItemPayload(Purchase purchase){
        Purchase = purchase;
    }
    public Purchase Purchase { get; }
}