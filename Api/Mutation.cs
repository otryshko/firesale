using Microsoft.EntityFrameworkCore;
public class Mutation
{
    public async Task<AddUserPayload> AddUserAsync(
        AddUserInput input,
        [Service] ApplicationDbContext context,
        [Service] MemoryStore memoryStore)
    {
        var user = new User
        {
            Name = input.Name,
            Balance = input.Balance
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        memoryStore.AddUser(user);

        return new AddUserPayload(user);
    }
    public async Task<AddFireSaleItemPayload> AddFireSaleItemAsync(
        AddFireSaleItemInput input,
        [Service] ApplicationDbContext context,
        [Service] MemoryStore memoryStore)
    {
        var fireSaleItem = new FireSaleItem
        {
            Name = input.Name,
            Price = input.Price,
            Quantity = input.Quantity,
            BeginAtUtc = input.BeginAtUtc
        };

        context.FireSaleItems.Add(fireSaleItem);
        await context.SaveChangesAsync();
        memoryStore.AddFireSaleItem(fireSaleItem);

        return new AddFireSaleItemPayload(fireSaleItem);
    }

    public async Task<PurchaseFireSaleItemPayload> PurchaseFireSaleItemAsync(
        PurchaseFireSaleItemInput input,
        [Service] ApplicationDbContext context,
        [Service] MemoryStore memoryStore)
    {
        return new PurchaseFireSaleItemPayload(memoryStore.StartPurchase(input.UserId, input.FireSaleItemId));
    }

    public async Task<CompletePurchasePayload> CompletePurchaseAsync(
        PurchaseFireSaleItemInput input,
        [Service] ApplicationDbContext context,
        [Service] MemoryStore memoryStore)
    {
        var purchase = memoryStore.MarkPurchaseAsCompleted(input.UserId, input.FireSaleItemId);

        using var transaction = RelationalDatabaseFacadeExtensions.BeginTransaction(context.Database, System.Data.IsolationLevel.Serializable);
        
        context.Purchases.Add(purchase);
        context.Entry(purchase.FireSaleItem).State = EntityState.Unchanged;
        context.Entry(purchase.User).State = EntityState.Unchanged;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CompletePurchasePayload(purchase);
    }

}