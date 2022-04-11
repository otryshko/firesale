public class Query
{
    public IQueryable<User> GetUsers([Service] MemoryStore memoryStore) =>
        memoryStore.Users.Values.AsQueryable();

    public IQueryable<FireSaleItem> GetAvailableFireSaleItems([Service] MemoryStore memoryStore, int userId) {
        var user = memoryStore.Users[userId];

        return memoryStore.Items.Values.Where(
            i => i.BeginAtUtc <= DateTime.UtcNow && 
            i.Price <= user.Balance && 
            memoryStore.Purchases[userId].Where(p => p.FireSaleItemId == i.Id && (p.State == PurchaseState.Pending || p.State == PurchaseState.Completed)).Count() == 0).AsQueryable();
    }

}